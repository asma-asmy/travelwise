from __future__ import annotations
from uuid import UUID

from app.agents.base import BaseAgent
from app.schemas.agent import (
    RiskDetectorInput,
    RiskDetectorOutput,
    RiskFactor,
)


class RiskDetectorAgent(BaseAgent):
    """
    Detects travel disruptions for a trip.
    Combines: destination risk profile, active alerts, weather-driven impact,
    and planned outdoor activities.

    All decisions come from tool outputs (deterministic). The LLM is not used.
    """
    name = "RiskDetectorAgent"

    async def run(self) -> RiskDetectorOutput:
        trip_id = self.context.trip_id

        # 1. Trip context
        ctx = await self.call_tool("GetTripContext", {"trip_id": str(trip_id)})
        if not ctx.success:
            return RiskDetectorOutput(
                overall_risk_level="UNKNOWN",
                disruptions=[],
                impacted_activity_ids=[],
                degraded=True,
                notes=[f"Failed to load trip: {ctx.error}"],
            )

        trip = ctx.output
        destination = trip["destination"]

        # 2. Destination base risk
        dest_risk = await self.call_tool("GetDestinationRisk", {"destination": destination})
        base_level = "LOW"
        known_risks: list[str] = []
        if dest_risk.success:
            base_level = dest_risk.output.get("base_level", "LOW").upper()
            known_risks = dest_risk.output.get("known_risks", [])

        # 3. Active alerts
        alerts_resp = await self.call_tool("GetSafetyAlerts", {"destination": destination})
        alerts = alerts_resp.output.get("alerts", []) if alerts_resp.success else []

        # 4. Scheduled activities
        sched = await self.call_tool("GetScheduledActivities", {"trip_id": str(trip_id)})
        scheduled = sched.output.get("scheduled_activities", []) if sched.success else []

        # ---- Deterministic risk assembly ----

        severity_rank = {"LOW": 0, "MEDIUM": 1, "HIGH": 2, "CRITICAL": 3}
        disruptions: list[RiskFactor] = []
        impacted: set[UUID] = set()

        # destination base
        if severity_rank.get(base_level, 0) >= 1:
            for kr in known_risks:
                disruptions.append(RiskFactor(
                    type="DESTINATION",
                    severity=base_level,
                    description=kr,
                ))

        # alerts
        for a in alerts:
            sev = a.get("severity", "MEDIUM").upper()
            disruptions.append(RiskFactor(
                type="ALERT",
                severity=sev,
                description=f"[{a.get('category','Alert')}] {a.get('message','')}",
            ))
            # High/Critical alerts impact all outdoor activities
            if severity_rank.get(sev, 0) >= 2:
                for s in scheduled:
                    if not s.get("is_indoor", False) and s.get("status") != "Cancelled":
                        try:
                            impacted.add(UUID(str(s["scheduled_activity_id"])))
                        except (KeyError, ValueError):
                            pass

        # outdoor hard-difficulty activities bump risk to MEDIUM at least
        hard_outdoor = [
            s for s in scheduled
            if not s.get("is_indoor", False)
            and s.get("difficulty") == "Hard"
            and s.get("status") != "Cancelled"
        ]
        if hard_outdoor:
            disruptions.append(RiskFactor(
                type="ACTIVITY",
                severity="MEDIUM",
                description=f"{len(hard_outdoor)} hard outdoor activity(ies) planned.",
            ))

        # overall level = highest severity among disruptions, or base level
        overall = "LOW"
        for d in disruptions:
            if severity_rank[d.severity] > severity_rank[overall]:
                overall = d.severity

        notes: list[str] = []
        if not dest_risk.success:
            notes.append("Destination risk unavailable; using default LOW.")
        if not alerts_resp.success:
            notes.append("Alerts unavailable; assuming none.")

        return RiskDetectorOutput(
            overall_risk_level=overall,
            disruptions=disruptions,
            impacted_activity_ids=list(impacted),
            degraded=bool(notes),
            notes=notes,
        )