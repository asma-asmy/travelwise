from __future__ import annotations
from datetime import datetime
from typing import Any, Optional
from uuid import UUID

from pydantic import BaseModel, Field

from app.schemas.agent import (
    BudgetAgentOutput,
    ReadinessAgentOutput,
    RecoveryAgentOutput,
    RiskDetectorOutput,
    ValidationAgentOutput,
)
from app.schemas.workflow import WorkflowStatus


class WorkflowState(BaseModel):
    # identity
    workflow_id: UUID
    trip_id: UUID
    triggered_by_user_id: UUID
    objective: str
    started_at: datetime = Field(default_factory=datetime.utcnow)

    # machine
    status: WorkflowStatus = WorkflowStatus.CREATED
    current_step: Optional[str] = None
    failure_reason: Optional[str] = None

    # plan
    plan: Optional[list[dict[str, Any]]] = None

    # agent outputs
    risk_output: Optional[RiskDetectorOutput] = None
    budget_output: Optional[BudgetAgentOutput] = None
    readiness_output: Optional[ReadinessAgentOutput] = None
    recovery_output: Optional[RecoveryAgentOutput] = None
    validation_output: Optional[ValidationAgentOutput] = None

    # audit
    events: list[dict[str, Any]] = Field(default_factory=list)
    agent_executions: list[dict[str, Any]] = Field(default_factory=list)
    tool_calls: list[dict[str, Any]] = Field(default_factory=list)