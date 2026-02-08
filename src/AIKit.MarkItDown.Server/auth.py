"""API key authentication module."""

from fastapi import Depends, HTTPException
from fastapi.security import APIKeyHeader
from config import API_KEY

# API key header
api_key_header = APIKeyHeader(name="x-api-key", auto_error=False)

def get_api_key(api_key: str = Depends(api_key_header)):
    """Dependency to validate API key if configured.

    If API_KEY is set in environment, requires valid X-API-Key header.
    If not set, allows anonymous access.
    """
    if API_KEY:
        if not api_key or api_key != API_KEY:
            raise HTTPException(status_code=401, detail="Invalid or missing API Key")
    # If no API_KEY configured, allow anonymous access