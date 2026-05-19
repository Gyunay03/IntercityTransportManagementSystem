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
using IntercityTransportManagementSystem.Services;

namespace IntercityTransportManagementSystem.Controllers
{
    public class ReservationsController : Controller
    {
        private readonly IntercityTransportManagementSystemDatabaseContext _context;
        private readonly IHubContext<ReservationHub> _hub;
        private readonly IReservationService _reservationService;
        private readonly INotificationService _notificationService;

        public ReservationsController(IntercityTransportManagementSystemDatabaseContext context, IHubContext<ReservationHub> hub, IReservationService reservationService, INotificationService notificationService)
        {
            _context = context;
            _hub = hub;
            _reservationService = reservationService;
            _notificationService = notificationService;
        }

        // GET: Reservations
        [HttpGet]
        public async Task<IActionResult> Index(ReservationIndexViewModel model, int page = 1, int pageSize = 20)
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
            if (!string.IsNullOrWhiteSpace(model.SearchString))
            {
                reservationsQuery = reservationsQuery.Where(r =>
                    r.Passenger.Name.Contains(model.SearchString) ||
                    r.Passenger.LastName.Contains(model.SearchString) ||
                    (r.Passenger.Name + " " + r.Passenger.LastName).Contains(model.SearchString) ||

                    r.Schedule.Route.StartDestination.Contains(model.SearchString) ||
                    r.Schedule.Route.FinalDestination.Contains(model.SearchString) ||
                    (r.Schedule.Route.StartDestination + " - " + r.Schedule.Route.FinalDestination).Contains(model.SearchString));
            }

            // Филтриране от дата на резервация
            if (model.ReservationTimeFrom.HasValue)
            {
                reservationsQuery = reservationsQuery.Where(r => r.ReservationTime >= model.ReservationTimeFrom.Value);
            }

            // Филтриране до дата на резервация
            if (model.ReservationTimeTo.HasValue)
            {
                var toDate = model.ReservationTimeTo.Value.Date.AddDays(1);
                reservationsQuery = reservationsQuery.Where(r => r.ReservationTime < toDate);
            }

            // Филтриране по дата на пътуване
            if (model.TravelDate.HasValue)
            {
                reservationsQuery = reservationsQuery.Where(r => r.Schedule.TravelDate == model.TravelDate.Value);
            }

            // Филтриране по статус
            if (model.Status.HasValue)
            {
                reservationsQuery = reservationsQuery.Where(r => r.Status == model.Status.Value);
            }

            // Сортиране
            switch (model.SortOrder)
            {
                case "reservationId":
                    reservationsQuery = reservationsQuery.OrderBy(r => r.Id);
                    break;

                case "reservationId_descending":
                    reservationsQuery = reservationsQuery.OrderByDescending(r => r.Id);
                    break;

                case "passenger":
                    reservationsQuery = reservationsQuery.OrderBy(r =>
                    (r.Passenger.Name + " " + r.Passenger.LastName));
                    break;

                case "passenger_descending":
                    reservationsQuery = reservationsQuery.OrderByDescending(r =>
                    (r.Passenger.Name + " " + r.Passenger.LastName));
                    break;

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

                case "seat":
                    reservationsQuery = reservationsQuery.OrderBy(r => r.Seat.Number);
                    break;

                case "seat_descending":
                    reservationsQuery = reservationsQuery.OrderByDescending(r => r.Seat.Number);
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

                case "isLocked":
                    reservationsQuery = reservationsQuery.OrderBy(r => r.IsLocked);
                    break;

                case "isLocked_descending":
                    reservationsQuery = reservationsQuery.OrderByDescending(r => r.IsLocked);
                    break;

                case "expirationTime":
                    reservationsQuery = reservationsQuery.OrderBy(r => r.ExpirationTime);
                    break;

                case "expirationTime_descending":
                    reservationsQuery = reservationsQuery.OrderByDescending(r => r.ExpirationTime);
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
                SearchString = model.SearchString,
                SortOrder = model.SortOrder,
                Status = model.Status,
                ReservationTimeFrom = model.ReservationTimeFrom,
                ReservationTimeTo = model.ReservationTimeTo,
                TravelDate = model.TravelDate,
                CurrentPage = page,
                TotalPages = totalPages
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ReservationsTable", viewModel);
            }

            return View(viewModel);
        }

        // GET: Reservations/Details/5
        [HttpGet]
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
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> LockSeat(int scheduleId, int seatId, int? passengerId)
        {
            var connectionId = Request.Headers["X-SignalR-ConnectionId"].ToString();

            var now = DateTime.Now;

            if (passengerId == null || passengerId <= 0)
            {
                return Json(new { success = false, message = "Невалиден или липсващ пътник." });
            }

            var passengerExists = await _context.Passengers
                .AnyAsync(p => p.Id == passengerId);
            
            if (!passengerExists)
            {
                return Json(new { success = false, message = "Избраният пътник не съществува в системата." });
            }

            int? currentUserId = null;
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdClaim))
            {
                currentUserId = int.Parse(userIdClaim);
            }

            var isAlreadyLocked = await _context.BusSeatLocks
                .AnyAsync(l => l.ScheduleId == scheduleId && l.SeatId == seatId && l.ExpiryTime > now);

            var isReserved = await _context.Reservations
                .AnyAsync(r => r.ScheduleId == scheduleId && r.SeatId == seatId && r.Status != ReservationStatus.Cancelled);

            if (isAlreadyLocked || isReserved)
            {
                return Json(new { success = false, message = "Мястото вече е заето или временно заключено." });
            }

            var existingLock = await _context.BusSeatLocks.FirstOrDefaultAsync(l =>
                l.ScheduleId == scheduleId &&
                l.SeatId == seatId &&
                (l.PassengerId == passengerId || l.UserId == currentUserId));

            if (existingLock != null && existingLock.PassengerId != passengerId)
            {
                return Json(new { success = false, message = "Това място вече е избрано от друг потребител." });
            }

            var lockSeat = new BusSeatLock
            {
                ScheduleId = scheduleId,
                SeatId = seatId,
                UserId = currentUserId,
                PassengerId = passengerId,
                ExpiryTime = now.AddMinutes(5),
                CreatedAt = now,
            };

            _context.BusSeatLocks.Add(lockSeat);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(connectionId))
            {
                await _hub.Clients.AllExcept(connectionId).SendAsync("SeatLocked", new 
                {
                    seatId, scheduleId, lockedBy = passengerId
                });
            }
            else
            {
                await _hub.Clients.All.SendAsync("SeatLocked", new
                {
                    seatId, scheduleId
                });
            }

            return Json(new { success = true });
        }

        // Метод, чрез който се отключва заключено място, когато е избрано друго
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> UnlockSeat(int scheduleId, int seatId, int? passengerId)
        {
            var connectionId = Request.Headers["X-SignalR-ConnectionId"].ToString();

            var now = DateTime.Now;

            int? currentUserId = null;
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdClaim))
            {
                currentUserId = int.Parse(userIdClaim);
            }

            var existingLock = await _context.BusSeatLocks.FirstOrDefaultAsync(l =>
                l.ScheduleId == scheduleId &&
                l.SeatId == seatId &&
                (l.PassengerId == passengerId || l.UserId == currentUserId));

            if (existingLock != null)
            {
                _context.BusSeatLocks.Remove(existingLock);
                await _context.SaveChangesAsync();

                await _hub.Clients.All.SendAsync("SeatUnlocked", new { scheduleId, seatId });

                return Json(new { success = true, message = "Мястото е отключено." });
            }

            return Json(new { success = false, message = "Не е намерено заключване за това място." });
        }

        // GET: Reservations/Create
        [HttpGet]
        public async Task <IActionResult> Create()
        {
            var ViewModel = new ReservationCreateViewModel
            {
                Schedules = _context.BusSchedules.ToList(),
                Seats = new List<BusSeat>()
            };

            if (User.IsInRole("Passenger"))
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userIdClaim != null)
                {
                    var userId = int.Parse(userIdClaim);
                    var passenger = _context.Passengers
                        .FirstOrDefault(p => p.UserId == userId);

                    if (passenger != null)
                    {
                        ViewModel.PassengerId = passenger.Id;
                    }
                }
            }

            else
            {
                var passengers = _context.Passengers
                    .Select(p => new { p.Id, FullName = p.Name + " " + p.LastName })
                    .ToList();
                ViewBag.PassengerId = new SelectList(passengers, "Id", "FullName");
            }   

            var routes = _context.Routes
                .Select(r => new { r.Id, RouteName = r.StartDestination + " - " + r.FinalDestination })
                .ToList();

            var busSchedules = _context.BusSchedules
                .Select(bs => new { bs.Id, Schedule = "Дата на пътуване: " + bs.TravelDate + " , " + "Час на тръгване " + bs.DepartureTime.ToString("HH:mm") + " , " + "Автобус: " + bs.Bus.RegistrationNumber })
                .ToList();

            var busSeats = _context.BusSeats
                .Select(st => new { st.Id, Seat = "Място :" + st.Number });

            await FillDropdowns(ViewModel.PassengerId, ViewModel.RouteId, ViewModel.ScheduleId, ViewModel.SeatId);

            return View(ViewModel);
        }

        // POST: Reservations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReservationCreateViewModel reservation)
        {
            if (User.IsInRole("Passenger"))
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var passenger = _context.Passengers
                    .FirstOrDefault(p => p.UserId == userId);

                if (passenger != null)
                {
                    reservation.PassengerId = passenger.Id;
                    ModelState.Remove("PassengerId");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Не е намерен профил на пътник за вашия акаунт. Моля, свържете се с администратор.");
                }
            }
            else
            {
                if (reservation.PassengerId == null || reservation.PassengerId == 0)
                {
                    ModelState.AddModelError("PassengerId", "Моля, изберете пътник.");
                }
            }

            if (reservation.RouteId == 0)
            {
                ModelState.AddModelError("RouteId", "Моля, изберете маршрут.");
            }

            if (reservation.ScheduleId == 0)
            {
                ModelState.AddModelError("ScheduleId", "Моля, изберете разписание.");   
            }

            if (!ModelState.IsValid)
            {
                await FillDropdowns(reservation.PassengerId, reservation.RouteId, reservation.ScheduleId);
                return View(reservation);
            }

            return RedirectToAction(nameof(SeatMap), 
                new 
                { scheduleId = reservation.ScheduleId, 
                  passengerId = reservation.PassengerId,
                  ticketType = reservation.TicketType
                });
        }

        // GET: Reservations/Edit/5
        [Authorize(Roles = "Administrator, Driver")]
        [HttpGet]
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

            await FillDropdowns(reservation.PassengerId, reservation.Schedule.RouteId , reservation.ScheduleId, reservation.SeatId, reservation.Id);

            return View(viewModel);
        }

        // POST: Reservations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Driver")]
        public async Task<IActionResult> Edit(int id, ReservationEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            var now = DateTime.Now;

            var isSeatReservedByOther = await _context.Reservations.AnyAsync(r =>
                r.ScheduleId == model.ScheduleId &&
                r.SeatId == model.SeatId &&
                r.Id != model.Id &&
                (r.IsActive &&
                (
                     (r.Status == ReservationStatus.Confirmed ||
                                   (r.Status == ReservationStatus.Pending && r.ExpirationTime > now)))
                    ||
                    (r.IsLocked && r.LockExpirationTime > now)
                ));

            var isSeatLockedByOther = await _context.BusSeatLocks.AnyAsync(l =>
                l.ScheduleId == model.ScheduleId &&
                l.SeatId == model.SeatId &&
                l.ExpiryTime > now &&
                l.PassengerId != model.PassengerId);

            if (isSeatReservedByOther || isSeatLockedByOther)
            {
                ModelState.AddModelError("SeatId", "Избраното място вече е заето или временно заключено.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var reservation = await _context.Reservations
                        .Include(r => r.Schedule)
                        .FirstOrDefaultAsync(m => m.Id == id);

                    var oldSeatId = reservation.SeatId;

                    if (reservation == null)
                    {
                        return NotFound();
                    }

                    reservation.PassengerId = model.PassengerId;
                    reservation.ScheduleId = model.ScheduleId;
                    reservation.SeatId = model.SeatId;
                    reservation.Status = model.Status;
                    reservation.ReservationTime = model.ReservationTime;

                    _context.Update(reservation);
                    await _context.SaveChangesAsync();

                    var existingLock = await _context.BusSeatLocks
                        .FirstOrDefaultAsync(l => l.ScheduleId == model.ScheduleId && l.SeatId == model.SeatId);
                    
                    if (existingLock != null)
                    {
                        _context.BusSeatLocks.Remove(existingLock);
                        await _context.SaveChangesAsync();
                    }

                    if (oldSeatId != model.SeatId)
                    {
                        await _hub.Clients.All.SendAsync("SeatUnlocked", new { scheduleId = model.ScheduleId, seatId = model.SeatId });
                    }

                    await _hub.Clients.All.SendAsync("SeatReserved", new { seatId = model.SeatId });

                    return RedirectToAction(nameof(Index));
                }

                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservationExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                } 
            }

             var schedule = _context.BusSchedules.Find(model.ScheduleId);
                if (schedule != null)
                {
                    model.BusSchedules = _context.BusSchedules
                                                .Where(bs => bs.RouteId == schedule.RouteId)
                                                .ToList();
                    model.BusSeats = _context.BusSeats
                                                .Where(st => st.BusId == schedule.BusId)
                                                .ToList();
                }
            
            var routes = _context.Routes
                .Select(r => new { r.Id, RouteName = r.StartDestination + " - " + r.FinalDestination })
                .ToList();

            var busSchedules = _context.BusSchedules
                .Select(bs => new { bs.Id, Schedule = "Дата на пътуване: " + bs.TravelDate + " , " + "Час на тръгване " + bs.DepartureTime.ToString("HH:mm") + " , " + "Автобус: " + bs.Bus.RegistrationNumber })
                .ToList();

            var busSeats = _context.BusSeats
                .Select(st => new { st.Id, Seat = "Място :" + st.Number });

            if (schedule.TravelDate < DateOnly.FromDateTime(now))
            {
                ModelState.AddModelError("", "Не може да редактирате резервация за изминало (старо) пътуване.");
            }

            await FillDropdowns(model.PassengerId, null, model.ScheduleId, model.SeatId, model.Id);

            return View(model);
        }

        // GET: Reservations/CancelReservation/5
        [Authorize(Roles = "Administrator, Driver")]
        [HttpGet]
        public async Task<IActionResult> CancelReservation(int? id)
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

        // POST: Reservations/CancelReservationConfirmed/5
        [HttpPost, ActionName("CancelReservationConfirmed")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Driver")]
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
            if (routeId == 0)
            {
                return Json(new List<object>());
            }

            var schedules = _context.BusSchedules
               .Where(s => s.RouteId == routeId)
               .Select(s => new { s.Id, Data = s.TravelDate.ToString("dd.MM.yyyy") + " " + s.DepartureTime }).ToList();

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

            var now = DateTime.Now;

            var reservedSeatIds = _context.Reservations
                .Where(r => r.ScheduleId == scheduleId && r.IsActive &&
                            (r.Status == ReservationStatus.Confirmed ||
                            (r.Status == ReservationStatus.Pending && r.ExpirationTime > now)))
                .Select(r => r.SeatId)
                .ToList();

            var lockedSeatIds = _context.BusSeatLocks
                .Where(l => l.ScheduleId == scheduleId && l.ExpiryTime > now)
                .Select(l => l.SeatId)
                .ToList();

            var unavailableSeatIds = reservedSeatIds.Union(lockedSeatIds).ToList();

            var seats = _context.BusSeats
                .Where(s => s.BusId == busId) 
                .Select(s => new 
                { 
                    s.Id, 
                    Number = s.Number,
                    IsAvailable = !unavailableSeatIds.Contains(s.Id)
                })
                .ToList();
            
            return Json(new { Success = true, seats = seats });
        }

        // Метод за потвърждаване на резервацията от администратора и шофьора
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Driver")]
        public async Task<IActionResult> ConfirmReservation(int id, int scheduleId, int seatId)
        {
            var isSeatAlreadyTaken = await _context.Reservations
                .AnyAsync(r => r.ScheduleId == scheduleId && 
                          r.SeatId == seatId && 
                          r.Id != id &&
                          r.Status == ReservationStatus.Confirmed);

            if (isSeatAlreadyTaken)
            {
                TempData["Error"] = "Мястото вече е заето. Моля, изберете друго.";
                return RedirectToAction("SeatMap", new { scheduleId}); 
            }

            var reservation = await _context.Reservations.FindAsync(id);

            if (reservation == null)
            {
                return NotFound("Резервацията не е намерена.");
            }

            if (reservation.Status != ReservationStatus.Pending)
            {
                return BadRequest("Резервацията не може да бъде потвърдена, защото не е в изчакване.");
            }

            if (reservation.ExpirationTime != null && reservation.ExpirationTime < DateTime.Now)
            {
                return BadRequest("Резервацията е изтекла и не може да бъде потвърдена.");
            }

            reservation.Status = ReservationStatus.Confirmed;
            reservation.ExpirationTime = null;
            reservation.IsActive = true;

            reservation.SeatId = seatId;

            try
            {
                await _context.SaveChangesAsync();

                await _hub.Clients.All.SendAsync("SeatReserved", new { scheduleId, seatId });

                TempData["Success"] = "Резервацията е потвърдена успешно!";
            }
            catch 
            {
                ModelState.AddModelError("", "Възникна грешка при записване в базата данни.");
            }

            return RedirectToAction(nameof(Index));
        }

        // Метод за визуализиране на картата с места и тяхното състоянието (свободни, заети, заключени)
        [HttpGet]
        [Authorize]
        public IActionResult SeatMap(int scheduleId, int? passengerId, TicketType ticketType, int? outboundReservationId = null)
        {
            Passenger passenger = null;

            var now = DateTime.Now;

            if (passengerId.HasValue && passengerId.Value > 0 && (User.IsInRole("Administrator") || User.IsInRole("Driver")))
            {
                passenger = _context.Passengers.FirstOrDefault(p => p.Id == passengerId.Value);
            }
            else if (User.IsInRole("Passenger"))
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                passenger = _context.Passengers
                        .FirstOrDefault(p => p.UserId == userId);
            }

            if (passenger == null)
            {
                TempData["Error"] = "Невалиден или липсващ пътник.";
                return RedirectToAction("Create");
            }

            var schedule = _context.BusSchedules
                .Include(s => s.Bus)
                .Include(s => s.Route)
                .FirstOrDefault(s => s.Id == scheduleId);

            if (schedule == null)
            {
                return NotFound();
            }

            var reservations = _context.Reservations
                .Where(r => r.ScheduleId == scheduleId && r.IsActive)
                .ToList();

            var busSeats = _context.BusSeats
                .Where(st => st.BusId == schedule.BusId)
                .ToList();

            var locks = _context.BusSeatLocks
                .Where(l => l.ScheduleId == scheduleId && l.ExpiryTime > now)
                .ToList();

            var seats = busSeats
                .Select(s => {
                    var res = reservations.FirstOrDefault(r => r.SeatId == s.Id && r.Status == ReservationStatus.Confirmed);
                    var pendingRes = reservations.FirstOrDefault(r => r.SeatId == s.Id && r.Status == ReservationStatus.Pending && r.ExpirationTime > now);

                    var seatLock = locks.FirstOrDefault(l => l.SeatId == s.Id);

                    bool isTaken = res != null || pendingRes != null;
                    bool isSelected = seatLock != null && seatLock.PassengerId == passengerId;
                    bool isLocked = seatLock != null && seatLock.PassengerId != passengerId;

                    return new SeatDto
                    {
                        SeatId = s.Id,
                        Number = s.Number,
                        IsTaken = isTaken,
                        IsSelected = isSelected,
                        IsLocked = isLocked
                    };
                }).ToList();

            var viewModel = new SeatMapViewModel
            {
                ScheduleId = scheduleId,
                PassengerId = passenger.Id,
                BusRegistrationNumber = schedule.Bus.RegistrationNumber,
                RouteName = $"{schedule.Route.StartDestination} - {schedule.Route.FinalDestination}",
                TravelDate = schedule.TravelDate,
                Seats = seats,
                TicketType = ticketType,
                OutboundReservationId = outboundReservationId
            };

            return View(viewModel);
        }

        // Метод, чрез който потребителят, шофьорът или администраторът може да избере място и да създаде резервация
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ConfirmSeat(int scheduleId, int seatId, int passengerId, TicketType ticketType, int? outboundReservationId = null)
        {
            if (seatId <= 0)
            {
                TempData["Error"] = "Моля, изберете валидно място.";
                return RedirectToAction("SeatMap", new { scheduleId, passengerId });
            }

            var result = await _reservationService.ConfirmSeatAsync(scheduleId, seatId, passengerId, ticketType, outboundReservationId);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("SeatMap", new { scheduleId, passengerId });
            }

            TempData["Success"] = "Мястото е успешно потвърдено!";

            if (ticketType == TicketType.Dvuposochen && outboundReservationId == null)
            {
                return RedirectToAction("SelectReturnTrip", new { outboundReservationId = result.ReservationId, passengerId });
            }

            return RedirectToAction("Checkout", "Payments", new { reservationId = result.ReservationId });
        }

        // Метод за попълване на падащите менюта
        private async Task FillDropdowns(int? selectedPassengerId = null, int? selectedRouteId = null, int? selectedScheduleId = null, int? selectedSeatId = null, int? currentReservationId = null)
        {
            var now = DateTime.Now;

            var passengers = await _context.Passengers.AsNoTracking()
                .Select(p => new { p.Id, FullName = p.Name + " " + p.LastName })
                .ToListAsync();

            var routes = await _context.Routes.AsNoTracking()
                .Select(r => new { r.Id, Routes = r.StartDestination + " - " + r.FinalDestination })
                .ToListAsync();

            var busSchedules = await _context.BusSchedules.AsNoTracking()
                .Select(bs => new { bs.Id, Schedule = "Дата: " + bs.TravelDate.ToString("dd.MM.yyyy") + ", Час: " + bs.DepartureTime.ToString("HH:mm") + ", Автобус: " + bs.Bus.RegistrationNumber })
                .ToListAsync();

            var busSeatsQuery = _context.BusSeats.AsNoTracking();

            if (selectedScheduleId.HasValue && selectedScheduleId.Value > 0)
            {
                var busId = await _context.BusSchedules
                    .Where(s => s.Id == selectedScheduleId)
                    .Select(s => s.BusId)
                    .FirstOrDefaultAsync();

                var occupiedSeatIds = _context.Reservations
                    .Where(r => r.ScheduleId == selectedScheduleId &&
                                r.Id != currentReservationId &&
                                r.IsActive &&
                                (r.Status == ReservationStatus.Confirmed ||
                                (r.Status == ReservationStatus.Pending && r.ExpirationTime > now)))
                    .Select(r => r.SeatId);

                var lockedSeatIds = _context.BusSeatLocks
                    .Where(l => l.ScheduleId == selectedScheduleId && l.ExpiryTime > now)
                    .Select(l => l.SeatId);

                var unavailableIds = occupiedSeatIds.Union(lockedSeatIds);

                busSeatsQuery = busSeatsQuery.Where(s => s.BusId == busId && !unavailableIds.Contains(s.Id));
            }

            var busSeats = await busSeatsQuery
                .Select(st => new { st.Id, Seat = "Място: " + st.Number })
                .ToListAsync();

            ViewData["PassengerId"] = new SelectList(passengers, "Id", "FullName", selectedPassengerId);
            ViewData["RouteId"] = new SelectList(routes, "Id", "Routes", selectedRouteId);
            ViewData["ScheduleId"] = new SelectList(busSchedules, "Id", "Schedule", selectedScheduleId);
            ViewData["SeatId"] = new SelectList(busSeats, "Id", "Seat", selectedSeatId);
        }

        // Метод за избор на дата на връщане
        [HttpGet]
        [Authorize]
        public IActionResult SelectReturnTrip(int outboundReservationId, int passengerId, DateTime? returnDate = null)
        {
            var outbound = _context.Reservations
                .Include(r => r.Schedule)
                    .ThenInclude(s => s.Route)
                .FirstOrDefault(r => r.Id == outboundReservationId);

            if (outbound == null)
            {
                return NotFound();
            }

            // Търсене на обратни маршрути
            var searchSchedules = _context.BusSchedules
                .Include(s => s.Route)
                .Include(s => s.Bus)
                .Where(s =>
                    s.Route.StartDestination == outbound.Schedule.Route.FinalDestination &&
                    s.Route.FinalDestination == outbound.Schedule.Route.StartDestination &&
                    s.TravelDate >= outbound.Schedule.TravelDate);

            if (returnDate.HasValue)
            {
                var dateOnly = DateOnly.FromDateTime(returnDate.Value);
                searchSchedules = searchSchedules.Where(s => s.TravelDate == dateOnly);
            }

            var returnSchedules = searchSchedules
                .OrderBy(s => s.TravelDate)
                .ThenBy(s => s.DepartureTime)
                .ToList();

            ViewBag.OutboundReservationId = outboundReservationId;
            ViewBag.PassengerId = passengerId;
            ViewBag.OutboundDate = outbound.Schedule.TravelDate;
            ViewBag.SelectedReturnDate = returnDate?.ToString("yyyy-MM-dd");

            return View(returnSchedules);
        }

        [HttpPost]
        [Authorize]
        public IActionResult SelectReturnTrip(int selectedReturnScheduleId, int outboundReservationId, int passengerId)
        {
            return RedirectToAction("SeatMap", new
            {
                scheduleId = selectedReturnScheduleId,
                passengerId = passengerId,
                ticketType = TicketType.Dvuposochen,
                outboundReservationId = outboundReservationId
            });
        }
    }
}
