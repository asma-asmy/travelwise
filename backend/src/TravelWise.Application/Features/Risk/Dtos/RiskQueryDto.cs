using TravelWise.Domain.Enums;

namespace TravelWise.Application.Features.Risk.Dtos;

public class SafetyAlertQueryDto
{
    public string? Destination { get; set; }
    public string? Category { get; set; }
    public RiskLevel? MinSeverity { get; set; }
    public bool? IsActive { get; set; }
    public string SortBy { get; set; } = "ValidFrom";       // ValidFrom | Severity
    public string SortDir { get; set; } = "desc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}