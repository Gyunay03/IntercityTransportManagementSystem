using IntercityTransportManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace IntercityTransportManagementSystem.BackgroundJobs
{
    public class SeatLocksCleanup
    {
        private readonly IntercityTransportManagementSystemDatabaseContext _context;

        public SeatLocksCleanup(IntercityTransportManagementSystemDatabaseContext context)
        {
            _context = context;
        }

        public async Task CleanExpiredLock()
        {
            var now = DateTime.Now;

            var expiredLocks = await _context.BusSeatLocks
                .Where(l => l.ExpiryTime < now)
                .ToListAsync();

            _context.BusSeatLocks.RemoveRange(expiredLocks);
            await _context.SaveChangesAsync();
        }
    }
}
