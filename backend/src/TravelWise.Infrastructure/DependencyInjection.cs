using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelWise.Application.Common.Interfaces;
using TravelWise.Application.Common.Options;
using TravelWise.Application.Features.Auth;
using TravelWise.Application.Features.Budget;
using TravelWise.Infrastructure.Auth;
using TravelWise.Infrastructure.Persistence;
using TravelWise.Infrastructure.Services.Budget;
using TravelWise.Application.Features.Activities;
using TravelWise.Infrastructure.Services.Activities;
using TravelWise.Application.Features.Risk;
using TravelWise.Infrastructure.Clients;
using TravelWise.Infrastructure.Services.Risk;

namespace TravelWise.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ---------- Options ----------
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        // ---------- DbContext ----------
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is missing.");

        services.AddDbContext<TravelWiseDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(TravelWiseDbContext).Assembly.FullName);
            }));

        // ---------- Auth ----------
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();

        // ---------- Budget Component ----------
        services.AddScoped<IBudgetService, BudgetService>();

        // ---------- Activities Component ----------
        services.AddScoped<IActivityService, ActivityService>();

        // ---------- Risk Component ----------
        services.Configure<WeatherApiOptions>(configuration.GetSection(WeatherApiOptions.SectionName));

        services.AddHttpClient<IWeatherClient, OpenMeteoWeatherClient>(client =>
        {
            var baseUrl = configuration.GetSection(WeatherApiOptions.SectionName)["BaseUrl"] ?? "https://api.open-meteo.com/v1";
            client.BaseAddress = new Uri(baseUrl.EndsWith('/') ? baseUrl : baseUrl + "/");
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        services.AddScoped<IRiskService, RiskService>();


        return services;
    }
}