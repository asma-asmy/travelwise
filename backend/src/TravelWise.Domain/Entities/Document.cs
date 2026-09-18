using TravelWise.Domain.Common;
using TravelWise.Domain.Enums;

namespace TravelWise.Domain.Entities;

public class Document : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public DocumentType Type { get; set; }
    public string Number { get; set; } = string.Empty;
    public DateOnly? IssuedDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string? FileRef { get; set; }
    public string? Notes { get; set; }
}