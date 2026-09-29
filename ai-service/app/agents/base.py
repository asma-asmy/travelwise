from __future__ import annotations
import logging
from abc import ABC, abstractmethod
from typing import Any
from uuid import uuid4

from app.schemas.agent import AgentContext
from app.schemas.tool import ToolResult
from app.tools.registry import invoke_tool

logger = logging.getLogger(__name__)


class BaseAgent(ABC):
    """
    Shared behaviour for all agents.

    Guarantees:
      - Agents can only call tools via invoke_tool()
      - Every tool call is allow-list-validated inside the registry
      - Failures are surfaced as structured records, never raised
      - Each agent produces one structured output validated by its schema
    """

    name: str = "BaseAgent"

    def __init__(self, context: AgentContext) -> None:
        self.context = context
        self.tool_calls: list[dict[str, Any]] = []

    async def call_tool(self, tool_name: str, input_payload: dict[str, Any]) -> ToolResult:
        """Invoke a tool through the registry. Records the call for auditing."""
        result = await invoke_tool(tool_name, self.name, input_payload)
        self.tool_calls.append({
            "tool_name": tool_name,
            "input": input_payload,
            "success": result.success,
            "error": result.error,
            "duration_ms": result.duration_ms,
        })
        return result

    @abstractmethod
    async def run(self) -> Any:
        """Execute the agent's task. Must return a schema-validated output."""
        raise NotImplementedError