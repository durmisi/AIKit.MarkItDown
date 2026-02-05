from fastapi import FastAPI, Request
from fastapi.responses import JSONResponse
import logging
import sys
import threading

from routes.convert import router
from routes.convert_uri import router as uri_router

# Set sys.excepthook for main thread uncaught exceptions (e.g., startup)
def custom_sys_excepthook(exc_type, exc_value, exc_traceback):
    logger.critical(f"Uncaught exception in main thread: {exc_value}", exc_info=(exc_type, exc_value, exc_traceback))
    sys.exit(1)  # Graceful exit to prevent hanging

sys.excepthook = custom_sys_excepthook

# Set threading.excepthook for thread exceptions
def custom_threading_excepthook(args):
    logger.error(f"Uncaught exception in thread {args.thread}: {args.exc_value}", exc_info=args.exc_value)

threading.excepthook = custom_threading_excepthook

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

@app.get("/", tags=["General"])
async def root():
    return {"message": "Hello from the MarkItDown Server!"}

@app.get("/health", tags=["Health"])
async def health():
    return {"status": "healthy"}

app.include_router(router)
app.include_router(uri_router)
