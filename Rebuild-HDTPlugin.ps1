# Rebuild-HDTPlugin.ps1

$ErrorActionPreference = "Stop"

# Stop HDT if running
$hdtProcess = Get-Process -Name "HearthstoneDeckTracker" -ErrorAction SilentlyContinue
if ($hdtProcess) {
    Write-Host "Stopping HearthstoneDeckTracker..."
    Stop-Process -Name "HearthstoneDeckTracker" -Force
    Start-Sleep -Seconds 2
}

# Build the project
Write-Host "Building HDTPlugin..."
Set-Location $PSScriptRoot
dotnet build -c Debug

# Copy to plugins folder
$pluginsPath = "$env:APPDATA\HearthstoneDeckTracker\Plugins"
$targetDll = Join-Path $pluginsPath "HDTPlugin.dll"
Copy-Item "bin\Debug\HDTPlugin.dll" $targetDll -Force
Write-Host "Copied to $targetDll"

# Copy debug settings if exists
if (Test-Path "debugSettings.json") {
    Copy-Item "debugSettings.json" $pluginsPath -Force
    Write-Host "Copied debugSettings.json"
}

# Restart HDT
Write-Host "Starting HearthstoneDeckTracker..."
Start-Process "$env:LOCALAPPDATA\HearthstoneDeckTracker\HearthstoneDeckTracker.exe"

Write-Host "Done!"
