using TravelWise.Domain.Common;

namespace TravelWise.Domain.Entities;

public class AuditLog : BaseEntity
{
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public Guid? PerformedByUserId { get; set; }
    public string MetadataJson { get; set; } = "{}";
}