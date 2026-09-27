from fastapi import Header, HTTPException, status

from app.config import settings


async def require_service_token(
    authorization: str | None = Header(default=None),
) -> None:
    """
    Verify the shared service token used between ASP.NET Core and this service.
    The token is sent as: Authorization: Bearer <AI_SERVICE_TOKEN>
    """
    if not authorization or not authorization.startswith("Bearer "):
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Missing bearer token")

    token = authorization.removeprefix("Bearer ").strip()
    if token != settings.ai_service_token:
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Invalid service token")