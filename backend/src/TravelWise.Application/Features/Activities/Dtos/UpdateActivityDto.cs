using TravelWise.Domain.Enums;

namespace TravelWise.Application.Features.Activities.Dtos;

public class UpdateActivityDto
{
    public string Name { get; set; } = string.Empty;
    public ActivityCategory Category { get; set; }
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public decimal EstimatedCost { get; set; }
    public string Currency { get; set; } = "LKR";
    public string Location { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsIndoor { get; set; }
    public string Difficulty { get; set; } = "Easy";
    public bool IsActive { get; set; } = true;
}