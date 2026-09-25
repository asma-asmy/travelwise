namespace TravelWise.Application.Common.Options;

public class WeatherApiOptions
{
    public const string SectionName = "WeatherApi";

    public string BaseUrl { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
    public int TimeoutSeconds { get; set; } = 5;
    public int CacheMinutes { get; set; } = 45;
}