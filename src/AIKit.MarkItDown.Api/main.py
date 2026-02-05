from fastapi import FastAPI, UploadFile, File, HTTPException
from markitdown import MarkItDown
import io
import os
import logging

# Configure logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(name)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

app = FastAPI()

# Initialize MarkItDown
logger.info("Initializing MarkItDown converter")
md = MarkItDown()

@app.get("/")
async def root():
    return {"message": "Hello from FastAPI in Aspire"}

@app.get("/health")
async def health():
    return {"status": "healthy"}

@app.post("/convert")
async def convert_file(file: UploadFile = File(...)):
    logger.info(f"Convert endpoint called with file: {file.filename}")
    if not file.filename:
        logger.warning("No file provided in convert request")
        raise HTTPException(status_code=400, detail="No file provided")
    
    try:
        content = await file.read()
        file_size_mb = len(content) / (1024 * 1024)
        logger.info(f"File size: {file_size_mb:.2f} MB")
        if len(content) > 200 * 1024 * 1024:  # 200MB limit
            logger.warning(f"File too large: {file_size_mb:.2f} MB")
            raise HTTPException(status_code=413, detail="File too large. Maximum size is 200MB.")
        
        file_extension = file.filename.split('.')[-1].lower() if '.' in file.filename else None
        logger.info(f"File extension: {file_extension}")
        
        # Use BytesIO for stream
        stream = io.BytesIO(content)
        result = md.convert_stream(stream, file_extension=file_extension)
        logger.info(f"Conversion successful for file: {file.filename}")
        
        return {"filename": file.filename, "markdown": result.text_content}
    except Exception as e:
        logger.error(f"Conversion failed for file {file.filename}: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Conversion failed: {str(e)}")
    finally:
        await file.close()