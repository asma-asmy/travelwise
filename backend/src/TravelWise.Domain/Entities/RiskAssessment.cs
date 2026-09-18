using TravelWise.Domain.Common;
using TravelWise.Domain.Enums;

namespace TravelWise.Domain.Entities;

public class RiskAssessment : BaseEntity
{
    public Guid TripId { get; set; }
    public Trip Trip { get; set; } = null!;

    public RiskLevel OverallLevel { get; set; }
    public string DetailsJson { get; set; } = "{}";
    public DateTime AssessedAt { get; set; } = DateTime.UtcNow;
}