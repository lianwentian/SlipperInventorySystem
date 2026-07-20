# Slipper Inventory System Setup Script for Windows (PowerShell)
# This script will setup the entire project and generate EXE

Write-Host "======================================" -ForegroundColor Green
Write-Host "Slipper Inventory System Setup" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Green
Write-Host ""

# Check if .NET 8 SDK is installed
Write-Host "Checking .NET 8 SDK installation..." -ForegroundColor Yellow
$dotnetVersion = & dotnet --version 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: .NET 8 SDK is not installed!" -ForegroundColor Red
    Write-Host "Please download and install from: https://dotnet.microsoft.com/en-us/download/dotnet/8.0" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}
Write-Host "[OK] .NET 8 SDK is installed: $dotnetVersion" -ForegroundColor Green
Write-Host ""

# Restore NuGet packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
& dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to restore NuGet packages" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}
Write-Host "[OK] NuGet packages restored" -ForegroundColor Green
Write-Host ""

# Build the project
Write-Host "Building project..." -ForegroundColor Yellow
& dotnet build -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to build project" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}
Write-Host "[OK] Project built successfully" -ForegroundColor Green
Write-Host ""

# Publish the application
Write-Host "Publishing application (this may take 2-3 minutes)..." -ForegroundColor Yellow
& dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to publish application" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}
Write-Host "[OK] Application published successfully" -ForegroundColor Green
Write-Host ""

Write-Host "======================================" -ForegroundColor Green
Write-Host "Build Completed Successfully!" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Green
Write-Host ""
Write-Host "The executable is located at: publish\SlipperIS.UI.exe" -ForegroundColor Cyan
Write-Host ""
Write-Host "Default Login Accounts:" -ForegroundColor Yellow
Write-Host "- Admin: admin / admin123" -ForegroundColor White
Write-Host "- Sales: sales / sales123" -ForegroundColor White
Write-Host "- Warehouse: warehouse / warehouse123" -ForegroundColor White
Write-Host ""
Write-Host "To run the application, double-click: publish\SlipperIS.UI.exe" -ForegroundColor Green
Write-Host ""
Read-Host "Press Enter to exit"
