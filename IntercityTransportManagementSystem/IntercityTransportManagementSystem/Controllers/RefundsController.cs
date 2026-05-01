using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IntercityTransportManagementSystem.Models;
using IntercityTransportManagementSystem.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
namespace IntercityTransportManagementSystem.Controllers
{
    public class RefundsController : Controller
    {
        private readonly IntercityTransportManagementSystemDatabaseContext _context;

        public RefundsController(IntercityTransportManagementSystemDatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Index()
        {
            var refunds = await _context.Refunds
                .Include(r => r.Payment)
                    .ThenInclude(p => p.Reservation)
                        .ThenInclude(res => res.Passenger)
                .Include(r => r.Payment)
                    .ThenInclude(p => p.Reservation)
                        .ThenInclude(res => res.Schedule)
                            .ThenInclude(s => s.Route)
                .OrderByDescending(r => r.RequestDate)
                .ToArrayAsync();

            return View(refunds);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Details(int id)
        {
            var refund = await _context.Refunds
                .Include(r => r.Payment)
                    .ThenInclude(p => p.Reservation)
                        .ThenInclude(res => res.Passenger)
                .Include(r => r.Payment)
                    .ThenInclude(p => p.Reservation)
                        .ThenInclude(res => res.Schedule)
                            .ThenInclude(s => s.Route)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (refund == null)
            {
                return NotFound();
            }

            return View(refund);
        }

        // Метод за потвърждаване на връщането
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Approve(int id)
        {
            var refund = await _context.Refunds
                .Include(r => r.Payment)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (refund == null)
            {
                return NotFound();
            }

            refund.Status = RefundStatus.Completed;
            refund.ProcessedDate = DateTime.Now;
            refund.AdminNotes = "Сумата е възстановена успешно по сметката на клиента.";
            refund.Payment.PaymentStatus = PaymentStatus.Refunded;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Плащането е маркирано като възстановено.";
            
            return RedirectToAction(nameof(Index));
        }

        // Метод за отказ на възстановяване на сумата на билета при анулиране
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            var refund = await _context.Refunds.FindAsync(id);
            if (refund == null)
            {
                return NotFound();
            }

            refund.Status = RefundStatus.Rejected;
            refund.ProcessedDate = DateTime.Now;
            refund.AdminNotes = "Отказ: " + reason;

            await _context.SaveChangesAsync();
            TempData["Info"] = "Заявката за възстановяване беше отказана.";

            return RedirectToAction(nameof(Index));
        }

        // История на връщанията
        [Authorize]
        public async Task<IActionResult> MyRefunds()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Challenge();
            }

            int currentUserId = int.Parse(userIdClaim);

            var myRefunds = await _context.Refunds
                .Include(r => r.Payment)
                    .ThenInclude(p => p.Reservation)
                        .ThenInclude(res => res.Schedule)
                            .ThenInclude(s => s.Route)
                .Where(r => r.Payment.Reservation.Passenger.UserId == currentUserId)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return View(myRefunds);
        }
    }
}
