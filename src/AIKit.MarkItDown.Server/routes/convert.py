from fastapi import APIRouter, UploadFile, File, HTTPException, Form
from fastapi.responses import Response
from markitdown import MarkItDown
import openai
import io
import logging
from models import MarkDownConfig
from typing import Optional

# Constants
MAX_FILE_SIZE = 200 * 1024 * 1024  # 200MB limit

# Configure logging
logger = logging.getLogger(__name__)

# Initialize MarkItDown
md = MarkItDown(enable_plugins=True)

router = APIRouter()

@router.post("/convert", tags=["Conversion"], summary="Convert file to Markdown")
async def convert_file(
    file: UploadFile = File(...), 
    extension: str = Form(None),
    config: Optional[MarkDownConfig] = None
):
    logger.info(f"Convert endpoint called with file: {file.filename}, extension: {extension}")
    if not file.filename:
        logger.warning("No file provided in convert request")
        raise HTTPException(status_code=400, detail="No file provided")
    
    try:
        content = await file.read()
        file_size_mb = len(content) / (1024 * 1024)
        logger.info(f"File size: {file_size_mb:.2f} MB")
        if len(content) > MAX_FILE_SIZE:
            logger.warning(f"File too large: {file_size_mb:.2f} MB")
            raise HTTPException(status_code=413, detail=f"File too large. Maximum size is {MAX_FILE_SIZE / (1024 * 1024)}MB.")
        
        file_extension = extension or (file.filename.split('.')[-1].lower() if '.' in file.filename else None)
        logger.info(f"File extension: {file_extension}")
        
        # Use BytesIO for stream
        stream = io.BytesIO(content)
        kwargs = {}
        if file_extension == 'pdf':
            kwargs['check_extractable'] = False
        if config:
            if config.docintel_endpoint:
                kwargs['docintel_endpoint'] = config.docintel_endpoint
            if config.llm_model:
                kwargs['llm_model'] = config.llm_model
            if config.llm_prompt:
                kwargs['llm_prompt'] = config.llm_prompt
            if config.keep_data_uris is not None:
                kwargs['keep_data_uris'] = config.keep_data_uris
            if config.enable_plugins is not None:
                kwargs['enable_plugins'] = config.enable_plugins
            if config.docintel_key:
                kwargs['docintel_key'] = config.docintel_key
            if config.llm_api_key and config.llm_model:
                client = openai.OpenAI(api_key=config.llm_api_key)
                kwargs['llm_client'] = client
        
        result = md.convert_stream(stream, file_extension=file_extension, **kwargs)
        logger.info(f"Conversion successful for file: {file.filename}")
        
        return Response(content=result.text_content, media_type="text/markdown")
    except Exception as e:
        logger.error(f"Conversion failed for file {file.filename}: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Conversion failed: {str(e)}")
    finally:
        await file.close()