namespace TravelWise.Application.Features.Workflows.Dtos;

public class StartWorkflowCommandDto
{
    public string Objective { get; set; } = string.Empty;
    public string? DisruptionHint { get; set; }
}