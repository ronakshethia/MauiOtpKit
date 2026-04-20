# Publishing MauiOtpKit to NuGet.org

This guide explains how to publish MauiOtpKit packages to NuGet.org using GitHub Actions.

---

## 📋 Prerequisites

1. **NuGet.org Account** - Create free account at https://www.nuget.org
2. **GitHub Repository** - Push code to GitHub
3. **NuGet API Key** - Generate from NuGet.org account settings

---

## 🔑 Step 1: Generate NuGet API Key

1. Go to https://www.nuget.org/account/apikeys
2. Sign in with your account
3. Click **"Create"** to generate new API key
4. Copy the generated key (keep it secret!)

**Important**: The API key has these scopes:
- Push new packages and new versions
- Push package metadata updates
- Remove packages from search results (optional)

---

## 🔐 Step 2: Configure GitHub Secret

1. Go to your GitHub repository
2. Navigate to **Settings** → **Secrets and variables** → **Actions**
3. Click **"New repository secret"**
4. Create secret named: `NUGET_API_KEY`
5. Paste your NuGet API key
6. Click **"Add secret"**

**Screenshot**: Settings → Secrets → New repository secret → `NUGET_API_KEY`

---

## 🚀 Step 3: Update Package Version

Before publishing, update the version number in both `.csproj` files:

### MauiOtpKit.Core/MauiOtpKit.Core.csproj
```xml
<Version>1.0.1</Version>  <!-- Increment version -->
```

### MauiOtpKit/MauiOtpKit.csproj
```xml
<Version>1.0.1</Version>  <!-- Same version as Core -->
```

**Version Format**: `MAJOR.MINOR.PATCH`
- `1.0.0` → `1.0.1` (patch - bug fixes)
- `1.0.0` → `1.1.0` (minor - new features)
- `1.0.0` → `2.0.0` (major - breaking changes)

---

## 📤 Step 4: Publish Package

### Option A: Automatic Publishing (Recommended)

The workflow automatically publishes when:
1. Changes are pushed to `main` branch AND changes affect `.csproj` files
2. A GitHub Release is published
3. Workflow is triggered manually

**Trigger automatic publishing**:
```bash
# Make changes and commit
git add MauiOtpKit.Core/MauiOtpKit.Core.csproj
git commit -m "Bump version to 1.0.1"
git push origin main
```

The GitHub Actions workflow will automatically:
- ✅ Build the solution
- ✅ Create NuGet packages
- ✅ Push to NuGet.org
- ✅ Create release summary

### Option B: Manual Trigger

1. Go to GitHub repository
2. Click **Actions** tab
3. Select **"Publish to NuGet"** workflow
4. Click **"Run workflow"**
5. Choose branch and click **"Run workflow"**

### Option C: GitHub Release

1. Create a GitHub Release:
   ```bash
   git tag -a v1.0.1 -m "Release version 1.0.1"
   git push origin v1.0.1
   ```
2. Go to GitHub → Releases
3. Click **"Create release"**
4. Set tag to `v1.0.1`
5. Click **"Publish release"**

Workflow automatically triggers on release!

---

## ✅ Verification

### Check Workflow Status

1. Go to **Actions** tab in GitHub
2. Click **"Publish to NuGet"** workflow
3. View recent runs
4. Green checkmark = **Success** ✅
5. Red X = **Failed** ❌

### View on NuGet.org

After successful publish:
1. Visit https://www.nuget.org/packages/MauiOtpKit.Core/
2. Visit https://www.nuget.org/packages/MauiOtpKit/
3. Verify version and metadata

May take 5-15 minutes to appear in search results.

---

## 🧪 Test Installation Locally

After publishing, test the NuGet package:

```bash
# Search for package
dotnet package search MauiOtpKit

# Install latest version
dotnet add package MauiOtpKit.Core --version 1.0.1
dotnet add package MauiOtpKit --version 1.0.1

# Verify installation
dotnet list package
```

---

## 🔧 Workflow File Details

The workflow (`.github/workflows/publish-nuget.yml`) performs these steps:

| Step | Action |
|------|--------|
| 1 | Checkout repository code |
| 2 | Setup .NET 8 SDK |
| 3 | Restore NuGet dependencies |
| 4 | Build in Release configuration |
| 5 | Pack MauiOtpKit.Core (.nupkg) |
| 6 | Pack MauiOtpKit (.nupkg) |
| 7 | Push packages to NuGet.org |
| 8 | Create release summary |
| 9 | Upload artifacts for verification |

---

## ⚠️ Troubleshooting

### Issue: "401 Unauthorized"
**Cause**: Invalid or expired API key
**Solution**:
1. Regenerate API key on NuGet.org
2. Update `NUGET_API_KEY` secret in GitHub
3. Retry workflow

### Issue: "409 Conflict - Version already exists"
**Cause**: Version already published on NuGet.org
**Solution**:
1. Increment version in `.csproj` files
2. Commit and push
3. Retry workflow

### Issue: "Package validation failed"
**Cause**: Missing metadata or invalid package content
**Solution**:
1. Check `.csproj` for required fields (Authors, Description, etc.)
2. Verify NuGet package locally: `dotnet package verify`
3. Review GitHub Actions logs for details

### Issue: Workflow doesn't trigger automatically
**Cause**: Path conditions not met
**Solution**:
- Push changes directly to `.csproj` files, not just other files
- Or use manual trigger: **Actions** → **Run workflow**

---

## 📊 Release Workflow Best Practices

### Semantic Versioning

```
MAJOR.MINOR.PATCH
  ↓     ↓      ↓
  1  .  0   .   1

1.0.0 = Initial release
1.0.1 = Bug fix / patch
1.1.0 = New feature (backward compatible)
2.0.0 = Breaking changes
```

### Release Checklist

- [ ] Update version in both `.csproj` files
- [ ] Update [CHANGELOG.md](../../CHANGELOG.md) with release notes
- [ ] Run local build: `dotnet build -c Release`
- [ ] Run tests (if applicable)
- [ ] Commit changes: `git commit -m "Release v1.0.1"`
- [ ] Create GitHub Release (or push to main)
- [ ] Verify on NuGet.org after 10-15 minutes
- [ ] Announce release in README or discussions

---

## 🔄 Continuous Publishing

The workflow is configured to:

✅ Auto-publish on push to `main` (if `.csproj` changed)
✅ Skip duplicate versions (using `--skip-duplicate` flag)
✅ Generate release summary automatically
✅ Archive packages as artifacts
✅ Report errors clearly

---

## 📝 Posting to NuGet Manually (Alternative)

If you prefer to publish manually without GitHub Actions:

```bash
# Build packages locally
dotnet pack MauiOtpKit.Core/MauiOtpKit.Core.csproj -c Release -o ./nupkg
dotnet pack MauiOtpKit/MauiOtpKit.csproj -c Release -o ./nupkg

# Push to NuGet.org
dotnet nuget push ./nupkg/MauiOtpKit.Core.1.0.1.nupkg \
  --api-key YOUR_API_KEY \
  --source https://api.nuget.org/v3/index.json

dotnet nuget push ./nupkg/MauiOtpKit.1.0.1.nupkg \
  --api-key YOUR_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

---

## 🎯 Next Steps

1. ✅ Create NuGet.org account
2. ✅ Generate API key
3. ✅ Add `NUGET_API_KEY` secret to GitHub
4. ✅ Update version numbers in `.csproj` files
5. ✅ Push to main or create Release
6. ✅ Monitor workflow in GitHub Actions
7. ✅ Verify packages on NuGet.org

---

## 📞 Support

- **NuGet.org Help**: https://docs.microsoft.com/en-us/nuget/
- **GitHub Actions Docs**: https://docs.github.com/en/actions
- **Package Issues**: Check NuGet.org support or GitHub issues

---

**Your MauiOtpKit packages are ready for the world! 🚀**
