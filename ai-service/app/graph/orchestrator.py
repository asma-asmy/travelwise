from __future__ import annotations
import logging
import time
from uuid import UUID, uuid4

from app.agents.budget_agent import BudgetAgent
from app.agents.readiness_agent import ReadinessAgent
from app.agents.recovery_agent import RecoveryAgent
from app.agents.risk_agent import RiskDetectorAgent
from app.agents.validation_agent import ValidationAgent
from app.graph.state import WorkflowState
from app.schemas.agent import AgentContext, RiskDetectorInput, RiskDetectorOutput
from app.schemas.workflow import WorkflowStatus

logger = logging.getLogger(__name__)


class Orchestrator:
    """
    The orchestrator drives a workflow through the state machine.
    It plans, delegates, collects outputs, triggers validation,
    and stops at AWAITING_APPROVAL.

    No LangGraph runtime is required for this implementation;
    the sequencing is explicit and deterministic. (LangGraph primitives
    can be layered on later if desired — the semantics remain identical.)
    """

    def __init__(self, workflow_id: UUID, trip_id: UUID, user_id: UUID, objective: str) -> None:
        self.state = WorkflowState(
            workflow_id=workflow_id,
            trip_id=trip_id,
            triggered_by_user_id=user_id,
            objective=objective,
        )
        self.context = AgentContext(
            workflow_id=workflow_id,
            trip_id=trip_id,
            triggered_by_user_id=user_id,
            objective=objective,
        )

    # ---------- Public entrypoint ----------

    async def run(self) -> WorkflowState:
        try:
            await self._plan()
            await self._run_risk()
            await self._run_budget()
            await self._run_readiness()
            await self._run_recovery()
            await self._validate()
            self._await_approval()
        except Exception as e:
            logger.exception("Workflow failed")
            self.state.status = WorkflowStatus.SAFE_FAILURE
            self.state.failure_reason = str(e)
            self._emit("workflow.failed", {"error": str(e)})
        return self.state

    # ---------- Steps ----------

    async def _plan(self) -> None:
        self.state.status = WorkflowStatus.PLANNING
        self.state.plan = [
            {"step_id": "1", "agent": "RiskDetectorAgent", "task": "detect_disruptions"},
            {"step_id": "2", "agent": "BudgetAgent", "task": "compute_financial_constraint"},
            {"step_id": "3", "agent": "ReadinessAgent", "task": "check_eligibility"},
            {"step_id": "4", "agent": "RecoveryAgent", "task": "propose_recovery"},
            {"step_id": "5", "agent": "ValidationAgent", "task": "validate_proposal"},
        ]
        self._emit("workflow.planned", {"plan": self.state.plan})
        self.state.status = WorkflowStatus.DELEGATING

    async def _run_risk(self) -> None:
        self._emit("agent.started", {"agent": "RiskDetectorAgent"})
        t0 = time.perf_counter()

        agent = RiskDetectorAgent(self.context)
        agent._pending_input = {  # RiskDetector takes no additional input
            "context": self.context,
            "trip_destination": "",
            "start_date": "",
            "end_date": "",
        }
        output = await agent.run()

        self.state.risk_output = output
        self.state.status = WorkflowStatus.AGENTS_RUNNING
        self._record_execution("RiskDetectorAgent", "detect_disruptions",
                               output.model_dump(mode="json"), agent.tool_calls, t0)
        self._emit("agent.completed", {"agent": "RiskDetectorAgent", "overall": output.overall_risk_level})

    async def _run_budget(self) -> None:
        self._emit("agent.started", {"agent": "BudgetAgent"})
        t0 = time.perf_counter()

        agent = BudgetAgent(self.context).with_input({
            "context": self.context.model_dump(),
            "proposed_extra_cost": 0.0,
        })
        output = await agent.run()

        self.state.budget_output = output
        self._record_execution("BudgetAgent", "compute_financial_constraint",
                               output.model_dump(mode="json"), agent.tool_calls, t0)
        self._emit("agent.completed", {"agent": "BudgetAgent", "fits": output.fits_proposal})

    async def _run_readiness(self) -> None:
        self._emit("agent.started", {"agent": "ReadinessAgent"})
        t0 = time.perf_counter()

        agent = ReadinessAgent(self.context).with_input({
            "context": self.context.model_dump(),
        })
        output = await agent.run()

        self.state.readiness_output = output
        self._record_execution("ReadinessAgent", "check_eligibility",
                               output.model_dump(mode="json"), agent.tool_calls, t0)
        self._emit("agent.completed", {"agent": "ReadinessAgent", "score": output.readiness_score})

    async def _run_recovery(self) -> None:
        if not self.state.risk_output:
            raise RuntimeError("Recovery requires risk output")

        budget_remaining = (
            self.state.budget_output.remaining_budget if self.state.budget_output else 0.0
        )

        self._emit("agent.started", {"agent": "RecoveryAgent"})
        t0 = time.perf_counter()

        agent = RecoveryAgent(self.context).with_input({
            "context": self.context.model_dump(),
            "disruptions": [d.model_dump() for d in self.state.risk_output.disruptions],
            "impacted_activity_ids": [str(i) for i in self.state.risk_output.impacted_activity_ids],
            "budget_remaining": budget_remaining,
            "budget_currency": "LKR",
            "preferences": {},
        })
        output = await agent.run()

        self.state.recovery_output = output
        self._record_execution("RecoveryAgent", "propose_recovery",
                               output.model_dump(mode="json"), agent.tool_calls, t0)
        self._emit("agent.completed", {"agent": "RecoveryAgent", "changes": len(output.changes)})

    async def _validate(self) -> None:
        if not self.state.recovery_output:
            raise RuntimeError("Validation requires a recovery proposal")

        self.state.status = WorkflowStatus.VALIDATING
        self._emit("agent.started", {"agent": "ValidationAgent"})
        t0 = time.perf_counter()

        agent = ValidationAgent(self.context).with_input({
            "context": self.context.model_dump(),
            "proposal": self.state.recovery_output.model_dump(mode="json"),
        })
        output = await agent.run()

        self.state.validation_output = output
        self._record_execution("ValidationAgent", "validate_proposal",
                               output.model_dump(mode="json"), agent.tool_calls, t0)
        self._emit("agent.completed", {"agent": "ValidationAgent", "passed": output.passed})

        if not output.passed:
            self.state.status = WorkflowStatus.VALIDATION_FAILED
            self.state.failure_reason = "; ".join(v.details for v in output.violations)

    def _await_approval(self) -> None:
        if self.state.status == WorkflowStatus.VALIDATION_FAILED:
            return
        self.state.status = WorkflowStatus.AWAITING_APPROVAL
        self.state.current_step = "human_approval"
        self._emit("workflow.awaiting_approval", {})

    # ---------- Audit helpers ----------

    def _emit(self, event_type: str, payload: dict) -> None:
        self.state.events.append({
            "event_type": event_type,
            "payload": payload,
            "at": time.time(),
        })

    def _record_execution(
        self,
        agent_name: str,
        task_name: str,
        output: dict,
        tool_calls: list[dict],
        t0: float,
    ) -> None:
        duration_ms = int((time.perf_counter() - t0) * 1000)
        self.state.agent_executions.append({
            "agent_name": agent_name,
            "task_name": task_name,
            "output": output,
            "status": "SUCCEEDED",
            "duration_ms": duration_ms,
        })
        for tc in tool_calls:
            self.state.tool_calls.append({
                "agent_name": agent_name,
                "tool_name": tc["tool_name"],
                "input": tc.get("input"),
                "success": tc.get("success"),
                "error": tc.get("error"),
                "duration_ms": tc.get("duration_ms"),
            })