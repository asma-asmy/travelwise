from __future__ import annotations
import logging
from typing import Any

from app.config import settings

logger = logging.getLogger(__name__)


class LLMClient:
    """
    Thin abstraction over an LLM. Supports:
      - provider="mock"     -> deterministic canned responses (no network)
      - provider="openai"   -> any OpenAI-compatible endpoint
      - provider="ollama"   -> local Ollama

    The agents use this for *reasoning-only* tasks (like rationale text).
    All deterministic decisions come from tool outputs, not the LLM.
    """

    def __init__(self) -> None:
        self.provider = settings.llm_provider.lower()
        self.api_key = settings.llm_api_key
        self.model = settings.llm_model
        self.base_url = settings.llm_base_url

        if self.provider not in ("mock", "openai", "ollama"):
            logger.warning("Unknown LLM provider '%s'; falling back to mock", self.provider)
            self.provider = "mock"

    async def complete(self, system: str, user: str) -> str:
        """Return a single string completion."""
        if self.provider == "mock":
            return self._mock_complete(system, user)
        if self.provider == "openai":
            return await self._openai_complete(system, user)
        if self.provider == "ollama":
            return await self._ollama_complete(system, user)
        return ""

    # ---------------- Mock ----------------

    def _mock_complete(self, system: str, user: str) -> str:
        """
        Deterministic placeholder. Returns a short, structured summary
        derived from keywords in the prompt — not from any model.
        """
        s = (system + " " + user).lower()
        if "recovery" in s or "propose" in s:
            return "Recovery plan assembled from deterministic tool outputs."
        if "risk" in s:
            return "Risk derived from weather, alerts, and activity exposure."
        if "budget" in s:
            return "Budget health computed from deterministic calculator."
        if "readiness" in s:
            return "Readiness derived from document requirements and expiry checks."
        return "Deterministic response."

    # ---------------- OpenAI-compatible ----------------

    async def _openai_complete(self, system: str, user: str) -> str:
        import httpx

        if not self.api_key:
            logger.warning("LLM provider is openai but api_key is empty; falling back to mock")
            return self._mock_complete(system, user)

        url = (self.base_url or "https://api.openai.com/v1") + "/chat/completions"
        body = {
            "model": self.model or "gpt-4o-mini",
            "messages": [
                {"role": "system", "content": system},
                {"role": "user", "content": user},
            ],
            "temperature": 0.2,
        }
        headers = {"Authorization": f"Bearer {self.api_key}"}

        try:
            async with httpx.AsyncClient(timeout=30) as client:
                resp = await client.post(url, json=body, headers=headers)
            if resp.status_code >= 400:
                logger.warning("LLM returned %s; falling back to mock", resp.status_code)
                return self._mock_complete(system, user)
            data = resp.json()
            return data["choices"][0]["message"]["content"]
        except Exception as e:
            logger.warning("LLM call failed: %s; falling back to mock", e)
            return self._mock_complete(system, user)

    # ---------------- Ollama ----------------

    async def _ollama_complete(self, system: str, user: str) -> str:
        import httpx

        url = (self.base_url or "http://localhost:11434") + "/api/chat"
        body = {
            "model": self.model or "llama3.1",
            "messages": [
                {"role": "system", "content": system},
                {"role": "user", "content": user},
            ],
            "stream": False,
        }

        try:
            async with httpx.AsyncClient(timeout=60) as client:
                resp = await client.post(url, json=body)
            if resp.status_code >= 400:
                logger.warning("Ollama returned %s; falling back to mock", resp.status_code)
                return self._mock_complete(system, user)
            return resp.json().get("message", {}).get("content", "")
        except Exception as e:
            logger.warning("Ollama call failed: %s; falling back to mock", e)
            return self._mock_complete(system, user)


llm_client = LLMClient()