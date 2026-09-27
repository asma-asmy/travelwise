from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(env_file=".env", env_file_encoding="utf-8", extra="ignore")

    # Service
    ai_service_host: str = "0.0.0.0"
    ai_service_port: int = 8000
    ai_service_env: str = "development"

    # Auth
    ai_service_token: str = "dev-token-change-me"

    # Backend
    backend_base_url: str = "http://localhost:5114"
    backend_timeout_seconds: int = 30

    # LLM
    llm_provider: str = "mock"
    llm_api_key: str = ""
    llm_model: str = ""
    llm_base_url: str = ""

    # Logging
    log_level: str = "INFO"


settings = Settings()