using IntercityTransportManagementSystem.Models;
using IntercityTransportManagementSystem.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using IntercityTransportManagementSystem.Hubs;

namespace IntercityTransportManagementSystem.BackgroundJobs
{
    public class BusRequestJob
    {
        private readonly IntercityTransportManagementSystemDatabaseContext _context;
        private readonly IHubContext<ReservationHub> _hub;
        public BusRequestJob(IntercityTransportManagementSystemDatabaseContext context, IHubContext<ReservationHub> hub)
        {
            _context = context;
            _hub = hub;
        }

        public async Task CheckCapacity()
        {
            var schedules = await _context.BusSchedules
                .Include(s => s.Bus)
                .Include(s => s.Reservations)
                .Where(s => s.Reservations.Count(r => r.Status == ReservationStatus.Confirmed) >= s.Bus.Capacity)
                .ToListAsync();

            var newRequests = new List<int>();

            foreach (var schedule in schedules)
            {
                var existingRequest = await _context.BusRequests
                    .FirstOrDefaultAsync(r =>
                        r.ScheduleId == schedule.Id &&
                        r.Status == BusRequestStatus.Pending);

                if (existingRequest != null)
                {
                    continue;
                }

                var request = new BusRequest
                {
                    ScheduleId = schedule.Id,
                    Status = BusRequestStatus.Pending,
                    RequestDate = DateTime.Now
                };

                _context.BusRequests.Add(request);
                newRequests.Add(schedule.Id);
            }

            await _context.SaveChangesAsync();

            foreach (var scheduleId in newRequests)
            {
                await _hub.Clients.All.SendAsync("NewBusRequest", new
                {
                    scheduleId
                });
            }
        }
    }
}
