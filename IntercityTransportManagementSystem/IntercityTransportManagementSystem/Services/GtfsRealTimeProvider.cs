using IntercityTransportManagementSystem.Models;
using NetTopologySuite.Geometries;
using ProtoBuf;
using TransitRealtime;

namespace IntercityTransportManagementSystem.Services
{
    public class GtfsRealTimeProvider : IVehicleProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _url;
        private readonly string _name;
        private readonly int _targetTripId;
        private readonly ILogger _logger;

        public GtfsRealTimeProvider(IHttpClientFactory httpClientFactory, string url, string name, int targetTripId, ILogger logger)
        {
            _httpClientFactory = httpClientFactory;
            _url = url;
            _name = name;
            _targetTripId = targetTripId;
            _logger = logger;
        }

        public string ProviderName => _name;

        public async Task<List<LiveBusPosition>> GetPositionsAsync()
        {
            var results = new List<LiveBusPosition>();
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("User-Agent", "University-Project-Tracker");

            try
            {
                using (var stream = await client.GetStreamAsync(_url))
                {
                    // Използване на protobuf-net за десериализация 
                    var feed = Serializer.Deserialize<FeedMessage>(stream);

                    foreach (var entity in feed.Entities)
                    {
                        if (entity.Vehicle != null && entity.Vehicle.Position != null)
                        {
                            double currentSpeed = entity.Vehicle.Position.Speed * 3.6;

                            results.Add(new LiveBusPosition
                            {
                                TripId = _targetTripId,
                                CurrentLocation = new Point(entity.Vehicle.Position.Longitude, entity.Vehicle.Position.Latitude) { SRID = 4326 },
                                LastUpdate = DateTime.Now,
                                IsRealTime = true,
                                Speed = currentSpeed
                            });

                            break;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Грешка при обновяване на данните: {ex.Message}");
            }

            return results;
        }
    }
}
