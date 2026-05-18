using IntercityTransportManagementSystem.Models;
using IntercityTransportManagementSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IntercityTransportManagementSystem.Services;
using IntercityTransportManagementSystem.DTOs;

namespace IntercityTransportManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TripsController : ControllerBase
    {
        private readonly ITripService _tripService;
        private readonly IntercityTransportManagementSystemDatabaseContext _context;

        public TripsController(ITripService tripService, IntercityTransportManagementSystemDatabaseContext context)
        {
            _tripService = tripService;
            _context = context;
        }

        [HttpGet("{tripId}/route")]
        public IActionResult GetTripRoute(int tripId)
        {
            var route = _tripService.GetRouteByTripId(tripId);

            if (route == null || !route.Any())
            {
                return NotFound("Няма намерен маршрут за този курс.");
            }

            return Ok(route);
        }

        [HttpGet("{tripId}/full-path")]
        public IActionResult GetFullPath(int tripId)
        {
            var path = _tripService.GetFullPathByTripId(tripId);

            if (path == null || !path.Any())
            {
                return NotFound("Няма генериран Shape (траектория) за този курс.");
            }

            return Ok(path);
        }

        [HttpPost("{tripId}/generate-shape")]
        public async Task<IActionResult> GenerateShape(int tripId)
        {
            await _tripService.PopulateShapesForTrip(tripId);
            return Ok(new { message = "Траекторията е генерирана успешно." });
        }
    }
}
