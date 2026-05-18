using IntercityTransportManagementSystem.Data;
using IntercityTransportManagementSystem.DTOs;
using IntercityTransportManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using NetTopologySuite.Geometries;

namespace IntercityTransportManagementSystem.Services
{
    public class TripService : ITripService
    {
        private readonly IntercityTransportManagementSystemDatabaseContext _context;

        public TripService(IntercityTransportManagementSystemDatabaseContext context)
        {
            _context = context;
        }

        public List<TripRouteStopDto> GetRouteByTripId(int tripId)
        {
            return _context.StopTimes
                .Where(st => st.TripId == tripId)
                .Include(st => st.Stop)
                .OrderBy(st => st.StopSequence)
                .Select(st => new TripRouteStopDto
                {
                    StopSequence = st.StopSequence,
                    StopName = st.Stop.StopName,
                    ArrivalTime = st.ArrivalTime,
                    DepartureTime = st.DepartureTime,
                    Latitude = st.Stop.Location.Y,
                    Longitude = st.Stop.Location.X
                })
                .ToList();
        }

        public async Task PopulateShapesForTrip(int tripId)
        {
            // Взимаме началната и крайната спирка за курса
            var stops = _context.StopTimes
                .Where(st => st.TripId == tripId)
                .OrderBy(st => st.StopSequence)
                .Select(st => st.Stop.Location)
                .ToList();

            if (stops.Count < 2) return;

            var start = stops.First();
            var end = stops.Last();

            // Извикваме OSRM API от сървъра
            using var httpClient = new HttpClient();
            var url = $"http://router.project-osrm.org/route/v1/driving/{start.X.ToString(System.Globalization.CultureInfo.InvariantCulture)},{start.Y.ToString(System.Globalization.CultureInfo.InvariantCulture)};{end.X.ToString(System.Globalization.CultureInfo.InvariantCulture)},{end.Y.ToString(System.Globalization.CultureInfo.InvariantCulture)}?overview=full&geometries=geojson";

            var response = await httpClient.GetStreamAsync(url);
            var doc = JsonDocument.Parse(response);
            var Coordinates = doc.RootElement.GetProperty("routes")[0].GetProperty("geometry").GetProperty("coordinates");

            // Изчисляване на стари записи за този Shape, ако има такива
            var existingShapes = _context.Shapes.Where(s => s.ShapeId == tripId);
            _context.Shapes.RemoveRange(existingShapes);

            // Записваме всяка точка от OSRM в таблица Shapes
            int sequence = 0;
            foreach (var coord in Coordinates.EnumerateArray())
            {
                var shapePoint = new Shape
                {
                    ShapeId = tripId,
                    Sequence = sequence++,
                    Location = new Point(coord[0].GetDouble(), coord[1].GetDouble()) { SRID = 4326 }
                };
                _context.Shapes.Add(shapePoint);
            }

            // Обвързваме курса с този Shape
            var trip = _context.Trips.Find(tripId);
            
            if (trip != null)
            {
                trip.ShapeId = tripId;
            }

            await _context.SaveChangesAsync();
        }

        public List<object> GetFullPathByTripId(int tripId)
        {
            var trip = _context.Trips.FirstOrDefault(t => t.TripId == tripId);
            if (trip == null || trip.ShapeId == null)
            {
                return new List<object>();
            }

            return _context.Shapes
                .Where(s => s.ShapeId == trip.ShapeId)
                .OrderBy(s => s.Sequence)
                .Select(s => new
                {
                    lat = s.Location.Y,
                    lng = s.Location.X
                })
                .Cast<object>()
                .ToList();
        }
    }
}
