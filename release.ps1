# PowerShell-Skript zum Veröffentlichen der Noteflow-App als eigenständige Windows-Exe

$ErrorActionPreference = 'Stop'

Write-Host "Starte Release-Build für Windows (self-contained, single file)..."

dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true

Write-Host "Fertig! Die ausführbare Datei findest du unter: bin\Release\net9.0\win-x64\publish\Noteflow.exe" -ForegroundColor Green
