# Branch Protection Rules

> GitHub branch protection configuration for RepoKit

## 📋 Overview

This document describes the recommended branch protection rules for the RepoKit repository. Branch protection enforces code quality standards and prevents accidental deployments.

---

## 🔒 Main Branch Protection

### For `main` branch:

**Basic Settings:**
```yaml
# Branch name pattern
branch_name_pattern: "main"

# Require a pull request before merging
require_pull_request_reviews:
  required_approving_review_count: 2
  dismiss_stale_reviews: true
  require_code_owner_reviews: true

# Require status checks to pass before merging
required_status_checks:
  strict: true
  contexts:
    - "🏗️ Build & Test (ubuntu-latest, 8.0.x)"
    - "🏗️ Build & Test (ubuntu-latest, 9.0.x)"
    - "🏗️ Build & Test (ubuntu-latest, 10.0.x)"
    - "🔍 Code Analysis"
    - "💭 SonarQube Code Quality"
    - "🎨 Linting & Formatting"
    - "🔐 Security Scan"

# Require branches to be up to date
require_status_checks_to_pass_before_merging: true

# Require signed commits
require_signed_commits: false

# Allow auto-merge
allow_auto_merge: true
  auto_merge_type: "SQUASH"

# Allow deletion of branch on merge
auto_delete_head_branch: true

# Enforce all of the above settings
enforce_admins: true

# Restrict who can push to matching branches
push_restrictions: []  # No additional restrictions

# Dismiss pull request approvals when new commits are pushed
dismiss_stale_reviews: true

# Require that new commits push with this status
required_linear_history: false
```

---

## 🌿 Develop Branch Protection

### For `develop` branch:

```yaml
branch_name_pattern: "develop"

require_pull_request_reviews:
  required_approving_review_count: 1
  dismiss_stale_reviews: true
  require_code_owner_reviews: false

required_status_checks:
  strict: true
  contexts:
    - "🏗️ Build & Test"
    - "🔍 Code Analysis"
    - "🎨 Linting & Formatting"

allow_auto_merge: true
auto_delete_head_branch: true
enforce_admins: false
```

---

## 🚀 Setup Instructions (Manual)

### Via GitHub Web UI:

1. **Navigate to Settings:**
   - Go to your repository
   - Click `Settings` → `Branches`

2. **Add Branch Protection Rule:**
   - Click `Add rule`
   - Enter branch name pattern (e.g., `main`)

3. **Configure Protection Rules:**

   ✅ **Require a pull request before merging**
   - Check "Require a pull request before merging"
   - Check "Dismiss stale pull request approvals when new commits are pushed"
   - Check "Require review from Code Owners"
   - Set required approving reviewers to **2** (for main)

   ✅ **Require status checks to pass**
   - Check "Require status checks to pass before merging"
   - Check "Require branches to be up to date before merging"
   - Select required checks:
     - Build (all .NET versions)
     - Code Analysis
     - Code Quality
     - Formatting
     - Security

   ✅ **Additional Settings**
   - Check "Allow auto-merge"
   - Check "Automatically delete head branches"
   - Check "Enforce all the above settings for administrators"

4. **Create Rule**
   - Click "Create"

---

## 🤖 Automated Setup (GitHub CLI)

### Using GitHub CLI (`gh`):

```bash
# Install GitHub CLI if needed
# https://cli.github.com/

# Login to GitHub
gh auth login

# Create branch protection for main
gh api repos/{owner}/{repo}/branches/main/protection \
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
  "allow_auto_merge": true,
  "auto_delete_head_branch": true,
  "enforce_admins": true
}
EOF

# Create branch protection for develop
gh api repos/{owner}/{repo}/branches/develop/protection \
  --input - << 'EOF'
{
  "required_status_checks": {
    "strict": true,
    "contexts": [
      "🏗️ Build & Test",
      "🔍 Code Analysis"
    ]
  },
  "required_pull_request_reviews": {
    "required_approving_review_count": 1,
    "dismiss_stale_reviews": true,
    "require_code_owner_reviews": false
  },
  "allow_auto_merge": true,
  "auto_delete_head_branch": true,
  "enforce_admins": false
}
EOF
```

---

## 📋 Using CODEOWNERS

Create `.github/CODEOWNERS` file:

```

# RepoKit.Core
src/RepoKit.Core/**       @SouravDas

# RepoKit.EfCore
src/RepoKit.EfCore/**     @SouravDas

# RepoKit.Dapper
src/RepoKit.Dapper/**     @SouravDas

# Configuration
*.yml                     @SouravDas
*.csproj                  @SouravDas
Directory.Build.props     @SouravDas

# Documentation
*.md                      @SouravDas
```

---

## ✅ Pre-Merge Checklist

Before merging to `main`, ensure:

- ✅ All CI  checks pass (green checkmarks)
- ✅ Code review completed (minimum 2 approvals)
- ✅ No merge conflicts
- ✅ Documentation updated
- ✅ Tests pass on all .NET versions (8.0, 9.0, 10.0)
- ✅ Code quality metrics within thresholds
- ✅ Security scan passed
- ✅ No unresolved conversations
- ✅ Commits are signed (recommended)

---

## 🔄 PR Workflow

### Creating a Pull Request:

1. **Create feature branch from `develop`:**
   ```bash
   git checkout -b feature/my-feature develop
   ```

2. **Commit with signed commits (recommended):**
   ```bash
   git commit -S -m "feat: add my feature"
   ```

3. **Push and create PR:**
   ```bash
   git push origin feature/my-feature
   ```

4. **Set PR title and description:**
   - Use conventional commits format
   - Reference related issues
   - Describe changes and testing

5. **Wait for CI to complete:**
   - All status checks must pass
   - Code review required

6. **Address review feedback:**
   - Make requested changes
   - Push new commits
   - CI runs again automatically

7. **Merge when ready:**
   - Use "Squash and merge" for cleaner history
   - Delete branch after merge

---

## 🚨 Bypassing Protection (Admin Only)

> **⚠️ Caution: Use only in emergencies!**

If you must bypass protection rules:

```bash
# Push with force (requires admin rights)
git push --force origin main

# Via GitHub CLI
gh api repos/{owner}/{repo}/branches/main/protection \
  --method DELETE

# Re-enable after emergency
# (Repeat setup steps above)
```

---

## 🔔 Status Checks Monitoring

### Track status check performance:

```bash
# View recent workflow runs
gh run list --repo SouravDas/RepoKit -L 20

# Get specific workflow status
gh run view {run-id}

# View check annotations/errors
gh run view {run-id} --log
```

---

## 📚 GitHub Documentation

For more information on branch protection:

- [About branch protection rules](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches/about-protected-branches)
- [Configuring protected branches](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches/managing-a-branch-protection-rule)
- [CODEOWNERS file](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-code-owners)
- [GitHub CLI documentation](https://cli.github.com/manual/)

---

## 🔐 Security Considerations

- 🔒 Enforce signed commits for all contributors
- 🔑 Use SSH keys with passphrases
- 👥 Require code owner reviews
- 📊 Monitor branch activity
- 🚨 Enable branch protection immediately after repo creation
- 🚬 Never disable enforce_admins for production branches

---

## ✨ Best Practices

1. **Main Branch:**
   - Production-ready code only
   - Require 2 approvals (peer review + code owner)
   - Require all status checks
   - Require up-to-date branches

2. **Develop Branch:**
   - Integration branch
   - Require 1 approval
   - Relax requirements slightly
   - Still require critical status checks

3. **Feature Branches:**
   - No protection needed
   - Use conventional commit names: `feature/`, `bugfix/`, `hotfix/`

4. **Release Process:**
   - Create release branch from develop: `release/v1.0.0`
   - Apply hotfixes there
   - Merge back to main and develop
   - Tag version on main

---

**Last Updated:** 2026-03-22
