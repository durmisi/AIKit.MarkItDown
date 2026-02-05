from fastapi import FastAPI, UploadFile, File, HTTPException, Request, Form
from fastapi.responses import JSONResponse
from markitdown import MarkItDown
import openai
import io
import os
import logging
import sys
import threading
from pydantic import BaseModel
from typing import Optional, Dict, Any, List
from models import MarkDownConfig, MarkDownResult, ConvertUriRequest

# Set sys.excepthook for main thread uncaught exceptions (e.g., startup)
def custom_sys_excepthook(exc_type, exc_value, exc_traceback):
    logger.critical(f"Uncaught exception in main thread: {exc_value}", exc_info=(exc_type, exc_value, exc_traceback))
    sys.exit(1)  # Graceful exit to prevent hanging

sys.excepthook = custom_sys_excepthook

# Set threading.excepthook for thread exceptions
def custom_threading_excepthook(args):
    logger.error(f"Uncaught exception in thread {args.thread}: {args.exc_value}", exc_info=args.exc_value)

threading.excepthook = custom_threading_excepthook

# Constants
MAX_FILE_SIZE = 200 * 1024 * 1024  # 200MB limit

# Configure logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(name)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

app = FastAPI(
    title="AIKit MarkItDown API",
    description="API for converting various file formats to Markdown using the MarkItDown library",
    version="1.0.0"
)

# Global exception handler for unhandled exceptions
@app.exception_handler(Exception)
async def global_exception_handler(request: Request, exc: Exception):
    logger.error(f"Unhandled exception: {exc}", exc_info=True)  # Logs full traceback
    return JSONResponse(
        status_code=500,
        content={"detail": "Internal server error. Please try again later."}
    )

# Middleware for exception handling
@app.middleware("http")
async def exception_handling_middleware(request: Request, call_next):
    try:
        response = await call_next(request)
        return response
    except Exception as exc:
        logger.error(f"Exception in middleware: {exc}", exc_info=True)
        return JSONResponse(
            status_code=500,
            content={"detail": "Internal server error."}
        )

# Initialize MarkItDown
logger.info("Initializing MarkItDown converter")
md = MarkItDown(enable_plugins=True)

@app.get("/", tags=["General"])
async def root():
    return {"message": "Hello from the MarkItDown Server!"}

@app.get("/health", tags=["Health"])
async def health():
    return {"status": "healthy"}

@app.post("/convert", tags=["Conversion"], summary="Convert file to Markdown")
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
        
        metadata = {}
        try:
            metadata = dict(result.metadata) if hasattr(result, 'metadata') and result.metadata else {}
        except:
            pass
        
        return MarkDownResult(
            text=result.text_content,
            title=getattr(result, 'title', None),
            metadata=metadata
        )
    except Exception as e:
        logger.error(f"Conversion failed for file {file.filename}: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Conversion failed: {str(e)}")
    finally:
        await file.close()

@app.post("/convert_uri", tags=["Conversion"], summary="Convert URI to Markdown")
async def convert_uri(request: ConvertUriRequest):
    logger.info(f"Convert URI endpoint called with URI: {request.uri}")
    
    try:
        kwargs = {}
        if request.config:
            config = request.config
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
        
        result = md.convert(request.uri, **kwargs)
        logger.info(f"URI conversion successful for: {request.uri}")
        
        metadata = {}
        try:
            metadata = dict(result.metadata) if hasattr(result, 'metadata') and result.metadata else {}
        except:
            pass
        
        return MarkDownResult(
            text=result.text_content,
            title=getattr(result, 'title', None),
            metadata=metadata
        )
    except Exception as e:
        logger.error(f"URI conversion failed for {request.uri}: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Conversion failed: {str(e)}")