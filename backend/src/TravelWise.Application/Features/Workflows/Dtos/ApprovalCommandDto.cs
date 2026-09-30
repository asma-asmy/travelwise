namespace TravelWise.Application.Features.Workflows.Dtos;

public class ApprovalCommandDto
{
    /// <summary>APPROVE | REJECT | REVISION_REQUIRED</summary>
    public string Decision { get; set; } = string.Empty;

    public string? Reason { get; set; }
}