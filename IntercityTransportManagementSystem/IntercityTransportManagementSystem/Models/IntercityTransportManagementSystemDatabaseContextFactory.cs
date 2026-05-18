using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IntercityTransportManagementSystem.Models
{
    public class IntercityTransportManagementSystemDatabaseContextFactory : IDesignTimeDbContextFactory<IntercityTransportManagementSystemDatabaseContext>
    {
        public IntercityTransportManagementSystemDatabaseContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<IntercityTransportManagementSystemDatabaseContext>();

            optionsBuilder.UseSqlServer(
                "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=IntercityTransportManagementSystem_Database;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False",
                x => x.UseNetTopologySuite());

            return new IntercityTransportManagementSystemDatabaseContext(optionsBuilder.Options);
        }
    }
}
