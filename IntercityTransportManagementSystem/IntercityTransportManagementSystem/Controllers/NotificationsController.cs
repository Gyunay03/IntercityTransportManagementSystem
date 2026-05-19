using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using IntercityTransportManagementSystem.Services;
using System.Security.Claims;

namespace IntercityTransportManagementSystem.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // GET: Notifications
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            var notifications = await _notificationService.GetUserNotificationsAsync(userId.Value);

            return View(notifications);
        }

        // GET: Notifications/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            var notification = await _notificationService.GetNotificationByIdAsync(id, userId.Value);

            if (notification == null)
            {
                return NotFound();
            }

            await _notificationService.MarkAsReadAsync(id, userId.Value);

            return View(notification);
        }

        // Метод, чрез който се маркират като прочетени уведомленията
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            await _notificationService.MarkAsReadAsync(id, userId.Value);

            return RedirectToAction(nameof(Index));
        }

        // Метод, чрез който броячът увеличава броя на непрочетените уведомления
        public async Task<IActionResult> UnreadCount()
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Json(0);
            }

            var count = await _notificationService.GetUnreadCountAsync(userId.Value);

            return Json(count);
        }

        private int? GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdString, out int userId))
            {
                return null;
            }

            return userId;
        }
    }
}
