using IntercityTransportManagementSystem.Models;
using System.Text.Json;
using NetTopologySuite.Geometries;

namespace IntercityTransportManagementSystem.Services
{
    public class FinlandJsonProvider : IVehicleProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public string ProviderName => "Finland-Digitransit";

        public FinlandJsonProvider(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<List<LiveBusPosition>> GetPositionsAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetStringAsync("https://api.digitransit.fi/realtime/vehicle-positions/v1/hfp/journey/ongoing/vp/");

            using var doc = JsonDocument.Parse(response);
            var positions = new List<LiveBusPosition>();

            foreach (var element in doc.RootElement.EnumerateArray())
            {
                if (element.TryGetProperty("VP", out var vp))
                {
                    positions.Add(new LiveBusPosition
                    {
                        CurrentLocation = new Point(vp.GetProperty("long").GetDouble(), vp.GetProperty("lat").GetDouble()) { SRID = 4326 },
                        LastUpdate = DateTime.Now,
                        IsRealTime = true
                    });
                }
            }
            
            return positions;
        }
    }
}
