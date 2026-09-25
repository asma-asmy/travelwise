namespace TravelWise.Application.Features.Activities.Dtos;

public class TravellerPreferenceDto
{
    public Guid UserId { get; set; }
    public List<string> Interests { get; set; } = new();
    public string Pace { get; set; } = "Moderate";
    public string? MobilityNotes { get; set; }
    public List<string> Dietary { get; set; } = new();
    public string BudgetTier { get; set; } = "Mid";
}