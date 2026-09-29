import pytest

from app.tools.registry import (
    get_tool,
    invoke_tool,
    is_allowed,
    list_tool_names,
)


def test_registry_lists_expected_tools():
    names = set(list_tool_names())
    expected = {
        "GetTripContext",
        "GetTripBudget",
        "GetExpenses",
        "CalculateBudgetHealth",
        "SearchActivities",
        "GetActivityDetails",
        "GetScheduledActivities",
        "GetSafetyAlerts",
        "GetDestinationRisk",
        "GetLatestRiskAssessment",
        "GetTravellerDocuments",
        "GetRequiredDocuments",
        "GetLatestReadiness",
    }
    assert expected.issubset(names)


def test_agent_permissions():
    assert is_allowed("GetTripBudget", "BudgetAgent") is True
    assert is_allowed("GetTripBudget", "ReadinessAgent") is False
    assert is_allowed("GetSafetyAlerts", "RiskDetectorAgent") is True
    assert is_allowed("GetSafetyAlerts", "RecoveryAgent") is False


@pytest.mark.asyncio
async def test_unknown_tool_fails():
    result = await invoke_tool("NotATool", "BudgetAgent", {})
    assert result.success is False
    assert "Unknown tool" in (result.error or "")


@pytest.mark.asyncio
async def test_disallowed_agent_fails():
    result = await invoke_tool("GetTripBudget", "ReadinessAgent", {"trip_id": "00000000-0000-0000-0000-000000000000"})
    assert result.success is False
    assert "not permitted" in (result.error or "")


@pytest.mark.asyncio
async def test_invalid_input_fails_schema():
    result = await invoke_tool("GetTripBudget", "BudgetAgent", {"trip_id": "not-a-uuid"})
    assert result.success is False
    assert "Invalid tool input" in (result.error or "")