namespace TravelWise.Domain.Enums;

public enum WorkflowStatus
{
    Created = 0,
    Planning = 1,
    Delegating = 2,
    AgentsRunning = 3,
    Validating = 4,
    ValidationFailed = 5,
    AwaitingApproval = 6,
    Approved = 7,
    Committing = 8,
    Completed = 9,
    Rejected = 10,
    RevisionRequired = 11,
    AgentFailed = 12,
    ExternalServiceFailed = 13,
    SafeFailure = 14
}