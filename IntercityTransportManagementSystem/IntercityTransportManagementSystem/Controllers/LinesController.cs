using Microsoft.AspNetCore.Mvc;
using IntercityTransportManagementSystem.Services;

namespace IntercityTransportManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LinesController : ControllerBase
    {
        private readonly ILineService _lineService;

        public LinesController(ILineService lineService)
        {
            _lineService = lineService;
        }

        [HttpGet]
        public IActionResult GetLines()
        {
            return Ok(_lineService.GetAllLines());
        }
    }
}
