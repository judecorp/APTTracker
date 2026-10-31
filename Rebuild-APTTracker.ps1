# Rebuild-APTTracker.ps1

param(
    [Parameter()][ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"

# Stop HDT if running
$hdtProcess = Get-Process -Name "HearthstoneDeckTracker" -ErrorAction SilentlyContinue
if ($hdtProcess) {
    Write-Host "Stopping HearthstoneDeckTracker..."
    Stop-Process -Name "HearthstoneDeckTracker" -Force
    Start-Sleep -Seconds 2
}

# Build the project
Write-Host "Building APTTracker ($Configuration)..."
Set-Location $PSScriptRoot
dotnet build APTTracker.csproj --configuration $Configuration --no-incremental
if ($LASTEXITCODE -ne 0) {
    throw "Build failed (exit code $LASTEXITCODE). Plugin deployment and host restart were skipped."
}

# Copy to plugins folder
$pluginsPath = "$env:APPDATA\HearthstoneDeckTracker\Plugins"
$targetDll = Join-Path $pluginsPath "APTTracker.dll"
$sourceDll = Join-Path "bin\$Configuration" "APTTracker.dll"
Copy-Item $sourceDll $targetDll -Force
Write-Host "Copied to $targetDll"

$hdtDir = "$env:LOCALAPPDATA\HearthstoneDeckTracker"
# Restart HDT
Write-Host "Starting HearthstoneDeckTracker..."
Start-Process "$hdtDir\HearthstoneDeckTracker.exe"

Write-Host "Done!"
