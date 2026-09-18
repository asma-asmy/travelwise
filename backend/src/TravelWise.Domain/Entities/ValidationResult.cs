using TravelWise.Domain.Common;

namespace TravelWise.Domain.Entities;

public class ValidationResult : BaseEntity
{
    public Guid WorkflowRunId { get; set; }
    public WorkflowRun WorkflowRun { get; set; } = null!;

    public string ValidatorName { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public string DetailsJson { get; set; } = "{}";
}