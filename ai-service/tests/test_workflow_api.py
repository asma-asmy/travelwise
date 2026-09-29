import os

import pytest
from httpx import ASGITransport, AsyncClient

from app.config import settings
from app.main import app


@pytest.fixture
def token() -> str:
    return settings.ai_service_token


@pytest.fixture
def trip_id() -> str:
    return os.getenv("AI_TEST_TRIP_ID", "")


@pytest.fixture
def user_id() -> str:
    return os.getenv("AI_TEST_USER_ID", "")


@pytest.mark.skipif(not os.getenv("AI_TEST_TRIP_ID"), reason="AI_TEST_TRIP_ID not set")
@pytest.mark.asyncio
async def test_start_workflow_reaches_awaiting_approval(token, trip_id, user_id):
    async with AsyncClient(
        transport=ASGITransport(app=app),
        base_url="http://test",
    ) as client:
        response = await client.post(
            "/internal/workflows/start",
            headers={"Authorization": f"Bearer {token}"},
            json={
                "trip_id": trip_id,
                "triggered_by_user_id": user_id,
                "objective": "Fix my trip after a disruption.",
            },
        )
    assert response.status_code == 200
    data = response.json()
    assert data["status"] in ("AWAITING_APPROVAL", "VALIDATION_FAILED")


@pytest.mark.asyncio
async def test_start_workflow_requires_token(trip_id, user_id):
    async with AsyncClient(
        transport=ASGITransport(app=app),
        base_url="http://test",
    ) as client:
        response = await client.post(
            "/internal/workflows/start",
            json={
                "trip_id": trip_id or "00000000-0000-0000-0000-000000000000",
                "triggered_by_user_id": user_id or "00000000-0000-0000-0000-000000000000",
                "objective": "test",
            },
        )
    assert response.status_code == 401


@pytest.mark.asyncio
async def test_ping_with_wrong_token():
    async with AsyncClient(
        transport=ASGITransport(app=app),
        base_url="http://test",
    ) as client:
        response = await client.get(
            "/internal/ping",
            headers={"Authorization": "Bearer wrong"},
        )
    assert response.status_code == 401


@pytest.mark.asyncio
async def test_ping_with_correct_token(token):
    async with AsyncClient(
        transport=ASGITransport(app=app),
        base_url="http://test",
    ) as client:
        response = await client.get(
            "/internal/ping",
            headers={"Authorization": f"Bearer {token}"},
        )
    assert response.status_code == 200
    assert response.json()["message"] == "pong"