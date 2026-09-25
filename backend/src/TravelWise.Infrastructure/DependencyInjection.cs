using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelWise.Application.Common.Interfaces;
using TravelWise.Application.Common.Options;
using TravelWise.Application.Features.Auth;
using TravelWise.Infrastructure.Auth;
using TravelWise.Infrastructure.Persistence;

namespace TravelWise.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Options
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        // DbContext
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is missing.");

        services.AddDbContext<TravelWiseDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(TravelWiseDbContext).Assembly.FullName);
            }));

        // Auth
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}