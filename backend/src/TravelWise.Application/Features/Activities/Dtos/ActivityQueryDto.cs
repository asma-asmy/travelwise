using TravelWise.Domain.Enums;

namespace TravelWise.Application.Features.Activities.Dtos;

public class ActivityQueryDto
{
    public string? Search { get; set; }
    public ActivityCategory? Category { get; set; }
    public string? Location { get; set; }
    public bool? IsIndoor { get; set; }
    public string? Difficulty { get; set; }
    public decimal? MinCost { get; set; }
    public decimal? MaxCost { get; set; }
    public int? MaxDurationMinutes { get; set; }
    public string SortBy { get; set; } = "Name";       // Name | Cost | Duration
    public string SortDir { get; set; } = "asc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}