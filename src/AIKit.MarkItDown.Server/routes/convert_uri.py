from fastapi import APIRouter, HTTPException
from fastapi.responses import Response
from markitdown import MarkItDown
import openai
import logging
from models import MarkDownConfig, ConvertUriRequest
from typing import Optional

# Configure logging
logger = logging.getLogger(__name__)

# Initialize MarkItDown
md = MarkItDown(enable_plugins=True)

router = APIRouter()

@router.post("/convert_uri", tags=["Conversion"], summary="Convert URI to Markdown")
async def convert_uri(request: ConvertUriRequest):
    logger.info(f"Convert URI endpoint called with URI: {request.uri}")
    
    try:
        kwargs = {}
        if request.config:
            config = request.config
            
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
        
        result = md.convert(request.uri, **kwargs)
        logger.info(f"URI conversion successful for: {request.uri}")
        
        return Response(content=result.text_content, media_type="text/markdown")
    except Exception as e:
        logger.error(f"URI conversion failed for {request.uri}: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Conversion failed: {str(e)}")