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
msbuild HDTPlugin.csproj /p:Configuration=Debug /t:Rebuild

# Copy to plugins folder
$pluginsPath = "$env:APPDATA\HearthstoneDeckTracker\Plugins"
$targetDll = Join-Path $pluginsPath "HDTPlugin.dll"
Copy-Item "bin\Debug\HDTPlugin.dll" $targetDll -Force
Write-Host "Copied to $targetDll"

# Copy MahApps IconPacks DLLs
$iconPacksCore = Join-Path $pluginsPath "MahApps.Metro.IconPacks.Core.dll"
$iconPacksMaterial = Join-Path $pluginsPath "MahApps.Metro.IconPacks.Material.dll"
Copy-Item "bin\Debug\MahApps.Metro.IconPacks.Core.dll" $iconPacksCore -Force
Copy-Item "bin\Debug\MahApps.Metro.IconPacks.Material.dll" $iconPacksMaterial -Force
Write-Host "Copied IconPacks DLLs"

# Copy debug settings if exists
if (Test-Path "debugSettings.json") {
    Copy-Item "debugSettings.json" $pluginsPath -Force
    Write-Host "Copied debugSettings.json"
}

$hdtDir = "$env:LOCALAPPDATA\HearthstoneDeckTracker"
# Restart HDT
Write-Host "Starting HearthstoneDeckTracker..."
Start-Process "$hdtDir\HearthstoneDeckTracker.exe"

Write-Host "Done!"
