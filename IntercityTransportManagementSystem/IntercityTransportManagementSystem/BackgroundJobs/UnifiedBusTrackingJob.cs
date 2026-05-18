using IntercityTransportManagementSystem.Hubs;
using IntercityTransportManagementSystem.Models;
using IntercityTransportManagementSystem.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries; 

namespace IntercityTransportManagementSystem.BackgroundJobs
{
    public class UnifiedBusTrackingJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<BusHub> _hubContext;
        private readonly ILogger<UnifiedBusTrackingJob> _logger;

        public UnifiedBusTrackingJob(IServiceScopeFactory scopeFactory, IHubContext<BusHub> hubContext, ILogger<UnifiedBusTrackingJob> logger)
        {
            _scopeFactory = scopeFactory;
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Unified Bus Tracking Job стартира...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<IntercityTransportManagementSystemDatabaseContext>();

                        var providers = scope.ServiceProvider.GetServices<IVehicleProvider>();

                        foreach (var provider in providers)
                        {
                            try
                            {
                                var positions = await provider.GetPositionsAsync();
                                foreach (var pos in positions)
                                {
                                    var existingPosition = await context.LiveBusPositions
                                        .FirstOrDefaultAsync(p => p.TripId == pos.TripId, stoppingToken);

                                    if (existingPosition != null)
                                    {
                                        if (existingPosition.IsRealTime && !pos.IsRealTime)
                                            continue;

                                        existingPosition.CurrentLocation = pos.CurrentLocation;
                                        existingPosition.LastUpdate = DateTime.Now;
                                        existingPosition.IsRealTime = pos.IsRealTime;
                                        existingPosition.Speed = pos.Speed;
                                        existingPosition.Heading = pos.Heading;
                                        existingPosition.VehicleNumber = pos.VehicleNumber;

                                    }
                                    else
                                    {
                                        context.LiveBusPositions.Add(pos);
                                    }

                                    // SignalR: Изпращане на позицията към картата в реално време
                                    await _hubContext.Clients.All.SendAsync(
                                        "UpdateBusPosition",
                                        pos.TripId,
                                        pos.CurrentLocation.Y,   // Latitude
                                        pos.CurrentLocation.X,   // Longitude
                                        pos.IsRealTime,
                                        pos.Speed,
                                        cancellationToken: stoppingToken
                                    );
                                }

                                await context.SaveChangesAsync(stoppingToken);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"Доставчикът {provider.ProviderName} се провали, но продължаваме с останалите: {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex) 
                {
                    _logger.LogError($"Грешка в UnifiedBusTrackingJob: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}
