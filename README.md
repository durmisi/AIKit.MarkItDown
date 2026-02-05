# AIKit.MarkItDown

[![.NET](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/)

A C# wrapper around the Python [markitdown](https://github.com/microsoft/markitdown) library, enabling seamless conversion of various file formats (PDF, DOCX, PPTX, XLSX, images, audio, HTML) to Markdown. This project uses pythonnet to embed Python runtime in .NET applications.

## Features

- Convert multiple file formats to Markdown
- Hybrid C#/Python architecture for robust file processing
- Supports both direct library usage and server-based API
- Easy integration with .NET applications

## Prerequisites

- .NET 10.0 SDK
- Python 3.8 or higher (automatically detected)
- Docker (optional, for containerized server)

## Installation

### As NuGet Package

1. Build and pack the package:

   ```bash
   cd src/AIKit.MarkItDown
   dotnet pack --configuration Release
   ```

2. Install locally or publish to NuGet feed:
   ```bash
   dotnet nuget push bin/Release/AIKit.MarkItDown.1.0.0.nupkg --source <your-feed>
   ```

Then, in your project:

```bash
dotnet add package AIKit.MarkItDown --version 1.0.0
```

### As Docker Image

1. Build the Docker image:

   ```bash
   cd src/AIKit.MarkItDown.Server
   docker build -t markitdown-server .
   ```

2. Run the container:
   ```bash
   docker run -d -p 8000:8000 markitdown-server
   ```

## Usage

### Direct Library Usage

```csharp
using AIKit.MarkItDown;

var converter = new MarkDownConverter();
string markdown = await converter.ConvertAsync("path/to/file.pdf");
```

### API Client Usage

```csharp
using AIKit.MarkItDown.Client;

var client = new MarkItDownClient("http://localhost:8000");
var response = await client.ConvertFileAsync("path/to/file.pdf");
string markdown = response.Markdown;
```

## Development

1. Clone the repository.
2. Install Python dependencies: Run `src/AIKit.MarkItDown/install.ps1`.
3. Build the solution: `dotnet build`.
4. Run tests: `dotnet test` (requires Python environment).
5. For local development with Aspire: Run `src/start.ps1`.

## Contributing

Contributions are welcome! Please submit issues and pull requests.
