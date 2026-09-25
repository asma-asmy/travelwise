using TravelWise.Domain.Enums;

namespace TravelWise.Application.Features.Risk.Dtos;

public class DestinationRiskDto
{
    public string Destination { get; set; } = string.Empty;
    public RiskLevel BaseLevel { get; set; }
    public string BaseLevelName { get; set; } = string.Empty;
    public List<string> KnownRisks { get; set; } = new();
    public string? Description { get; set; }
}