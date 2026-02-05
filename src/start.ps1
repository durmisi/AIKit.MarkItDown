Write-Host "Starting Aspire application..."
Set-Location $PSScriptRoot\AIKit.MarkItDown.AppHost
Write-Host "Changed directory to: $(Get-Location)"
Write-Host "Running dotnet run..."
dotnet run