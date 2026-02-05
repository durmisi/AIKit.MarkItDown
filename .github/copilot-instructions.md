# Project Overview

AIKit.MarkItDown is a C# wrapper around the Python `markitdown` library for converting various file formats (PDF, DOCX, etc.) to Markdown. It uses pythonnet to embed Python runtime in .NET.

## Architecture

- **Core Component**: `MarkDownConverter` class handles Python initialization and file conversion
- **Python Integration**: Static constructor detects Python executable and DLL path, initializes PythonEngine
- **Conversion Flow**: Uses Py.GIL() context to import `markitdown` and call `convert()` method
- **Error Handling**: Wraps PythonException in C# Exception with descriptive messages

## Dependencies & Setup

- **NuGet**: `pythonnet` (3.0.5) for Python embedding
- **Python**: Requires Python 3.8+ with `markitdown[all]` installed
- **Installation**: Run `src/AIKit.MarkItDown/install.ps1` to install Python dependencies
- **Runtime Detection**: Automatically finds Python via `python`, `python3`, or `py` commands

## Build & Test Workflow

- **Build**: Standard `dotnet build` in `src/` directory
- **Test**: `dotnet test` requires Python environment; tests use xUnit with ITestOutputHelper for output
- **Debug**: Ensure Python DLL path is correctly detected; check console for initialization errors
- **CI/CD**: Build matrix should include Python installation step

## Code Patterns

- **Python Runtime Management**: Always use `using (Py.GIL())` for thread-safe Python operations
- **Exception Translation**: Catch `PythonException` and re-throw as `Exception` with context
- **Static Initialization**: Python setup happens once in static constructor
- **Process Execution**: Use `Process` class with redirected output for Python command detection

## Key Files

- `MarkDownConverter.cs`: Main conversion logic and Python integration
- `install.ps1`: Python dependency installation script
- `MarkDownConverterTests.cs`: Unit tests with file conversion examples

## Common Pitfalls

- Python not found: Ensure `python`/`python3`/`py` is in PATH
- DLL mismatch: Version detection may fail; verify `pythonXY.dll` exists in Python directory
- GIL not acquired: All Python operations must be within `Py.GIL()` scope
- Missing dependencies: Run `install.ps1` before testing or running
