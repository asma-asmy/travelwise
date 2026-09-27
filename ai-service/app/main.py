from __future__ import annotations
import logging

from fastapi import FastAPI

from app import __version__
from app.api.routes import router
from app.config import settings

# Logging
logging.basicConfig(
    level=getattr(logging, settings.log_level.upper(), logging.INFO),
    format="%(asctime)s %(levelname)s %(name)s :: %(message)s",
)


def create_app() -> FastAPI:
    app = FastAPI(
        title="TravelWise AI Service",
        version=__version__,
        description="Internal agentic AI service for TravelWise. Not exposed to clients.",
    )

    app.include_router(router)

    @app.get("/health", tags=["system"])
    async def health() -> dict:
        return {
            "status": "healthy",
            "service": "travelwise-ai",
            "version": __version__,
            "env": settings.ai_service_env,
        }

    return app


app = create_app()