using TravelWise.Domain.Enums;

namespace TravelWise.Application.Features.Risk.Dtos;

public class RiskAssessmentDto
{
    public Guid Id { get; set; }
    public Guid TripId { get; set; }
    public RiskLevel OverallLevel { get; set; }
    public string OverallLevelName { get; set; } = string.Empty;
    public DateTime AssessedAt { get; set; }
    public List<RiskFactorDto> Factors { get; set; } = new();
    public List<Guid> ImpactedActivityIds { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

public class RiskFactorDto
{
    public string Type { get; set; } = string.Empty;         // WEATHER | ALERT | ACTIVITY
    public RiskLevel Severity { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? RelatedActivityId { get; set; }
}