namespace TravelWise.Application.Features.Trips.Dtos;

public class TripDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string? Origin { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string TripType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TravellerCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}