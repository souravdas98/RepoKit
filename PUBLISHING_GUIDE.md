# NuGet Publishing Guide for RepoKit

This guide explains how to publish RepoKit packages to NuGet.org.

## Prerequisites

1. **NuGet Account**
   - Create account at [nuget.org](https://www.nuget.org)
   - Verify your email

2. **API Key**
   - Log in to [nuget.org](https://www.nuget.org)
   - Go to Account Settings → API Keys
   - Copy your API key (keep it SECRET!)

3. **Local Setup**
   - .NET 8.0+ SDK installed
   - Git repository with proper .gitignore
   - All tests passing locally

## Pre-Publishing Checklist

Before publishing, ensure:

- [ ] All tests pass: `dotnet test`
- [ ] Code builds successfully: `dotnet build -c Release`
- [ ] XML docs are generated: `dotnet build -c Release`
- [ ] Version updated in `Directory.Build.props`
- [ ] CHANGELOG.md updated with new features
- [ ] README.md updated if needed
- [ ] No breaking changes (or major version bump)
- [ ] Git repository is clean: `git status`
- [ ] Changes committed and pushed: `git push`

## Manual Publishing Steps

### Step 1: Update Version

Edit `Directory.Build.props`:

```xml
<PropertyGroup>
  <VersionPrefix>1.0.1</VersionPrefix>
  <VersionSuffix></VersionSuffix>
</PropertyGroup>
```

### Step 2: Update Changelog

Add entry to `CHANGELOG.md`:

```markdown
## [1.0.1] - 2024-03-23

### Fixed
- Fixed pagination bug in EfRepository
- Improved error messages in DapperRepository

### Changed
- Updated documentation

### Added
- Null validation for edge cases
```

### Step 3: Commit Changes

```bash
git add .
git commit -m "Prepare v1.0.1 release

- Update version in Directory.Build.props
- Update CHANGELOG.md
- Add null validation to repositories"

git tag -a v1.0.1 -m "Release version 1.0.1"
git push origin main
git push origin v1.0.1
```

### Step 4: Clean Build

```bash
# Clean previous builds
dotnet clean

# Perform full release build
dotnet build -c Release

# Run all tests
dotnet test -c Release
```

### Step 5: Create NuGet Packages

```bash
# Create packages
dotnet pack -c Release -o ./bin/Release

# Verify packages exist
ls bin/Release/*.nupkg
```

### Step 6: Publish to NuGet

**Option A: Using the Helper Script**

Linux/macOS:
```bash
chmod +x publish.sh
./publish.sh YOUR_API_KEY
```

Windows:
```cmd
publish.bat YOUR_API_KEY
```

**Option B: Manual Publishing**

```bash
# Publish RepoKit.Core
dotnet nuget push bin/Release/RepoKit.Core.1.0.1.nupkg \
    --api-key YOUR_API_KEY \
    --source https://api.nuget.org/v3/index.json

# Publish RepoKit.EfCore
dotnet nuget push bin/Release/RepoKit.EfCore.1.0.1.nupkg \
    --api-key YOUR_API_KEY \
    --source https://api.nuget.org/v3/index.json

# Publish RepoKit.Dapper
dotnet nuget push bin/Release/RepoKit.Dapper.1.0.1.nupkg \
    --api-key YOUR_API_KEY \
    --source https://api.nuget.org/v3/index.json
```

### Step 7: Verify Publishing

1. **Check NuGet.org**
   - https://www.nuget.org/packages/RepoKit.Core
   - https://www.nuget.org/packages/RepoKit.EfCore
   - https://www.nuget.org/packages/RepoKit.Dapper

2. **Wait for Indexing** (5-10 minutes usually)

3. **Test Installation**
   ```bash
   dotnet add package RepoKit.Core --version 1.0.1
   ```

4. **Generate Release Notes**
   - Go to GitHub Releases
   - Create new release with version tag
   - Add changelog content
   - Publish release

## Automated Publishing (CI/CD)

For GitHub Actions, create `.github/workflows/publish.yml`:

```yaml
name: Publish to NuGet

on:
  push:
    tags:
      - 'v*'

jobs:
  publish:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      
      - name: Build
        run: dotnet build -c Release
      
      - name: Test
        run: dotnet test -c Release --no-build
      
      - name: Pack
        run: dotnet pack -c Release -o ./bin/Release
      
      - name: Publish
        run: |
          for package in bin/Release/*.nupkg; do
            if [[ ! "$package" == *.snupkg ]]; then
              dotnet nuget push "$package" \
                --api-key ${{ secrets.NUGET_API_KEY }} \
                --source https://api.nuget.org/v3/index.json \
                --skip-duplicate
            fi
          done
```

Then add secret to GitHub:
1. Go to Settings → Secrets and variables → Actions
2. Add secret: `NUGET_API_KEY` = Your NuGet API key

To publish:
```bash
git tag v1.0.1
git push origin v1.0.1
```

## Troubleshooting

### Error: "401 Unauthorized"

**Problem**: Invalid or expired API key

**Solution**:
1. Verify API key is correct
2. Check for typos
3. Regenerate key from nuget.org if needed

### Error: "409 Conflict - Package already exists"

**Problem**: Package version already published

**Solution**:
1. Increment version and retry
2. Or delete old package from nuget.org (if needed)

### Error: "Package contains invalid metadata"

**Problem**: XML documentation or other metadata issues

**Solution**:
1. Run `dotnet build -c Release` locally
2. Check for compiler warnings
3. Verify all required fields in .csproj files
4. Check for missing README.md or LICENSE

### Packages Not Appearing on NuGet.org

**Problem**: Publishing succeeded but packages not visible

**Solution**:
1. Wait 5-10 minutes for indexing
2. Clear browser cache
3. Check NuGet search with exact name
4. Verify package is marked as public (not unlisted)

## Version Numbering Strategy

Follow [Semantic Versioning](https://semver.org/):

- **MAJOR** (1.0.0 → 2.0.0): Breaking changes
- **MINOR** (1.0.0 → 1.1.0): New features (backward compatible)
- **PATCH** (1.0.0 → 1.0.1): Bug fixes

Examples:
- `1.0.0` - Initial release
- `1.0.1` - Bug fix
- `1.1.0` - New read-only repository feature
- `2.0.0` - Breaking change (removed deprecated method)

## Pre-release Versions

For beta versions, use `VersionSuffix`:

```xml
<PropertyGroup>
  <VersionPrefix>1.1.0</VersionPrefix>
  <VersionSuffix>beta.1</VersionSuffix>
</PropertyGroup>
```

This creates version `1.1.0-beta.1`.

Publish with:
```bash
dotnet nuget push bin/Release/RepoKit.Core.1.1.0-beta.1.nupkg ...
```

## Documentation Updates

After publishing, update:

1. **GitHub Release Notes**
   - Go to Releases
   - Create new release
   - Add release notes from CHANGELOG.md

2. **README.md**
   - Update install instructions if needed
   - Add new features to features list

3. **Project Website** (if any)
   - Update version numbers
   - Add release notes

## Maintenance

### Monitoring

- Watch for issues after release
- Monitor NuGet stats: https://nuget.info/packages/RepoKit.Core
- Check GitHub issues and discussions

### Support

- Respond to issues promptly
- Provide patches for critical bugs
- Plan next release based on feedback

### Deprecation

To deprecate a package version:

1. Go to NuGet.org package page
2. Edit package
3. Mark as deprecated
4. Add deprecation message

## Security

- **Never share your API key**
- Store key in secure location or environment variable
- Rotate key periodically
- Use scope-limited keys if possible
- Enable multi-factor authentication on nuget.org

## Additional Resources

- [NuGet.org Publish Docs](https://docs.microsoft.com/en-us/nuget/nuget-org/publish-a-package)
- [Semantic Versioning](https://semver.org/)
- [Package Metadata](https://docs.microsoft.com/en-us/nuget/reference/nuspec)
- [dotnet nuget push](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-nuget-push)

---

**Questions?** Create an issue or discussion in the [GitHub repository](https://github.com/SouravDas/RepoKit).
