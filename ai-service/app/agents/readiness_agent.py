from __future__ import annotations
from datetime import date

from app.agents.base import BaseAgent
from app.schemas.agent import ReadinessAgentInput, ReadinessAgentOutput


class ReadinessAgent(BaseAgent):
    """
    Determines whether the traveller is eligible for the trip / proposed plan,
    based on held documents vs required documents and expiry rules.

    Deterministic — no LLM.
    """
    name = "ReadinessAgent"

    async def run(self) -> ReadinessAgentOutput:
        input_data = ReadinessAgentInput(**self._pending_input)
        trip_id = input_data.context.trip_id

        ctx = await self.call_tool("GetTripContext", {"trip_id": str(trip_id)})
        if not ctx.success:
            return ReadinessAgentOutput(
                readiness_score=0,
                blocking=["Failed to load trip context"],
                warnings=[],
                eligible_for_proposal=False,
                degraded=True,
            )

        trip = ctx.output
        user_id = trip["user_id"]
        destination = trip["destination"]
        trip_end = date.fromisoformat(trip["end_date"])
        today = date.today()

        # 1. Required documents for this destination
        reqs = await self.call_tool("GetRequiredDocuments", {"destination": destination})
        requirements = reqs.output.get("requirements", []) if reqs.success else []

        # 2. Held documents
        held_resp = await self.call_tool("GetTravellerDocuments", {"user_id": str(user_id)})
        held = held_resp.output.get("documents", []) if held_resp.success else []
        held_by_type = {d["type"]: d for d in held}

        blocking: list[str] = []
        warnings: list[str] = []

        mandatory_count = 0
        mandatory_met = 0

        for r in requirements:
            doc_type = r["document_type"]
            if r.get("is_mandatory"):
                mandatory_count += 1

            doc = held_by_type.get(doc_type)
            if not doc:
                if r.get("is_mandatory"):
                    blocking.append(f"Missing mandatory document: {doc_type}")
                continue

            expiry_str = doc.get("expiry_date")
            if expiry_str:
                expiry = date.fromisoformat(expiry_str)
                if expiry < today:
                    if r.get("is_mandatory"):
                        blocking.append(f"Mandatory document expired: {doc_type}")
                    continue
                if today <= expiry <= trip_end:
                    warnings.append(f"{doc_type} expires during the trip on {expiry}")

            if r.get("is_mandatory"):
                mandatory_met += 1

        # Score: mandatory coverage (70) + no blocking (30)
        if mandatory_count == 0:
            score = 100 if not blocking else 70
        else:
            score = int((mandatory_met / mandatory_count) * 70)
        if not blocking:
            score += 30
        score = max(0, min(score, 100))

        return ReadinessAgentOutput(
            readiness_score=score,
            blocking=blocking,
            warnings=warnings,
            eligible_for_proposal=(len(blocking) == 0),
            degraded=False,
        )

    _pending_input: dict = {}

    def with_input(self, payload: dict) -> "ReadinessAgent":
        self._pending_input = payload
        return self