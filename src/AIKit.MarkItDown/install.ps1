# PowerShell script to install Python dependencies for AIKit.MarkItDown

# Check if Python is available via 'py' launcher
if (!(Get-Command py -ErrorAction SilentlyContinue)) {
    Write-Host "Python is not installed or not in PATH. Please install Python 3.8+ from https://www.python.org/"
    exit 1
}

# Check Python version
$pythonVersion = py -c "import sys; print(f'{sys.version_info.major}.{sys.version_info.minor}')"
$versionParts = $pythonVersion -split '\.'
$major = [int]$versionParts[0]
$minor = [int]$versionParts[1]
if ($major -lt 3 -or ($major -eq 3 -and $minor -lt 8)) {
    Write-Host "Python 3.8+ is required. Current version: $pythonVersion"
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
py -m pip install "markitdown[all]"

# Install additional packages for LLM and Document Intelligence features
Write-Host "Installing OpenAI package..."
py -m pip install openai

Write-Host "Installing Azure Document Intelligence package..."
py -m pip install azure-ai-documentintelligence

# Install sample plugin for demonstration
Write-Host "Installing markitdown-sample-plugin..."
py -m pip install markitdown-sample-plugin

if ($LASTEXITCODE -eq 0) {
    Write-Host "Installation completed successfully."
} else {
    Write-Host "Installation failed."
    exit 1
}