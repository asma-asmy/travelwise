from __future__ import annotations
import json
import logging
from pathlib import Path
from threading import Lock
from uuid import UUID

from app.graph.state import WorkflowState

logger = logging.getLogger(__name__)

STORE_DIR = Path(__file__).resolve().parent.parent.parent / ".workflow_store"
STORE_DIR.mkdir(exist_ok=True)


class WorkflowStore:
    """
    File-backed store. Each workflow is written as a JSON file named
    <workflow_id>.json inside .workflow_store/. Survives process restarts.
    """

    def __init__(self) -> None:
        self._lock = Lock()

    def _path(self, workflow_id: UUID) -> Path:
        return STORE_DIR / f"{workflow_id}.json"

    def save(self, state: WorkflowState) -> None:
        path = self._path(state.workflow_id)
        with self._lock:
            path.write_text(state.model_dump_json(), encoding="utf-8")

    def get(self, workflow_id: UUID) -> WorkflowState | None:
        path = self._path(workflow_id)
        if not path.exists():
            return None
        try:
            raw = path.read_text(encoding="utf-8")
            return WorkflowState.model_validate_json(raw)
        except Exception as e:
            logger.warning("Failed to load workflow %s: %s", workflow_id, e)
            return None

    def delete(self, workflow_id: UUID) -> None:
        path = self._path(workflow_id)
        if path.exists():
            path.unlink()

    def count(self) -> int:
        return len(list(STORE_DIR.glob("*.json")))

    def list_ids(self) -> list[UUID]:
        ids: list[UUID] = []
        for path in STORE_DIR.glob("*.json"):
            try:
                ids.append(UUID(path.stem))
            except ValueError:
                continue
        return ids


workflow_store = WorkflowStore()