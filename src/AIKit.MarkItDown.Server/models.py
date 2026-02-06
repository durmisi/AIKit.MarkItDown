"""Pydantic models for the MarkItDown API."""

from pydantic import BaseModel
from typing import Optional, Dict, Any, List


class MarkDownConfig(BaseModel):
    """Configuration model for Markdown conversion settings."""
    docintel_endpoint: Optional[str] = None
    llm_api_key: Optional[str] = None
    llm_model: Optional[str] = None
    llm_prompt: Optional[str] = None
    keep_data_uris: Optional[bool] = None
    enable_plugins: Optional[bool] = None
    docintel_key: Optional[str] = None


class MarkDownResult(BaseModel):
    """Result model for Markdown conversion output."""
    text: str
    title: Optional[str] = None
    metadata: Dict[str, Any] = {}


class ConvertUriRequest(BaseModel):
    """Request model for URI conversion."""
    uri: str
    config: Optional[MarkDownConfig] = None