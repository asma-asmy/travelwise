using TravelWise.Domain.Common;

namespace TravelWise.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    public TravellerPreference? Preference { get; set; }
    public ICollection<Document> Documents { get; set; } = new List<Document>();
}