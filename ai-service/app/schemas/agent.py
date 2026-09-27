from __future__ import annotations
from typing import Any, Optional
from uuid import UUID
from pydantic import BaseModel, Field


# ---------- Common ----------

class AgentContext(BaseModel):
    workflow_id: UUID
    trip_id: UUID
    triggered_by_user_id: UUID
    objective: str


class RiskFactor(BaseModel):
    type: str                              # WEATHER | ALERT | ACTIVITY | DESTINATION
    severity: str                          # LOW | MEDIUM | HIGH | CRITICAL
    description: str
    related_activity_id: Optional[UUID] = None


# ---------- Risk Detector Agent ----------

class RiskDetectorInput(BaseModel):
    context: AgentContext
    trip_destination: str
    start_date: str                        # ISO yyyy-MM-dd
    end_date: str


class RiskDetectorOutput(BaseModel):
    overall_risk_level: str
    disruptions: list[RiskFactor] = Field(default_factory=list)
    impacted_activity_ids: list[UUID] = Field(default_factory=list)
    degraded: bool = False
    notes: list[str] = Field(default_factory=list)


# ---------- Budget Agent ----------

class BudgetAgentInput(BaseModel):
    context: AgentContext
    proposed_extra_cost: float = 0.0       # what the recovery proposes to spend


class BudgetAgentOutput(BaseModel):
    remaining_budget: float
    total_budget: float
    total_spent: float
    health_status: str
    fits_proposal: bool
    violations: list[str] = Field(default_factory=list)
    degraded: bool = False


# ---------- Readiness Agent ----------

class ReadinessAgentInput(BaseModel):
    context: AgentContext


class ReadinessAgentOutput(BaseModel):
    readiness_score: int
    blocking: list[str] = Field(default_factory=list)
    warnings: list[str] = Field(default_factory=list)
    eligible_for_proposal: bool
    degraded: bool = False


# ---------- Recovery Agent ----------

class RecoveryChange(BaseModel):
    change_type: str                       # RESCHEDULE | SUBSTITUTE | REMOVE | ADD
    activity_id: Optional[UUID] = None
    new_activity_id: Optional[UUID] = None
    new_date: Optional[str] = None         # ISO yyyy-MM-dd
    new_start_time: Optional[str] = None   # HH:mm:ss
    reason: str


class RecoveryAgentInput(BaseModel):
    context: AgentContext
    disruptions: list[RiskFactor]
    impacted_activity_ids: list[UUID]
    budget_remaining: float
    budget_currency: str
    preferences: dict[str, Any] = Field(default_factory=dict)


class RecoveryAgentOutput(BaseModel):
    proposal_id: UUID
    changes: list[RecoveryChange]
    estimated_cost_delta: float
    estimated_risk_after: str
    rationale_summary: str


# ---------- Validation Agent ----------

class ValidationAgentInput(BaseModel):
    context: AgentContext
    proposal: RecoveryAgentOutput


class ValidationViolation(BaseModel):
    rule: str                              # e.g. BR-01
    severity: str                          # BLOCKER | WARNING
    details: str


class ValidationAgentOutput(BaseModel):
    passed: bool
    violations: list[ValidationViolation] = Field(default_factory=list)
    checked_rules: list[str] = Field(default_factory=list)