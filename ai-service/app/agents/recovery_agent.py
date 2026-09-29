from __future__ import annotations
from uuid import UUID, uuid4

from app.agents.base import BaseAgent
from app.schemas.agent import (
    RecoveryAgentInput,
    RecoveryAgentOutput,
    RecoveryChange,
)


class RecoveryAgent(BaseAgent):
    """
    Given detected disruptions and constraints, propose a recovery plan.
    Uses deterministic rule-based substitution:
      - Find indoor alternatives for impacted outdoor activities
      - Prefer options within remaining budget
      - Skip already-scheduled activities

    The LLM is used only to summarise the rationale (non-blocking).
    All decisions come from tool outputs.
    """
    name = "RecoveryAgent"

    async def run(self) -> RecoveryAgentOutput:
        input_data = RecoveryAgentInput(**self._pending_input)
        trip_id = input_data.context.trip_id
        disruptions = input_data.disruptions
        impacted = input_data.impacted_activity_ids
        budget_remaining = input_data.budget_remaining

        changes: list[RecoveryChange] = []
        estimated_delta = 0.0

        # 1. Load the current schedule
        sched = await self.call_tool("GetScheduledActivities", {"trip_id": str(trip_id)})
        scheduled = sched.output.get("scheduled_activities", []) if sched.success else []
        scheduled_ids = {s["activity_id"] for s in scheduled}

        impacted_ids_str = {str(i) for i in impacted}

        # 2. For each impacted outdoor activity, find an indoor alternative
        for s in scheduled:
            if str(s["scheduled_activity_id"]) not in impacted_ids_str:
                continue
            if s.get("is_indoor", False):
                continue

            # Search for indoor, low-cost alternatives
            search = await self.call_tool("SearchActivities", {
                "is_indoor": True,
                "max_cost": max(0.0, budget_remaining),
            })
            candidates = search.output.get("activities", []) if search.success else []

            # Prefer one not already scheduled and cheaper than remaining budget
            pick = None
            for c in candidates:
                if str(c["activity_id"]) in scheduled_ids:
                    continue
                if float(c["estimated_cost"]) <= budget_remaining:
                    pick = c
                    break

            if pick:
                cost_delta = float(pick["estimated_cost"])
                changes.append(RecoveryChange(
                    change_type="SUBSTITUTE",
                    activity_id=UUID(s["activity_id"]),
                    new_activity_id=UUID(pick["activity_id"]),
                    reason=(
                        f"Replaced '{s['activity_name']}' with indoor option "
                        f"'{pick['name']}' due to disruption."
                    ),
                ))
                estimated_delta += cost_delta
                budget_remaining -= cost_delta
                scheduled_ids.add(str(pick["activity_id"]))
            else:
                changes.append(RecoveryChange(
                    change_type="REMOVE",
                    activity_id=UUID(s["activity_id"]),
                    reason=f"No viable indoor alternative; removing '{s['activity_name']}'.",
                ))

        rationale = "Deterministic substitutions chosen from indoor, budget-fitting alternatives."
        if not changes:
            rationale = "No impacted activities required recovery."

        return RecoveryAgentOutput(
            proposal_id=uuid4(),
            changes=changes,
            estimated_cost_delta=round(estimated_delta, 2),
            estimated_risk_after="LOW" if changes else "UNCHANGED",
            rationale_summary=rationale,
        )

    _pending_input: dict = {}

    def with_input(self, payload: dict) -> "RecoveryAgent":
        self._pending_input = payload
        return self