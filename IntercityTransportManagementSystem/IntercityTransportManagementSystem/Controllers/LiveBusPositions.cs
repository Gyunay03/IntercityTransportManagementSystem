using IntercityTransportManagementSystem.DTOs;
using IntercityTransportManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IntercityTransportManagementSystem.Controllers
{
    public class LiveBusPositions : Controller
    {
        private readonly IntercityTransportManagementSystemDatabaseContext _context;

        public LiveBusPositions(IntercityTransportManagementSystemDatabaseContext context)
        {
            _context = context;
        }

        [HttpGet("api/trips/{tripId}/live-position")]
        public async Task<ActionResult<LiveBusPositionDto>> GetLivePosition(int tripId)
        {
            var position = await _context.LiveBusPositions
                .FirstOrDefaultAsync(p => p.TripId == tripId);

            if (position == null)
            {
                return Ok(null);
            }

            return new LiveBusPositionDto
            {
                TripId = position.TripId,
                VehicleNumber = position.VehicleNumber,
                Latitude = position.CurrentLocation.Y,
                Longitude = position.CurrentLocation.X,
                IsRealTime = position.IsRealTime,
                Speed = (double)position.Speed,
                Heading = position.Heading
            };
        }
    }
}
