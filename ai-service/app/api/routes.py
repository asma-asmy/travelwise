from __future__ import annotations
import logging
from uuid import UUID

from fastapi import APIRouter, Depends, HTTPException, status

from app.auth.service_token import require_service_token
from app.graph.runner import resume_workflow, start_workflow
from app.graph.store import workflow_store
from app.schemas.workflow import (
    ResumeWorkflowRequest,
    StartWorkflowRequest,
    WorkflowSnapshot,
)

logger = logging.getLogger(__name__)

router = APIRouter(prefix="/internal", tags=["internal"])


@router.get("/ping")
async def ping(_: None = Depends(require_service_token)) -> dict:
    return {"message": "pong", "auth": "ok"}


@router.post("/workflows/start", response_model=WorkflowSnapshot)
async def start(
    request: StartWorkflowRequest,
    _: None = Depends(require_service_token),
) -> WorkflowSnapshot:
    """
    Start a new workflow. Runs all agents, validates, and returns
    the state machine at AWAITING_APPROVAL (or a terminal failure state).
    """
    logger.info("Starting workflow for trip %s", request.trip_id)

    state = await start_workflow(
        trip_id=request.trip_id,
        user_id=request.triggered_by_user_id,
        objective=request.objective,
    )

    return _to_snapshot(state)


@router.post("/workflows/{workflow_id}/resume", response_model=WorkflowSnapshot)
async def resume(
    workflow_id: UUID,
    request: ResumeWorkflowRequest,
    _: None = Depends(require_service_token),
) -> WorkflowSnapshot:
    """
    Resume a workflow after human approval/rejection/revision.
    """
    if request.workflow_id != workflow_id:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="workflow_id in path and body must match.",
        )

    logger.info("Resuming workflow %s with decision %s", workflow_id, request.decision)

    state = await resume_workflow(
        workflow_id=workflow_id,
        decision=request.decision,
        reason=request.reason,
        decided_by_user_id=request.decided_by_user_id,
    )

    if state is None:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail=f"Workflow {workflow_id} not found in this service instance.",
        )

    return _to_snapshot(state)


@router.get("/workflows/{workflow_id}", response_model=WorkflowSnapshot)
async def get_workflow(
    workflow_id: UUID,
    _: None = Depends(require_service_token),
) -> WorkflowSnapshot:
    """Read the current snapshot of a workflow from the in-memory store."""
    state = workflow_store.get(workflow_id)
    if state is None:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail=f"Workflow {workflow_id} not found in this service instance.",
        )
    return _to_snapshot(state)


@router.get("/workflows")
async def list_workflows(_: None = Depends(require_service_token)) -> dict:
    """Diagnostic endpoint: list all workflows tracked in this instance."""
    return {"count": workflow_store.count(), "workflow_ids": [str(i) for i in workflow_store.list_ids()]}


def _to_snapshot(state) -> WorkflowSnapshot:
    """Convert the internal state model to the public snapshot DTO."""
    from app.schemas.workflow import PlanStep, WorkflowPlan

    plan_model = None
    if state.plan:
        plan_model = WorkflowPlan(plan=[PlanStep(**s) for s in state.plan])

    return WorkflowSnapshot(
        workflow_id=state.workflow_id,
        trip_id=state.trip_id,
        status=state.status,
        plan=plan_model,
        current_step=state.current_step,
        result=_build_result(state),
        failure_reason=state.failure_reason,
        started_at=state.started_at,
        completed_at=None,
        agent_executions=state.agent_executions,
        tool_calls=state.tool_calls,
        events=state.events,
    )


def _build_result(state) -> dict | None:
    """Assemble the auditable result payload from agent outputs."""
    if not state.recovery_output:
        return None

    return {
        "proposal": state.recovery_output.model_dump(mode="json"),
        "risk": state.risk_output.model_dump(mode="json") if state.risk_output else None,
        "budget": state.budget_output.model_dump(mode="json") if state.budget_output else None,
        "readiness": state.readiness_output.model_dump(mode="json") if state.readiness_output else None,
        "validation": state.validation_output.model_dump(mode="json") if state.validation_output else None,
        "agent_executions": state.agent_executions,
        "tool_calls": state.tool_calls,
        "events": state.events,
    }