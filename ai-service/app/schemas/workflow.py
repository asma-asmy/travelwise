from __future__ import annotations
from datetime import datetime
from enum import Enum
from typing import Any, Optional
from uuid import UUID
from pydantic import BaseModel, Field


class WorkflowStatus(str, Enum):
    CREATED = "CREATED"
    PLANNING = "PLANNING"
    DELEGATING = "DELEGATING"
    AGENTS_RUNNING = "AGENTS_RUNNING"
    VALIDATING = "VALIDATING"
    VALIDATION_FAILED = "VALIDATION_FAILED"
    AWAITING_APPROVAL = "AWAITING_APPROVAL"
    APPROVED = "APPROVED"
    COMMITTING = "COMMITTING"
    COMPLETED = "COMPLETED"
    REJECTED = "REJECTED"
    REVISION_REQUIRED = "REVISION_REQUIRED"
    AGENT_FAILED = "AGENT_FAILED"
    EXTERNAL_SERVICE_FAILED = "EXTERNAL_SERVICE_FAILED"
    SAFE_FAILURE = "SAFE_FAILURE"


class AgentExecutionStatus(str, Enum):
    PENDING = "PENDING"
    RUNNING = "RUNNING"
    SUCCEEDED = "SUCCEEDED"
    FAILED = "FAILED"
    SKIPPED = "SKIPPED"


class StartWorkflowRequest(BaseModel):
    trip_id: UUID
    objective: str = Field(..., min_length=5, max_length=1000)
    triggered_by_user_id: UUID
    disruption_hint: Optional[str] = None


class ResumeWorkflowRequest(BaseModel):
    workflow_id: UUID
    decision: str = Field(..., pattern="^(APPROVE|REJECT|REVISION_REQUIRED)$")
    reason: Optional[str] = None
    decided_by_user_id: UUID


class PlanStep(BaseModel):
    step_id: str
    agent: str
    task: str
    depends_on: list[str] = Field(default_factory=list)


class WorkflowPlan(BaseModel):
    plan: list[PlanStep]


class WorkflowSnapshot(BaseModel):
    workflow_id: UUID
    trip_id: UUID
    status: WorkflowStatus
    plan: Optional[WorkflowPlan] = None
    current_step: Optional[str] = None
    result: Optional[dict[str, Any]] = None
    failure_reason: Optional[str] = None
    started_at: datetime
    completed_at: Optional[datetime] = None