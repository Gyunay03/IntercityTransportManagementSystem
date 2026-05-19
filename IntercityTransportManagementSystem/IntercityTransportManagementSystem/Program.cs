using IntercityTransportManagementSystem.Data;
using IntercityTransportManagementSystem.Hubs;
using IntercityTransportManagementSystem.Models;
using IntercityTransportManagementSystem.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Hangfire.SqlServer;
using IntercityTransportManagementSystem.BackgroundJobs;
using IntercityTransportManagementSystem.Filters;

namespace IntercityTransportManagementSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            //builder.Services.AddHostedService<ReservationCleanupService>();
            builder.Services.AddSignalR();
            builder.Services.AddHangfire(config =>
                config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddHangfireServer();
            builder.Services.AddScoped<ExpiredReservationsCleanup>();
            builder.Services.AddScoped<SeatLocksCleanup>();
            builder.Services.AddScoped<BusRequestJob>();
            builder.Services.AddScoped<IReservationService, ReservationService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                });

            builder.Services.AddDbContext<IntercityTransportManagementSystemDatabaseContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    x => x.UseNetTopologySuite()
                ));
            builder.Services.AddScoped<ITripService, TripService>();

            //builder.Services.AddHostedService<BusSimulationJob>();
            builder.Services.AddScoped<ILineService, LineService>();
            builder.Services.AddHttpClient();
            //builder.Services.AddSingleton<IVehicleProvider, FinlandJsonProvider>();
            builder.Services.AddScoped<IVehicleProvider, SimulationProvider>();
            builder.Services.AddScoped<IVehicleProvider, EnturIntercityProvider>();
            
            builder.Services.AddHostedService<UnifiedBusTrackingJob>();

            /*
            builder.Services.AddSingleton<IVehicleProvider>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<GtfsRealTimeProvider>>();
                return new GtfsRealTimeProvider(
                    sp.GetRequiredService<IHttpClientFactory>(),
                    "https://cdn.mbta.com/realtime/VehiclePositions.pb",
                    "USA-Boston",
                    1,
                    logger);
            });
            */    

            var app = builder.Build();
            var env = builder.Environment;

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = scope.ServiceProvider.GetRequiredService<IntercityTransportManagementSystemDatabaseContext>();
                DbInitializer.Seed(context);
            }

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("Missing ConnectionString: DefaultConnection");
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapHub<ReservationHub>("/reservationHub");
            app.MapHub<BusHub>("/busHub");

            // Добавяне на Dashboard (UI)
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthFilter() }
            });

            RecurringJob.AddOrUpdate<ExpiredReservationsCleanup>(
                "expire-reservations",
                job => job.ExpireReservation(),
                Cron.Minutely);

            RecurringJob.AddOrUpdate<SeatLocksCleanup>(

                "clean-seatLocks",
                job => job.CleanExpiredLock(),
                Cron.Minutely);

            RecurringJob.AddOrUpdate<BusRequestJob>(
                "check-capacity",
                job => job.CheckCapacity(),
                "*/5 * * * *");

            app.Run();
        }
    }
}
