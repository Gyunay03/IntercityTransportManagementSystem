using IntercityTransportManagementSystem.Models;
using IntercityTransportManagementSystem.Enums;
using Microsoft.AspNetCore.Identity;

namespace IntercityTransportManagementSystem.Data
{
    public class DbInitializer
    {
        public static void SeedAdmin(IntercityTransportManagementSystemDatabaseContext context)
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
                CreatedAt = DateTime.UtcNow
            };

            admininistrator.Password = passwordHasher.HashPassword(admininistrator, "Admin123");

            context.Users.Add(admininistrator);
            context.SaveChanges();
        }
    }
}
