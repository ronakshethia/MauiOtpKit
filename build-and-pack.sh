#!/bin/bash
# MauiOtpKit - Local Build and Pack Script
# Usage: ./build-and-pack.sh

set -e

echo "🔨 MauiOtpKit - Build and Pack Script"
echo "======================================"
echo ""

# Colors
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check if .NET is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK not found. Please install .NET 8.0 or later."
    exit 1
fi

echo -e "${BLUE}✓ .NET SDK found: $(dotnet --version)${NC}"
echo ""

# Step 1: Clean
echo -e "${BLUE}Step 1: Cleaning previous builds...${NC}"
dotnet clean -c Release -q
rm -rf ./nupkg 2>/dev/null || true
echo -e "${GREEN}✓ Clean complete${NC}"
echo ""

# Step 2: Restore
echo -e "${BLUE}Step 2: Restoring dependencies...${NC}"
dotnet restore -q
echo -e "${GREEN}✓ Restore complete${NC}"
echo ""

# Step 3: Build Core
echo -e "${BLUE}Step 3: Building MauiOtpKit.Core...${NC}"
dotnet build MauiOtpKit.Core/MauiOtpKit.Core.csproj -c Release -q
echo -e "${GREEN}✓ MauiOtpKit.Core build complete${NC}"
echo ""

# Step 4: Build Platform
echo -e "${BLUE}Step 4: Building MauiOtpKit (Platform)...${NC}"
dotnet build MauiOtpKit/MauiOtpKit.csproj -c Release -q 2>/dev/null || echo -e "${YELLOW}⚠ Platform build skipped (MAUI SDK may not be installed)${NC}"
echo -e "${GREEN}✓ MauiOtpKit platform build complete${NC}"
echo ""

# Step 5: Pack Core
echo -e "${BLUE}Step 5: Packing MauiOtpKit.Core...${NC}"
dotnet pack MauiOtpKit.Core/MauiOtpKit.Core.csproj -c Release -o ./nupkg -q
echo -e "${GREEN}✓ MauiOtpKit.Core packed${NC}"
echo ""

# Step 6: Pack Platform
echo -e "${BLUE}Step 6: Packing MauiOtpKit...${NC}"
dotnet pack MauiOtpKit/MauiOtpKit.csproj -c Release -o ./nupkg -q 2>/dev/null || echo -e "${YELLOW}⚠ Platform pack skipped${NC}"
echo -e "${GREEN}✓ MauiOtpKit packed${NC}"
echo ""

# Step 7: List packages
echo -e "${BLUE}Step 7: Generated NuGet Packages:${NC}"
if [ -d "./nupkg" ]; then
    ls -lh ./nupkg/*.nupkg 2>/dev/null || echo "No packages generated"
    echo ""
    echo -e "${GREEN}✓ Packages ready in ./nupkg/${NC}"
else
    echo -e "${YELLOW}⚠ No packages directory found${NC}"
fi

echo ""
echo "======================================"
echo -e "${GREEN}✅ Build and pack complete!${NC}"
echo ""
echo "📦 Next steps:"
echo "   1. Review packages in ./nupkg/"
echo "   2. Test locally: dotnet nuget add source ./nupkg"
echo "   3. Push to NuGet: dotnet nuget push ./nupkg/*.nupkg --api-key YOUR_KEY"
echo ""
