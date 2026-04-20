# GitHub Actions Workflow - NuGet Publishing

## 📋 Quick Reference

**File**: `.github/workflows/publish-nuget.yml`

---

## 🚀 What It Does

Automatically builds, packs, and publishes MauiOtpKit packages to NuGet.org when:

1. ✅ Code is pushed to `main` branch (with `.csproj` changes)
2. ✅ GitHub Release is published
3. ✅ Workflow is manually triggered

---

## 🔄 Automated Trigger Points

### 1. Push to Main Branch
```bash
git add MauiOtpKit.Core/MauiOtpKit.Core.csproj
git commit -m "Bump version to 1.0.1"
git push origin main
```
**Result**: Workflow automatically runs → publishes to NuGet

### 2. GitHub Release
```bash
git tag -a v1.0.1 -m "Release 1.0.1"
git push origin v1.0.1
```
**Result**: Create release on GitHub → workflow runs

### 3. Manual Trigger
- Go to: **Actions** → **Publish to NuGet** → **Run workflow**
- Select branch and click **Run**

---

## 📦 Workflow Steps

| # | Step | Command |
|---|------|---------|
| 1 | Checkout | `git checkout` |
| 2 | Setup .NET 8 | `actions/setup-dotnet@v4` |
| 3 | Restore | `dotnet restore` |
| 4 | Build | `dotnet build -c Release` |
| 5 | Pack Core | `dotnet pack MauiOtpKit.Core/...` |
| 6 | Pack Platform | `dotnet pack MauiOtpKit/...` |
| 7 | Push to NuGet | `dotnet nuget push *.nupkg` |
| 8 | Summary | Create GitHub release notes |
| 9 | Artifacts | Upload packages for verification |

---

## 🔑 Required Setup (One Time)

### Step 1: Create NuGet.org Account
https://www.nuget.org/users/account/LogOn

### Step 2: Generate API Key
1. Login to NuGet.org
2. Go to Account Settings → API Keys
3. Create new key
4. Copy the key (keep secret!)

### Step 3: Add GitHub Secret
1. Go to GitHub repo
2. Settings → Secrets and variables → Actions
3. Click **New repository secret**
4. Name: `NUGET_API_KEY`
5. Value: (paste your API key)
6. Click **Add secret**

**Done!** Workflow will use this key automatically.

---

## 📊 How to Check Status

### View Workflow Runs
1. GitHub repo → **Actions** tab
2. Click **Publish to NuGet** workflow
3. View recent runs with status:
   - ✅ Green = Success
   - ❌ Red = Failed
   - ⏳ Yellow = Running

### View Logs
1. Click on a workflow run
2. Click **Build** step to expand logs
3. Scroll to see detailed output

### Check NuGet.org
After successful publish (5-15 min):
- https://www.nuget.org/packages/MauiOtpKit.Core/
- https://www.nuget.org/packages/MauiOtpKit/

---

## ⚙️ Workflow Configuration

### Trigger Conditions
```yaml
on:
  push:
    branches: [main]
    paths:
      - 'MauiOtpKit.Core/MauiOtpKit.Core.csproj'
      - 'MauiOtpKit/MauiOtpKit.csproj'
  release:
    types: [published]
  workflow_dispatch:  # Manual trigger
```

### Environment
- **Runner**: `ubuntu-latest`
- **.NET Version**: 8.0.x
- **Configuration**: Release

### Push Settings
```bash
--skip-duplicate  # Ignore if version already published
--api-key         # From GitHub secret
--source          # https://api.nuget.org/v3/index.json
```

---

## 🐛 Troubleshooting

### Problem: "401 Unauthorized"
**Cause**: Invalid API key
**Fix**:
1. Generate new API key on NuGet.org
2. Update `NUGET_API_KEY` secret in GitHub
3. Re-run workflow

### Problem: "409 Conflict"
**Cause**: Version already exists on NuGet
**Fix**:
1. Increment version in `.csproj` files
2. Commit and push
3. Re-run workflow

### Problem: "Workflow not triggered"
**Cause**: Path conditions not met
**Fix**:
1. Ensure `.csproj` files are modified
2. Or use manual trigger (Run workflow button)

### Problem: Build fails
**Cause**: MAUI SDK not available on runner
**Fix**:
- This is expected for non-MAUI SDK
- Core package still builds
- Platform package skips gracefully

---

## 📝 Local Testing

Test the build locally before pushing:

```bash
# Clean
dotnet clean -c Release

# Build
dotnet build -c Release

# Pack
dotnet pack MauiOtpKit.Core/MauiOtpKit.Core.csproj -c Release -o ./nupkg
dotnet pack MauiOtpKit/MauiOtpKit.csproj -c Release -o ./nupkg

# List packages
ls ./nupkg/*.nupkg
```

Or use the build scripts:
```bash
# Windows
.\build-and-pack.ps1

# Linux/macOS
./build-and-pack.sh
```

---

## 🔐 Security Best Practices

✅ API key stored in GitHub Secrets (never in code)
✅ Workflow doesn't log or expose API key
✅ HTTPS-only communication with NuGet.org
✅ `--skip-duplicate` prevents accidental overrides

---

## 📚 Related Files

- **Workflow**: `.github/workflows/publish-nuget.yml`
- **Full Guide**: `docs/NUGET_PUBLISHING.md`
- **Build Scripts**: `build-and-pack.ps1`, `build-and-pack.sh`
- **Core Package**: `MauiOtpKit.Core/MauiOtpKit.Core.csproj`
- **Platform Package**: `MauiOtpKit/MauiOtpKit.csproj`

---

## 📋 Checklist Before Publishing

- [ ] Updated version in both `.csproj` files
- [ ] Updated [CHANGELOG.md](../../CHANGELOG.md)
- [ ] Committed all changes
- [ ] `NUGET_API_KEY` secret is configured
- [ ] Local build test passed
- [ ] Git push to main (or create release)
- [ ] Monitor workflow in Actions tab
- [ ] Verify packages on NuGet.org (5-15 min)

---

**Your MauiOtpKit packages are ready for the world! 🚀**
