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

namespace IntercityTransportManagementSystem.Controllers
{
    public class BusSchedulesController : Controller
    {
        private readonly IntercityTransportManagementSystemDatabaseContext _context;

        public BusSchedulesController(IntercityTransportManagementSystemDatabaseContext context)
        {
            _context = context;
        }

        // GET: BusSchedules
        public async Task<IActionResult> Index(string searchString, string sortOrder, DateOnly? travelDate, string departureTime, int page = 1, int pageSize = 5)
        {
            var busSchedulesQuery = _context.BusSchedules
                .Include(b => b.Bus)
                .Include(b => b.Driver)
                .Include(b => b.Route)
                .AsNoTracking()
                .AsQueryable();

            // Търсене по регистрационен номер на автобус, име и/или фамилия на шофьор, начална и/или крайна дестинация (маршрут)
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                busSchedulesQuery = busSchedulesQuery.Where(b =>
                    b.Bus.RegistrationNumber.Contains(searchString) ||

                    b.Driver.Name.Contains(searchString) ||
                    b.Driver.LastName.Contains(searchString) ||
                    (b.Driver.Name + " " + b.Driver.LastName).Contains(searchString) ||
                    
                    b.Route.StartDestination.Contains(searchString) ||
                    b.Route.FinalDestination.Contains(searchString) ||
                    (b.Route.StartDestination + " - " + b.Route.FinalDestination).Contains(searchString));
            }

            // Филтриране по дата на пътуване
            if (travelDate.HasValue)
            {
                busSchedulesQuery = busSchedulesQuery.Where(b => b.TravelDate == travelDate.Value);
            }

            // Преобразуване на типа на полето departureTime
            TimeOnly? parsedDepartureTime = null;
            if (!string.IsNullOrEmpty(departureTime))
            {
                parsedDepartureTime = TimeOnly.Parse(departureTime);
            }

            // Филтриране по час на тръгване
            if (parsedDepartureTime.HasValue)
            {
                busSchedulesQuery = busSchedulesQuery.Where(b => b.DepartureTime == parsedDepartureTime.Value);
            }

            // Сортиране
            switch (sortOrder)
            {
                case "startDestination":
                    busSchedulesQuery = busSchedulesQuery.OrderBy(b => b.Route.StartDestination);
                    break;
                
                case "startDestination_descending":
                    busSchedulesQuery = busSchedulesQuery.OrderByDescending(b => b.Route.StartDestination);
                    break;
                
                case "finalDestination":
                    busSchedulesQuery = busSchedulesQuery.OrderBy(b => b.Route.FinalDestination);
                    break;
                
                case "finalDestination_descending":
                    busSchedulesQuery = busSchedulesQuery.OrderByDescending(b => b.Route.FinalDestination);
                    break;
                
                case "travelDate":
                    busSchedulesQuery = busSchedulesQuery.OrderBy(b => b.TravelDate);
                    break;
                
                case "travelDate_descending":
                    busSchedulesQuery = busSchedulesQuery.OrderByDescending(b => b.TravelDate);
                    break;
                
                case "departureTime":
                    busSchedulesQuery = busSchedulesQuery.OrderBy(b => b.DepartureTime);
                    break;
                
                case "departureTime_descending":
                    busSchedulesQuery = busSchedulesQuery.OrderByDescending(b => b.DepartureTime);
                    break;

                case "arrivalTime":
                    busSchedulesQuery = busSchedulesQuery.OrderBy(b => b.ArrivalTime);
                    break;

                case "arrivalTime_descending":
                    busSchedulesQuery = busSchedulesQuery.OrderByDescending(b => b.ArrivalTime);
                    break;

                case "driverName":
                    busSchedulesQuery = busSchedulesQuery
                        .OrderBy(b => b.Driver.Name)
                        .ThenBy(b => b.Driver.LastName);
                    break;
                
                case "driverName_descending":
                    busSchedulesQuery = busSchedulesQuery
                        .OrderByDescending(b => b.Driver.Name)
                        .ThenBy(b => b.Driver.LastName);
                    break;

                case "busRegistrationNumber":
                    busSchedulesQuery = busSchedulesQuery.OrderBy(b => b.Bus.RegistrationNumber);
                    break;

                case "busRegistrationNumber_descending":
                    busSchedulesQuery = busSchedulesQuery.OrderByDescending(b => b.Bus.RegistrationNumber);
                    break;

                default:
                    busSchedulesQuery = busSchedulesQuery.OrderBy(b => b.TravelDate);
                    break;
            }

            // Странициране
            var busSchedules = await busSchedulesQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var allBusSchedules = await busSchedulesQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(allBusSchedules / (double)pageSize);

            var viewModel = new BusScheduleIndexViewModel
            {
                BusSchedules = busSchedules,
                SearchString = searchString,
                SortOrder = sortOrder,
                TravelDate = travelDate,
                DepartureTime = parsedDepartureTime,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        // GET: BusSchedules/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var busSchedule = await _context.BusSchedules
                .Include(b => b.Bus)
                .Include(b => b.Driver)
                .Include(b => b.Route)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (busSchedule == null)
            {
                return NotFound();
            }

            return View(busSchedule);
        }

        // GET: BusSchedules/Create
        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            FillDropDowns();
            return View();
        }

        // POST: BusSchedules/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create([Bind("Id,RouteId,BusId,DriverId,TravelDate,DepartureTime,ArrivalTime")] BusSchedule busSchedule)
        {
            if (busSchedule.ArrivalTime <= busSchedule.DepartureTime)
            {
                ModelState.AddModelError("", "Часът на пристигане трябва да е след часа на тръгване.");
            }

            if (busSchedule.TravelDate < DateOnly.FromDateTime(DateTime.Now))
            {
                ModelState.AddModelError("TravelDate", "Не може да се добави разписание с дата на пътуване от минал период.");
            }

            bool existsBusSchedule = await _context.BusSchedules.AnyAsync(b =>
                b.BusId == busSchedule.BusId &&
                b.TravelDate == busSchedule.TravelDate &&
                b.DepartureTime == busSchedule.DepartureTime);
            
            // Проверка дали съществува разписание на автобус по определено време
            if (existsBusSchedule)
            {
                ModelState.AddModelError("", "Вече съществува разписание за този автобус по тово време.");
            }

            bool driverBusy = await _context.BusSchedules.AnyAsync(b =>
                b.DriverId == busSchedule.DriverId &&
                b.TravelDate == busSchedule.TravelDate &&
                b.DepartureTime == busSchedule.DepartureTime);

            // Проверка дали е зает шофьор в даден час на тръгване
            if (driverBusy)
            {
                ModelState.AddModelError("", "Шофьорът вече има курс в този час на тръгване.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(busSchedule);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            FillDropDowns(busSchedule.RouteId, busSchedule.BusId, busSchedule.DriverId);

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState)
                {
                    Console.WriteLine(error.Key);
                    foreach (var e in error.Value.Errors)
                    {
                        Console.WriteLine(e.ErrorMessage);
                    }
                }
            }

            return View(busSchedule);
        }

        // GET: BusSchedules/Edit/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var busSchedule = await _context.BusSchedules.FindAsync(id);
            if (busSchedule == null)
            {
                return NotFound();
            }

            FillDropDowns(busSchedule.RouteId, busSchedule.BusId, busSchedule.DriverId);
            
            return View(busSchedule);
        }

        // POST: BusSchedules/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RouteId,BusId,DriverId,TravelDate,DepartureTime,ArrivalTime")] BusSchedule busSchedule)
        {
            if (id != busSchedule.Id)
            {
                return NotFound();
            }

            if (busSchedule.ArrivalTime <= busSchedule.DepartureTime)
            {
                ModelState.AddModelError("", "Часът на пристигане трябва да е след часа на тръгване.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingBusSchedule = await _context.BusSchedules.FindAsync(id);
                    if (existingBusSchedule == null)
                    {
                        return NotFound();
                    }

                    existingBusSchedule.RouteId = busSchedule.RouteId;
                    existingBusSchedule.BusId = busSchedule.BusId;
                    existingBusSchedule.DriverId = busSchedule.DriverId;
                    existingBusSchedule.TravelDate = busSchedule.TravelDate;
                    existingBusSchedule.DepartureTime = busSchedule.DepartureTime;
                    existingBusSchedule.ArrivalTime = busSchedule.ArrivalTime;
                    
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BusScheduleExists(busSchedule.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            FillDropDowns(busSchedule.RouteId, busSchedule.BusId, busSchedule.DriverId);
            
            return View(busSchedule);
        }

        // GET: BusSchedules/Delete/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var busSchedule = await _context.BusSchedules
                .Include(b => b.Bus)
                .Include(b => b.Driver)
                .Include(b => b.Route)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (busSchedule == null)
            {
                return NotFound();
            }

            return View(busSchedule);
        }

        // POST: BusSchedules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var busSchedule = await _context.BusSchedules.FindAsync(id);
            if (busSchedule != null)
            {
                _context.BusSchedules.Remove(busSchedule);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BusScheduleExists(int id)
        {
            return _context.BusSchedules.Any(e => e.Id == id);
        }

        private void FillDropDowns(int? selectedRouteId = null, int? selectedBusId = null, int? selectedDriverId = null)
        {
            ViewData["RouteId"] = new SelectList(_context.Routes.AsNoTracking()
                .Select(r => new { r.Id, RouteName = r.StartDestination + " - " + r.FinalDestination }),
                "Id", "RouteName", selectedRouteId);
            ViewData["BusId"] = new SelectList(_context.Buses.AsNoTracking(), "Id", "RegistrationNumber", selectedBusId);
            ViewData["DriverId"] = new SelectList(_context.Drivers.AsNoTracking()
                .Select(d => new { d.Id, DriverName = d.Name + " " + d.LastName }),
                "Id", "DriverName", selectedDriverId);
        }
    }
}
