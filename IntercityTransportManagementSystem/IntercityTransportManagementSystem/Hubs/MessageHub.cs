using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace IntercityTransportManagementSystem.Hubs
{
    [Authorize]
    public class MessageHub : Hub
    {
        // Метод за присъединяване към групов чат за конктретно разпидание (курс)
        public async Task JoinScheduleGroup(int scheduleId)
        {
            string groupName = $"Schedule_{scheduleId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        // Метод за напускане на групата
        public async Task LeaveScheduleGroup(int scheduleId)
        {
            string groupName = $"Schedule_{scheduleId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
    }
}
