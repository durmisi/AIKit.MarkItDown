from fastapi import FastAPI, UploadFile, File, HTTPException
from markitdown import MarkItDown
import io
import os

app = FastAPI()

# Initialize MarkItDown
md = MarkItDown()

@app.get("/")
async def root():
    return {"message": "Hello from FastAPI in Aspire"}

@app.get("/health")
async def health():
    return {"status": "healthy"}

@app.post("/convert")
async def convert_file(file: UploadFile = File(...)):
    if not file.filename:
        raise HTTPException(status_code=400, detail="No file provided")
    
    try:
        content = await file.read()
        if len(content) > 200 * 1024 * 1024:  # 200MB limit
            raise HTTPException(status_code=413, detail="File too large. Maximum size is 200MB.")
        
        file_extension = file.filename.split('.')[-1].lower() if '.' in file.filename else None
        
        # Use BytesIO for stream
        stream = io.BytesIO(content)
        result = md.convert_stream(stream, file_extension=file_extension)
        
        return {"filename": file.filename, "markdown": result.text_content}
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Conversion failed: {str(e)}")
    finally:
        await file.close()