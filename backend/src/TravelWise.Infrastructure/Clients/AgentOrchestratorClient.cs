using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TravelWise.Application.Common.Exceptions;
using TravelWise.Application.Common.Interfaces;
using TravelWise.Application.Common.Options;
using TravelWise.Application.Features.Workflows.Dtos;

namespace TravelWise.Infrastructure.Clients;

public class AgentOrchestratorClient : IAgentOrchestratorClient
{
    private readonly HttpClient _http;
    private readonly ILogger<AgentOrchestratorClient> _logger;
    private readonly JsonSerializerOptions _json;

    public AgentOrchestratorClient(
        HttpClient http,
        IOptions<AiServiceOptions> options,
        ILogger<AgentOrchestratorClient> logger)
    {
        _http = http;
        _logger = logger;

        _json = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<WorkflowSnapshotDto> StartAsync(StartWorkflowRequestDto request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("/internal/workflows/start", request, _json, ct);
        return await ParseOrThrow(response, "start");
    }

    public async Task<WorkflowSnapshotDto> ResumeAsync(ResumeWorkflowRequestDto request, CancellationToken ct = default)
    {
        var url = $"/internal/workflows/{request.WorkflowId}/resume";
        var response = await _http.PostAsJsonAsync(url, request, _json, ct);
        return await ParseOrThrow(response, "resume");
    }

    public async Task<WorkflowSnapshotDto?> GetAsync(Guid workflowId, CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"/internal/workflows/{workflowId}", ct);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        return await ParseOrThrow(response, "get");
    }

    private async Task<WorkflowSnapshotDto> ParseOrThrow(HttpResponseMessage response, string op)
    {
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("AgentOrchestratorClient.{Op} failed: {Status} {Body}", op, response.StatusCode, body);
            throw new AppException(
                $"AI service {op} failed with {response.StatusCode}: {Truncate(body, 500)}",
                502);
        }

        try
        {
            var snapshot = JsonSerializer.Deserialize<WorkflowSnapshotDto>(body, _json);
            if (snapshot is null)
                throw new AppException("AI service returned an empty workflow snapshot.", 502);
            return snapshot;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize workflow snapshot.");
            throw new AppException($"Failed to parse AI service response: {ex.Message}", 502);
        }
    }

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s.Substring(0, max) + "...";
}