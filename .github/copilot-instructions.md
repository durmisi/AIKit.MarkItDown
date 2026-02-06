# Project Overview

AIKit.MarkItDown is a C# wrapper around the Python `markitdown` library for converting various file formats (PDF, DOCX, etc.) to Markdown. It uses pythonnet to embed Python runtime in .NET, with alternative Python-based server for cross-platform compatibility.

## Architecture

- **Core Component**: `MarkDownConverter` class handles file conversion via Worker or Server
- **Worker (Embedding)**: C# executable embeds Python runtime using pythonnet; detects Python DLL, initializes PythonEngine, uses Py.GIL() for thread-safe operations
- **Server (Standalone)**: Python FastAPI server (`main.py`) for direct markitdown usage; Client library (`MarkItDownClient.cs`) for HTTP communication
- **Data Flow**: Files -> MarkDownConverter -> Worker/Server -> Markdown output; Worker for Windows/.NET embedding, Server for Linux/Python-native
- **Service Boundaries**: Worker isolates Python embedding; Server provides REST API; Client abstracts communication

## Dependencies & Setup

- **NuGet**: `pythonnet` (3.0.5) for embedding; `Microsoft.Extensions.Http` for Client
- **Python**: 3.10+ required; install via `src/AIKit.MarkItDown/install.ps1` (cross-platform PowerShell script)
- **Runtime Detection**: Worker auto-finds Python exe/DLL; prefers `python3` on Linux, `py` on Windows
- **Integration Points**: pythonnet for C#/Python interop; HTTP for Client-Server; external libs like openai, azure-ai-documentintelligence

## Build & Test Workflow

- **Build**: `dotnet build src/` copies Worker exe to output; `src/start.ps1` launches AppHost
- **Test**: `dotnet test` requires Python setup; Worker tests embed Python, Client tests mock HTTP
- **Debug**: Check Worker console for Python init errors; use `python -c "import markitdown"` to verify
- **CI/CD**: Run `install.ps1` before tests; ensure Python 3.10+ in environment

## Code Patterns

- **Python Embedding**: Use `Py.Import("markitdown")` then `md_module.GetAttr("MarkItDown")`; always in `using (Py.GIL())`
- **Exception Handling**: Wrap `PythonException` in custom `MarkItDownConversionException`
- **Cross-Platform**: Detect OS for DLL paths (libpython.so on Linux); set `PYTHONNET_RUNTIME=coreclr` on non-Windows
- **Async Patterns**: Worker uses `Process.Start` with stdin/stdout; Client uses `HttpClient` with JSON

## Key Files

- `MarkDownConverter.cs`: Orchestrates conversion, chooses Worker/Server
- `Program.cs` (Worker): Embeds Python, imports markitdown
- `main.py` (Server): FastAPI routes for conversion
- `MarkItDownClient.cs`: HTTP client for Server
- `install.ps1`: Cross-platform Python setup

## Common Pitfalls

- **Python Version**: Must be 3.10+; install.ps1 checks and installs markitdown[all]
- **DLL Detection**: On Linux, falls back to common lib dirs or ldd; ensure python3-dev installed
- **GIL Management**: All Python calls in Py.GIL() block; no threading without BeginAllowThreads()
- **Path Issues**: Worker exe copied to output; relative paths from assembly location
