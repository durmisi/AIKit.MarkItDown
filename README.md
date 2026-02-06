# AIKit.MarkItDown

[![NuGet Version](https://img.shields.io/nuget/v/AIKit.MarkItDown)](https://www.nuget.org/packages/AIKit.MarkItDown/)
[![.NET](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/)

A C# wrapper around the Python [markitdown](https://github.com/microsoft/markitdown) library, enabling seamless conversion of various file formats to Markdown. This project uses pythonnet to embed Python runtime in .NET applications, providing a hybrid C#/Python architecture for robust file processing.

## Overview

AIKit.MarkItDown allows developers to integrate advanced file-to-Markdown conversion capabilities into .NET applications. It supports a wide range of formats including documents (PDF, DOCX, PPTX, XLSX), images (with OCR), audio (with transcription), web content, and more. The library leverages Microsoft's markitdown library's features like Azure Document Intelligence, OpenAI LLM integration, and a plugin system.

Key capabilities:

- **File Conversion**: Convert local files, streams, or URLs to Markdown
- **Advanced Features**: OCR, speech transcription, LLM image descriptions
- **Integration Options**: Direct library usage, REST API server, or .NET client
- **Plugin Support**: Extensible with custom plugins
- **Thread-Safe**: Process isolation ensures safe concurrent usage

## Architecture

The solution uses a hybrid C#/Python architecture:

- **Core Library** (`AIKit.MarkItDown`): C# API surface with configuration and error handling
- **Worker Process** (`AIKit.MarkItDown.Worker`): Isolated .NET executable that manages Python runtime using pythonnet
- **Server** (`AIKit.MarkItDown.Server`): Python FastAPI server for REST API access
- **Client** (`AIKit.MarkItDown.Client`): C# client library for server communication

The worker process approach isolates Python GIL management and runtime issues from the main application, ensuring thread safety and reliability.

## Prerequisites

- **.NET 10.0 SDK** or higher
- **Python 3.8** or higher (automatically detected from PATH or common locations)
- **Docker** (optional, for containerized server deployment)
- **Azure CLI** (optional, for Azure Document Intelligence features)
- **OpenAI API Key** (optional, for LLM-powered features)

## Installation

### NuGet Package

Install the library in your .NET project:

```bash
dotnet add package AIKit.MarkItDown
```

Or using Package Manager:

```powershell
Install-Package AIKit.MarkItDown
```

### Docker Image

For server deployment:

```bash
cd src/AIKit.MarkItDown.Server
docker build -t markitdown-server .
docker run -d -p 8000:8000 markitdown-server
```

### From Source

1. Clone the repository:

   ```bash
   git clone https://github.com/your-repo/AIKit.MarkItDown.git
   cd AIKit.MarkItDown
   ```

2. Install Python dependencies:

   ```powershell
   cd src/AIKit.MarkItDown
   .\install.ps1
   ```

3. Build the solution:
   ```bash
   dotnet build
   ```

## Configuration

Configure conversion behavior using the `MarkDownConfig` class:

```csharp
using AIKit.MarkItDown;

var config = new MarkDownConfig
{
    DocIntelEndpoint = "https://your-doc-intel-endpoint.cognitiveservices.azure.com/",
    DocIntelKey = "your-doc-intel-key",
    OpenAiApiKey = "your-openai-api-key",
    LlmModel = "gpt-4o",
    LlmPrompt = "Custom prompt for LLM features",
    KeepDataUris = true,
    EnablePlugins = true,
    Plugins = new List<string> { "custom_plugin" }
};
```

### Configuration Properties

- `DocIntelEndpoint`: Azure Document Intelligence endpoint URL
- `DocIntelKey`: Azure Document Intelligence authentication key
- `OpenAiApiKey`: OpenAI API key for LLM features
- `LlmModel`: LLM model name (e.g., "gpt-4o")
- `LlmPrompt`: Custom prompt for LLM processing
- `KeepDataUris`: Preserve image data URIs in output
- `EnablePlugins`: Enable third-party plugins
- `Plugins`: List of plugin module names to load

Validate configuration requirements:

```csharp
MarkDownConverter.ValidateConfigRequirements(config);
```

**Note**: Some features (like plugins) are currently library-only and not fully supported in the server API.

## Usage

### Direct Library Usage

```csharp
using AIKit.MarkItDown;

// Basic conversion
var converter = new MarkDownConverter();
string markdown = converter.Convert("path/to/file.pdf");


var docIntelConfig = new DocIntelConfig
{
    Endpoint = "https://your-doc-intel-endpoint.cognitiveservices.azure.com/",
    Key = "your-doc-intel-key"
};
var openAiConfig = new OpenAIConfig
{
    ApiKey = "your-openai-api-key",
    Model = "gpt-4o"
};
var converter = new MarkDownConverter(docIntelConfig, openAiConfig);
string markdown = converter.Convert("path/to/file.pdf");

var config = new MarkDownConfig { KeepDataUris = true, LlmPrompt = "Custom prompt" };
string markdown = await converter.ConvertAsync("path/to/file.pdf", config);

// Convert streams
using (var stream = File.OpenRead("file.pdf"))
{
    string markdown = converter.Convert(stream, "pdf", config);
}

// Convert URLs
string markdown = converter.ConvertUri("https://www.youtube.com/watch?v=example", config);

// Async operations
string markdown = await converter.ConvertAsync("file.pdf", config);
```

### API Client Usage

```csharp
using AIKit.MarkItDown.Client;

var client = new MarkItDownClient(httpClient, logger);

// Convert file
var result = await client.ConvertAsync("path/to/file.pdf");
string markdown = result.Text;

// With configuration
var config = new MarkDownConfig { KeepDataUris = true };
var result = await client.ConvertAsync("path/to/file.pdf", config);

// Extension override
var result = await client.ConvertAsync("path/to/file", "pdf", config);
```

### Server API Usage

The server provides REST endpoints for remote conversion:

- `POST /convert`: Convert uploaded files
- `POST /convert_uri`: Convert URLs/URIs

Example using curl:

```bash
# Convert file
curl -X POST -F "file=@document.pdf" http://localhost:8000/convert

# Convert URI
curl -X POST -H "Content-Type: application/json" \
  -d '{"uri": "https://example.com"}' \
  http://localhost:8000/convert_uri
```

See [Server README](src/AIKit.MarkItDown.Server/README.md) for complete API documentation.

## Testing

Run the test suite (requires Python environment):

```bash
dotnet test
```

Tests include unit tests for the core library, integration tests for the worker, and E2E tests for the client/server. Sample test files are provided in `TestShared/files/`.

## Deployment

### Local Development with Aspire

Use .NET Aspire for orchestrated local development:

1. Open the solution in Visual Studio or VS Code
2. Run the `AIKit.MarkItDown.AppHost` project
3. The server starts on port 8000

### Docker Deployment

```bash
cd src/AIKit.MarkItDown.Server
docker build -t markitdown-server .
docker run -d -p 8000:8000 markitdown-server
```

Or using Docker Compose:

```bash
docker compose up --build
```

Environment variables:

- `PORT`: Server port (default: 8000)

### Production Considerations

- Ensure Python dependencies are installed in the deployment environment
- Configure Azure/OpenAI credentials securely (avoid hardcoding)
- Monitor worker process resource usage in high-concurrency scenarios

## Troubleshooting

### Python Detection Issues

If Python is not found:

- Ensure Python 3.8+ is installed and in PATH
- On Windows, check common locations: `C:\Python38\`, `C:\Python39\`, etc.
- Run `python --version` or `python3 --version` to verify

### DLL Mismatch Errors

- Verify pythonnet version (3.0.5) matches Python version
- Ensure `pythonXY.dll` exists in Python installation directory
- Reinstall pythonnet if needed: `dotnet add package pythonnet --version 3.0.5`

### GIL-Related Issues

- All Python operations must occur within `Py.GIL()` context
- Avoid long-running operations on the main thread
- Use async methods for non-blocking conversions

### Conversion Failures

- Check file format support and file accessibility
- Validate configuration for required services (Azure Doc Intel, OpenAI)
- Review worker process logs for detailed error messages

### Server Issues

- Verify Docker container has required system dependencies (ffmpeg, tesseract-ocr)
- Check server logs: `docker logs markitdown-server`
- Ensure port 8000 is available

## Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Ensure all tests pass
5. Submit a pull request

For major changes, please open an issue first to discuss the proposed changes.
