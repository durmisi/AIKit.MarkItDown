# PowerShell script to install Python dependencies for AIKit.MarkItDown
#https://github.com/microsoft/markitdown

# Check if Python is available
$pythonCmd = "python3"
$isWindows = $IsWindows -or ([System.Environment]::OSVersion.Platform -eq "Win32NT")
if ($isWindows) {
    $pythonCmd = "python"
}
try {
    $pythonVersion = & $pythonCmd -c "import sys; print(f'{sys.version_info.major}.{sys.version_info.minor}')" 2>$null
    if ($LASTEXITCODE -ne 0) { throw "Command failed" }
} catch {
    if ($isWindows) {
        # Try to install Python using winget
        if (Get-Command winget -ErrorAction SilentlyContinue) {
            Write-Host "Python is not installed or not working. Installing Python 3.10+ using winget..."
            try {
                winget install --id Python.Python.3.10 --accept-source-agreements --accept-package-agreements
                # Refresh environment variables
                $env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")
                # Try again
                $pythonVersion = & $pythonCmd -c "import sys; print(f'{sys.version_info.major}.{sys.version_info.minor}')"
                if ($LASTEXITCODE -ne 0) { throw "Still failed" }
            } catch {
                Write-Host "Failed to install or run Python. Please install Python 3.10+ manually from https://www.python.org/"
                exit 1
            }
        } else {
            Write-Host "Python is not installed and winget is not available. Please install Python 3.10+ from https://www.python.org/"
            exit 1
        }
    } else {
        Write-Host "python3 is not installed or not in PATH. Please install Python 3.10+ (e.g., sudo apt install python3 python3-pip python3-dev)"
        exit 1
    }
}

# Check Python version
$pythonVersion = & $pythonCmd -c "import sys; print(f'{sys.version_info.major}.{sys.version_info.minor}')"
$versionParts = $pythonVersion -split '\.'
$major = [int]$versionParts[0]
$minor = [int]$versionParts[1]
if ($major -lt 3 -or ($major -eq 3 -and $minor -lt 10)) {
    Write-Host "Python 3.10+ is required. Current version: $pythonVersion"
    exit 1
}

# Install markitdown with all optional dependencies
# MarkItDown optional dependencies include:
# - [all]: All optional dependencies
# - [pptx]: PowerPoint files
# - [docx]: Word files
# - [xlsx]: Excel files
# - [xls]: Older Excel files
# - [pdf]: PDF files
# - [outlook]: Outlook messages
# - [az-doc-intel]: Azure Document Intelligence
# - [audio-transcription]: Audio transcription
# - [youtube-transcription]: YouTube transcription
#
# For OpenAI support, install openai separately: pip install openai
# For Azure Document Intelligence, use [az-doc-intel] or install azure-ai-documentintelligence
Write-Host "Installing markitdown[all]..."
& $pythonCmd -m pip install "markitdown[all]"

# Install additional packages for LLM and Document Intelligence features
Write-Host "Installing OpenAI package..."
& $pythonCmd -m pip install openai

Write-Host "Installing Azure Document Intelligence package..."
& $pythonCmd -m pip install azure-ai-documentintelligence

if ($LASTEXITCODE -eq 0) {
    Write-Host "Installation completed successfully."
} else {
    Write-Host "Installation failed."
    exit 1
}