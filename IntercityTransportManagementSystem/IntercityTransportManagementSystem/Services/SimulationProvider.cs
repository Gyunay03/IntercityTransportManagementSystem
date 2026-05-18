using IntercityTransportManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;

namespace IntercityTransportManagementSystem.Services
{
    public class SimulationProvider : IVehicleProvider
    {
        private readonly IServiceProvider _serviceProvider;
        public string ProviderName => "Bulgaria-Simulation";

        public SimulationProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        
        public async Task<List<LiveBusPosition>> GetPositionsAsync()
        {
            var results = new List<LiveBusPosition>();

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<IntercityTransportManagementSystemDatabaseContext>();

                var realTimeTripIds = await context.LiveBusPositions
                    .Where(b => b.VehicleNumber == "NOR-777")
                    .Select(b => b.TripId)
                    .ToListAsync();

                // всички активни курсове, които имат траектория (Shape)
                var activeTrips = await context.Trips
                    .Where(t => t.ShapeId != null && !realTimeTripIds.Contains(t.TripId))
                    .ToListAsync();

                foreach (var trip in activeTrips)
                {
                    // Вземаме спирките и часовете за този курс, подредени по последователност
                    var stopTimes = await context.StopTimes
                        .Include(st => st.Stop)
                        .Where(st => st.TripId == trip.TripId)
                        .OrderBy(st => st.StopSequence)
                        .ToListAsync();

                    if (stopTimes.Count < 2) continue;

                    double departureSeconds = stopTimes.First().DepartureTime.TotalSeconds;
                    double arrivalSeconds = stopTimes.Last().ArrivalTime.TotalSeconds;
                    double totalDurationSeconds = arrivalSeconds - departureSeconds;

                    if (totalDurationSeconds <= 0) continue;

                    bool useDemoMode = true;
                    double realSecondsToday =  DateTime.Now.TimeOfDay.TotalSeconds;
                    double totalSecondsToday = useDemoMode ? departureSeconds + (realSecondsToday % totalDurationSeconds) : realSecondsToday;

                    // Проверка дали автобусът се движи по разписание
                    if (totalSecondsToday >= departureSeconds && totalSecondsToday <= arrivalSeconds)
                    {
                        // Изчисляване на прогреса
                        double progressPercentage = (totalSecondsToday - departureSeconds) / totalDurationSeconds;

                        // Точките на траекторията от таблица Shapes
                        var shapePoints = await context.Shapes
                            .Where(s => s.ShapeId == trip.ShapeId)
                            .OrderBy(s => s.Sequence)
                            .Select(s => s.Location.Coordinate)
                            .ToArrayAsync();

                        if (shapePoints.Length > 1)
                        {
                            var lineString = new LineString(shapePoints);
                            var indexedLine = new LengthIndexedLine(lineString);

                            // Намиране на текущия участък между две спирки и позициониране на автобуса върху него
                            var currentSegment = stopTimes
                                .Zip(stopTimes.Skip(1), (current, next) => new { Current = current, Next = next })
                                .FirstOrDefault(segment =>
                                    totalSecondsToday >= segment.Current.DepartureTime.TotalSeconds &&
                                    totalSecondsToday <= segment.Next.ArrivalTime.TotalSeconds);

                            Coordinate currentLocation;
                            double currentSpeed = 0.0;

                            if (currentSegment != null)
                            {
                                double segmentStartSeconds = currentSegment.Current.DepartureTime.TotalSeconds;
                                double segmentEndSeconds = currentSegment.Next.ArrivalTime.TotalSeconds;
                                double segmentDurationSeconds = segmentEndSeconds - segmentStartSeconds;

                                double segmentProgress = segmentDurationSeconds > 0
                                    ? (totalSecondsToday - segmentStartSeconds) / segmentDurationSeconds
                                    : 0.0;

                                segmentProgress = Math.Clamp(segmentProgress, 0.0, 1.0);

                                double startIndex = indexedLine.Project(currentSegment.Current.Stop.Location.Coordinate);
                                double endIndex = indexedLine.Project(currentSegment.Next.Stop.Location.Coordinate);
                                double currentIndex = startIndex + ((endIndex - startIndex) * segmentProgress);

                                currentLocation = indexedLine.ExtractPoint(currentIndex);

                                double segmentDistanceKm = CalculateDistanceKm(
                                    indexedLine.ExtractLine(Math.Min(startIndex, endIndex), Math.Max(startIndex, endIndex)).Coordinates);

                                double segmentDurationHours = segmentDurationSeconds / 3600.0;

                                if (segmentDurationHours > 0)
                                {
                                    double averageSegmentSpeed = segmentDistanceKm / segmentDurationHours;
                                    double speedFactor = 0.75 + (0.35 * Math.Sin(segmentProgress * Math.PI));
                                    double trafficFactor = 0.95 + (0.10 * Math.Sin(totalSecondsToday / 45.0 + trip.TripId));

                                    currentSpeed = averageSegmentSpeed * speedFactor * trafficFactor;
                                }

                                if (currentSpeed > 0 && currentSpeed < 10)
                                {
                                    currentSpeed = 10.0;
                                }

                                else if (currentSpeed > 100)
                                {
                                    currentSpeed = 85.0;
                                }
                            }
                            else 
                            {
                                currentLocation = indexedLine.ExtractPoint(lineString.Length * progressPercentage);
                            }

                            results.Add(new LiveBusPosition
                            {
                                TripId = trip.TripId,
                                CurrentLocation = new Point(currentLocation.X, currentLocation.Y) { SRID = 4326 },
                                LastUpdate = DateTime.Now,
                                IsRealTime = false,
                                Speed = Math.Round(currentSpeed, 1)
                            });
                        }
                    }   
                }
            }

            return results;
        }

        private static double CalculateDistanceKm(Coordinate[] coordinates)
        {
            if (coordinates.Length < 2)
            {
                return 0.0;
            }

            double distanceKm = 0.0;

            for (int i = 1; i < coordinates.Length; i++)
            {
                distanceKm += CalculateDistanceKm(coordinates[i - 1], coordinates[i]);
            }

            return distanceKm;
        }

        private static double CalculateDistanceKm(Coordinate start, Coordinate end)
        {
            const double earthRadiusKm = 6371.0;

            double startLat = DegreesToRadians(start.Y);
            double endLat = DegreesToRadians(end.Y);
            double deltaLat = DegreesToRadians(end.Y - start.Y);
            double deltaLng = DegreesToRadians(end.X - start.X);

            double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                       Math.Cos(startLat) * Math.Cos(endLat) *
                       Math.Sin(deltaLng / 2) * Math.Sin(deltaLng / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return earthRadiusKm * c;
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }
}
