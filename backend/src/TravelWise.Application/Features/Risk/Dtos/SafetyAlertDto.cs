using TravelWise.Domain.Enums;

namespace TravelWise.Application.Features.Risk.Dtos;

public class SafetyAlertDto
{
    public Guid Id { get; set; }
    public string Destination { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public RiskLevel Severity { get; set; }
    public string SeverityName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public bool IsActive { get; set; }
}

public class CreateSafetyAlertDto
{
    public string Destination { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public RiskLevel Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
}

public class UpdateSafetyAlertDto
{
    public string Category { get; set; } = string.Empty;
    public RiskLevel Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public bool IsActive { get; set; }
}