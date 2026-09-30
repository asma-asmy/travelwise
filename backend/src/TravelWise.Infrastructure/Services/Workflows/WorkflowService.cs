using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TravelWise.Application.Common.Exceptions;
using TravelWise.Application.Common.Interfaces;
using TravelWise.Application.Features.Budget.Dtos;
using TravelWise.Application.Features.Workflows;
using TravelWise.Application.Features.Workflows.Dtos;
using TravelWise.Domain.Entities;
using TravelWise.Domain.Enums;
using TravelWise.Infrastructure.Persistence;
using ScheduledActivityEntity = TravelWise.Domain.Entities.ScheduledActivity;
using TripEntity = TravelWise.Domain.Entities.Trip;
using WorkflowRunEntity = TravelWise.Domain.Entities.WorkflowRun;

namespace TravelWise.Infrastructure.Services.Workflows;

public class WorkflowService : IWorkflowService
{
    private readonly TravelWiseDbContext _db;
    private readonly IAgentOrchestratorClient _orchestrator;
    private readonly ILogger<WorkflowService> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = false };

    public WorkflowService(
        TravelWiseDbContext db,
        IAgentOrchestratorClient orchestrator,
        ILogger<WorkflowService> logger)
    {
        _db = db;
        _orchestrator = orchestrator;
        _logger = logger;
    }

    // ---------- Start ----------

    public async Task<WorkflowResponseDto> StartAsync(
        Guid tripId,
        Guid userId,
        bool isStaff,
        StartWorkflowCommandDto command,
        CancellationToken ct = default)
    {
        var trip = await EnsureTripAccessAsync(tripId, userId, isStaff, ct);

        // Create the WorkflowRun row up front so we have a stable local ID
        var run = new WorkflowRunEntity
        {
            TripId = tripId,
            Objective = command.Objective.Trim(),
            Status = WorkflowStatus.Created,
            CurrentStep = "created"
        };
        _db.WorkflowRuns.Add(run);
        await _db.SaveChangesAsync(ct);

        AddEvent(run.Id, "workflow.created", new { tripId, userId, objective = run.Objective });
        await _db.SaveChangesAsync(ct);

        // Call the AI service
        WorkflowSnapshotDto snapshot;
        try
        {
            snapshot = await _orchestrator.StartAsync(new StartWorkflowRequestDto
            {
                WorkflowId = run.Id,
                TripId = tripId,
                TriggeredByUserId = trip.UserId,   // always the trip owner
                Objective = run.Objective,
                DisruptionHint = command.DisruptionHint
            }, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI service start failed for workflow {Id}", run.Id);
            run.Status = WorkflowStatus.ExternalServiceFailed;
            run.FailureReason = ex.Message;
            AddEvent(run.Id, "workflow.external_failed", new { error = ex.Message });
            await _db.SaveChangesAsync(ct);
            throw;
        }

        // Persist the AI snapshot
        await PersistSnapshotAsync(run, snapshot, ct);

        // Reload with all navigation for the response
        return await LoadResponseAsync(run.Id, ct);
    }

    // ---------- Get ----------

    public async Task<WorkflowResponseDto?> GetAsync(
        Guid workflowId,
        Guid userId,
        bool isStaff,
        CancellationToken ct = default)
    {
        var run = await _db.WorkflowRuns
            .Include(w => w.Trip)
            .FirstOrDefaultAsync(w => w.Id == workflowId, ct);

        if (run is null) return null;

        if (!isStaff && run.Trip.UserId != userId)
            throw new ForbiddenException("You do not have access to this workflow.");

        return await LoadResponseAsync(run.Id, ct);
    }

    // ---------- List (approval queue) ----------

    public async Task<PagedResult<WorkflowResponseDto>> ListAsync(
        string? status,
        Guid userId,
        bool isStaff,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var q = _db.WorkflowRuns.AsQueryable();

        if (isStaff)
        {
            // Staff sees all workflows (optionally filtered)
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<WorkflowStatus>(status, true, out var statusEnum))
                q = q.Where(w => w.Status == statusEnum);
        }
        else
        {
            // Traveller sees only their own
            q = q.Where(w => w.Trip.UserId == userId);
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<WorkflowStatus>(status, true, out var statusEnum))
                q = q.Where(w => w.Status == statusEnum);
        }

        q = q.OrderByDescending(w => w.CreatedAt);

        var total = await q.CountAsync(ct);
        var p = Math.Max(1, page);
        var ps = Math.Clamp(pageSize, 1, 100);

        var ids = await q.Skip((p - 1) * ps).Take(ps).Select(w => w.Id).ToListAsync(ct);

        var items = new List<WorkflowResponseDto>();
        foreach (var id in ids)
            items.Add(await LoadResponseAsync(id, ct));

        return new PagedResult<WorkflowResponseDto>
        {
            Items = items,
            TotalCount = total,
            Page = p,
            PageSize = ps
        };
    }

    // ---------- Decide ----------

    public async Task<WorkflowResponseDto> DecideAsync(
        Guid workflowId,
        Guid userId,
        bool isStaff,
        ApprovalCommandDto command,
        CancellationToken ct = default)
    {
        var run = await _db.WorkflowRuns
            .Include(w => w.Trip)
            .FirstOrDefaultAsync(w => w.Id == workflowId, ct)
            ?? throw new NotFoundException("Workflow not found.");

        // Only staff can approve/reject high-impact actions
        if (!isStaff)
            throw new ForbiddenException("Only TravelOpsStaff or Admin can decide on a workflow.");

        if (run.Status != WorkflowStatus.AwaitingApproval)
            throw new AppException($"Workflow is in status {run.Status}, not AWAITING_APPROVAL.", 409);

        // Record the decision
        var decision = command.Decision switch
        {
            "APPROVE" => ApprovalDecisionType.Approve,
            "REJECT" => ApprovalDecisionType.Reject,
            "REVISION_REQUIRED" => ApprovalDecisionType.RequestRevision,
            _ => throw new AppException("Invalid decision.", 400)
        };

        _db.ApprovalDecisions.Add(new ApprovalDecision
        {
            WorkflowRunId = run.Id,
            DecidedByUserId = userId,
            Decision = decision,
            Reason = command.Reason,
            DecidedAt = DateTime.UtcNow
        });
        AddEvent(run.Id, "workflow.decision_recorded", new
        {
            decision = command.Decision,
            by = userId,
            reason = command.Reason
        });
        await _db.SaveChangesAsync(ct);

        // Call the AI service to resume
        WorkflowSnapshotDto snapshot;
        try
        {
            snapshot = await _orchestrator.ResumeAsync(new ResumeWorkflowRequestDto
            {
                WorkflowId = workflowId,
                Decision = command.Decision,
                Reason = command.Reason,
                DecidedByUserId = userId
            }, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI service resume failed for workflow {Id}", run.Id);
            run.Status = WorkflowStatus.ExternalServiceFailed;
            run.FailureReason = ex.Message;
            AddEvent(run.Id, "workflow.resume_failed", new { error = ex.Message });
            await _db.SaveChangesAsync(ct);
            throw;
        }

        // Persist resumed snapshot
        await PersistSnapshotAsync(run, snapshot, ct);

        // On APPROVE, commit the recovery changes transactionally
        if (command.Decision == "APPROVE" && snapshot.Status == "COMPLETED")
        {
            await CommitRecoveryAsync(run, snapshot, ct);
        }

        return await LoadResponseAsync(run.Id, ct);
    }

    // ---------- Transactional commit ----------

    private async Task CommitRecoveryAsync(WorkflowRunEntity run, WorkflowSnapshotDto snapshot, CancellationToken ct)
    {
        if (snapshot.Result is null) return;

        var resultJson = snapshot.Result.Value;
        if (!resultJson.TryGetProperty("proposal", out var proposal))
            return;

        if (!proposal.TryGetProperty("changes", out var changes) || changes.ValueKind != JsonValueKind.Array)
            return;

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            foreach (var change in changes.EnumerateArray())
            {
                var changeType = change.TryGetProperty("change_type", out var ct1) ? ct1.GetString() : null;
                if (string.IsNullOrEmpty(changeType)) continue;

                var activityIdStr = change.TryGetProperty("activity_id", out var a1) ? a1.GetString() : null;
                var newActivityIdStr = change.TryGetProperty("new_activity_id", out var a2) ? a2.GetString() : null;
                var newDate = change.TryGetProperty("new_date", out var d1) ? d1.GetString() : null;
                var newStart = change.TryGetProperty("new_start_time", out var s1) ? s1.GetString() : null;

                switch (changeType)
                {
                    case "SUBSTITUTE":
                        if (Guid.TryParse(activityIdStr, out var oldId) &&
                            Guid.TryParse(newActivityIdStr, out var newId))
                        {
                            var target = await _db.ScheduledActivities
                                .FirstOrDefaultAsync(s => s.TripId == run.TripId && s.ActivityId == oldId, ct);
                            if (target is not null)
                            {
                                var newActivity = await _db.Activities.FirstOrDefaultAsync(a => a.Id == newId, ct);
                                if (newActivity is not null)
                                {
                                    target.ActivityId = newId;
                                    target.EndTime = target.StartTime.AddMinutes(newActivity.DurationMinutes);
                                    target.Status = "Recovered";
                                }
                            }
                        }
                        break;

                    case "REMOVE":
                        if (Guid.TryParse(activityIdStr, out var remId))
                        {
                            var target = await _db.ScheduledActivities
                                .FirstOrDefaultAsync(s => s.TripId == run.TripId && s.ActivityId == remId, ct);
                            if (target is not null) target.Status = "Cancelled";
                        }
                        break;

                    case "RESCHEDULE":
                        if (Guid.TryParse(activityIdStr, out var resId) &&
                            !string.IsNullOrEmpty(newDate) &&
                            DateOnly.TryParse(newDate, out var parsedDate) &&
                            !string.IsNullOrEmpty(newStart) &&
                            TimeOnly.TryParse(newStart, out var parsedStart))
                        {
                            var target = await _db.ScheduledActivities
                                .FirstOrDefaultAsync(s => s.TripId == run.TripId && s.ActivityId == resId, ct);
                            if (target is not null)
                            {
                                target.ScheduledDate = parsedDate;
                                target.StartTime = parsedStart;
                                target.Status = "Recovered";
                            }
                        }
                        break;

                    case "ADD":
                        if (Guid.TryParse(newActivityIdStr, out var addId) &&
                            !string.IsNullOrEmpty(newDate) &&
                            DateOnly.TryParse(newDate, out var addDate))
                        {
                            var activity = await _db.Activities.FirstOrDefaultAsync(a => a.Id == addId, ct);
                            if (activity is not null)
                            {
                                var startTime = !string.IsNullOrEmpty(newStart) && TimeOnly.TryParse(newStart, out var st)
                                    ? st
                                    : new TimeOnly(9, 0);

                                _db.ScheduledActivities.Add(new ScheduledActivityEntity
                                {
                                    TripId = run.TripId,
                                    ActivityId = addId,
                                    ScheduledDate = addDate,
                                    StartTime = startTime,
                                    EndTime = startTime.AddMinutes(activity.DurationMinutes),
                                    Status = "Recovered",
                                    Notes = "Added by recovery workflow."
                                });
                            }
                        }
                        break;
                }
            }

            run.Status = WorkflowStatus.Completed;
            run.CompletedAt = DateTime.UtcNow;
            AddEvent(run.Id, "workflow.committed", new { changesApplied = changes.GetArrayLength() });

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            _logger.LogInformation("Workflow {Id} committed transactionally.", run.Id);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    // ---------- Persistence of snapshots ----------

    private async Task PersistSnapshotAsync(WorkflowRunEntity run, WorkflowSnapshotDto snapshot, CancellationToken ct)
    {
        // Update the run
        run.Status = ParseStatus(snapshot.Status);
        run.CurrentStep = snapshot.CurrentStep;
        run.FailureReason = snapshot.FailureReason;
        run.PlanJson = snapshot.Plan?.GetRawText();
        run.ResultJson = snapshot.Result?.GetRawText();
        if (snapshot.Status == "COMPLETED" || snapshot.Status == "REJECTED")
            run.CompletedAt = DateTime.UtcNow;

        // Persist each agent execution
        foreach (var ex in snapshot.AgentExecutions)
        {
            var existing = await _db.AgentExecutions
                .FirstOrDefaultAsync(e => e.WorkflowRunId == run.Id && e.AgentName == ex.AgentName, ct);

            if (existing is null)
            {
                _db.AgentExecutions.Add(new AgentExecution
                {
                    WorkflowRunId = run.Id,
                    AgentName = ex.AgentName,
                    TaskName = ex.TaskName,
                    InputJson = "{}",
                    OutputJson = ex.Output?.GetRawText(),
                    Status = ParseAgentStatus(ex.Status),
                    StartedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow,
                    DurationMs = ex.DurationMs
                });
            }
            else
            {
                existing.OutputJson = ex.Output?.GetRawText();
                existing.Status = ParseAgentStatus(ex.Status);
                existing.DurationMs = ex.DurationMs;
                existing.CompletedAt = DateTime.UtcNow;
            }
        }
        await _db.SaveChangesAsync(ct);

        // Persist tool calls
        foreach (var tc in snapshot.ToolCalls)
        {
            var agentExec = await _db.AgentExecutions
                .FirstOrDefaultAsync(e => e.WorkflowRunId == run.Id && e.AgentName == tc.AgentName, ct);

            _db.ToolCalls.Add(new ToolCall
            {
                AgentExecutionId = agentExec?.Id ?? Guid.Empty,
                ToolName = tc.ToolName,
                InputJson = tc.Input?.GetRawText() ?? "{}",
                OutputJson = null,
                Status = tc.Success ? AgentExecutionStatus.Succeeded : AgentExecutionStatus.Failed,
                DurationMs = tc.DurationMs,
                ErrorMessage = tc.Error
            });
        }

        // Persist validations
        if (snapshot.Result is not null &&
            snapshot.Result.Value.TryGetProperty("validation", out var validation))
        {
            var passed = validation.TryGetProperty("passed", out var p) && p.GetBoolean();
            var validatorName = "ValidationAgent";

            var existing = await _db.ValidationResults
                .FirstOrDefaultAsync(v => v.WorkflowRunId == run.Id && v.ValidatorName == validatorName, ct);

            if (existing is null)
            {
                _db.ValidationResults.Add(new ValidationResult
                {
                    WorkflowRunId = run.Id,
                    ValidatorName = validatorName,
                    Passed = passed,
                    DetailsJson = validation.GetRawText()
                });
            }
            else
            {
                existing.Passed = passed;
                existing.DetailsJson = validation.GetRawText();
            }
        }

        // Persist events
        foreach (var evt in snapshot.Events)
        {
            _db.WorkflowEvents.Add(new WorkflowEvent
            {
                WorkflowRunId = run.Id,
                EventType = evt.EventType,
                PayloadJson = evt.Payload?.GetRawText() ?? "{}"
            });
        }

        await _db.SaveChangesAsync(ct);
    }

    private async Task<WorkflowResponseDto> LoadResponseAsync(Guid workflowId, CancellationToken ct)
    {
        var run = await _db.WorkflowRuns
            .Include(w => w.Executions)
                .ThenInclude(e => e.ToolCalls)
            .Include(w => w.Validations)
            .Include(w => w.Approvals)
            .FirstOrDefaultAsync(w => w.Id == workflowId, ct)
            ?? throw new NotFoundException("Workflow not found.");

        JsonElement? plan = null;
        JsonElement? result = null;
        if (!string.IsNullOrWhiteSpace(run.PlanJson))
            plan = JsonDocument.Parse(run.PlanJson).RootElement.Clone();
        if (!string.IsNullOrWhiteSpace(run.ResultJson))
            result = JsonDocument.Parse(run.ResultJson).RootElement.Clone();

        return new WorkflowResponseDto
        {
            Id = run.Id,
            TripId = run.TripId,
            Objective = run.Objective,
            Status = run.Status.ToString(),
            CurrentStep = run.CurrentStep,
            CreatedAt = run.CreatedAt,
            CompletedAt = run.CompletedAt,
            FailureReason = run.FailureReason,
            Plan = plan,
            Result = result,
            AgentExecutions = run.Executions.Select(e => new AgentExecutionResponseDto
            {
                Id = e.Id,
                AgentName = e.AgentName,
                TaskName = e.TaskName,
                Status = e.Status.ToString(),
                DurationMs = e.DurationMs,
                OutputJson = e.OutputJson,
                ErrorMessage = e.ErrorMessage,
                StartedAt = e.StartedAt,
                CompletedAt = e.CompletedAt
            }).ToList(),
            ToolCalls = run.Executions.SelectMany(e => e.ToolCalls).Select(tc => new ToolCallResponseDto
            {
                Id = tc.Id,
                ToolName = tc.ToolName,
                InputJson = tc.InputJson,
                OutputJson = tc.OutputJson,
                Status = tc.Status.ToString(),
                DurationMs = tc.DurationMs,
                ErrorMessage = tc.ErrorMessage
            }).ToList(),
            Validations = run.Validations.Select(v => new ValidationResultResponseDto
            {
                Id = v.Id,
                ValidatorName = v.ValidatorName,
                Passed = v.Passed,
                DetailsJson = v.DetailsJson,
                CreatedAt = v.CreatedAt
            }).ToList(),
            Approvals = run.Approvals.Select(a => new ApprovalDecisionResponseDto
            {
                Id = a.Id,
                DecidedByUserId = a.DecidedByUserId,
                Decision = a.Decision.ToString(),
                Reason = a.Reason,
                DecidedAt = a.DecidedAt
            }).ToList()
        };
    }

    // ---------- Helpers ----------

    private async Task<TripEntity> EnsureTripAccessAsync(Guid tripId, Guid userId, bool isStaff, CancellationToken ct)
    {
        var trip = await _db.Trips.FirstOrDefaultAsync(t => t.Id == tripId, ct)
            ?? throw new NotFoundException("Trip not found.");

        if (!isStaff && trip.UserId != userId)
            throw new ForbiddenException("You do not have access to this trip.");

        return trip;
    }

    private void AddEvent(Guid workflowRunId, string eventType, object payload)
    {
        _db.WorkflowEvents.Add(new WorkflowEvent
        {
            WorkflowRunId = workflowRunId,
            EventType = eventType,
            PayloadJson = JsonSerializer.Serialize(payload, JsonOpts)
        });
    }

    private static WorkflowStatus ParseStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status)) return WorkflowStatus.Created;

        // The AI service emits names like "AWAITING_APPROVAL" (underscored).
        // Our enum members are PascalCase without underscores (e.g. "AwaitingApproval").
        // Normalize by removing underscores before parsing.
        var normalized = status.Replace("_", string.Empty);

        return Enum.TryParse<WorkflowStatus>(normalized, ignoreCase: true, out var s)
            ? s
            : WorkflowStatus.Created;
    }
    private static AgentExecutionStatus ParseAgentStatus(string status) =>
        Enum.TryParse<AgentExecutionStatus>(status, true, out var s) ? s : AgentExecutionStatus.Pending;
}