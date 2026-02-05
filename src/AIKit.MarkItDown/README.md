# AIKit.MarkItDown

[![NuGet Version](https://img.shields.io/nuget/v/AIKit.MarkItDown)](https://www.nuget.org/packages/AIKit.MarkItDown/)
[![.NET](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/)

A C# wrapper around the Python [markitdown](https://github.com/microsoft/markitdown) library, enabling seamless conversion of various file formats to Markdown. This package uses pythonnet to embed Python runtime in .NET applications.

## Features

- Convert multiple file formats to Markdown
- Advanced features: OCR, speech transcription, LLM image descriptions
- Azure Document Intelligence integration
- Plugin system support
- Hybrid C#/Python architecture for robust file processing
- Automatic Python environment detection and setup
- Easy integration with .NET applications

## Supported Formats

- **Documents**: PDF, DOCX, PPTX, XLSX
- **Images**: PNG, JPG, JPEG, GIF, BMP, TIFF, WEBP (with EXIF metadata and OCR)
- **Audio**: MP3, WAV, M4A, FLAC (with EXIF metadata and speech transcription)
- **Web**: HTML, HTM
- **Text**: TXT, CSV, JSON, XML
- **Archives**: ZIP (iterates over contents)
- **URLs**: YouTube, and other web URLs
- **EBooks**: EPUB
- **Emails**: MSG
- And more...

## Prerequisites

- .NET 10.0 or higher
- Python 3.8 or higher (automatically detected from PATH)
- For advanced features: Azure CLI (for Doc Intel), OpenAI API key (for LLM features)

## Installation

Install the package via NuGet:

```bash
dotnet add package AIKit.MarkItDown
```

Or using Package Manager:

```powershell
Install-Package AIKit.MarkItDown
```

### Python Dependencies

Run the installation script to set up Python dependencies:

```powershell
.\install.ps1
```

This installs markitdown with all optional dependencies for full feature support.

## Usage

### Basic Usage

```csharp
using AIKit.MarkItDown;

// Create converter instance
var converter = new MarkDownConverter();

// Convert a file to markdown
string markdown = converter.Convert("path/to/your/file.pdf");

Console.WriteLine(markdown);
```

### Advanced Usage with Configuration

```csharp
using AIKit.MarkItDown;

// Configure advanced features
var config = new MarkDownConfig
{
    DocIntelEndpoint = "https://your-doc-intel-endpoint.cognitiveservices.azure.com/",
    DocIntelKey = "your-doc-intel-key",
    LlmModel = "gpt-4o",
    KeepDataUris = true,
    EnablePlugins = true
};

// Convert with config
string markdown = converter.Convert("document.pdf", config);
```

### Convert Streams

```csharp
using (var stream = File.OpenRead("file.pdf"))
{
    string markdown = converter.Convert(stream, "pdf");
}
```

### Convert URLs

```csharp
string markdown = converter.ConvertUri("https://www.youtube.com/watch?v=example");
```

### Error Handling

```csharp
using AIKit.MarkItDown;

try
{
    var converter = new MarkDownConverter();
    string markdown = converter.Convert("file.pdf");
    // Process result
}
catch (MarkItDownConversionException ex)
{
    Console.WriteLine($"Conversion failed: {ex.Message}");
}
```

### Async Operations

```csharp
// All convert methods have async equivalents
string markdown = await converter.ConvertAsync("file.pdf", config);
string streamMarkdown = await converter.ConvertAsync(stream, "pdf", config);
string uriMarkdown = await converter.ConvertAsyncUri("https://example.com", config);
```

### Creating OpenAI Client

```csharp
var config = new MarkDownConfig
{
    OpenAiApiKey = "your-api-key",
    LlmModel = "gpt-4o"
};
```

The OpenAI client will be created automatically when needed.

### Configuration Validation

```csharp
// Validate that required packages are installed for config
MarkDownConverter.ValidateConfigRequirements(config);
```

## API Reference

### MarkDownConfig Class

- `string? DocIntelEndpoint` - Azure Document Intelligence endpoint
- `string? OpenAiApiKey` - OpenAI API key for LLM features
- `string? LlmModel` - LLM model name (e.g., "gpt-4o")
- `string? LlmPrompt` - Custom prompt for LLM features
- `bool KeepDataUris` - Preserve image data URIs
- `bool EnablePlugins` - Enable 3rd-party plugins
- `string? DocIntelKey` - Azure Doc Intel authentication key
- `List<string> Plugins` - List of plugin module names

### MarkDownConverter Class

- `MarkDownConverter()` - Constructor with automatic Python detection
- `string Convert(string filePath)` - Convert file to markdown
- `string Convert(string filePath, MarkDownConfig config)` - Convert with config
- `string Convert(Stream stream, string extension, MarkDownConfig config = null)` - Convert stream
- `string ConvertUri(string uri, MarkDownConfig config = null)` - Convert URL
- `Task<string> ConvertAsync(string filePath)` - Async file conversion
- `Task<string> ConvertAsync(string filePath, MarkDownConfig config)` - Async file conversion with config
- `Task<string> ConvertAsync(Stream stream, string extension, MarkDownConfig config = null)` - Async stream conversion
- `Task<string> ConvertAsyncUri(string uri, MarkDownConfig config = null)` - Async URI conversion
- `static PyObject CreateOpenAiClient(string apiKey)` - Create Python OpenAI client
- `static void ValidateConfigRequirements(MarkDownConfig config)` - Validate config dependencies

## Notes

- The package automatically handles Python environment setup
- First conversion may take longer due to Python initialization
- Ensure Python and required packages are installed via `install.ps1`
- For Azure/OpenAI features, configure credentials securely
- For server-based usage, check the full project repository

## License

[Specify license here]</content>
<parameter name="filePath">c:\Projects\AIKit.MarkItDown\README.md
