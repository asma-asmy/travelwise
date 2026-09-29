from __future__ import annotations

from app.agents.base import BaseAgent
from app.schemas.agent import BudgetAgentInput, BudgetAgentOutput


class BudgetAgent(BaseAgent):
    """
    Provides a deterministic financial constraint for the workflow.
    Determines whether a proposed recovery would exceed remaining budget.
    """
    name = "BudgetAgent"

    async def run(self) -> BudgetAgentOutput:
        input_data = BudgetAgentInput(**self._pending_input)
        trip_id = input_data.context.trip_id
        proposed_extra = input_data.proposed_extra_cost or 0.0

        health = await self.call_tool("CalculateBudgetHealth", {"trip_id": str(trip_id)})
        if not health.success:
            # Degraded: cannot establish budget
            return BudgetAgentOutput(
                remaining_budget=0.0,
                total_budget=0.0,
                total_spent=0.0,
                health_status="UNKNOWN",
                fits_proposal=False,
                violations=["Budget health unavailable"],
                degraded=True,
            )

        h = health.output
        remaining = float(h["remaining_amount"])
        total_budget = float(h["total_budget"])
        total_spent = float(h["total_spent"])
        status = h["health_status"]

        fits = proposed_extra <= remaining
        violations: list[str] = []
        if not fits:
            violations.append(
                f"Proposed extra cost {proposed_extra:.2f} exceeds remaining budget {remaining:.2f}"
            )

        return BudgetAgentOutput(
            remaining_budget=remaining,
            total_budget=total_budget,
            total_spent=total_spent,
            health_status=status,
            fits_proposal=fits,
            violations=violations,
            degraded=False,
        )

    # The orchestrator injects the input at dispatch time.
    _pending_input: dict = {}

    def with_input(self, payload: dict) -> "BudgetAgent":
        self._pending_input = payload
        return self