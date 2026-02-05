# AIKit.MarkItDown.Server

This is the Python FastAPI server for the AIKit.MarkItDown project, providing file-to-Markdown conversion via HTTP API.

## Local Development (Aspire)

This server is integrated with the .NET Aspire environment. To run locally with Aspire:

1. Open the solution in Visual Studio or VS Code.
2. Run the `AIKit.MarkItDown.AppHost` project.

The server will start on port 8000 via Uvicorn.

## Docker Deployment

To run the server in a Docker container:

### Using PowerShell Script

Run the provided script for easy build and start:

```powershell
.\build-and-run.ps1
```

This script will build the image, stop any existing container, and start a new one.

### Manual Commands

#### Build the Image

```bash
docker build -t markitdown-server .
```

#### Run the Container

```bash
docker run -d --name markitdown-server -p 8000:8000 markitdown-server
```

Or using Docker Compose:

```bash
docker compose up --build
```

### Environment Variables

- `PORT`: Port to run the server on (default: 8000)

### Testing

- Health check: `GET http://localhost:8000/health`
- Convert file: `POST http://localhost:8000/convert` with multipart/form-data file upload

The server supports various file formats including PDF, DOCX, PPTX, XLSX, images, audio, and HTML.
