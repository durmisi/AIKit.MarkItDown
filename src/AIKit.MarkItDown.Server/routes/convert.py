from fastapi import APIRouter, UploadFile, File, HTTPException, Form
from fastapi.responses import Response
import io
import logging
from models import MarkDownConfig
from typing import Optional
from utils import build_conversion_kwargs
from converter import md
from constants import MAX_FILE_SIZE

# Configure logging
logger = logging.getLogger(__name__)

router = APIRouter()

@router.post("/convert", tags=["Conversion"], summary="Convert file to Markdown")
async def convert_file(
    file: UploadFile = File(...), 
    extension: str = Form(None),
    config: Optional[MarkDownConfig] = None
):
    """Convert an uploaded file to Markdown format.
    
    Args:
        file: The file to convert.
        extension: Optional file extension override.
        config: Optional configuration for the conversion.
        
    Returns:
        Markdown content as plain text response.
        
    Raises:
        HTTPException: For validation errors or conversion failures.
    """
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
            kwargs.update(build_conversion_kwargs(config))
        
        result = md.convert_stream(stream, file_extension=file_extension, **kwargs)
        logger.info(f"Conversion successful for file: {file.filename}")
        
        return Response(content=result.text_content, media_type="text/markdown")
    except ValueError as e:
        logger.warning(f"Validation error for file {file.filename}: {str(e)}")
        raise HTTPException(status_code=400, detail=str(e))
    except Exception as e:
        logger.error(f"Conversion failed for file {file.filename}: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Conversion failed: {str(e)}")
    finally:
        await file.close()
