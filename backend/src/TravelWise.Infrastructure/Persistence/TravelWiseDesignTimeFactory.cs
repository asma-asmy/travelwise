using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TravelWise.Infrastructure.Persistence;

public class TravelWiseDesignTimeFactory : IDesignTimeDbContextFactory<TravelWiseDbContext>
{
    public TravelWiseDbContext CreateDbContext(string[] args)
    {
        var apiProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "TravelWise.Api");

        var config = new ConfigurationBuilder()
            .SetBasePath(apiProjectPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = config.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is missing.");

        var options = new DbContextOptionsBuilder<TravelWiseDbContext>()
            .UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(TravelWiseDbContext).Assembly.FullName);
            })
            .Options;

        return new TravelWiseDbContext(options);
    }
}