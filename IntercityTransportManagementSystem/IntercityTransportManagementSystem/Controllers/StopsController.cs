using Microsoft.AspNetCore.Mvc;
using IntercityTransportManagementSystem.Data;
using IntercityTransportManagementSystem.DTOs;
using System.Linq;
using IntercityTransportManagementSystem.Models;

namespace IntercityTransportManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StopsController : ControllerBase
    {
        private readonly IntercityTransportManagementSystemDatabaseContext _context;

        public StopsController(IntercityTransportManagementSystemDatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAllStops()
        {
            var stops = _context.Stops
                .Select(s => new StopDto
                {
                    Id = s.StopId,
                    Name = s.StopName,
                    Latitude = s.Location.Y,    // географска ширина
                    Longitude = s.Location.X    // географска дължина
                })
                .ToList();

            return Ok(stops);
        }
    }
}
