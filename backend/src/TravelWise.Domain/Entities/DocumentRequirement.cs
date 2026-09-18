using TravelWise.Domain.Common;
using TravelWise.Domain.Enums;

namespace TravelWise.Domain.Entities;

public class DocumentRequirement : BaseEntity
{
    public string Destination { get; set; } = string.Empty;
    public TripType TripType { get; set; } = TripType.Leisure;
    public DocumentType DocumentType { get; set; }
    public bool IsMandatory { get; set; } = true;
    public string? Notes { get; set; }
}