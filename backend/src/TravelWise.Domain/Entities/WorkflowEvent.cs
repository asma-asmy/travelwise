using TravelWise.Domain.Common;

namespace TravelWise.Domain.Entities;

public class WorkflowEvent : BaseEntity
{
    public Guid WorkflowRunId { get; set; }
    public WorkflowRun WorkflowRun { get; set; } = null!;

    public string EventType { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = "{}";
}