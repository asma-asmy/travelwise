using TravelWise.Application.Features.Risk.Dtos;

namespace TravelWise.Application.Common.Interfaces;

public interface IWeatherClient
{
    Task<WeatherDto?> GetWeatherAsync(string destination, double latitude, double longitude, DateOnly date, CancellationToken ct = default);
}