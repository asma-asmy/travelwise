namespace TravelWise.Application.Features.Risk.Dtos;

public class WeatherDto
{
    public string Destination { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public double? MinTemperatureC { get; set; }
    public double? MaxTemperatureC { get; set; }
    public double? PrecipitationMm { get; set; }
    public double? WindSpeedKph { get; set; }
    public string? Condition { get; set; }
    public bool FromCache { get; set; }
    public DateTime FetchedAt { get; set; }
}