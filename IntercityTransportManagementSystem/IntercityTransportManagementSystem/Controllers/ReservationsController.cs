using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IntercityTransportManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using IntercityTransportManagementSystem.ViewModels;
using IntercityTransportManagementSystem.Enums;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using IntercityTransportManagementSystem.Hubs;

namespace IntercityTransportManagementSystem.Controllers
{
    public class ReservationsController : Controller
    {
        private readonly IntercityTransportManagementSystemDatabaseContext _context;

        private readonly IHubContext<ReservationHub> _hub;

        public ReservationsController(IntercityTransportManagementSystemDatabaseContext context, IHubContext<ReservationHub> hub)
        {
            _context = context;
            _hub = hub;
        }

        // GET: Reservations
        public async Task<IActionResult> Index(string searchString, string sortOrder, DateTime? reservationTimeFrom, DateTime? reservationTimeTo, DateOnly? travelDate, ReservationStatus? status, int page = 1, int pageSize = 20)
        {
            var reservationsQuery = _context.Reservations
                .Include(r => r.Passenger)
                .Include(r => r.Schedule)
                    .ThenInclude(r => r.Route)
                .Include(r => r.Schedule)
                    .ThenInclude(r => r.Bus)
                .Include(r => r.Seat)
                .AsNoTracking()
                .AsQueryable();

            // Търсене по име и фамилия на пътници и/или разписание
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                reservationsQuery = reservationsQuery.Where(r =>
                    r.Passenger.Name.Contains(searchString) ||
                    r.Passenger.LastName.Contains(searchString) ||
                    (r.Passenger.Name + " " + r.Passenger.LastName).Contains(searchString) ||

                    r.Schedule.Route.StartDestination.Contains(searchString) ||
                    r.Schedule.Route.FinalDestination.Contains(searchString) ||
                    (r.Schedule.Route.StartDestination + " - " + r.Schedule.Route.FinalDestination).Contains(searchString));
            }

            // Филтриране от дата на резервация
            if (reservationTimeFrom.HasValue)
            {
                reservationsQuery = reservationsQuery.Where(r => r.ReservationTime >= reservationTimeFrom.Value);
            }

            // Филтриране до дата на резервация
            if (reservationTimeTo.HasValue)
            {
                var toDate = reservationTimeTo.Value.Date.AddDays(1);
                reservationsQuery = reservationsQuery.Where(r => r.ReservationTime < toDate);
            }

            // Филтриране по дата на пътуване
            if (travelDate.HasValue)
            {
                reservationsQuery = reservationsQuery.Where(r => r.Schedule.TravelDate == travelDate.Value);
            }

            // Филтриране по статус
            if (status.HasValue)
            {
                reservationsQuery = reservationsQuery.Where(r => r.Status == status.Value);
            }

            // Сортиране
            switch (sortOrder)
            {
                case "route":
                    reservationsQuery = reservationsQuery.OrderBy(r =>
                    (r.Schedule.Route.StartDestination + " - " + r.Schedule.Route.FinalDestination));
                    break;

                case "route_descending":
                    reservationsQuery = reservationsQuery.OrderByDescending(r =>
                    (r.Schedule.Route.StartDestination + " - " + r.Schedule.Route.FinalDestination));
                    break;

                case "registrationNumber":
                    reservationsQuery = reservationsQuery.OrderBy(r => r.Schedule.Bus.RegistrationNumber);
                    break;

                case "registrationNumber_descending":
                    reservationsQuery = reservationsQuery.OrderByDescending(r => r.Schedule.Bus.RegistrationNumber);
                    break;

                case "reservationTime":
                    reservationsQuery = reservationsQuery.OrderBy(r => r.ReservationTime);
                    break;

                case "reservationTime_descending":
                    reservationsQuery = reservationsQuery.OrderByDescending(r => r.ReservationTime);
                    break;

                case "travelDate":
                    reservationsQuery = reservationsQuery.OrderBy(r => r.Schedule.TravelDate);
                    break;

                case "travelDate_descending":
                    reservationsQuery = reservationsQuery.OrderByDescending(r => r.Schedule.TravelDate);
                    break;

                case "passenger":
                    reservationsQuery = reservationsQuery.OrderBy(r =>
                    (r.Passenger.Name + " " + r.Passenger.LastName));
                    break;

                case "passenger_descending":
                    reservationsQuery = reservationsQuery.OrderByDescending(r =>
                    (r.Passenger.Name + " " + r.Passenger.LastName));
                    break;

                case "departureTime":
                    reservationsQuery = reservationsQuery.OrderBy(r => r.Schedule.DepartureTime);
                    break;

                case "departureTime_descending":
                    reservationsQuery = reservationsQuery.OrderByDescending(r => r.Schedule.DepartureTime);
                    break;

                case "arrivalTime":
                    reservationsQuery = reservationsQuery.OrderBy(r => r.Schedule.ArrivalTime);
                    break;

                case "arrivalTime_descending":
                    reservationsQuery = reservationsQuery.OrderByDescending(r => r.Schedule.ArrivalTime);
                    break;

                case "status":
                    reservationsQuery = reservationsQuery.OrderBy(r => r.Status);
                    break;

                case "status_descending":
                    reservationsQuery = reservationsQuery.OrderByDescending(r => r.Status);
                    break;

                case "isActiveReservation":
                    reservationsQuery = reservationsQuery.OrderBy(r => r.IsActive);
                    break;

                case "isActiveReservation_descending":
                    reservationsQuery = reservationsQuery.OrderByDescending(r => r.IsActive);
                    break;

                default:
                    reservationsQuery = reservationsQuery.OrderByDescending(r => r.ReservationTime);
                    break;
            }

            // Странициране
            var allReservations = await reservationsQuery.CountAsync();
            var reservations = await reservationsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            var totalPages = (int)Math.Ceiling(allReservations / (double)pageSize);
            var viewModel = new ReservationIndexViewModel
            {
                Reservations = reservations,
                SearchString = searchString,
                SortOrder = sortOrder,
                Status = status,
                ReservationTimeFrom = reservationTimeFrom,
                ReservationTimeTo = reservationTimeTo,
                TravelDate = travelDate,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        // GET: Reservations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Passenger)
                .Include(r => r.Schedule)
                    .ThenInclude(r => r.Route)
                .Include(r => r.Schedule)
                    .ThenInclude(r => r.Bus)
                .Include(r => r.Seat)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // Метод, чрез който се заключва място, когато е избрано от потребител
        [HttpPost]
        public async Task<IActionResult> LockSeat(int scheduleId, int seatId)
        {
            var now = DateTime.UtcNow;

            var isSeatTaken = _context.Reservations.Any(r =>
                r.ScheduleId == scheduleId &&
                r.SeatId == seatId &&
                (
                    r.IsActive ||
                    (r.IsLocked && r.LockExpirationTime > now)
                ));

            if (isSeatTaken)
            {
                return BadRequest("Мястото вече е заето.");
            }

            var existingLock = _context.Reservations.FirstOrDefault(r =>
                r.ScheduleId == scheduleId &&
                r.SeatId == seatId &&
                r.IsLocked &&
                r.LockExpirationTime > now);

            if (existingLock != null)
            {
                return BadRequest("Мястото вече е заключено.");
            }

            var lockReservation = new Reservation
            {
                ScheduleId = scheduleId,
                SeatId = seatId,
                Status = ReservationStatus.Pending,
                IsLocked = true,
                LockExpirationTime = now.AddMinutes(5),
                IsActive = false
            };

            _context.Add(lockReservation);
            await _context.SaveChangesAsync();

            await _hub.Clients.All.SendAsync("SeatLocked", new
            {
                scheduleId,
                seatId
            });

            return Ok();
        }

        // GET: Reservations/Create
        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            var ViewModel = new ReservationCreateViewModel
            {
                Schedules = _context.BusSchedules.ToList(),
                Seats = new List<BusSeat>()
            };

            return View(ViewModel);
        }

        // POST: Reservations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create(ReservationCreateViewModel reservation)
        {
            var schedule = _context.BusSchedules
                .Include(s => s.Route)
                .Include(s => s.Bus)
                .FirstOrDefault(s => s.Id == reservation.ScheduleId);

            var selectedSeat = _context.BusSeats
                .Include(s => s.Bus)
                .Include(s => s.Number)
                .FirstOrDefault(s => s.Id == reservation.SeatId);
            
            if (ModelState.IsValid)
            {
                // Проверка дали разписанието съществува
                if (schedule == null)
                {
                    ModelState.AddModelError("", "Избраното разписание не съществува.");
                    return View(reservation);
                }

                // Проверка на мястото в автобуса дали вече не е резервирано от друг потребител
                var now = DateTime.UtcNow;

                var isSeatReserved = _context.Reservations.Any(r => 
                    r.ScheduleId == reservation.ScheduleId && 
                    r.SeatId == reservation.SeatId &&
                    (
                        // Активна места
                        (r.IsActive &&
                            (
                                r.Status == ReservationStatus.Confirmed ||
                                (r.Status == ReservationStatus.Pending && 
                                 r.ExpirationTime != null &&
                                 r.ExpirationTime > now)
                            )
                        )
                        ||
                        // Заключени места
                        (r.IsLocked && r.LockExpirationTime > now)
                    ));

                if (isSeatReserved)
                {
                    ModelState.AddModelError("", "Мястото вече е резервирано от друг потребител.");
                    return View(reservation);
                }

                var existingLock = await _context.Reservations
                    .FirstOrDefaultAsync(r =>
                        r.ScheduleId == reservation.ScheduleId &&
                        r.SeatId == reservation.SeatId &&
                        r.IsLocked &&
                        r.LockExpirationTime > now);

                if (existingLock != null)
                {
                    _context.Reservations.Remove(existingLock);
                }

                // Създаване на нова резервация
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                var newReservation = new Reservation
                {
                    ScheduleId = reservation.ScheduleId,
                    SeatId = reservation.SeatId,
                    PassengerId = userId,
                    ReservationTime = DateTime.UtcNow,
                    Status = ReservationStatus.Pending,
                    IsActive = true,
                    ExpirationTime = DateTime.UtcNow.AddMinutes(60)
                };

                try
                {
                    _context.Add(newReservation);
                    await _context.SaveChangesAsync();

                    await _hub.Clients.All.SendAsync("SeatReserved", new
                    {
                        reservation.ScheduleId,
                        reservation.SeatId
                    });
                }
                catch(DbUpdateException)
                {
                    ModelState.AddModelError("", "Мястото беше резервирано преди няколко секунди. Моля, изберете друго.");
                    return View(reservation);
                }
                
                return RedirectToAction(nameof(Index));
            }

            return View(reservation);
        }

        // GET: Reservations/Edit/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Schedule)
                    .ThenInclude(s => s.Route)
                .Include(r => r.Schedule)
                    .ThenInclude(s => s.Bus)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }

            var viewModel = new ReservationEditViewModel
            {
                Id = reservation.Id,
                PassengerId = reservation.PassengerId,
                ScheduleId = reservation.ScheduleId,
                SeatId = reservation.SeatId,
                Status = reservation.Status,
                ReservationTime = reservation.ReservationTime,
                
                BusSchedules = _context.BusSchedules
                                .Where(bs => bs.RouteId == reservation.Schedule.RouteId)
                                .ToList(),
                
                BusSeats = _context.BusSeats
                                .Where(st => st.BusId == reservation.Schedule.BusId)
                                .ToList()
            };

            return View(viewModel);
        }

        // POST: Reservations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, ReservationEditViewModel editViewModel)
        {
            if (id != editViewModel.Id)
            {
                return NotFound();
            }

            var now = DateTime.UtcNow;

            var isSeatAlreadyReserved = _context.Reservations.Any(r =>
                r.ScheduleId == editViewModel.ScheduleId &&
                r.SeatId == editViewModel.SeatId &&
                r.Id != editViewModel.Id &&
                r.IsActive &&
                (
                    r.Status == ReservationStatus.Confirmed ||
                    (r.Status == ReservationStatus.Pending &&
                     r.ExpirationTime != null &&
                     r.ExpirationTime > now)
                ));

            if (isSeatAlreadyReserved)
            {
                ModelState.AddModelError("SeatId", "Мястото вече е резервирано.");
            }    

            if (ModelState.IsValid)
            {
                try
                {
                    var reservation = await _context.Reservations.FindAsync(editViewModel.Id);
                    reservation.ScheduleId = editViewModel.ScheduleId;
                    reservation.SeatId = editViewModel.SeatId;
                    reservation.Status = editViewModel.Status;
                    reservation.ReservationTime = editViewModel.ReservationTime;

                    _context.Update(reservation);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }

                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservationExists(editViewModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                } 
            }

             var schedule = _context.BusSchedules.Find(editViewModel.ScheduleId);
                if (schedule != null)
                {
                    editViewModel.BusSchedules = _context.BusSchedules
                                                .Where(bs => bs.RouteId == schedule.RouteId)
                                                .ToList();
                    editViewModel.BusSeats = _context.BusSeats
                                                .Where(st => st.BusId == schedule.BusId)
                                                .ToList();
                }

            return View(editViewModel);
        }

        // GET: Reservations/CancelReservation/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> CancelReservation(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Passenger)
                .Include(r => r.Schedule)
                .Include(r => r.Seat)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // POST: Reservations/CancelReservationConfirmed/5
        [HttpPost, ActionName("CancelReservationConfirmed")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> CancelReservationConfirmed(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                reservation.IsActive = false;
                reservation.Status = ReservationStatus.Cancelled;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.Id == id);
        }

        // Метод за динамично зареждане на списъка с разписания след като е избран маршрут
        public JsonResult GetSchedules(int routeId)
        {
            var schedules = _context.BusSchedules
                .Where(s => s.RouteId == routeId)
                .Select(s => new { s.Id, Data = s.TravelDate.ToString("dd.MM.yyyy") + " " + s.DepartureTime }).ToList();
            
            if (!schedules.Any())
            {
                return Json(new { Success = false, Message = "Избраното разписание не съществува." });
            }

            if (routeId == 0)
            {
                return Json(new { Success = false, Message = "Този маршрут не съществува." });
            }
            
            return Json(schedules);
        }

        // Метод за динамично обновяване на списъка с местата в автобуса след избор на разписание
        public JsonResult GetSeats(int scheduleId)
        {
            var busId = _context.BusSchedules
                .Where(s => s.Id == scheduleId)
                .Select(s => s.BusId)
                .FirstOrDefault();

            if (busId == 0)
            {
                return Json(new { Success = false, Message = "Няма налични места за това разписание.", seats = new List<object>() });
            }

            var now = DateTime.UtcNow;

            var reservedSeatId = _context.Reservations
                .Where(r =>
                       r.ScheduleId == scheduleId &&
                       (
                            // Истински активни резервации 
                            (r.IsActive &&
                                (
                                    r.Status == ReservationStatus.Confirmed ||
                                    (r.Status == ReservationStatus.Pending &&
                                     r.ExpirationTime != null &&
                                     r.ExpirationTime > now)
                                )
                            )    
                            ||
                            // Заключени места
                            (r.IsLocked && r.LockExpirationTime > now)
                       )
                    )
                .Select(r => r.SeatId)
                .ToList();
            
            var seats = _context.BusSeats
                .Where(s => s.BusId == busId) 
                .Select(s => new 
                { 
                    s.Id, 
                    Number = s.Number,
                    IsAvailable = !reservedSeatId.Contains(s.Id)
                })
                .ToList();
            
            return Json(new { Success = true, seats = seats });
        }

        // Метод за потвърждаване на резервацията от администратора
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ConfirmReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);

            if (reservation == null)
            {
                return NotFound(); 
            }

            if (reservation.Status != ReservationStatus.Pending)
            {
                return BadRequest("Резервацията не може да бъде потвърдена.");
            }

            if (reservation.ExpirationTime != null && reservation.ExpirationTime < DateTime.UtcNow)
            {
                return BadRequest("Резервацията е изтекла.");
            }

            reservation.Status = ReservationStatus.Confirmed;
            reservation.ExpirationTime = null;
            reservation.IsActive = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
