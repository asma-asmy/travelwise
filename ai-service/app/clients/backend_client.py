from __future__ import annotations
import logging
from typing import Any, Optional
from uuid import UUID

import httpx

from app.config import settings

logger = logging.getLogger(__name__)


class BackendClient:
    """
    HTTP client for the internal service-to-service calls to ASP.NET Core.
    Every call carries the shared service token.
    Never called from clients — only from agents/tools.
    """

    def __init__(self) -> None:
        self._base_url = settings.backend_base_url.rstrip("/")
        self._timeout = settings.backend_timeout_seconds
        self._service_token = settings.ai_service_token

    def _headers(self) -> dict[str, str]:
        return {
            "Authorization": f"Bearer {self._service_token}",
            "Accept": "application/json",
        }

    async def get(self, path: str, params: Optional[dict[str, Any]] = None) -> tuple[int, Any]:
        url = f"{self._base_url}{path}"
        try:
            async with httpx.AsyncClient(timeout=self._timeout) as client:
                resp = await client.get(url, headers=self._headers(), params=params)
            return resp.status_code, self._decode(resp)
        except httpx.HTTPError as e:
            logger.warning("Backend GET %s failed: %s", path, e)
            return 0, {"error": str(e)}

    async def post(self, path: str, json_body: Optional[dict[str, Any]] = None) -> tuple[int, Any]:
        url = f"{self._base_url}{path}"
        try:
            async with httpx.AsyncClient(timeout=self._timeout) as client:
                resp = await client.post(url, headers=self._headers(), json=json_body)
            return resp.status_code, self._decode(resp)
        except httpx.HTTPError as e:
            logger.warning("Backend POST %s failed: %s", path, e)
            return 0, {"error": str(e)}

    async def put(self, path: str, json_body: Optional[dict[str, Any]] = None) -> tuple[int, Any]:
        url = f"{self._base_url}{path}"
        try:
            async with httpx.AsyncClient(timeout=self._timeout) as client:
                resp = await client.put(url, headers=self._headers(), json=json_body)
            return resp.status_code, self._decode(resp)
        except httpx.HTTPError as e:
            logger.warning("Backend PUT %s failed: %s", path, e)
            return 0, {"error": str(e)}

    @staticmethod
    def _decode(resp: httpx.Response) -> Any:
        if resp.status_code == 204 or not resp.content:
            return None
        try:
            return resp.json()
        except Exception:
            return {"raw": resp.text}


# Singleton — reused across the app
backend_client = BackendClient()