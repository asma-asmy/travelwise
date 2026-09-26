using TravelWise.Domain.Enums;

namespace TravelWise.Application.Features.Readiness.Dtos;

public class UpdateDocumentDto
{
    public DocumentType Type { get; set; }
    public string Number { get; set; } = string.Empty;
    public DateOnly? IssuedDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string? FileRef { get; set; }
    public string? Notes { get; set; }
}