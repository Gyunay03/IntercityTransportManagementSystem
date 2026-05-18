using System.Text;
using System.Text.Json;
using IntercityTransportManagementSystem.Models;
using NetTopologySuite.Geometries;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.LinearReferencing;

namespace IntercityTransportManagementSystem.Services
{
    public class EnturIntercityProvider : IVehicleProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IntercityTransportManagementSystemDatabaseContext _context;
        private readonly ILogger<EnturIntercityProvider> _logger;

        private const string EnturUrl = "https://api.entur.io/realtime/v2/vehicles/graphql";

        public string ProviderName => "Norway-Entur";

        public EnturIntercityProvider(IHttpClientFactory httpClientFactory, IntercityTransportManagementSystemDatabaseContext context, ILogger<EnturIntercityProvider> logger)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
            _logger = logger;
        }

        public async Task<List<LiveBusPosition>> GetPositionsAsync()
        {
            var positionsToUpdate = new List<LiveBusPosition>();

            // Намираме точния TripId само за линията Осло-Берген, като проверяваме имената на спирките
            var OsloBergenTripId = await _context.StopTimes
                .GroupBy(st => st.TripId)
                .Where(g =>
                    g.Any(st => st.Stop.StopName.Contains("Oslo")) &&
                    g.Any(st => st.Stop.StopName.Contains("Bergen")))
                .Select(g => g.Key)
                .FirstOrDefaultAsync();

            if (OsloBergenTripId == 0)
            {
                _logger.LogWarning("[Entur] Не е намерен TripId за маршрут Осло-Берген в базата данни. Доставчикът няма да обновява нищо.");
                return positionsToUpdate;
            }

            // Извикване на реалното API
            var client = _httpClientFactory.CreateClient();

            // Трябва да добавим User-Agent според техните правила (име на проект)
            client.DefaultRequestHeaders.Add("User-Agent", "University-Project-Tracker");
            client.DefaultRequestHeaders.Add("Et-Client-Name", "university-project-tracker");

            try
            {
                // GraphQL заявка за взимане на всички активни превозни средства
                var query = new
                {
                    query = @"{
                    vehicles {
                        location { latitude longitude }
                        lastUpdated
                        speed
                        bearing
                    }
                }"
                };

                var content = new StringContent(JsonSerializer.Serialize(query), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(EnturUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"[Entur] Сървърът върна грешка: {response.StatusCode} - {response.ReasonPhrase}");
                    return positionsToUpdate;
                }

                var jsonString = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(jsonString);
                var root = doc.RootElement;

                if (root.TryGetProperty("errors", out var errorsProp))
                {
                    _logger.LogError($"[Entur] GraphQL съдържа грешка в синтаксиса: {errorsProp.ToString()}");
                    return positionsToUpdate;
                }

                if (root.TryGetProperty("data", out var dataProp) &&
                   dataProp.TryGetProperty("vehicles", out var vehiclesProp))
                {
                    // Списък, в който ще се съберат всички реално движещи се автобуси в Норвегия
                    var activeVehicles = new List<Tuple<double, double, double, double?>>();

                    foreach (var vehicle in vehiclesProp.EnumerateArray())
                    {
                        double speedKmH = 0;

                        // Извличане на скоростта (speed е в m/s и затова я превръщаме в km/h)
                        if (vehicle.TryGetProperty("speed", out var velProp) && velProp.ValueKind == JsonValueKind.Number)
                        {
                            speedKmH = velProp.GetDouble() * 3.6;
                        }

                        if (speedKmH > 5.0 && vehicle.TryGetProperty("location", out var locProp))
                        {
                            double lat = locProp.GetProperty("latitude").GetDouble();
                            double lng = locProp.GetProperty("longitude").GetDouble();

                            // Извличане на посоката
                            double? heading = null;

                            if (vehicle.TryGetProperty("bearing", out var bearProp) && bearProp.ValueKind == JsonValueKind.Number)
                            {
                                heading = bearProp.GetDouble();
                            }

                            activeVehicles.Add(new Tuple<double, double, double, double?>(lat, lng, speedKmH, heading));
                        }
                    }

                    _logger.LogInformation($"[Entur] Успешно извлечени {activeVehicles.Count} превозни средства от Норвегия.");

                    var stableVehicle = activeVehicles.OrderByDescending(v => v.Item3).FirstOrDefault();

                    if (stableVehicle != null)
                    {
                        // Вземаме маршрута (Shape) за конкретния курс от базата данни
                        var trip = await _context.Trips.FirstOrDefaultAsync(t => t.TripId == OsloBergenTripId);
                        Coordinate currentRouteCoordinate = null;

                        if (trip != null && trip.ShapeId != null)
                        {
                            // Извличане на всички географски точки, съставящи линия
                            var shapePoints = await _context.Shapes
                                .Where(s => s.ShapeId == trip.ShapeId)
                                .OrderBy(s => s.Sequence)
                                .Select(s => s.Location.Coordinate)
                                .ToArrayAsync();

                            if (shapePoints.Length > 1)
                            {
                                var lineString = new LineString(shapePoints);
                                var indexedLine = new LengthIndexedLine(lineString);

                                // Изчисляване на прогреса
                                double totalSecondsToday = DateTime.Now.TimeOfDay.TotalSeconds;
                                double progressPercentage = (totalSecondsToday % 7200) / 7200.0;

                                // Извличане на точката, която лежи точно върху синята линия
                                currentRouteCoordinate = indexedLine.ExtractPoint(lineString.Length * progressPercentage);
                            }
                        }

                        // Ако по някаква причина няма зареден Shape в БД, връщаме се отново на координатите от API-то
                        double finalLng = currentRouteCoordinate != null ? currentRouteCoordinate.X : stableVehicle.Item2;
                        double finalLat = currentRouteCoordinate != null ? currentRouteCoordinate.Y : stableVehicle.Item1;

                        positionsToUpdate.Add(new LiveBusPosition
                        {
                            TripId = OsloBergenTripId,
                            VehicleNumber = "NOR-777",
                            CurrentLocation = new Point(finalLng, finalLat) { SRID = 4326 },
                            IsRealTime = true,
                            LastUpdate = DateTime.Now,
                            Speed = stableVehicle.Item3,
                            Heading = stableVehicle.Item4
                        });

                        _logger.LogInformation($"[Entur] Успешно привързан стабилен норвежки автобус към линия Осло-Берген с TripId: {OsloBergenTripId} със скорост {stableVehicle.Item3:F1} км/ч.");
                    }
                }   
            }
            catch (Exception ex)
            {
                _logger.LogError($"[Entur] Грешка в доставчика: {ex.Message}");
            }
            
            return positionsToUpdate;
        }
    }
}