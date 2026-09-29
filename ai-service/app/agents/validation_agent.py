from __future__ import annotations

from app.agents.base import BaseAgent
from app.schemas.agent import (
    RecoveryAgentOutput,
    ValidationAgentInput,
    ValidationAgentOutput,
    ValidationViolation,
)


class ValidationAgent(BaseAgent):
    """
    Deterministic enforcement of business rules on a proposed recovery.

    Checks:
      BR-01: Recovery cost delta <= remaining budget
      BR-02: All referenced activities exist (via backend)
      BR-03: No schedule conflicts on the new plan
      BR-04: Traveller remains eligible (readiness not blocking)
      BR-05: Schema of the proposal is well-formed

    No LLM is used. Every check is rule-based and reproducible.
    """
    name = "ValidationAgent"

    async def run(self) -> ValidationAgentOutput:
        input_data = ValidationAgentInput(**self._pending_input)
        trip_id = input_data.context.trip_id
        proposal: RecoveryAgentOutput = input_data.proposal

        violations: list[ValidationViolation] = []
        checked_rules: list[str] = []

        # BR-01: budget
        checked_rules.append("BR-01")
        health = await self.call_tool("CalculateBudgetHealth", {"trip_id": str(trip_id)})
        if health.success:
            remaining = float(health.output["remaining_amount"])
            if proposal.estimated_cost_delta > remaining:
                violations.append(ValidationViolation(
                    rule="BR-01",
                    severity="BLOCKER",
                    details=(
                        f"Recovery cost {proposal.estimated_cost_delta:.2f} exceeds "
                        f"remaining budget {remaining:.2f}"
                    ),
                ))

        # BR-02: activities exist
        checked_rules.append("BR-02")
        sched = await self.call_tool("GetScheduledActivities", {"trip_id": str(trip_id)})
        scheduled_ids = set()
        if sched.success:
            scheduled_ids = {s["activity_id"] for s in sched.output.get("scheduled_activities", [])}

        for c in proposal.changes:
            if c.new_activity_id and str(c.new_activity_id) not in scheduled_ids:
                # New activities are allowed; just verify they exist in the catalogue
                details = await self.call_tool(
                    "GetActivityDetails", {"activity_id": str(c.new_activity_id)}
                )
                if not details.success:
                    violations.append(ValidationViolation(
                        rule="BR-02",
                        severity="BLOCKER",
                        details=f"Referenced activity {c.new_activity_id} does not exist",
                    ))

        # BR-03: schedule conflicts (basic — check no two SUBSTITUTE changes target same slot)
        checked_rules.append("BR-03")
        # (Simple deterministic check; full conflict detection runs on the backend
        # during commit, but we pre-check obvious cases here.)
        affected_slots = {}
        for c in proposal.changes:
            if c.activity_id and c.change_type in ("SUBSTITUTE", "RESCHEDULE"):
                affected_slots.setdefault(str(c.activity_id), 0)
                affected_slots[str(c.activity_id)] += 1
        for slot, count in affected_slots.items():
            if count > 1:
                violations.append(ValidationViolation(
                    rule="BR-03",
                    severity="BLOCKER",
                    details=f"Activity {slot} targeted by {count} changes (conflict)",
                ))

        # BR-04: readiness
        checked_rules.append("BR-04")
        readiness = await self.call_tool("GetLatestReadiness", {"trip_id": str(trip_id)})
        if readiness.success and readiness.output:
            blocking_json = readiness.output.get("blocking_json") or "[]"
            if blocking_json not in ("[]", "", None):
                violations.append(ValidationViolation(
                    rule="BR-04",
                    severity="WARNING",
                    details="Existing readiness assessment has blocking issues",
                ))

        # BR-05: schema
        checked_rules.append("BR-05")
        try:
            RecoveryAgentOutput.model_validate(proposal.model_dump())
        except Exception as e:
            violations.append(ValidationViolation(
                rule="BR-05",
                severity="BLOCKER",
                details=f"Proposal schema invalid: {e}",
            ))

        passed = not any(v.severity == "BLOCKER" for v in violations)

        return ValidationAgentOutput(
            passed=passed,
            violations=violations,
            checked_rules=checked_rules,
        )

    _pending_input: dict = {}

    def with_input(self, payload: dict) -> "ValidationAgent":
        self._pending_input = payload
        return self