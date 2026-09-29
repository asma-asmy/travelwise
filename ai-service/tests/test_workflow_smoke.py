import asyncio
import os
from uuid import UUID

import pytest

from app.graph.orchestrator import Orchestrator
from app.schemas.workflow import WorkflowStatus

# Configure via env: AI_TEST_TRIP_ID
TRIP_ID = os.getenv("AI_TEST_TRIP_ID", "")
USER_ID = os.getenv("AI_TEST_USER_ID", "")


@pytest.mark.skipif(not TRIP_ID or not USER_ID, reason="Set AI_TEST_TRIP_ID and AI_TEST_USER_ID to run")
@pytest.mark.asyncio
async def test_full_workflow_reaches_awaiting_approval():
    orch = Orchestrator(
        workflow_id=UUID(int=0),
        trip_id=UUID(TRIP_ID),
        user_id=UUID(USER_ID),
        objective="Fix my trip after a disruption.",
    )
    state = await orch.run()

    assert state.status in (
        WorkflowStatus.AWAITING_APPROVAL,
        WorkflowStatus.VALIDATION_FAILED,
        WorkflowStatus.SAFE_FAILURE,
    )

    # All five agents should have executed (unless safe failure)
    if state.status != WorkflowStatus.SAFE_FAILURE:
        agent_names = {e["agent_name"] for e in state.agent_executions}
        assert "RiskDetectorAgent" in agent_names
        assert "BudgetAgent" in agent_names
        assert "ReadinessAgent" in agent_names
        assert "RecoveryAgent" in agent_names
        assert "ValidationAgent" in agent_names