namespace TravelWise.Application.Features.Risk.Dtos;

public class SafetyInformationDto
{
    public string Destination { get; set; } = string.Empty;
    public string? EmergencyNumber { get; set; }
    public string? HospitalInfo { get; set; }
    public string? LocalAdvisory { get; set; }
    public List<SafetyAlertDto> ActiveAlerts { get; set; } = new();
}