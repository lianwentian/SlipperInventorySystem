#!/bin/bash
# Slipper Inventory System Setup Script for Linux/Mac
# This script will setup the entire project and generate executable

echo "======================================"
echo "Slipper Inventory System Setup"
echo "======================================"
echo ""

# Check if .NET 8 SDK is installed
echo "Checking .NET 8 SDK installation..."
if ! command -v dotnet &> /dev/null; then
    echo "ERROR: .NET 8 SDK is not installed!"
    echo "Please download and install from: https://dotnet.microsoft.com/en-us/download/dotnet/8.0"
    exit 1
fi

echo "[OK] .NET 8 SDK is installed"
echo ""

# Restore NuGet packages
echo "Restoring NuGet packages..."
dotnet restore
if [ $? -ne 0 ]; then
    echo "ERROR: Failed to restore NuGet packages"
    exit 1
fi
echo "[OK] NuGet packages restored"
echo ""

# Build the project
echo "Building project..."
dotnet build -c Release
if [ $? -ne 0 ]; then
    echo "ERROR: Failed to build project"
    exit 1
fi
echo "[OK] Project built successfully"
echo ""

# Publish the application
echo "Publishing application..."
dotnet publish -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true -o publish
if [ $? -ne 0 ]; then
    echo "ERROR: Failed to publish application"
    exit 1
fi
echo "[OK] Application published successfully"
echo ""

echo "======================================"
echo "Build Completed Successfully!"
echo "======================================"
echo ""
echo "The executable is located at: publish/SlipperIS.UI"
echo ""
echo "Default Login Accounts:"
echo "- Admin: admin / admin123"
echo "- Sales: sales / sales123"
echo "- Warehouse: warehouse / warehouse123"
echo ""
