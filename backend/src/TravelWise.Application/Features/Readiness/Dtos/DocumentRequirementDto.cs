using TravelWise.Domain.Enums;

namespace TravelWise.Application.Features.Readiness.Dtos;

public class DocumentRequirementDto
{
    public Guid Id { get; set; }
    public string Destination { get; set; } = string.Empty;
    public TripType TripType { get; set; }
    public string TripTypeName { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
    public string DocumentTypeName { get; set; } = string.Empty;
    public bool IsMandatory { get; set; }
    public string? Notes { get; set; }
}

public class CreateDocumentRequirementDto
{
    public string Destination { get; set; } = string.Empty;
    public TripType TripType { get; set; }
    public DocumentType DocumentType { get; set; }
    public bool IsMandatory { get; set; } = true;
    public string? Notes { get; set; }
}

public class UpdateDocumentRequirementDto
{
    public TripType TripType { get; set; }
    public DocumentType DocumentType { get; set; }
    public bool IsMandatory { get; set; }
    public string? Notes { get; set; }
}