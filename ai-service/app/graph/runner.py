from __future__ import annotations
from uuid import UUID, uuid4

from app.graph.orchestrator import Orchestrator
from app.graph.state import WorkflowState
from app.graph.store import workflow_store
from app.schemas.workflow import WorkflowStatus


async def start_workflow(
    workflow_id: UUID,                         # ← NEW parameter
    trip_id: UUID,
    user_id: UUID,
    objective: str,
) -> WorkflowState:
    orchestrator = Orchestrator(
        workflow_id=workflow_id,               # ← use the caller's ID
        trip_id=trip_id,
        user_id=user_id,
        objective=objective,
    )
    state = await orchestrator.run()
    workflow_store.save(state)
    return state


async def resume_workflow(
    workflow_id: UUID,
    decision: str,
    reason: str | None,
    decided_by_user_id: UUID,
) -> WorkflowState | None:
    """
    Resume a workflow after a human decision.
    decision: APPROVE | REJECT | REVISION_REQUIRED
    """
    state = workflow_store.get(workflow_id)
    if state is None:
        return None

    if state.status != WorkflowStatus.AWAITING_APPROVAL:
        # Only workflows awaiting approval may be resumed
        state.events.append({
            "event_type": "workflow.resume_rejected",
            "payload": {
                "reason": f"Workflow is in status {state.status.value}; not awaiting approval.",
                "decision": decision,
            },
        })
        workflow_store.save(state)
        return state

    if decision == "APPROVE":
        state.status = WorkflowStatus.APPROVED
        state.events.append({
            "event_type": "workflow.approved",
            "payload": {"decided_by": str(decided_by_user_id), "reason": reason},
        })
        # In this implementation the actual commit (writing changes to the
        # schedule) is done by ASP.NET Core after this response returns.
        state.status = WorkflowStatus.COMPLETED
        state.events.append({
            "event_type": "workflow.completed",
            "payload": {"at": None},
        })

    elif decision == "REJECT":
        state.status = WorkflowStatus.REJECTED
        state.events.append({
            "event_type": "workflow.rejected",
            "payload": {"decided_by": str(decided_by_user_id), "reason": reason},
        })

    elif decision == "REVISION_REQUIRED":
        state.status = WorkflowStatus.REVISION_REQUIRED
        state.events.append({
            "event_type": "workflow.revision_requested",
            "payload": {"decided_by": str(decided_by_user_id), "reason": reason},
        })

    workflow_store.save(state)
    return state