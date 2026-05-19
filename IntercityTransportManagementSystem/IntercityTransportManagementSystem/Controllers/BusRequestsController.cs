using IntercityTransportManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IntercityTransportManagementSystem.Enums;
using Microsoft.AspNetCore.Authorization;
using IntercityTransportManagementSystem.Services;

namespace IntercityTransportManagementSystem.Controllers
{
    public class BusRequestsController : Controller
    {
        private readonly IntercityTransportManagementSystemDatabaseContext _context;
        private readonly INotificationService _notificationService;
        public BusRequestsController(IntercityTransportManagementSystemDatabaseContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            var requests = await _context.BusRequests
                .Include(br => br.Schedule)
                    .ThenInclude(s => s.Route)
                .ToListAsync();

            var buses = await _context.Buses.ToListAsync();
            ViewBag.Buses = buses;

            return View(requests);
        }

        // Одобряване на заявка за нов курс при препълване на автобус
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Approve(int id, int newBusId)
        {
            var request = await _context.BusRequests
                .Include(br => br.Schedule)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                return NotFound();
            }

            if (request.Status != BusRequestStatus.Pending)
            {
                return BadRequest("Already processed.");
            }

            var newSchedule = new BusSchedule
            {
                RouteId = request.Schedule.RouteId,
                DepartureTime = request.Schedule.DepartureTime,
                ArrivalTime = request.Schedule.ArrivalTime,
                BusId = newBusId
            };

            _context.BusSchedules.Add(newSchedule);

            request.Status = BusRequestStatus.Approved;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Отказване на заявка за нов курс при препълване на автобус
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Reject(int id)
        {
            var request = await _context.BusRequests.FindAsync(id);

            if (request == null)
            {
                return NotFound();
            }

            request.Status = BusRequestStatus.Rejected;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
