# CI/CD Pipeline Documentation

> Complete guide to RepoKit's Continuous Integration and Continuous Deployment pipeline

## 📋 Table of Contents

1. [Overview](#overview)
2. [GitHub Actions Workflows](#github-actions-workflows)
3. [Branch Protection Rules](#branch-protection-rules)
4. [Secrets Configuration](#secrets-configuration)
5. [Status Checks](#status-checks)
6. [Monitoring & Troubleshooting](#monitoring--troubleshooting)

---

## 🎯 Overview

RepoKit uses a comprehensive CI/CD pipeline powered by **GitHub Actions** to ensure code quality, security, and reliability. The pipeline automatically:

✅ **Builds** across Windows, macOS, and Linux  
✅ **Tests** on .NET 8, 9, and 10  
✅ **Analyzes** code quality with SonarCloud  
✅ **Scans** for security vulnerabilities  
✅ **Checks** code formatting and documentation  
✅ **Publishes** to NuGet.org on release  

---

## 🔄 GitHub Actions Workflows

### 1️⃣ **CI Pipeline** (`.github/workflows/ci.yml`)

**When Triggered:**
- On push to `main` or `develop` branches
- On pull requests to `main` or `develop`
- Manual workflow dispatch

**Jobs:**

| Job | Purpose | Duration |
|-----|---------|----------|
| 🏗️ **Build & Test** | Compiles code, runs unit tests on 3 OS × 3 .NET versions | ~10-15 min |
| 🔍 **Code Analysis** | CodeQL security scanning, vulnerability detection | ~5-10 min |
| 💭 **Code Quality** | SonarCloud analysis, code metrics | ~8-12 min |
| 🎨 **Linting** | Code formatting, XML documentation checks | ~2-5 min |
| 🔐 **Security** | Trivy vulnerability scanning, dependency checks | ~3-5 min |
| 📊 **Coverage** | Test coverage report with PR comment | ~5-8 min |

**Matrix Testing:**
```
OS: [ubuntu-latest, windows-latest, macos-latest]
.NET: [8.0.x, 9.0.x, 10.0.x]
= 9 parallel builds
```

**Outputs:**
- ✅ Build artifacts in GitHub Packages
- 📊 Test results and coverage reports
- 🔒 Security findings in Security tab

---

### 2️⃣ **Publish Pipeline** (`.github/workflows/publish.yml`)

**When Triggered:**
- On GitHub release creation
- Manual workflow dispatch with version input

**Jobs:**

| Job | Purpose |
|-----|---------|
| 📋 **Pre-Publish** | Validates build, runs tests, checks docs |
| 🚀 **Publish** | Creates NuGet packages, publishes to NuGet.org & GitHub Packages |
| 📢 **Post-Publish** | Creates release notes, sends Slack notification |

**Publishes Three Packages:**
1. `RepoKit.Core` - Interfaces and DI registration
2. `RepoKit.EfCore` - Entity Framework Core implementation
3. `RepoKit.Dapper` - Dapper implementation

---

### 3️⃣ **Maintenance Pipeline** (`.github/workflows/maintenance.yml`)

**When Triggered:**
- Scheduled: Daily at 2 AM UTC
- Manual workflow dispatch

**Jobs:**

| Job | Purpose |
|-----|---------|
| 🔄 **Check Updates** | Detects outdated NuGet packages |
| 🔐 **Security Scan** | Checks for known vulnerabilities |
| 🎨 **Auto-Format** | Applies code formatting, creates PR |

---

## 🔒 Branch Protection Configuration

### Main Branch

```yaml
✅ Require pull request review: 2 approvals
✅ Require CODEOWNERS review: true
✅ Dismiss stale reviews: true
✅ Require status checks to pass:
   - Build (all combinations)
   - Code analysis
   - Code quality
   - Formatting
   - Security scan
✅ Require up-to-date branches: true
✅ Allow auto-merge: true (squash)
✅ Auto-delete head branches: true
✅ Enforce for admins: true
```

### Develop Branch

```yaml
✅ Require pull request review: 1 approval
✅ Require CODEOWNERS review: false
✅ Dismiss stale reviews: true
✅ Require status checks to pass:
   - Build
   - Code analysis
✅ Require up-to-date branches: true
✅ Allow auto-merge: true
✅ Auto-delete head branches: true
```

### Setup Branch Protection

**Via GitHub CLI:**

```bash
# Login
gh auth login

# Configure main branch
gh api repos/SouravDas/RepoKit/branches/main/protection \
  --input - << 'EOF'
{
  "required_status_checks": {
    "strict": true,
    "contexts": [
      "🏗️ Build & Test",
      "🔍 Code Analysis",
      "💭 SonarQube Code Quality",
      "🎨 Linting & Formatting",
      "🔐 Security Scan"
    ]
  },
  "required_pull_request_reviews": {
    "required_approving_review_count": 2,
    "dismiss_stale_reviews": true,
    "require_code_owner_reviews": true
  },
  "enforce_admins": true
}
EOF
```

More details in [BRANCH_PROTECTION.md](BRANCH_PROTECTION.md)

---

## 🔑 Required Secrets Configuration

### GitHub Secrets

Set these in `Settings → Secrets and variables → Actions`:

| Secret | Purpose | Scope |
|--------|---------|-------|
| `NUGET_API_KEY` | Authenticate with NuGet.org | Publish workflow |
| `GITHUB_TOKEN` | GitHub API access (auto-provided) | All workflows |
| `SONAR_TOKEN` | SonarCloud authentication | Code quality |
| `SLACK_WEBHOOK_URL` | Post notifications to Slack | Publish workflow |

### Setting Up Secrets

**NUGET_API_KEY:**
1. Go to [NuGet.org API Keys](https://www.nuget.org/account/apikeys)
2. Create new key with "Push" scope
3. In GitHub: Settings → Secrets → New secret
4. Name: `NUGET_API_KEY`
5. Value: Paste the NuGet key

**SONAR_TOKEN:**
1. Go to [SonarCloud](https://sonarcloud.io/account/security)
2. Generate token
3. In GitHub: Add as `SONAR_TOKEN`

**SLACK_WEBHOOK_URL (Optional):**
1. Create Slack workspace or use existing
2. Create incoming webhook in App Directory
3. In GitHub: Add as `SLACK_WEBHOOK_URL`

---

## ✅ Status Checks

### Required Passing Checks

All these must pass before merging to `main`:

```
✅ 🏗️ Build & Test (ubuntu-latest, 8.0.x)
✅ 🏗️ Build & Test (ubuntu-latest, 9.0.x)
✅ 🏗️ Build & Test (ubuntu-latest, 10.0.x)
✅ 🏗️ Build & Test (windows-latest, 8.0.x)
✅ 🏗️ Build & Test (windows-latest, 9.0.x)
✅ 🏗️ Build & Test (windows-latest, 10.0.x)
✅ 🏗️ Build & Test (macos-latest, 8.0.x)
✅ 🏗️ Build & Test (macos-latest, 9.0.x)
✅ 🏗️ Build & Test (macos-latest, 10.0.x)
✅ 🔍 Code Analysis
✅ 💭 SonarQube Code Quality
✅ 🎨 Linting & Formatting
✅ 🔐 Security Scan
```

### Viewing Status

**In Pull Request:**
- Scroll to "Checks" section
- Click on check name for details
- View logs of failed steps

**In GitHub Actions:**
- https://github.com/SouravDas/RepoKit/actions
- Click workflow run
- Click failed step to see error logs

---

## 🚀 Deployment Process

### Automatic Publishing

1. **Create Release:**
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

2. **Create GitHub Release:**
   - Go to [Releases](https://github.com/SouravDas/RepoKit/releases)
   - Click "Draft new release"
   - Select tag: `v1.0.0`
   - Add release notes from CHANGELOG.md
   - Click "Publish release"

3. **CI/CD Triggers:**
   - Publish workflow starts automatically
   - Runs pre-publish validation
   - Creates NuGet packages
   - Publishes to NuGet.org & GitHub Packages
   - Creates release notes
   - Sends Slack notification

4. **Verification:**
   - Check NuGet.org: https://www.nuget.org/packages/RepoKit.Core/
   - Check GitHub Packages: https://github.com/SouravDas/RepoKit/packages
   - Look for Slack notification

---

## 📊 Monitoring & Troubleshooting

### View Workflow Runs

```bash
# List recent runs
gh run list --repo SouravDas/RepoKit -L 10

# Get specific run details
gh run view {RUN_ID}

# View logs for a run
gh run view {RUN_ID} --log

# Download artifacts
gh run download {RUN_ID}
```

### Common Issues

#### ❌ Build Fails on Windows but Passes on Linux

**Possible Causes:**
- Line ending issues (CRLF vs LF)
- Path separator differences
- Missing SDK on Windows

**Solutions:**
```bash
# Set git config
git config --global core.autocrlf true

# Or in .gitattributes:
* text=auto
*.cs text eol=lf
```

#### ❌ Tests Fail Due to Timeout

**Solution:** Increase timeout in workflow:
```yaml
timeout-minutes: 30  # Default is 360
```

#### ❌ Security Scan Blocking Merge

**Review findings:**
- Go to Security → Code scanning
- Review Trivy results
- Mark as "Resolved" if false positive
- Update dependencies if real vulnerability

#### ❌ Code Quality Gate Failing

**Options:**
1. Fix issues flagged by SonarCloud
2. Adjust quality gate in sonar-project.properties
3. Contact code owner for exception

#### ❌ NuGet API Key Invalid

**Fix:**
1. Verify API key hasn't expired
2. Check it has "Push" permission
3. Update secret in GitHub
4. Re-run publish workflow

---

## 📈 Performance Optimization

### Reduce Build Time

1. **Use cache:**
   ```yaml
   - uses: actions/setup-dotnet@v4
     with:
       cache: true
   ```

2. **Parallel jobs:**
   - CI pipeline runs 9 builds in parallel
   - Reduces total time to ~15 min instead of 135 min

3. **Conditional runs:**
   - Skip code quality if docs-only change
   - Skip security scan for version bumps

### Current Metrics

- **Average CI duration:** 12-18 minutes
- **Publish duration:** 8-12 minutes
- **Total with all checks:** ~20 minutes

---

## 🔐 Security Best Practices

✅ **DO:**
- Use branch protection for all important branches
- Require code review before merge
- Sign commits (GPG or SSH)
- Store API keys only in GitHub secrets
- Regularly update dependencies
- Review security scan results
- Use CODEOWNERS file

❌ **DON'T:**
- Store credentials in code/YAML
- Use PAT instead of GITHUB_TOKEN
- Disable security checks
- Skip code review
- Use force push to protected branches
- Share secrets publicly

---

## 📚 Additional Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [SonarCloud Documentation](https://docs.sonarcloud.io/)
- [CodeQL Documentation](https://codeql.github.com/docs/)
- [NuGet Publishing Guide](PUBLISHING_GUIDE.md)
- [Branch Protection Guide](BRANCH_PROTECTION.md)

---

## 🆘 Support

For CI/CD issues:

1. Check workflow logs in GitHub Actions
2. Review job output for error messages
3. Check [Troubleshooting](#monitoring--troubleshooting) section
4. Open GitHub Issue with:
   - Workflow name
   - Run ID
   - Error message
   - Steps to reproduce

---

**Last Updated:** 2026-03-22  
**Maintainer:** Sourav Das
