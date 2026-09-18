using TravelWise.Domain.Common;

namespace TravelWise.Domain.Entities;

public class ReadinessAssessment : BaseEntity
{
    public Guid TripId { get; set; }
    public Trip Trip { get; set; } = null!;

    public int Score { get; set; }               // 0..100
    public string MissingJson { get; set; } = "[]";
    public string ExpiringJson { get; set; } = "[]";
    public string BlockingJson { get; set; } = "[]";
    public DateTime AssessedAt { get; set; } = DateTime.UtcNow;
}