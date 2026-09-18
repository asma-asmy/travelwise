using TravelWise.Domain.Common;

namespace TravelWise.Domain.Entities;

public class TravellerPreference : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string InterestsJson { get; set; } = "[]";
    public string Pace { get; set; } = "Moderate";     // Relaxed | Moderate | Packed
    public string? MobilityNotes { get; set; }
    public string DietaryJson { get; set; } = "[]";
    public string BudgetTier { get; set; } = "Mid";    // Budget | Mid | Luxury
}