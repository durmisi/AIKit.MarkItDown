# AIKit.MarkItDown

[![NuGet Version](https://img.shields.io/nuget/v/AIKit.MarkItDown)](https://www.nuget.org/packages/AIKit.MarkItDown/)
[![.NET](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/)

A C# wrapper around the Python [markitdown](https://github.com/microsoft/markitdown) library, enabling seamless conversion of various file formats to Markdown. This package uses pythonnet to embed Python runtime in .NET applications.

## Features

- Convert multiple file formats to Markdown
- Hybrid C#/Python architecture for robust file processing
- Automatic Python environment detection and setup
- Easy integration with .NET applications
- Async/await support

## Supported Formats

- **Documents**: PDF, DOCX, PPTX, XLSX
- **Images**: PNG, JPG, JPEG, GIF, BMP, TIFF, WEBP
- **Audio**: MP3, WAV, M4A, FLAC
- **Web**: HTML, HTM
- **Text**: TXT, CSV, JSON, XML
- **Archives**: ZIP (with markdown content)
- And more...

## Prerequisites

- .NET 10.0 or higher
- Python 3.8 or higher (automatically detected from PATH or common installation locations)

## Installation

Install the package via NuGet:

```bash
dotnet add package AIKit.MarkItDown
```

Or using Package Manager:

```powershell
Install-Package AIKit.MarkItDown
```

## Usage

### Basic Usage

```csharp
using AIKit.MarkItDown;

// Create converter instance
var converter = new MarkDownConverter();

// Convert a file to markdown
string markdown = await converter.ConvertAsync("path/to/your/file.pdf");

Console.WriteLine(markdown);
```

### With Custom Python Path

```csharp
using AIKit.MarkItDown;

// Specify Python executable path if needed
var converter = new MarkDownConverter("path/to/python.exe");

string markdown = await converter.ConvertAsync("document.docx");
```

### Error Handling

```csharp
using AIKit.MarkItDown;

try
{
    var converter = new MarkDownConverter();
    string markdown = await converter.ConvertAsync("file.pdf");
    // Process markdown
}
catch (Exception ex)
{
    Console.WriteLine($"Conversion failed: {ex.Message}");
}
```

## API Reference

### MarkDownConverter Class

- `MarkDownConverter()` - Constructor with automatic Python detection
- `MarkDownConverter(string pythonPath)` - Constructor with explicit Python path
- `Task<string> ConvertAsync(string filePath)` - Convert file to markdown asynchronously

## Notes

- The package automatically handles Python environment setup
- First conversion may take longer due to Python initialization
- Ensure Python and required packages are installed (see project repository for details)
- For server-based usage, check the full project repository

## License

[Specify license here]</content>
<parameter name="filePath">c:\Projects\AIKit.MarkItDown\README.md
