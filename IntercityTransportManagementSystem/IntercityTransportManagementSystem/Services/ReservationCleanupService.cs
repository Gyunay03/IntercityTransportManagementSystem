using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using IntercityTransportManagementSystem.Models;
using IntercityTransportManagementSystem.Enums;
using Microsoft.EntityFrameworkCore;

namespace IntercityTransportManagementSystem.Services
{
    public class ReservationCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ReservationCleanupService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider
                        .GetRequiredService<IntercityTransportManagementSystemDatabaseContext>();

                    var now = DateTime.Now;

                    // Автоматично обновяване на резервациите от статус "Чакаща" на "Отменена" след изтичане на определения срок (60 минути)
                    var updatedPendingReservation = await context.Reservations
                        .Where(r => r.Status == ReservationStatus.Pending &&
                                    r.ExpirationTime != null &&
                                    r.ExpirationTime <= now)
                        .ExecuteUpdateAsync(r => r
                            .SetProperty(p => p.Status, ReservationStatus.Cancelled)
                            .SetProperty(p => p.IsActive, false));

                    Console.WriteLine($"{DateTime.Now} : {updatedPendingReservation} маркиране/маркирания на преминали резервации като 'Отменена'.");

                    // Автоматично обновяване на резервациите, чиято дата на пътуване вече е минала, като неактивни
                    var updtdPTravelDateRes = await context.Reservations
                        .Where(r => r.IsActive && r.Schedule.TravelDate < DateOnly.FromDateTime(DateTime.UtcNow))
                        .ExecuteUpdateAsync(r => r
                            .SetProperty(p => p.IsActive, false));

                    Console.WriteLine($"{DateTime.Now} : {updtdPTravelDateRes} маркиране на резервацията, чиято дата на пътуване е преминала, като 'Неактивна.'");

                    // Изтриване на старите заключени места при резервация 
                    var deleteLockSeat = await context.BusSeatLocks
                        .Where(l => l.ExpiryTime <= now)
                        .ExecuteDeleteAsync();

                    Console.WriteLine($"{DateTime.Now} : {deleteLockSeat} изтрит/изтрити заключвания.");
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
