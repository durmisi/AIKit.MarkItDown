"""Route for URI conversion endpoint."""

from fastapi import APIRouter, HTTPException
from fastapi.responses import Response
import logging
from models import  ConvertUriRequest
from utils import build_conversion_kwargs, merge_configs
from config import default_config
from converter import md

# Configure logging
logger = logging.getLogger(__name__)

router = APIRouter()


@router.post("/convert_uri", tags=["Conversion"], summary="Convert URI to Markdown")
async def convert_uri(request: ConvertUriRequest):
    """Convert a URI to Markdown format.

    Args:
        request: The request containing URI and optional config (overrides defaults from .env).

    Returns:
        Markdown content as plain text response.

    Raises:
        HTTPException: For validation errors or conversion failures.
    """
    logger.info(f"Convert URI endpoint called with URI: {request.uri}")

    try:
        # Merge default config with request config
        effective_config = merge_configs(default_config, request.config)

        kwargs = {}
        if effective_config:
            kwargs.update(build_conversion_kwargs(effective_config))

        result = md.convert(request.uri, **kwargs)
        logger.info(f"URI conversion successful for: {request.uri}")

        return Response(content=result.text_content, media_type="text/markdown")
    except ValueError as e:
        logger.warning(f"Validation error for URI {request.uri}: {str(e)}")
        raise HTTPException(status_code=400, detail=str(e))
    except Exception as e:
        logger.error(f"URI conversion failed for {request.uri}: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Conversion failed: {str(e)}")

