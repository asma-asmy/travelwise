using TravelWise.Domain.Common;
using TravelWise.Domain.Enums;

namespace TravelWise.Domain.Entities;

public class SafetyAlert : BaseEntity
{
    public string Destination { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;  // Weather | Health | Transport | Safety
    public RiskLevel Severity { get; set; }
    public string Message { get; set; } = string.Empty;

    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public bool IsActive { get; set; } = true;
}