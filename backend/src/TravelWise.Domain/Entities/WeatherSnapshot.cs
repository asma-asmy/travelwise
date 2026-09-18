using TravelWise.Domain.Common;

namespace TravelWise.Domain.Entities;

public class WeatherSnapshot : BaseEntity
{
    public string Destination { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public string PayloadJson { get; set; } = "{}";
    public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
}