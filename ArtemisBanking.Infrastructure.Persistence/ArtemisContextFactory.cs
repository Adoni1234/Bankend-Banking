using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using ArtemisBanking.Infrastructure.Persistence.Context;

namespace ArtemisBanking.Infrastructure.Persistence
{
    public class ArtemisContextFactory : IDesignTimeDbContextFactory<ArtemisContext>
    {
        public ArtemisContext CreateDbContext(string[] args)
        {
            var basePath = Path.GetFullPath(
                Path.Combine(Directory.GetCurrentDirectory(), "../ArtemisBankingApi")
            );

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<ArtemisContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new ArtemisContext(optionsBuilder.Options);
        }
    }
}
