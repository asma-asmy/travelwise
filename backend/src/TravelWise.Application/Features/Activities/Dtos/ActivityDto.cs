using TravelWise.Domain.Enums;

namespace TravelWise.Application.Features.Activities.Dtos;

public class ActivityDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ActivityCategory Category { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public decimal EstimatedCost { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsIndoor { get; set; }
    public string Difficulty { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}