# AIKit.MarkItDown

[![NuGet Version](https://img.shields.io/nuget/v/AIKit.MarkItDown)](https://www.nuget.org/packages/AIKit.MarkItDown/)
[![.NET](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/)

**Effortlessly convert files to Markdown in your .NET applications** — A powerful C# wrapper around Microsoft's markitdown library that brings advanced file conversion capabilities to .NET developers.

## What It Offers

AIKit.MarkItDown is a production-ready .NET library that enables seamless integration of file-to-Markdown conversion into your applications. It wraps Microsoft's markitdown Python library, providing:

- **Wide Format Support**: Convert PDFs, DOCX, PPTX, XLSX, images (with OCR), audio files (with transcription), web pages, and more to clean Markdown
- **AI-Powered Features**: Optional integration with Azure Document Intelligence for advanced document processing and OpenAI for LLM-enhanced image descriptions
- **Thread-Safe Design**: Isolated worker process prevents Python GIL issues in multi-threaded .NET apps
- **Multiple Integration Options**: Use as a direct library, REST API server, or .NET client
- **Extensible Plugin System**: Add custom converters for specialized formats
- **Enterprise-Ready**: Docker deployment, health checks, and robust error handling

## Why It's Useful

In today's content-rich applications, processing diverse file formats is essential. AIKit.MarkItDown solves common challenges:

- **Unified Output**: Convert any supported file to standardized Markdown, perfect for RAG systems, content management, or document processing pipelines
- **AI Enhancement**: Leverage LLMs for richer content extraction (e.g., describe images, transcribe audio) without managing Python dependencies yourself
- **Performance**: Worker process isolation ensures your .NET app remains responsive during conversions
- **Developer Experience**: Pure C# API with async support, configuration validation, and comprehensive error messages
- **Cost-Effective**: Free and open-source, with optional paid services (Azure/OpenAI) only when needed

Whether you're building a document indexing system, content aggregator, or AI-powered application, AIKit.MarkItDown provides the file processing backbone without the complexity of managing Python in .NET.

## How It Works

AIKit.MarkItDown uses a hybrid C#/Python architecture to safely embed Python functionality in .NET applications. Below are the architecture diagrams for both integration options.

### Option 1: Direct Library Integration Architecture

```
┌─────────────────┐    JSON over stdin/stdout    ┌─────────────────┐
│   .NET App      │ ──────────────────────────► │   Worker         │
│ (Your Code)     │                             │ Process          │
│                 │ ◄────────────────────────── │                 │
└─────────────────┘                             └─────────────────┘
                                                   │
                                                   ▼
                                            ┌─────────────────┐
                                            │   Python         │
                                            │   markitdown     │
                                            │   Library        │
                                            └─────────────────┘
```

**Flow**:

1. Your .NET code calls `MarkDownConverter.Convert()`
2. Worker process (isolated .NET executable) receives JSON request via stdin/stdout
3. Python runtime initializes using pythonnet, imports markitdown
4. Conversion happens in Python with AI processing if configured
5. Result flows back as JSON, deserialized to C# objects

**Benefits**: Maximum performance, tight integration, full control over configuration.

### Option 2: Separate Server Deployment Architecture

```
┌─────────────────┐    HTTP/REST API    ┌─────────────────┐
│   Any Client    │ ──────────────────► │   Python Server  │
│ (.NET, JS, etc.)│                     │   (FastAPI)      │
│                 │ ◄────────────────── │                 │
└─────────────────┘                     └─────────────────┘
                                           │
                                           ▼
                                    ┌─────────────────┐
                                    │   markitdown    │
                                    │   Library       │
                                    │   (Direct)      │
                                    └─────────────────┘
```

**Flow**:

1. Client sends HTTP request with file/URL to Python server (FastAPI)
2. Server directly imports and uses markitdown library
3. Conversion happens in Python with AI processing if configured
4. Server returns Markdown result via HTTP response
5. Client receives standardized JSON response

**Benefits**: Scalable, language-agnostic, centralized service, easier monitoring.

### Key Design Decisions

- **Worker Isolation**: Python's Global Interpreter Lock (GIL) can block threads in embedded scenarios. By running conversions in a separate process, your main app stays responsive
- **JSON Serialization**: Simple, reliable communication between C# and worker without complex IPC
- **Static Initialization**: Python setup happens once per worker lifetime for efficiency
- **Error Translation**: Python exceptions are caught and re-thrown as descriptive C# exceptions

Both architectures ensure thread safety, reliability, and performance while providing different deployment flexibility.

## Installation

### Quick Install (NuGet)

Add to your .NET project:

`ash
dotnet add package AIKit.MarkItDown
`

That's it! The library handles Python dependency detection automatically.

### Prerequisites

- **.NET 10.0 SDK** or higher
- **Python 3.8+** installed and accessible via PATH (library auto-detects python, python3, or py)
- Optional: Azure CLI for Document Intelligence, OpenAI API key for LLM features

### From Source (Development)

1. Clone and navigate:
   `ash
git clone https://github.com/your-repo/AIKit.MarkItDown.git
cd AIKit.MarkItDown/src/AIKit.MarkItDown
`

2. Install Python dependencies:
   `powershell
.\install.ps1
`
   This installs markitdown[all], openai, zure-ai-documentintelligence, and a sample plugin.

3. Build:
   `ash
dotnet build
`

The install script verifies Python version and installs all required packages via pip.

## Integration Options

AIKit.MarkItDown offers **two main ways** to integrate file conversion into your applications:

### Option 1: Direct Library Integration (NuGet Package)

**Best for**: Monolithic applications, tight coupling, maximum performance

- Add the NuGet package to your .NET project
- Call conversion methods directly from your code
- Worker process runs alongside your application
- Full control over configuration and error handling

### Option 2: Separate Server Deployment (REST API)

**Best for**: Microservices, distributed systems, multiple consumers, centralized processing

- Deploy the server as a separate service (Docker/Kubernetes)
- Call via HTTP REST API from any client
- Centralized conversion service for multiple applications
- Easier scaling and monitoring

Choose based on your architecture needs - both options provide the same conversion capabilities with identical output.

## Quick Start

### For Direct Library Usage

Convert your first file in 3 lines:

```csharp
using AIKit.MarkItDown;

var converter = new MarkDownConverter();
string markdown = converter.Convert("sample.pdf");
Console.WriteLine(markdown);
```

**Output Example:**

```markdown
# Sample Document

This is a PDF converted to Markdown.

## Features

- Preserves headings
- Extracts text content
- Handles basic formatting
```

Start the server:

```bash
cd src/AIKit.MarkItDown.Server
docker run -d -p 8000:8000 markitdown-server
```

Convert via HTTP:

```bash
curl -X POST -F "file=@document.pdf" http://localhost:8000/convert
```

## Usage Examples

### Direct Library Integration

#### Basic File Conversion

```csharp
// Local files
string pdfMarkdown = converter.Convert("document.pdf");
string docxMarkdown = converter.Convert("report.docx");

// Streams (useful for uploaded files)
using var stream = File.OpenRead("presentation.pptx");
string pptxMarkdown = converter.Convert(stream, "pptx");

// URLs (web pages, YouTube videos, etc.)
string webMarkdown = converter.ConvertUri("https://example.com/article");
```

#### Advanced Features

```csharp
var config = new MarkDownConfig
{
    // AI-powered image descriptions
    OpenAiApiKey = "sk-...",
    LlmModel = "gpt-4o",
    LlmPrompt = "Describe this image in detail for accessibility",

    // Advanced document processing
    DocIntelEndpoint = "https://...",
    DocIntelKey = "...",

    // Preserve images as data URIs
    KeepDataUris = true,

    // Enable custom plugins
    EnablePlugins = true,
    Plugins = new List<string> { "my_custom_plugin" }
};

string enhancedMarkdown = await converter.ConvertAsync("rich-document.pdf", config);
```

#### Async Operations

```csharp
// Non-blocking conversion
string markdown = await converter.ConvertAsync("large-file.pdf");

// With cancellation
using var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromMinutes(5));
string markdown = await converter.ConvertAsync("file.pdf", config, cts.Token);
```

#### Error Handling

```csharp
try
{
    string markdown = converter.Convert("file.pdf");
}
catch (MarkItDownConversionException ex)
{
    Console.WriteLine($"Conversion failed: {ex.Message}");
    // Handle specific error types
}
```

### Separate Server Deployment

#### Client-Server Usage

For distributed setups, use the client library:

```csharp
using AIKit.MarkItDown.Client;

var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:8000") };
var client = new MarkItDownClient(httpClient);

var result = await client.ConvertAsync("document.pdf");
string markdown = result.Text;
```

#### REST API Direct Calls

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

## API Reference

### MarkDownConverter (Core Class)

```csharp
public class MarkDownConverter
{
    // Constructors
    public MarkDownConverter()
    public MarkDownConverter(DocIntelConfig? docIntel = null, OpenAIConfig? openAi = null)

    // Synchronous methods
    public string Convert(string filePath)
    public string Convert(Stream stream, string extension)
    public string ConvertUri(string uri)
    public string Convert(string filePath, MarkDownConfig? config)
    public string Convert(Stream stream, string extension, MarkDownConfig? config)
    public string ConvertUri(string uri, MarkDownConfig? config)

    // Asynchronous methods
    public Task<string> ConvertAsync(string filePath, MarkDownConfig? config = null, CancellationToken cancellationToken = default)
    public Task<string> ConvertUriAsync(string uri, MarkDownConfig? config = null, CancellationToken cancellationToken = default)
    public Task<string> ConvertAsync(Stream stream, string extension, MarkDownConfig? config = null, CancellationToken cancellationToken = default)

    // Validation
    public static void ValidateConfigRequirements(MarkDownConfig config)
}
```

### MarkDownConfig

```csharp
public class MarkDownConfig
{
    // Azure Document Intelligence
    public DocIntelConfig? DocIntel { get; set; }

    // OpenAI Configuration
    public OpenAIConfig? OpenAI { get; set; }

    // LLM Settings
    public string? LlmModel { get; set; } = "gpt-4o";
    public string? LlmPrompt { get; set; }

    // Output Options
    public bool? KeepDataUris { get; set; } = false;

    // Plugin System
    public bool? EnablePlugins { get; set; } = false;
    public List<string> Plugins { get; set; } = new();
}
```

### DocIntelConfig & OpenAIConfig

```csharp
public class DocIntelConfig
{
    public string? Endpoint { get; set; }
    public string? Key { get; set; }
}

public class OpenAIConfig
{
    public string? ApiKey { get; set; }
    public string? Model { get; set; }
}
```

### MarkItDownClient

```csharp
public class MarkItDownClient : IDisposable
{
    public MarkItDownClient(HttpClient httpClient, ILogger<MarkItDownClient>? logger = null)

    // File conversion methods
    public Task<string> ConvertAsync(Stream fileStream, string fileName, string? extension = null, MarkDownConfig? config = null)
    public Task<MarkDownResult> ConvertAsync(string filePath, MarkDownConfig? config = null)
    public Task<MarkDownResult> ConvertAsync(Stream stream, string extension, MarkDownConfig? config = null)

    // URI conversion
    public Task<string> ConvertUriAsync(string uri, MarkDownConfig? config = null)
}
```

## Configuration

### Basic Setup

Most conversions work without configuration. For AI features:

```csharp
var config = new MarkDownConfig
{
    // Azure Document Intelligence (for advanced PDF/Word processing)
    DocIntel = new DocIntelConfig
    {
        Endpoint = Environment.GetEnvironmentVariable("DOCINTEL_ENDPOINT"),
        Key = Environment.GetEnvironmentVariable("DOCINTEL_KEY")
    },

    // OpenAI (for LLM image descriptions, audio transcription)
    OpenAI = new OpenAIConfig
    {
        ApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY"),
        Model = "gpt-4o"
    },

    LlmPrompt = "Describe this image for accessibility purposes",
    KeepDataUris = true,
    EnablePlugins = true
};
```

### Environment Variables

Set these securely (never hardcode):

**For C# Library:**

- `DOCINTEL_ENDPOINT` - Azure Document Intelligence endpoint
- `DOCINTEL_KEY` - Azure Document Intelligence key
- `OPENAI_API_KEY` - OpenAI API key

**For Python Server:**

- `DOCINTEL_ENDPOINT` - Azure Document Intelligence endpoint
- `DOCINTEL_KEY` - Azure Document Intelligence key
- `OPENAI_API_KEY` - OpenAI API key
- `OPENAI_MODEL` - OpenAI model (default: gpt-4o)
- `LLM_PROMPT` - Custom LLM prompt

### Plugin Configuration

Enable custom plugins:

```csharp
var config = new MarkDownConfig
{
    EnablePlugins = true,
    Plugins = new List<string> { "my_plugin", "another_plugin" }
};
```

Plugins are Python modules that extend markitdown's conversion capabilities.

## Deployment

### Local Development

Use .NET Aspire for orchestrated development:

1. Open AIKit.MarkItDown.slnx in Visual Studio/VS Code
2. Run AIKit.MarkItDown.AppHost
3. Server starts on http://localhost:8000

### Docker (Production)

`ash
cd src/AIKit.MarkItDown.Server
docker build -t markitdown-server .
docker run -d -p 8000:8000 \
  -e AZURE_DOC_INTELLIGENCE_ENDPOINT="..." \
  -e AZURE_DOC_INTELLIGENCE_KEY="..." \
  -e OPENAI_API_KEY="..." \
  markitdown-server
`

### Docker Compose

`yaml
version: '3.8'
services:
  markitdown:
    build: ./src/AIKit.MarkItDown.Server
    ports:
      - "8000:8000"
    environment:
      - AZURE_DOC_INTELLIGENCE_ENDPOINT=
      - AZURE_DOC_INTELLIGENCE_KEY=
      - OPENAI_API_KEY=
`

### Production Considerations

- **Security**: Use secrets management for API keys, never commit them
- **Scaling**: Worker processes are lightweight; monitor resource usage
- **File Limits**: Default 100MB limit; adjust based on your needs
- **Logging**: Enable structured logging for troubleshooting
- **Health Checks**: Server includes /health endpoint for monitoring

## Troubleshooting

### Python Not Found

**Error**: "Python executable not found"

**Solution**:

- Install Python 3.8+ from python.org
- Ensure python or python3 is in PATH
- On Windows, check registry detection or set PYTHONHOME

### DLL Mismatch

**Error**: "Unable to load pythonXY.dll"

**Solution**:

- Verify pythonnet 3.0.5 matches your Python version
- Reinstall: dotnet add package pythonnet --version 3.0.5
- Check Python installation integrity

### GIL Issues

**Error**: Thread blocking or deadlocks

**Solution**:

- Use async methods for non-blocking operations
- Avoid long conversions on UI threads
- Worker process handles GIL isolation automatically

### Conversion Failures

**Common Issues**:

- Unsupported file format (check markitdown docs)
- Corrupted files
- Missing AI service credentials
- Network timeouts for URLs

**Debug**:

- Enable logging: ILogger in client/server
- Check worker process output
- Validate file accessibility

### Server Issues

**Container Problems**:

- Ensure Docker has required system deps: fmpeg, esseract-ocr
- Check logs: docker logs <container>
- Verify port availability

**API Errors**:

- Validate request format
- Check file size limits
- Review server logs for detailed errors

## Contributing

We welcome contributions! Here's how to get started:

1. **Fork** the repository
2. **Clone** your fork: git clone https://github.com/your-username/AIKit.MarkItDown.git
3. **Create** a feature branch: git checkout -b feature/my-awesome-feature
4. **Install** dependencies: Run install.ps1 in src/AIKit.MarkItDown
5. **Test** your changes: dotnet test
6. **Submit** a pull request

### Development Setup

- Use Visual Studio 2022+ or VS Code with C# extensions
- Install .NET Aspire workload: dotnet workload install aspire
- Run tests before submitting: dotnet test --verbosity normal

### Guidelines

- Follow existing code patterns (async/await, error handling)
- Add unit tests for new features
- Update documentation for API changes
- Use meaningful commit messages

For major changes, please open an issue first to discuss.

## TODO

- **Test OpenAI Integration**: The solution has not been fully tested with OpenAI client features due to lack of API credentials. Requires valid OpenAI API key for testing LLM-powered image descriptions and other AI features.
- **Test Azure Document Intelligence**: Azure Document Intelligence integration is not tested due to missing Azure credentials. Requires Azure subscription and Document Intelligence resource for testing advanced document processing features.
- **Publish NuGet Package**: Package the AIKit.MarkItDown library and publish to NuGet.org for public consumption.
- **Publish Docker Image**: Build and publish the Docker image for the server component to a container registry (Docker Hub, GitHub Container Registry, etc.).
- **CI/CD Pipeline**: Implement automated testing pipeline that includes Python environment setup and credential management for full integration testing.
