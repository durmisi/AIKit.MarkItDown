# Use ARG for platform to avoid constant value warning
ARG PLATFORM=linux/amd64

# Build stage
FROM --platform=$PLATFORM python:3.13-slim-bullseye AS builder

# Install build dependencies
RUN apt-get update && apt-get install -y \
    build-essential \
    ffmpeg \
    exiftool \
    tesseract-ocr \
    poppler-utils \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Copy requirements and install Python dependencies
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Runtime stage
FROM --platform=$PLATFORM python:3.13-slim-bullseye

# Install runtime system dependencies
RUN apt-get update && apt-get install -y \
    ffmpeg \
    exiftool \
    tesseract-ocr \
    poppler-utils \
    && rm -rf /var/lib/apt/lists/*

# Copy installed Python packages from builder
COPY --from=builder /usr/local/lib/python3.13/site-packages /usr/local/lib/python3.13/site-packages
COPY --from=builder /usr/local/bin /usr/local/bin

# Set working directory
WORKDIR /app

# Set environment variables to disable GPU providers for onnxruntime
ENV ORT_PROVIDERS="CPUExecutionProvider"

# Copy the application code
COPY main.py ./main.py
COPY routes ./routes
COPY constants.py ./constants.py
COPY models.py ./models.py
COPY utils.py ./utils.py
COPY config.py ./config.py
COPY converter.py ./converter.py
COPY auth.py ./auth.py

# Expose the port
EXPOSE 8000

# Run the application
CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8000"]