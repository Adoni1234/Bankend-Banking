using ArtemisBanking.Infrastructure.Identity.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ArtemisBanking.Infrastructure.Identity
{
    public class IdentityContextFactory : IDesignTimeDbContextFactory<IdentityContext>
    {
        public IdentityContext CreateDbContext(string[] args)
        {
            // Buscar el appsettings.json del proyecto WebApp (1 nivel arriba)
            var basePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../ArtemisBankingApi"));


            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var connectionString = configuration.GetConnectionString("IdentityConnection");

            var optionsBuilder = new DbContextOptionsBuilder<IdentityContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new IdentityContext(optionsBuilder.Options);
        }
    }
}
