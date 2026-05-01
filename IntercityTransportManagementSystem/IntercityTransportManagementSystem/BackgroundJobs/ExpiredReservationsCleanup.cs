using IntercityTransportManagementSystem.Models;
using IntercityTransportManagementSystem.Enums;
using Microsoft.EntityFrameworkCore;

namespace IntercityTransportManagementSystem.BackgroundJobs
{
    public class ExpiredReservationsCleanup
    {
        private readonly IntercityTransportManagementSystemDatabaseContext _context;

        public ExpiredReservationsCleanup(IntercityTransportManagementSystemDatabaseContext context)
        {
            _context = context;
        }

        public async Task ExpireReservation()
        {
            var now = DateTime.Now;

            var expiredReservations = await _context.Reservations
                .Where(r => r.ExpirationTime != null && r.ExpirationTime < now && r.Status == ReservationStatus.Pending)
                .ToListAsync();

            foreach (var reservation in expiredReservations)
            {
                reservation.Status = ReservationStatus.Expired;
                reservation.IsActive = false;

                var seatLock = await _context.BusSeatLocks
                    .FirstOrDefaultAsync(l => l.ScheduleId == reservation.ScheduleId && l.SeatId == reservation.SeatId);

                if (seatLock != null)
                {
                    _context.BusSeatLocks.Remove(seatLock);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
