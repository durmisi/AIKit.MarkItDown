"""Main FastAPI application for the MarkItDown server."""

from fastapi import FastAPI, Request
from fastapi.responses import JSONResponse
import logging
import sys
import threading

from routes.convert import router
from routes.convert_uri import router as uri_router
from constants import VERSION


def custom_sys_excepthook(exc_type, exc_value, exc_traceback):
    """Custom exception hook for main thread uncaught exceptions.

    Logs the exception and exits gracefully to prevent hanging.

    Args:
        exc_type: The exception type.
        exc_value: The exception value.
        exc_traceback: The traceback object.
    """
    logger.critical(f"Uncaught exception in main thread: {exc_value}", exc_info=(exc_type, exc_value, exc_traceback))
    sys.exit(1)  # Graceful exit to prevent hanging


def custom_threading_excepthook(args):
    """Custom exception hook for thread uncaught exceptions.

    Logs the exception in the thread.

    Args:
        args: The exception arguments from threading.
    """
    logger.error(f"Uncaught exception in thread {args.thread}: {args.exc_value}", exc_info=args.exc_value)


# Configure logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(name)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

app = FastAPI(
    title="AIKit MarkItDown API",
    description="API for converting various file formats to Markdown using the MarkItDown library",
    version=VERSION
)


async def global_exception_handler(request: Request, exc: Exception):
    """Global exception handler for unhandled exceptions in the API.

    Logs the full traceback and returns a 500 error response.

    Args:
        request: The incoming request.
        exc: The exception that occurred.

    Returns:
        JSONResponse: A 500 internal server error response.
    """
    logger.error(f"Unhandled exception: {exc}", exc_info=True)  # Logs full traceback
    return JSONResponse(
        status_code=500,
        content={"detail": "Internal server error. Please try again later."}
    )


async def exception_handling_middleware(request: Request, call_next):
    """Middleware for handling exceptions in HTTP requests.

    Catches any exceptions during request processing and returns a 500 error.

    Args:
        request: The incoming request.
        call_next: The next middleware or endpoint handler.

    Returns:
        Response: The response from the next handler or an error response.
    """
    try:
        response = await call_next(request)
        return response
    except Exception as exc:
        logger.error(f"Exception in middleware: {exc}", exc_info=True)
        return JSONResponse(
            status_code=500,
            content={"detail": "Internal server error."}
        )


async def root():
    """Root endpoint that returns a welcome message.

    Returns:
        dict: A dictionary with a welcome message.
    """
    return {"message": "Hello from the MarkItDown Server!"}


async def health():
    """Health check endpoint.

    Returns:
        dict: A dictionary with health status and version.
    """
    return {"status": "healthy", "version": VERSION}

app.include_router(router)
app.include_router(uri_router)

app.add_api_route("/", root, methods=["GET"])
app.add_api_route("/health", health, methods=["GET"])
