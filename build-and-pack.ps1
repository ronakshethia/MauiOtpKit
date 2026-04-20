# MauiOtpKit - Local Build and Pack Script (PowerShell)
# Usage: .\build-and-pack.ps1

param(
    [switch]$SkipPlatform = $false,
    [switch]$Verbose = $false
)

$ErrorActionPreference = "Stop"

Write-Host "🔨 MauiOtpKit - Build and Pack Script" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Check if .NET is installed
try {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET SDK found: $dotnetVersion" -ForegroundColor Green
}
catch {
    Write-Host "❌ .NET SDK not found. Please install .NET 8.0 or later." -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 1: Clean
Write-Host "Step 1: Cleaning previous builds..." -ForegroundColor Cyan
dotnet clean -c Release $(if (-not $Verbose) { "-q" })
if (Test-Path "./nupkg") {
    Remove-Item -Path "./nupkg" -Recurse -Force
}
Write-Host "✓ Clean complete" -ForegroundColor Green
Write-Host ""

# Step 2: Restore
Write-Host "Step 2: Restoring dependencies..." -ForegroundColor Cyan
dotnet restore $(if (-not $Verbose) { "-q" })
Write-Host "✓ Restore complete" -ForegroundColor Green
Write-Host ""

# Step 3: Build Core
Write-Host "Step 3: Building MauiOtpKit.Core..." -ForegroundColor Cyan
dotnet build MauiOtpKit.Core/MauiOtpKit.Core.csproj -c Release $(if (-not $Verbose) { "-q" })
Write-Host "✓ MauiOtpKit.Core build complete" -ForegroundColor Green
Write-Host ""

# Step 4: Build Platform
if (-not $SkipPlatform) {
    Write-Host "Step 4: Building MauiOtpKit (Platform)..." -ForegroundColor Cyan
    try {
        dotnet build MauiOtpKit/MauiOtpKit.csproj -c Release $(if (-not $Verbose) { "-q" })
        Write-Host "✓ MauiOtpKit platform build complete" -ForegroundColor Green
    }
    catch {
        Write-Host "⚠ Platform build skipped (MAUI SDK may not be installed)" -ForegroundColor Yellow
    }
    Write-Host ""
}

# Step 5: Pack Core
Write-Host "Step 5: Packing MauiOtpKit.Core..." -ForegroundColor Cyan
dotnet pack MauiOtpKit.Core/MauiOtpKit.Core.csproj -c Release -o ./nupkg $(if (-not $Verbose) { "-q" })
Write-Host "✓ MauiOtpKit.Core packed" -ForegroundColor Green
Write-Host ""

# Step 6: Pack Platform
if (-not $SkipPlatform) {
    Write-Host "Step 6: Packing MauiOtpKit..." -ForegroundColor Cyan
    try {
        dotnet pack MauiOtpKit/MauiOtpKit.csproj -c Release -o ./nupkg $(if (-not $Verbose) { "-q" })
        Write-Host "✓ MauiOtpKit packed" -ForegroundColor Green
    }
    catch {
        Write-Host "⚠ Platform pack skipped" -ForegroundColor Yellow
    }
    Write-Host ""
}

# Step 7: List packages
Write-Host "Step 7: Generated NuGet Packages:" -ForegroundColor Cyan
if (Test-Path "./nupkg") {
    Get-ChildItem -Path "./nupkg" -Filter "*.nupkg" | ForEach-Object {
        $size = "{0:N2} MB" -f ($_.Length / 1MB)
        Write-Host "  - $($_.Name) ($size)" -ForegroundColor White
    }
    Write-Host ""
    Write-Host "✓ Packages ready in ./nupkg/" -ForegroundColor Green
}
else {
    Write-Host "⚠ No packages directory found" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "✅ Build and pack complete!" -ForegroundColor Green
Write-Host ""
Write-Host "📦 Next steps:" -ForegroundColor Cyan
Write-Host "   1. Review packages in ./nupkg/" -ForegroundColor White
Write-Host "   2. Test locally: dotnet nuget add source ./nupkg" -ForegroundColor White
Write-Host "   3. Push to NuGet: dotnet nuget push ./nupkg/*.nupkg --api-key YOUR_KEY" -ForegroundColor White
Write-Host ""
