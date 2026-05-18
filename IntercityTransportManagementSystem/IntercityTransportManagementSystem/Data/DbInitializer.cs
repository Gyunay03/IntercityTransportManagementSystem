using IntercityTransportManagementSystem.Models;
using IntercityTransportManagementSystem.Enums;
using Microsoft.AspNetCore.Identity;
using NetTopologySuite.Geometries;

namespace IntercityTransportManagementSystem.Data
{
    public class DbInitializer
    {
        public static void Seed(IntercityTransportManagementSystemDatabaseContext context)
        {
            SeedAdmin(context);
            SeedStops(context);
            SeedTransportData(context);
        }

        private static void SeedAdmin(IntercityTransportManagementSystemDatabaseContext context)
        {
            if (context.Users.Any(u => u.Role == UserRole.Administrator))
                return;

            var passwordHasher = new PasswordHasher<User>();

            var admininistrator = new User
            {
                Name = "System",
                LastName = "Administrator",
                Email = "admin@system.com",
                Role = UserRole.Administrator,
                IsActive = true,
                IsEmailVerified = true,
                CreatedAt = DateTime.Now
            };

            admininistrator.Password = passwordHasher.HashPassword(admininistrator, "Admin123");

            context.Users.Add(admininistrator);
            context.SaveChanges();
        }

        private static void SeedStops(IntercityTransportManagementSystemDatabaseContext context) 
        {
            if (context.Stops.Any())
                return;

            var stops = new List<Stop>
            {
                new Stop
                {
                    StopName = "Централна автогара София",
                    Location = new Point(23.3219, 42.7122) { SRID = 4326}
                },
                new Stop
                {
                    StopName = "Автогара Юг Пловдив",
                    Location = new Point(24.7493, 42.1345) { SRID = 4326}
                },
                new Stop
                {
                    StopName = "Автогара Варна",
                    Location = new Point(27.8985, 43.2144) { SRID = 4326}
                }
            };

            context.Stops.AddRange(stops);
            context.SaveChanges();
        }

        private static void SeedTransportData(IntercityTransportManagementSystemDatabaseContext context)
        {
            if (context.TransportLines.Any()) return;

            // Създаване на линия
            var line = new TransportLine
            {
                LongName = "София - Пловдив",
                ShortName = "SF-PLV",
                RouteType = 3
            };

            context.TransportLines.Add(line);
            context.SaveChanges();

            // Създаване на Курс
            var trip = new Trip
            {
                LineId = line.LineId,
                TripOriginalId = "TRIP_001",
                DirectionId = false
            };

            context.Trips.Add(trip);
            context.SaveChanges();

            // Свързване на спирките с курса
            var SofiaStop = context.Stops.FirstOrDefault(s => s.StopName.Contains("София"));
            var PlovidvStop = context.Stops.FirstOrDefault(s => s.StopName.Contains("Пловдив"));

            if (SofiaStop != null && PlovidvStop != null)
            {
                context.StopTimes.AddRange(
                    new StopTime
                    {
                        TripId = trip.TripId,
                        StopId = SofiaStop.StopId,
                        StopSequence = 1,
                        ArrivalTime = new TimeSpan(8, 0, 0),
                        DepartureTime = new TimeSpan(8, 15, 0)
                    },
                    new StopTime
                    {
                        TripId = trip.TripId,
                        StopId = PlovidvStop.StopId,
                        StopSequence = 2,
                        ArrivalTime = new TimeSpan(10, 30, 0),
                        DepartureTime = new TimeSpan(10, 45, 0)
                    }
                );

                context.SaveChanges();
            }
        }
    }
}
