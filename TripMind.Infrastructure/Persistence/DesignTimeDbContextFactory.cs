using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TripMind.Infrastructure.Persistence
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TripMindDbContext>
    {
        public TripMindDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddUserSecrets<DesignTimeDbContextFactory>()
                .AddEnvironmentVariables()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<TripMindDbContext>();
            optionsBuilder.UseSqlServer(config.GetConnectionString("DefaultConnection"));

            return new TripMindDbContext(optionsBuilder.Options);
        }
    }
}