using Microsoft.AspNetCore.SignalR;

namespace IntercityTransportManagementSystem.Hubs
{
    public class ReservationHub : Hub
    {
        public async Task LockSeat(int seatId)
        {
            await Clients.All.SendAsync("SeatLocked", seatId);
        }

        public async Task SeatUnlocked(int scheduleId, int seatId)
        {
            await Clients.All.SendAsync("SeatUnlocked", new { scheduleId, seatId });
        }
    }
}
