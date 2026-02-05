import openai
from models import MarkDownConfig
from typing import Dict, Any
import logging

logger = logging.getLogger(__name__)

def validate_config(config: MarkDownConfig):
    """Validate the MarkDownConfig for required pairs."""
    if (config.docintel_endpoint and not config.docintel_key) or (config.docintel_key and not config.docintel_endpoint):
        raise ValueError("Both docintel_endpoint and docintel_key must be provided together.")
    if config.llm_api_key and not config.llm_model:
        raise ValueError("Both llm_model and llm_api_key must be provided together.")

def build_conversion_kwargs(config: MarkDownConfig) -> Dict[str, Any]:
    """Build kwargs dict from MarkDownConfig for markitdown conversion.
    
    Args:
        config: The MarkDownConfig object containing conversion settings.
        
    Returns:
        Dict of kwargs to pass to markitdown conversion methods.
        
    Raises:
        ValueError: If config validation fails.
    """
    validate_config(config)
    kwargs = {}

    if config.docintel_endpoint:
        kwargs['docintel_endpoint'] = config.docintel_endpoint
    if config.docintel_key:
        kwargs['docintel_key'] = config.docintel_key

    if config.llm_model:
        kwargs['llm_model'] = config.llm_model
    if config.llm_prompt:
        kwargs['llm_prompt'] = config.llm_prompt

    if config.llm_api_key and config.llm_model:
        client = openai.OpenAI(api_key=config.llm_api_key)
        kwargs['llm_client'] = client

    if config.keep_data_uris is not None:
        kwargs['keep_data_uris'] = config.keep_data_uris
    if config.enable_plugins is not None:
        kwargs['enable_plugins'] = config.enable_plugins

    return kwargs

