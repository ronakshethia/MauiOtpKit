# 🚀 NuGet Publishing Setup - Complete Guide

## What Was Created

### 1. **GitHub Actions Workflow** ✅
- **File**: `.github/workflows/publish-nuget.yml`
- **Triggers**: Push to main, Release, Manual trigger
- **Actions**: Build → Pack → Push to NuGet.org
- **Features**:
  - Automatic dependency restoration
  - Release build configuration
  - Skip-duplicate handling
  - Release summary generation
  - Artifact archival

### 2. **Documentation** ✅
- **File**: `docs/NUGET_PUBLISHING.md` (1000+ words)
  - Step-by-step API key setup
  - GitHub secret configuration
  - Version management
  - Publishing triggers
  - Verification process
  - Troubleshooting guide
  - Best practices

- **File**: `.github/workflows/WORKFLOW_REFERENCE.md`
  - Quick reference guide
  - Workflow steps breakdown
  - Trigger conditions
  - Status checking
  - Troubleshooting matrix

### 3. **Build Scripts** ✅
- **PowerShell**: `build-and-pack.ps1`
  - Windows users
  - Color-coded output
  - Verbose option
  - Error handling
  - Package listing

- **Bash**: `build-and-pack.sh`
  - Linux/macOS users
  - Shell script version
  - Same functionality as PowerShell
  - Color output
  - Version checking

### 4. **Updated README** ✅
- Added **📦 Publishing to NuGet** section
- Added **NUGET_PUBLISHING.md** to docs links
- Added build script references
- Quick start instructions

---

## 🎯 How to Use

### **Option 1: Fully Automated** (Recommended)

```bash
# 1. Update version
# Edit: MauiOtpKit.Core/MauiOtpKit.Core.csproj
#       MauiOtpKit/MauiOtpKit.csproj
# Change: <Version>1.0.0</Version> → <Version>1.0.1</Version>

# 2. Commit and push
git add .
git commit -m "Release v1.0.1"
git push origin main

# GitHub Actions automatically:
# ✅ Builds solution
# ✅ Creates NuGet packages
# ✅ Pushes to NuGet.org
# ✅ Creates release summary
```

### **Option 2: Local Build + Manual Push**

```bash
# Windows
.\build-and-pack.ps1

# Linux/macOS
./build-and-pack.sh

# Manually push
dotnet nuget push ./nupkg/*.nupkg --api-key YOUR_KEY --source https://api.nuget.org/v3/index.json
```

### **Option 3: Manual GitHub Actions**

1. Go to GitHub → **Actions**
2. Select **Publish to NuGet** workflow
3. Click **Run workflow**
4. Select branch and click **Run**

---

## 📋 Prerequisites (One Time Setup)

```bash
# 1. Create NuGet.org account
#    → https://www.nuget.org

# 2. Generate API key
#    → Account Settings → API Keys → Create

# 3. Add GitHub secret
#    → Settings → Secrets → NUGET_API_KEY

# Done! Workflow will use automatically
```

---

## 🔄 Publishing Workflow

```
Local Development
       ↓
Update Version (1.0.1)
       ↓
Commit & Push
       ↓
GitHub Actions Triggered
       ↓
├─ Checkout code
├─ Setup .NET SDK
├─ Restore dependencies
├─ Build Release
├─ Pack Core package
├─ Pack Platform package
├─ Push to NuGet.org
├─ Generate summary
└─ Archive artifacts
       ↓
Verify on NuGet.org
(5-15 minutes)
       ↓
Package Available to Users
       ↓
dotnet add package MauiOtpKit
```

---

## 📊 Workflow Features

| Feature | Details |
|---------|---------|
| **Triggers** | Push (main), Release, Manual |
| **Build** | Release configuration, all dependencies |
| **Packaging** | Both Core & Platform packages |
| **Publishing** | Skip duplicates, error handling |
| **Artifacts** | Archive packages for verification |
| **Notifications** | Release summary in Actions |
| **Failures** | Detailed error messages |

---

## ✅ Pre-Publication Checklist

Before publishing, verify:

```bash
# 1. Version updated
grep -n "Version" MauiOtpKit.Core/MauiOtpKit.Core.csproj
grep -n "Version" MauiOtpKit/MauiOtpKit.csproj
# Should show: <Version>1.0.1</Version>

# 2. Local build successful
dotnet build -c Release

# 3. Packages created
ls ./nupkg/*.nupkg
# Should show 2 packages (Core + Platform)

# 4. API key configured
# Check GitHub Settings → Secrets → NUGET_API_KEY
```

---

## 🚀 Publishing Steps

### Step 1: Prepare Release
```bash
# Update version in both .csproj files
# Edit CHANGELOG.md
# Commit all changes
git add .
git commit -m "Release v1.0.1"
```

### Step 2: Trigger Publishing
```bash
# Option A: Push to main
git push origin main

# Option B: Create release
git tag -a v1.0.1 -m "Release 1.0.1"
git push origin v1.0.1

# Option C: Manual trigger in GitHub Actions
```

### Step 3: Monitor
```bash
# Watch GitHub Actions
# Actions tab → Publish to NuGet → Recent runs
# Wait for green checkmark ✅
```

### Step 4: Verify
```bash
# Check NuGet.org (5-15 minutes)
# https://www.nuget.org/packages/MauiOtpKit.Core/
# https://www.nuget.org/packages/MauiOtpKit/

# Test installation
dotnet add package MauiOtpKit.Core --version 1.0.1
dotnet add package MauiOtpKit --version 1.0.1
```

---

## 🐛 Troubleshooting

### "Workflow didn't trigger"
✅ Solution: Push changes to `.csproj` files or use manual trigger

### "401 Unauthorized"
✅ Solution: Verify `NUGET_API_KEY` secret is correct

### "409 Conflict - Version exists"
✅ Solution: Increment version and retry

### "Build failed"
✅ Solution: Check GitHub Actions logs for details

See [docs/NUGET_PUBLISHING.md](docs/NUGET_PUBLISHING.md) for more troubleshooting.

---

## 📚 Files Created

```
.github/
└── workflows/
    ├── publish-nuget.yml          # GitHub Actions workflow
    └── WORKFLOW_REFERENCE.md      # Quick reference guide

docs/
└── NUGET_PUBLISHING.md            # Complete publishing guide

Root:
├── build-and-pack.ps1             # PowerShell build script
└── build-and-pack.sh              # Bash build script

README.md (updated)
└── Added publishing section & links
```

---

## 🎓 Documentation Links

- **Quick Reference**: [.github/workflows/WORKFLOW_REFERENCE.md](.github/workflows/WORKFLOW_REFERENCE.md)
- **Full Guide**: [docs/NUGET_PUBLISHING.md](docs/NUGET_PUBLISHING.md)
- **README Section**: [README.md#-publishing-to-nuget](README.md#-publishing-to-nuget)

---

## 🔐 Security

✅ API key stored securely in GitHub Secrets
✅ Never committed to repository
✅ HTTPS-only communication
✅ Workflow runs with limited access
✅ Logs don't expose sensitive data

---

## 📈 Version Numbering

Use **Semantic Versioning**:

```
1.0.0  →  1.0.1  (Patch: bug fixes)
1.0.0  →  1.1.0  (Minor: new features)
1.0.0  →  2.0.0  (Major: breaking changes)
```

Update BOTH `.csproj` files to same version!

---

## 🎯 Next Actions

1. ✅ Create NuGet.org account (if not done)
2. ✅ Generate API key
3. ✅ Add `NUGET_API_KEY` secret to GitHub
4. ✅ Update version in `.csproj` files
5. ✅ Push to main or create release
6. ✅ Monitor GitHub Actions
7. ✅ Verify on NuGet.org

---

## 💡 Pro Tips

- Use `build-and-pack.ps1` or `.sh` to test locally before pushing
- Automate version bumps with semantic versioning tools
- Monitor NuGet.org analytics after publishing
- Consider versioning strategy (alpha, beta, release candidates)
- Keep CHANGELOG.md updated with each release

---

**Your MauiOtpKit packages are ready for enterprise distribution! 🎉**

For questions, see:
- [docs/NUGET_PUBLISHING.md](docs/NUGET_PUBLISHING.md)
- [.github/workflows/WORKFLOW_REFERENCE.md](.github/workflows/WORKFLOW_REFERENCE.md)
- GitHub Actions official docs: https://docs.github.com/en/actions
