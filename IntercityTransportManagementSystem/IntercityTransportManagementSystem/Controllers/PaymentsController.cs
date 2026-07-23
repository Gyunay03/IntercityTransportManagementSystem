using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IntercityTransportManagementSystem.Models;
using IntercityTransportManagementSystem.Enums;
using Microsoft.AspNetCore.SignalR;
using IntercityTransportManagementSystem.Hubs;
using IntercityTransportManagementSystem.ViewModels;
using QRCoder;
using System.Security.Claims;
using IntercityTransportManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;

namespace IntercityTransportManagementSystem.Controllers
{
    [Authorize]
    public class PaymentsController : Controller
    {
        private readonly IntercityTransportManagementSystemDatabaseContext _context;
        private readonly IHubContext<ReservationHub> _hub;
        private readonly INotificationService _notificationService;

        public PaymentsController(IntercityTransportManagementSystemDatabaseContext context, IHubContext<ReservationHub> hub, INotificationService notificationService)
        {
            _context = context;
            _hub = hub;
            _notificationService = notificationService;
        }

        // Метод за преглед и избор на плащане 
        [HttpGet]
        public async Task<IActionResult> Checkout(int reservationId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Passenger)
                .Include(r => r.Seat)
                .Include(r => r.Schedule)
                    .ThenInclude(s => s.Route)
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null || reservation.Status != ReservationStatus.Pending)
            {
                return RedirectToAction("Index", "Reservations"); 
            }

            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var passenger = await _context.Passengers
                    .FirstOrDefaultAsync(p => p.UserId == currentUserId);

            if (passenger == null || reservation.PassengerId != passenger.Id)
            {
                return Forbid();
            }

            decimal totalPrice = reservation.Schedule.Route.TicketPrice;

            if (reservation.TicketType == TicketType.Dvuposochen && reservation.ReturnReservationId.HasValue)
            {
                var returRes = await _context.Reservations
                    .Include(r => r.Schedule.Route)
                    .FirstOrDefaultAsync(r => r.Id == reservation.ReturnReservationId);

                if (returRes != null)
                { 
                    totalPrice = (reservation.Schedule.Route.TicketPrice + returRes.Schedule.Route.TicketPrice) * 0.90m;
                }
            }

            ViewBag.TotalPrice = totalPrice;
            return View(reservation);
        }

        // Метод за финализиране на процеса на плащане
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(int reservationId, PaymentMethod paymentMethod)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Schedule)
                    .ThenInclude(s => s.Route)
                .Include(r => r.Passenger)
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null)
            {
                return BadRequest("Невалидна резервация.");
            }

            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var passenger = await _context.Passengers
                    .FirstOrDefaultAsync(p => p.UserId == currentUserId);

            if (passenger == null || reservation.PassengerId != passenger.Id)
            {
                return Forbid();
            }

            var seatLock = await _context.BusSeatLocks
               .FirstOrDefaultAsync(l => l.ScheduleId == reservation.ScheduleId && l.SeatId == reservation.SeatId);

            if (seatLock != null)
            {
                _context.BusSeatLocks.Remove(seatLock);
            }

            if (paymentMethod != PaymentMethod.Online && paymentMethod != PaymentMethod.Card)
            {
                paymentMethod = PaymentMethod.Cash;
            }

            decimal amountToPay = reservation.Schedule.Route.TicketPrice;
            if (reservation.TicketType == TicketType.Dvuposochen)
            {
                amountToPay *= 1.8m;
            }

            if (paymentMethod == PaymentMethod.Online)
            {
                await Task.Delay(2000);

                var payment = new Payment
                {
                    PassengerId = reservation.PassengerId,
                    Sum = amountToPay,
                    ReservationId = reservation.Id,
                    PaymentMethod = paymentMethod,
                    PaymentDate = DateTime.Now
                };
                
                _context.Payments.Add(payment);
            }

            reservation.Status = ReservationStatus.Confirmed;
            reservation.ExpirationTime = null;
            reservation.IsActive = true;

            await _context.SaveChangesAsync();

            if (reservation.Passenger.UserId != null)
            {
                await _notificationService.CreateNotificationAsync(reservation.Passenger.UserId.Value, "Успешно купен билет",
                    $"Вашият билет за {reservation.Schedule.Route.StartDestination} - {reservation.Schedule.Route.FinalDestination} " +
                    $"на {reservation.Schedule.TravelDate:dd.MM.yyyy} в {reservation.Schedule.DepartureTime:HH:mm} часа е успешно купен.",
                    NotificationType.TicketPurchased);
            }

            await _hub.Clients.All.SendAsync("SeatReserved", new
            {
                seatId = reservation.SeatId,
                scheduleId = reservation.ScheduleId
            });

            return RedirectToAction(nameof(Success), new { reservationId = reservation.Id, method = paymentMethod });
        }

        // Метод за отразяване на състоянието на процеса (успех, билет и потвърждение)
        public async Task<IActionResult> Success(int reservationId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Passenger)
                .Include(r => r.Seat)
                .Include(r => r.Schedule)
                    .ThenInclude(s => s.Route)
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null)
            {
                return NotFound();
            }

            //
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.ReservationId == reservationId ||
                                    (reservation.ReturnReservationId.HasValue && p.ReservationId == reservation.ReturnReservationId));

            if (payment == null)
            {
                var outbound = await _context.Reservations.FirstOrDefaultAsync(r => r.ReturnReservationId == reservationId);
                if (outbound != null)
                {
                    payment = await _context.Payments.FirstOrDefaultAsync(p => p.ReservationId == outbound.Id);
                }
            }

            ViewBag.Payment = payment;

            if (reservation.TicketType == TicketType.Dvuposochen)
            {
                var outbound = await _context.Reservations
                    .Include(r => r.Seat)
                    .Include(r => r.Schedule)
                        .ThenInclude(s => s.Route)
                    .FirstOrDefaultAsync(r => r.ReturnReservationId == reservation.Id);

                var returnTrip = outbound != null ? reservation : await _context.Reservations
                    .Include(r => r.Seat)
                    .Include(r => r.Schedule)
                        .ThenInclude(s => s.Route)
                    .FirstOrDefaultAsync(r => r.Id == reservation.ReturnReservationId);

                ViewBag.Outbound = outbound ?? reservation;
                ViewBag.Return = returnTrip;
            }

            if (User.IsInRole("Passenger"))
            {
                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                var passenger = await _context.Passengers
                    .FirstOrDefaultAsync(p => p.UserId == currentUserId);

                if (passenger == null || reservation.PassengerId != passenger.Id)
                {
                    return Forbid();
                }
            }
            else if (!User.IsInRole("Administrator") && !User.IsInRole("Driver"))
            {
                return Forbid();
            }

            return View(reservation);
        }

        // GET: Payments
        [HttpGet]
        public async Task<IActionResult> Index(string searchString, string sortOrder, PaymentMethod? paymentMethod, DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 20)
        {
            var paymentsQuery = _context.Payments
                .Include(p => p.Passenger)
                .Include(p => p.Reservation)
                    .ThenInclude(s => s.Schedule)
                        .ThenInclude(r => r.Route)
                .Include(p => p.Reservation)
                    .ThenInclude(r => r.Schedule)
                        .ThenInclude(s => s.Bus)
                .Include(p => p.Reservation)
                    .ThenInclude(r => r.Seat)
                .AsNoTracking()
                .AsQueryable();

            if (User.IsInRole("Passenger"))
            {
                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                var passenger = await _context.Passengers
                    .FirstOrDefaultAsync(p => p.UserId == currentUserId);

                if (passenger == null)
                {
                    return Forbid();
                }

                paymentsQuery = paymentsQuery.Where(p => p.PassengerId == passenger.Id);
            }
            else if (!User.IsInRole("Administrator") && !User.IsInRole("Driver"))
            {
                return Forbid();
            }

            // Филтриране по име и фамилия на пътник
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                paymentsQuery = paymentsQuery.Where(p =>
                    (p.Passenger.Name + " " + p.Passenger.LastName).Contains(searchString));
            }

            // Филтриране по метод на плащане
            if (paymentMethod.HasValue)
            {
                paymentsQuery = paymentsQuery.Where(p => p.PaymentMethod == paymentMethod.Value);
            }

            // Филтриране на период от дата на плащане
            if (fromDate.HasValue)
            {
                paymentsQuery = paymentsQuery.Where(p => p.PaymentDate >= fromDate.Value);
            }

            // Филтриране на период до дата на плащане
            if (toDate.HasValue)
            {
                paymentsQuery = paymentsQuery.Where(p => p.PaymentDate <= toDate.Value);
            }

            // Сортиране
            switch (sortOrder)
            {
                case "passenger":
                    paymentsQuery = paymentsQuery.OrderBy(p =>
                    (p.Passenger.Name + " " + p.Passenger.LastName));
                    break;

                case "passenger_descending":
                    paymentsQuery = paymentsQuery.OrderByDescending(p =>
                    (p.Passenger.Name + " " + p.Passenger.LastName));
                    break;

                case "route":
                    paymentsQuery = paymentsQuery.OrderBy(p =>
                    (p.Reservation.Schedule.Route.StartDestination + " - " + p.Reservation.Schedule.Route.FinalDestination));
                    break;

                case "route_descending":
                    paymentsQuery = paymentsQuery.OrderByDescending(p =>
                    (p.Reservation.Schedule.Route.StartDestination + " - " + p.Reservation.Schedule.Route.FinalDestination));
                    break;

                case "travelDate":
                    paymentsQuery = paymentsQuery.OrderBy(p => p.Reservation.Schedule.TravelDate);
                    break;

                case "travelDate_descending":
                    paymentsQuery = paymentsQuery.OrderByDescending(p => p.Reservation.Schedule.TravelDate);
                    break;

                case "departureTime":
                    paymentsQuery = paymentsQuery.OrderBy(p => p.Reservation.Schedule.DepartureTime);
                    break;

                case "departureTime_descending":
                    paymentsQuery = paymentsQuery.OrderByDescending(p => p.Reservation.Schedule.DepartureTime);
                    break;

                case "bus":
                    paymentsQuery = paymentsQuery.OrderBy(p => p.Reservation.Schedule.Bus.RegistrationNumber);
                    break;

                case "bus_descending":
                    paymentsQuery = paymentsQuery.OrderByDescending(p => p.Reservation.Schedule.Bus.RegistrationNumber);
                    break;

                case "seat":
                    paymentsQuery = paymentsQuery.OrderBy(p => p.Reservation.Seat.Number);
                    break;

                case "seat_descending":
                    paymentsQuery = paymentsQuery.OrderByDescending(p => p.Reservation.Seat.Number);
                    break;

                case "sum":
                    paymentsQuery = paymentsQuery.OrderBy(p => p.Sum);
                    break;
                
                case "sum_descending":
                    paymentsQuery = paymentsQuery.OrderByDescending(p => p.Sum);
                    break;
                
                case "paymentDate":
                    paymentsQuery = paymentsQuery.OrderBy(p => p.PaymentDate);
                    break;

                case "paymentDate_descending":
                    paymentsQuery = paymentsQuery.OrderByDescending(p => p.PaymentDate);
                    break;

                case "paymentMethod":
                    paymentsQuery = paymentsQuery.OrderBy(p => p.PaymentMethod);
                    break;

                case "paymentMethod_descending":
                    paymentsQuery = paymentsQuery.OrderByDescending(p => p.PaymentMethod);
                    break;

                case "paymentStatus":
                    paymentsQuery = paymentsQuery.OrderBy(p => p.PaymentStatus);
                    break;

                case "paymentStatus_descending":
                    paymentsQuery = paymentsQuery.OrderByDescending(p => p.PaymentStatus);
                    break;

                default:
                    paymentsQuery = paymentsQuery.OrderBy(p => p.PaymentDate);
                    break;
            }

            // Странициране
            var allPayments = await paymentsQuery.CountAsync();
            var payments = await paymentsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(allPayments / (double)pageSize);
            var viewModel = new PaymentIndexViewModel
            {
                Payments = payments,
                SearchString = searchString,
                SortOrder = sortOrder,
                FromDate = fromDate,
                ToDate = toDate,
                PaymentMethod = paymentMethod,
                CurrentPage = page,
                TotalPages = totalPages
            };
            
            return View(viewModel);
        }

        // GET: Payments/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments
                .Include(p => p.Passenger)
                .Include(p => p.Reservation)
                    .ThenInclude(s => s.Schedule)
                        .ThenInclude(r => r.Route)
                .Include(p => p.Reservation)
                    .ThenInclude(r => r.Schedule)
                        .ThenInclude(s => s.Bus)
                .Include(p => p.Reservation)
                    .ThenInclude(r => r.Seat)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (payment == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Passenger"))
            {
                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                var passenger = await _context.Passengers
                    .FirstOrDefaultAsync(p => p.UserId == currentUserId);

                if (passenger == null || payment.PassengerId != passenger.Id)
                {
                    return Forbid();
                }
            }
            else if (!User.IsInRole("Administrator") && !User.IsInRole("Driver"))
            {
                return Forbid();
            }

            return View(payment);
        }

        // GET: Payments/CancelPayment/5
        [HttpGet]
        public async Task<IActionResult> CancelPayment(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments
                .Include(p => p.Passenger)
                .Include(p => p.Reservation)
                    .ThenInclude(s => s.Schedule)
                         .ThenInclude(r => r.Route)
                .Include(p => p.Reservation)
                    .ThenInclude(r => r.Schedule)
                        .ThenInclude(s => s.Bus)
                .Include(p => p.Reservation)
                    .ThenInclude(r => r.Seat)
                .FirstOrDefaultAsync(m => m.Id == id || m.ReservationId == id);

            if (payment == null || payment.Reservation == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Passenger"))
            {
                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var passenger = await _context.Passengers
                    .FirstOrDefaultAsync(p => p.UserId == currentUserId);

                if (passenger == null || payment.PassengerId != passenger.Id)
                {
                    return Forbid();
                }
            }

            return View(payment);
        }

        // POST: Payments/CancelPaymentConfirmed/5
        [HttpPost, ActionName("CancelPayment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelPaymentConfirmed(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Reservation)
                    .ThenInclude(r => r.Schedule)
                .FirstOrDefaultAsync(p => p.Id == id || p.ReservationId == id);

            if (payment == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Passenger"))
            {
                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var passenger = await _context.Passengers
                    .FirstOrDefaultAsync(p => p.UserId == currentUserId);

                if (passenger == null || payment.PassengerId != passenger.Id)
                {
                    return Forbid();
                }
            }

            var travelDateTime = payment.Reservation.Schedule.TravelDate.ToDateTime(payment.Reservation.Schedule.DepartureTime);

            if (DateTime.Now > travelDateTime.AddDays(-1))
            {
                TempData["Error"] = "Анулирането е възможно най-късно 24 часа преди тръгване.";
                return RedirectToAction(nameof(Index));
            }

            payment.PaymentStatus = PaymentStatus.Cancelled;

            if (payment.Reservation != null)
            {
                payment.Reservation.Status = ReservationStatus.Cancelled;
                payment.Reservation.IsActive = false; 
            }

            // Създаване на запис за връщане на пари
            var refund = new Refund
            {
                PaymentId = payment.Id,
                Amount = payment.Sum,
                RequestDate = DateTime.Now,
                Status = RefundStatus.Pending,
                AdminNotes = "Автоматично генерирана заявка при анулиране от потребител."
            };

            _context.Refunds.Add(refund);
            _context.Update(payment);
            await _context.SaveChangesAsync();

            var refundPassenger = await _context.Passengers
                .FirstOrDefaultAsync(p => p.Id == payment.PassengerId);

            if (refundPassenger?.UserId != null)
            {
                await _notificationService.CreateNotificationAsync(refundPassenger.UserId.Value, "Заявка за възстановяване на сумата",
                    "Билетът е анулиран и е създадена заявка за възстановяване на сумата.",
                    NotificationType.SystemMessage);
            }
            
            await _hub.Clients.All.SendAsync("UpdateSeatStatus",
                    payment.Reservation.ScheduleId, payment.Reservation.SeatId, "Available");

            TempData["Success"] = "Билетът е анулиран. Заявката ви за възстановяване на сумата е приета за обработка.";
            return RedirectToAction(nameof(Index));
        }

        private bool PaymentExists(int id)
        {
            return _context.Payments.Any(e => e.Id == id);
        }

        // Метод за автоматично попълване на падащите менюта
        private async Task FillDropdowns(int? selectedPassengerId = null, int? selectedReservationId = null)
        {
            var passengers = await _context.Passengers.AsNoTracking()
                .Select(p => new { p.Id, FullName = p.Name + " " + p.LastName })
                .ToListAsync();

            var reservations = await _context.Reservations.AsNoTracking()
                .Select(r => new
                {
                    r.Id, 
                    ReservationInfo = "Резервация #" + r.Id + " | " + r.Schedule.Route.StartDestination + " - " + r.Schedule.Route.FinalDestination + " | Място:  " + r.Seat.Number
                })
                .ToListAsync();

            ViewData["PassengerId"] = new SelectList(passengers, "Id", "FullName", selectedPassengerId);
            ViewData["ReservationId"] = new SelectList(reservations, "Id", "ReservationInfo", selectedReservationId);
        }

        // Метод за генериране на QR код за билет
        [HttpGet]
        public async Task<IActionResult> GenerateTicketQRCode(int reservationId)
        {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Passenger"))
            {
                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var passenger = await _context.Passengers
                        .FirstOrDefaultAsync(p => p.UserId == currentUserId);

                if (passenger == null || reservation.PassengerId != passenger.Id)
                {
                    return Forbid();
                }
            }
            else if (!User.IsInRole("Administrator") && !User.IsInRole("Driver"))
            {
                return Forbid();
            }

            // Генериране на пълния URL адрес към метода  Success
            string ticketUrl = Url.Action("Success", "Payments", new { reservationId = reservationId, method = "QR" }, protocol: Request.Scheme);

            // Генериране на QR кода
            using (QRCodeGenerator qRCodeGenerator = new QRCodeGenerator())
            using (QRCodeData qRCodeData = qRCodeGenerator.CreateQrCode(ticketUrl, QRCodeGenerator.ECCLevel.Q))
            using (PngByteQRCode qRCode = new PngByteQRCode(qRCodeData))
            {
                byte[] qRCodeImage = qRCode.GetGraphic(20);

                return File(qRCodeImage, "image/png");
            }
        }
    }
}
