"""Configuration loading from environment variables."""

import os
from dotenv import load_dotenv
from models import MarkDownConfig

# Load environment variables from .env file if it exists
load_dotenv()

# Optional API key for authentication
API_KEY = os.getenv("API_KEY")

def load_default_config() -> MarkDownConfig:
    """Load default configuration from environment variables.

    Returns:
        MarkDownConfig: Default configuration with values from .env
    """
    return MarkDownConfig(
        docintel_endpoint=os.getenv("DOCINTEL_ENDPOINT"),
        docintel_key=os.getenv("DOCINTEL_KEY"),
        llm_api_key=os.getenv("OPENAI_API_KEY"),
        llm_model=os.getenv("OPENAI_MODEL"),
        llm_prompt=os.getenv("LLM_PROMPT"),
        keep_data_uris=True,
        enable_plugins=True
    )

# Global default config
default_config = load_default_config()