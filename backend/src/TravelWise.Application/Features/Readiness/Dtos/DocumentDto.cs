using TravelWise.Domain.Enums;

namespace TravelWise.Application.Features.Readiness.Dtos;

public class DocumentDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DocumentType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public DateOnly? IssuedDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string? FileRef { get; set; }
    public string? Notes { get; set; }
    public bool IsExpired { get; set; }
    public int? DaysUntilExpiry { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}