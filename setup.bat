@echo off
REM Slipper Inventory System Setup Script for Windows (Batch)
REM This script will setup the entire project and generate EXE

echo ======================================
echo Slipper Inventory System Setup
echo ======================================
echo.

REM Check if .NET 8 SDK is installed
echo Checking .NET 8 SDK installation...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET 8 SDK is not installed!
    echo Please download and install from: https://dotnet.microsoft.com/en-us/download/dotnet/8.0
    pause
    exit /b 1
)

echo [OK] .NET 8 SDK is installed
echo.

REM Restore NuGet packages
echo Restoring NuGet packages...
dotnet restore
if errorlevel 1 (
    echo ERROR: Failed to restore NuGet packages
    pause
    exit /b 1
)
echo [OK] NuGet packages restored
echo.

REM Build the project
echo Building project...
dotnet build -c Release
if errorlevel 1 (
    echo ERROR: Failed to build project
    pause
    exit /b 1
)
echo [OK] Project built successfully
echo.

REM Publish the application
echo Publishing application...
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish
if errorlevel 1 (
    echo ERROR: Failed to publish application
    pause
    exit /b 1
)
echo [OK] Application published successfully
echo.

echo ======================================
echo Build Completed Successfully!
echo ======================================
echo.
echo The executable is located at: publish\SlipperIS.UI.exe
echo.
echo Default Login Accounts:
echo - Admin: admin / admin123
echo - Sales: sales / sales123
echo - Warehouse: warehouse / warehouse123
echo.
pause
