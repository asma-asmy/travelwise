from __future__ import annotations
from typing import Optional
from uuid import UUID
from pydantic import BaseModel, Field


# ---------- Trip context ----------

class TripContextInput(BaseModel):
    trip_id: UUID


class TripContextOutput(BaseModel):
    trip_id: UUID
    user_id: UUID
    title: str
    destination: str
    start_date: str
    end_date: str
    currency: str
    trip_type: str
    status: str
    traveller_count: int


# ---------- Budget ----------

class GetTripBudgetInput(BaseModel):
    trip_id: UUID


class BudgetCategoryOut(BaseModel):
    category_id: UUID
    name: str
    limit_amount: float


class GetTripBudgetOutput(BaseModel):
    budget_id: UUID
    trip_id: UUID
    total_amount: float
    currency: str
    categories: list[BudgetCategoryOut] = Field(default_factory=list)


class GetExpensesInput(BaseModel):
    trip_id: UUID


class ExpenseOut(BaseModel):
    expense_id: UUID
    amount: float
    currency: str
    description: str
    spent_at: str
    budget_category_id: Optional[UUID] = None


class GetExpensesOutput(BaseModel):
    trip_id: UUID
    expenses: list[ExpenseOut] = Field(default_factory=list)


class CalculateBudgetHealthInput(BaseModel):
    trip_id: UUID


class CategoryOverrunOut(BaseModel):
    category_id: UUID
    category_name: str
    limit_amount: float
    spent_amount: float
    overrun_amount: float
    utilization_percent: float


class CalculateBudgetHealthOutput(BaseModel):
    trip_id: UUID
    health_status: str
    total_budget: float
    total_spent: float
    remaining_amount: float
    trip_days_total: int
    trip_days_elapsed: int
    trip_days_remaining: int
    average_daily_spend: float
    recommended_daily_cap: float
    projected_end_balance: float
    category_overruns: list[CategoryOverrunOut] = Field(default_factory=list)
    warnings: list[str] = Field(default_factory=list)


# ---------- Activities ----------

class SearchActivitiesInput(BaseModel):
    category: Optional[str] = None
    location: Optional[str] = None
    is_indoor: Optional[bool] = None
    max_cost: Optional[float] = None


class ActivityOut(BaseModel):
    activity_id: UUID
    name: str
    category: str
    description: str
    duration_minutes: int
    estimated_cost: float
    currency: str
    location: str
    is_indoor: bool
    difficulty: str


class SearchActivitiesOutput(BaseModel):
    activities: list[ActivityOut] = Field(default_factory=list)


class GetActivityDetailsInput(BaseModel):
    activity_id: UUID


class GetScheduledActivitiesInput(BaseModel):
    trip_id: UUID


class ScheduledActivityOut(BaseModel):
    scheduled_activity_id: UUID
    activity_id: UUID
    activity_name: str
    category: str
    is_indoor: bool
    difficulty: str
    scheduled_date: str
    start_time: str
    end_time: str
    status: str


class GetScheduledActivitiesOutput(BaseModel):
    trip_id: UUID
    scheduled_activities: list[ScheduledActivityOut] = Field(default_factory=list)


# ---------- Risk ----------

class GetSafetyAlertsInput(BaseModel):
    destination: str


class SafetyAlertOut(BaseModel):
    alert_id: UUID
    destination: str
    category: str
    severity: str
    message: str
    valid_from: str
    valid_to: str


class GetSafetyAlertsOutput(BaseModel):
    destination: str
    alerts: list[SafetyAlertOut] = Field(default_factory=list)


class GetDestinationRiskInput(BaseModel):
    destination: str


class GetDestinationRiskOutput(BaseModel):
    destination: str
    base_level: str
    known_risks: list[str] = Field(default_factory=list)


class GetLatestRiskAssessmentInput(BaseModel):
    trip_id: UUID


class GetLatestRiskAssessmentOutput(BaseModel):
    assessment_id: Optional[UUID] = None
    trip_id: Optional[UUID] = None
    overall_level: Optional[str] = None
    details_json: Optional[str] = None
    assessed_at: Optional[str] = None


# ---------- Readiness ----------

class GetTravellerDocumentsInput(BaseModel):
    user_id: UUID


class DocumentOut(BaseModel):
    document_id: UUID
    type: str
    number: str
    issued_date: Optional[str] = None
    expiry_date: Optional[str] = None


class GetTravellerDocumentsOutput(BaseModel):
    user_id: UUID
    documents: list[DocumentOut] = Field(default_factory=list)


class GetRequiredDocumentsInput(BaseModel):
    destination: str


class DocumentRequirementOut(BaseModel):
    requirement_id: UUID
    destination: str
    trip_type: str
    document_type: str
    is_mandatory: bool
    notes: Optional[str] = None


class GetRequiredDocumentsOutput(BaseModel):
    destination: str
    requirements: list[DocumentRequirementOut] = Field(default_factory=list)


class GetLatestReadinessInput(BaseModel):
    trip_id: UUID


class GetLatestReadinessOutput(BaseModel):
    assessment_id: Optional[UUID] = None
    trip_id: Optional[UUID] = None
    score: Optional[int] = None
    missing_json: Optional[str] = None
    expiring_json: Optional[str] = None
    blocking_json: Optional[str] = None
    assessed_at: Optional[str] = None