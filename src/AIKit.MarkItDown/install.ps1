# PowerShell script to install Python dependencies for AIKit.MarkItDown

# Check if Python is available via 'py' launcher
if (!(Get-Command py -ErrorAction SilentlyContinue)) {
    Write-Host "Python is not installed or not in PATH. Please install Python 3.8+ from https://www.python.org/"
    exit 1
}

# Install markitdown with all optional dependencies
Write-Host "Installing markitdown[all]..."
py -m pip install "markitdown[all]"

if ($LASTEXITCODE -eq 0) {
    Write-Host "Installation completed successfully."
} else {
    Write-Host "Installation failed."
    exit 1
}