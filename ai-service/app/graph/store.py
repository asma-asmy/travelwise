from __future__ import annotations
from threading import Lock
from uuid import UUID

from app.graph.state import WorkflowState


class WorkflowStore:
    """
    In-memory store for workflow state, keyed by workflow_id.
    Thread-safe. Lost on process restart by design — the authoritative
    record lives in PostgreSQL on the ASP.NET Core side.
    """

    def __init__(self) -> None:
        self._lock = Lock()
        self._store: dict[UUID, WorkflowState] = {}

    def save(self, state: WorkflowState) -> None:
        with self._lock:
            self._store[state.workflow_id] = state

    def get(self, workflow_id: UUID) -> WorkflowState | None:
        with self._lock:
            return self._store.get(workflow_id)

    def delete(self, workflow_id: UUID) -> None:
        with self._lock:
            self._store.pop(workflow_id, None)

    def count(self) -> int:
        with self._lock:
            return len(self._store)

    def list_ids(self) -> list[UUID]:
        with self._lock:
            return list(self._store.keys())


workflow_store = WorkflowStore()