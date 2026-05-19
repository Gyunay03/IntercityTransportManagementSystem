using IntercityTransportManagementSystem.Enums;
using IntercityTransportManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace IntercityTransportManagementSystem.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IntercityTransportManagementSystemDatabaseContext _context;

        public NotificationService(IntercityTransportManagementSystemDatabaseContext context)
        {
            _context = context;
        }

        public async Task CreateNotificationAsync(int userId, string title, string message, NotificationType type)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
            {
                return;
            }

            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }

        public async Task<Notification?> GetNotificationByIdAsync(int id, int userId)
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
        }
    }
}
