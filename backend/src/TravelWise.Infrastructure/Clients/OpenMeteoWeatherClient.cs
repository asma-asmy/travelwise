using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TravelWise.Application.Common.Interfaces;
using TravelWise.Application.Common.Options;
using TravelWise.Application.Features.Risk.Dtos;
using TravelWise.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace TravelWise.Infrastructure.Clients;

public class OpenMeteoWeatherClient : IWeatherClient
{
    private readonly HttpClient _http;
    private readonly TravelWiseDbContext _db;
    private readonly WeatherApiOptions _options;
    private readonly ILogger<OpenMeteoWeatherClient> _logger;

    public OpenMeteoWeatherClient(
        HttpClient http,
        TravelWiseDbContext db,
        IOptions<WeatherApiOptions> options,
        ILogger<OpenMeteoWeatherClient> logger)
    {
        _http = http;
        _db = db;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<WeatherDto?> GetWeatherAsync(string destination, double latitude, double longitude, DateOnly date, CancellationToken ct = default)
    {
        // 1. Check cache first
        var cached = await _db.WeatherSnapshots
            .FirstOrDefaultAsync(w => w.Destination == destination && w.Date == date && w.ExpiresAt > DateTime.UtcNow, ct);

        if (cached is not null)
        {
            _logger.LogInformation("Weather cache hit for {Destination} on {Date}", destination, date);
            return DeserializeCached(cached);
        }

        // 2. Call the external API
        try
        {
            var url = BuildUrl(latitude, longitude, date);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

            var response = await _http.GetAsync(url, cts.Token);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Weather API returned {Status} for {Destination}", response.StatusCode, destination);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cts.Token);
            var weather = ParseOpenMeteoResponse(json, destination, date);
            if (weather is null) return null;

            // 3. Cache the result
            var snapshot = new Domain.Entities.WeatherSnapshot
            {
                Destination = destination,
                Date = date,
                PayloadJson = JsonSerializer.Serialize(weather),
                FetchedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_options.CacheMinutes)
            };

            // Upsert: delete existing and re-add to respect unique index
            var existing = await _db.WeatherSnapshots
                .FirstOrDefaultAsync(w => w.Destination == destination && w.Date == date, ct);
            if (existing is not null) _db.WeatherSnapshots.Remove(existing);

            _db.WeatherSnapshots.Add(snapshot);
            await _db.SaveChangesAsync(ct);

            return weather;
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Weather API timeout for {Destination}", destination);
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Weather API call failed for {Destination}", destination);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching weather for {Destination}", destination);
            return null;
        }
    }

    private string BuildUrl(double latitude, double longitude, DateOnly date)
    {
        var baseUrl = _options.BaseUrl.TrimEnd('/');
        var dateStr = date.ToString("yyyy-MM-dd");

        return $"{baseUrl}/forecast?latitude={latitude}&longitude={longitude}" +
               $"&daily=temperature_2m_max,temperature_2m_min,precipitation_sum,wind_speed_10m_max" +
               $"&start_date={dateStr}&end_date={dateStr}&timezone=auto";
    }

    private static WeatherDto? ParseOpenMeteoResponse(string json, string destination, DateOnly date)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("daily", out var daily))
                return null;

            double? minTemp = TryGetArrayValue(daily, "temperature_2m_min");
            double? maxTemp = TryGetArrayValue(daily, "temperature_2m_max");
            double? precip = TryGetArrayValue(daily, "precipitation_sum");
            double? wind = TryGetArrayValue(daily, "wind_speed_10m_max");

            return new WeatherDto
            {
                Destination = destination,
                Date = date,
                MinTemperatureC = minTemp,
                MaxTemperatureC = maxTemp,
                PrecipitationMm = precip,
                WindSpeedKph = wind,
                Condition = null,   // Open-Meteo daily doesn't include condition text by default
                FromCache = false,
                FetchedAt = DateTime.UtcNow
            };
        }
        catch
        {
            return null;
        }
    }

    private static double? TryGetArrayValue(JsonElement daily, string propertyName)
    {
        if (!daily.TryGetProperty(propertyName, out var arr) || arr.ValueKind != JsonValueKind.Array)
            return null;
        if (arr.GetArrayLength() == 0) return null;
        var first = arr[0];
        return first.ValueKind == JsonValueKind.Number ? first.GetDouble() : null;
    }

    private static WeatherDto? DeserializeCached(Domain.Entities.WeatherSnapshot snapshot)
    {
        try
        {
            var dto = JsonSerializer.Deserialize<WeatherDto>(snapshot.PayloadJson);
            if (dto is not null) dto.FromCache = true;
            return dto;
        }
        catch
        {
            return null;
        }
    }
}