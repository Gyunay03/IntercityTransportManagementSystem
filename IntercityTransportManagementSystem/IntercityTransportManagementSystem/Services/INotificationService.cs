using IntercityTransportManagementSystem.Enums;
using IntercityTransportManagementSystem.Models;

namespace IntercityTransportManagementSystem.Services
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(int userId, string title, string message, NotificationType type);

        Task<List<Notification>> GetUserNotificationsAsync(int userId);

        Task<int> GetUnreadCountAsync(int userId);

        Task MarkAsReadAsync(int notificationId, int userId);

        Task<Notification?> GetNotificationByIdAsync(int id, int userId);
    }
}
