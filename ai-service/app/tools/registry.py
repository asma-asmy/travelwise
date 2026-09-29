from __future__ import annotations
import re
import time
from dataclasses import dataclass
from typing import Any, Awaitable, Callable

from pydantic import BaseModel, ValidationError

from app.clients.backend_client import backend_client
from app.schemas.tool import ToolResult
from app.tools import definitions as D


# ============================================================================
# Case conversion helpers
# ============================================================================
#
# ASP.NET Core emits camelCase JSON (REST convention).
# Python schemas use snake_case (PEP 8).
# These helpers bridge the two at the tool boundary.

def _to_snake_case(name: str) -> str:
    """Convert camelCase or PascalCase to snake_case."""
    s1 = re.sub(r"(.)([A-Z][a-z]+)", r"\1_\2", name)
    return re.sub(r"([a-z0-9])([A-Z])", r"\1_\2", s1).lower()


def _to_camel_case(name: str) -> str:
    """Convert snake_case to camelCase."""
    parts = name.split("_")
    return parts[0] + "".join(p.title() for p in parts[1:])


def _deep_to_snake(obj: Any) -> Any:
    """Recursively convert dict keys from camelCase to snake_case."""
    if isinstance(obj, dict):
        return {_to_snake_case(k): _deep_to_snake(v) for k, v in obj.items()}
    if isinstance(obj, list):
        return [_deep_to_snake(item) for item in obj]
    return obj


# ============================================================================
# Registry types
# ============================================================================

@dataclass(frozen=True)
class ToolDefinition:
    name: str
    description: str
    input_schema: type[BaseModel]
    output_schema: type[BaseModel]
    allowed_agents: frozenset[str]
    handler: Callable[[BaseModel], Awaitable[Any]]


_REGISTRY: dict[str, ToolDefinition] = {}


def register(defn: ToolDefinition) -> None:
    _REGISTRY[defn.name] = defn


def get_tool(name: str) -> ToolDefinition | None:
    return _REGISTRY.get(name)


def list_tool_names() -> list[str]:
    return sorted(_REGISTRY.keys())


def is_allowed(tool_name: str, agent_name: str) -> bool:
    tool = _REGISTRY.get(tool_name)
    if tool is None:
        return False
    return agent_name in tool.allowed_agents


# ============================================================================
# Handler helpers
# ============================================================================

def _unmodel(input_model: BaseModel) -> dict[str, Any]:
    """Pydantic v2 dump — returns plain dict with snake_case keys."""
    return input_model.model_dump(mode="json", by_alias=False)


def _camel_query_params(input_model: BaseModel) -> dict[str, Any]:
    """
    Dump a Pydantic model and convert its snake_case keys to camelCase,
    filtering out None values. Used for query-string parameters that
    must match ASP.NET Core's camelCase conventions.
    """
    raw = _unmodel(input_model)
    params: dict[str, Any] = {}
    for k, v in raw.items():
        if v is None:
            continue
        if isinstance(v, bool):
            params[_to_camel_case(k)] = str(v).lower()
        else:
            params[_to_camel_case(k)] = v
    return params


async def _unwrap(status: int, body: Any, *, acceptable: tuple[int, ...] = (200, 204)) -> Any:
    if status not in acceptable:
        raise RuntimeError(f"Backend returned {status}: {body}")
    return body


# ============================================================================
# Tool handlers
# ============================================================================
#
# Every handler calls backend_client and returns a plain dict.
# Handlers never touch the database directly and never call other tools.

async def _h_get_trip_context(inp: D.TripContextInput) -> Any:
    status, body = await backend_client.get(f"/api/internal/tools/trip-context/{inp.trip_id}")
    return await _unwrap(status, body)


async def _h_get_trip_budget(inp: D.GetTripBudgetInput) -> Any:
    status, body = await backend_client.get(f"/api/internal/tools/budget/{inp.trip_id}")
    return await _unwrap(status, body)


async def _h_get_expenses(inp: D.GetExpensesInput) -> Any:
    status, body = await backend_client.get(f"/api/internal/tools/budget/{inp.trip_id}/expenses")
    return await _unwrap(status, body)


async def _h_calculate_budget_health(inp: D.CalculateBudgetHealthInput) -> Any:
    status, body = await backend_client.get(f"/api/internal/tools/budget/{inp.trip_id}/health")
    return await _unwrap(status, body)


async def _h_search_activities(inp: D.SearchActivitiesInput) -> Any:
    params = _camel_query_params(inp)
    status, body = await backend_client.get("/api/internal/tools/activities/search", params=params)
    return await _unwrap(status, body)


async def _h_get_activity_details(inp: D.GetActivityDetailsInput) -> Any:
    status, body = await backend_client.get(f"/api/internal/tools/activities/{inp.activity_id}")
    return await _unwrap(status, body)


async def _h_get_scheduled_activities(inp: D.GetScheduledActivitiesInput) -> Any:
    status, body = await backend_client.get(
        f"/api/internal/tools/activities/trip/{inp.trip_id}/scheduled"
    )
    return await _unwrap(status, body)


async def _h_get_safety_alerts(inp: D.GetSafetyAlertsInput) -> Any:
    status, body = await backend_client.get(
        f"/api/internal/tools/risk/destination/{inp.destination}/alerts"
    )
    return await _unwrap(status, body)


async def _h_get_destination_risk(inp: D.GetDestinationRiskInput) -> Any:
    status, body = await backend_client.get(
        f"/api/internal/tools/risk/destination/{inp.destination}/risk"
    )
    return await _unwrap(status, body)


async def _h_get_latest_risk_assessment(inp: D.GetLatestRiskAssessmentInput) -> Any:
    status, body = await backend_client.get(
        f"/api/internal/tools/risk/{inp.trip_id}/latest"
    )
    return await _unwrap(status, body, acceptable=(200, 204))


async def _h_get_traveller_documents(inp: D.GetTravellerDocumentsInput) -> Any:
    status, body = await backend_client.get(
        f"/api/internal/tools/readiness/user/{inp.user_id}/documents"
    )
    return await _unwrap(status, body)


async def _h_get_required_documents(inp: D.GetRequiredDocumentsInput) -> Any:
    status, body = await backend_client.get(
        f"/api/internal/tools/readiness/destination/{inp.destination}/requirements"
    )
    return await _unwrap(status, body)


async def _h_get_latest_readiness(inp: D.GetLatestReadinessInput) -> Any:
    status, body = await backend_client.get(
        f"/api/internal/tools/readiness/{inp.trip_id}/latest"
    )
    return await _unwrap(status, body, acceptable=(200, 204))


# ============================================================================
# Registration
# ============================================================================
#
# allowed_agents values must match the agent names used in Phase 7C:
#   "Orchestrator", "RiskDetectorAgent", "BudgetAgent", "ReadinessAgent",
#   "RecoveryAgent", "ValidationAgent"

register(ToolDefinition(
    name="GetTripContext",
    description="Read trip metadata: destination, dates, currency, trip type.",
    input_schema=D.TripContextInput,
    output_schema=D.TripContextOutput,
    allowed_agents=frozenset({"Orchestrator", "RiskDetectorAgent", "RecoveryAgent", "ValidationAgent"}),
    handler=_h_get_trip_context,
))

register(ToolDefinition(
    name="GetTripBudget",
    description="Read the trip's total budget and category limits.",
    input_schema=D.GetTripBudgetInput,
    output_schema=D.GetTripBudgetOutput,
    allowed_agents=frozenset({"BudgetAgent", "RecoveryAgent"}),
    handler=_h_get_trip_budget,
))

register(ToolDefinition(
    name="GetExpenses",
    description="Read the trip's expenses.",
    input_schema=D.GetExpensesInput,
    output_schema=D.GetExpensesOutput,
    allowed_agents=frozenset({"BudgetAgent"}),
    handler=_h_get_expenses,
))

register(ToolDefinition(
    name="CalculateBudgetHealth",
    description="Deterministic budget health calculation (deterministic, not LLM).",
    input_schema=D.CalculateBudgetHealthInput,
    output_schema=D.CalculateBudgetHealthOutput,
    allowed_agents=frozenset({"BudgetAgent", "ValidationAgent"}),
    handler=_h_calculate_budget_health,
))

register(ToolDefinition(
    name="SearchActivities",
    description="Search the activity catalogue with filters (category, location, indoor, max cost).",
    input_schema=D.SearchActivitiesInput,
    output_schema=D.SearchActivitiesOutput,
    allowed_agents=frozenset({"RecoveryAgent"}),
    handler=_h_search_activities,
))

register(ToolDefinition(
    name="GetActivityDetails",
    description="Get full details of one activity.",
    input_schema=D.GetActivityDetailsInput,
    output_schema=D.ActivityOut,
    allowed_agents=frozenset({"RecoveryAgent"}),
    handler=_h_get_activity_details,
))

register(ToolDefinition(
    name="GetScheduledActivities",
    description="List scheduled activities on the trip (what's currently planned).",
    input_schema=D.GetScheduledActivitiesInput,
    output_schema=D.GetScheduledActivitiesOutput,
    allowed_agents=frozenset({"RiskDetectorAgent", "RecoveryAgent", "ValidationAgent"}),
    handler=_h_get_scheduled_activities,
))

register(ToolDefinition(
    name="GetSafetyAlerts",
    description="Active safety alerts for a destination.",
    input_schema=D.GetSafetyAlertsInput,
    output_schema=D.GetSafetyAlertsOutput,
    allowed_agents=frozenset({"RiskDetectorAgent"}),
    handler=_h_get_safety_alerts,
))

register(ToolDefinition(
    name="GetDestinationRisk",
    description="Base risk profile for a destination.",
    input_schema=D.GetDestinationRiskInput,
    output_schema=D.GetDestinationRiskOutput,
    allowed_agents=frozenset({"RiskDetectorAgent"}),
    handler=_h_get_destination_risk,
))

register(ToolDefinition(
    name="GetLatestRiskAssessment",
    description="Most recent risk assessment for the trip, if any.",
    input_schema=D.GetLatestRiskAssessmentInput,
    output_schema=D.GetLatestRiskAssessmentOutput,
    allowed_agents=frozenset({"RiskDetectorAgent", "RecoveryAgent"}),
    handler=_h_get_latest_risk_assessment,
))

register(ToolDefinition(
    name="GetTravellerDocuments",
    description="Read the traveller's held documents (type, expiry).",
    input_schema=D.GetTravellerDocumentsInput,
    output_schema=D.GetTravellerDocumentsOutput,
    allowed_agents=frozenset({"ReadinessAgent"}),
    handler=_h_get_traveller_documents,
))

register(ToolDefinition(
    name="GetRequiredDocuments",
    description="Required documents for a destination.",
    input_schema=D.GetRequiredDocumentsInput,
    output_schema=D.GetRequiredDocumentsOutput,
    allowed_agents=frozenset({"ReadinessAgent"}),
    handler=_h_get_required_documents,
))

register(ToolDefinition(
    name="GetLatestReadiness",
    description="Most recent readiness assessment for the trip, if any.",
    input_schema=D.GetLatestReadinessInput,
    output_schema=D.GetLatestReadinessOutput,
    allowed_agents=frozenset({"ReadinessAgent", "ValidationAgent"}),
    handler=_h_get_latest_readiness,
))


# ============================================================================
# Public invocation API
# ============================================================================

async def invoke_tool(tool_name: str, agent_name: str, raw_input: dict[str, Any]) -> ToolResult:
    """
    Central gateway for every agent tool call.

    Guarantees:
      - Tool must exist in the registry
      - Agent must be on the tool's allow-list
      - Input must validate against the tool's input schema
      - Backend output must validate against the tool's output schema
      - Failures are returned as structured ToolResult (never raised to the caller)

    Backend output (camelCase) is converted to snake_case before schema validation.
    """
    t0 = time.perf_counter()

    tool = _REGISTRY.get(tool_name)
    if tool is None:
        return ToolResult(tool_name=tool_name, success=False, error="Unknown tool")

    if agent_name not in tool.allowed_agents:
        return ToolResult(
            tool_name=tool_name,
            success=False,
            error=f"Agent '{agent_name}' is not permitted to call '{tool_name}'",
        )

    # Validate input
    try:
        parsed_input = tool.input_schema.model_validate(raw_input)
    except ValidationError as ve:
        return ToolResult(
            tool_name=tool_name,
            success=False,
            error=f"Invalid tool input: {ve.errors()}",
            duration_ms=int((time.perf_counter() - t0) * 1000),
        )

    # Execute handler (which calls ASP.NET Core)
    try:
        raw_output = await tool.handler(parsed_input)
    except Exception as e:
        return ToolResult(
            tool_name=tool_name,
            success=False,
            error=f"Tool execution failed: {e}",
            duration_ms=int((time.perf_counter() - t0) * 1000),
        )

    # Validate output (after camelCase -> snake_case conversion)
    try:
        normalized_output = _deep_to_snake(raw_output)
        validated_output = tool.output_schema.model_validate(normalized_output)
    except ValidationError as ve:
        return ToolResult(
            tool_name=tool_name,
            success=False,
            error=f"Tool returned invalid output: {ve.errors()}",
            duration_ms=int((time.perf_counter() - t0) * 1000),
        )

    return ToolResult(
        tool_name=tool_name,
        success=True,
        output=validated_output.model_dump(mode="json"),
        duration_ms=int((time.perf_counter() - t0) * 1000),
    )