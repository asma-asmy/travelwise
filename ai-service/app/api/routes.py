from fastapi import APIRouter, Depends

from app.auth.service_token import require_service_token

router = APIRouter(prefix="/internal", tags=["internal"])


@router.get("/ping")
async def ping(_: None = Depends(require_service_token)) -> dict:
    return {"message": "pong", "auth": "ok"}