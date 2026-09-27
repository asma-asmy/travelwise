from __future__ import annotations
from typing import Any
from pydantic import BaseModel


class ToolResult(BaseModel):
    tool_name: str
    success: bool
    output: Any = None
    error: str | None = None
    duration_ms: int = 0