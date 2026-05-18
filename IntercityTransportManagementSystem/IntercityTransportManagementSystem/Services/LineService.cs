using IntercityTransportManagementSystem.Data;
using IntercityTransportManagementSystem.DTOs;
using IntercityTransportManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace IntercityTransportManagementSystem.Services
{
    public class LineService : ILineService
    {
        private readonly IntercityTransportManagementSystemDatabaseContext _context;

        public LineService(IntercityTransportManagementSystemDatabaseContext context)
        {
            _context = context;
        }

        public List<LineDto> GetAllLines()
        {
            return _context.TransportLines
                .Select(l => new LineDto
                {
                    LineId = l.LineId,
                    Name = l.LongName ?? l.ShortName ?? "Неизвестна линия",
                    DefaultTripId = l.Trips.Select(t => t.TripId).FirstOrDefault()
                })
                .Where(l => l.DefaultTripId != 0)
                .ToList();
        }
    }
}
