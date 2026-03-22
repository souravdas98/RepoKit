# 🚀 Push RepoKit to GitHub

> Step-by-step guide to push your local repository to GitHub

## ✅ Completed Locally

Your project has been initialized with:
- ✅ Git repository created
- ✅ 37 files committed
- ✅ Initial commit: `9cbf007 feat: initial RepoKit project with CI/CD infrastructure`
- ✅ On branch: `main`

## 📋 Next Steps

### Step 1: Create Repository on GitHub

1. Go to [GitHub.com](https://github.com)
2. Click **"+"** → **"New repository"**
3. Fill in:
   - **Repository name:** `RepoKit`
   - **Description:** `A lightweight .NET repository pattern library with automatic DI registration`
   - **Visibility:** Public (if sharing) or Private
   - **Initialize:** DO NOT check any boxes (we have files already)
4. Click **"Create repository"**

### Step 2: Add Remote & Push

**Copy your repository URL from GitHub** (looks like `https://github.com/YOUR_USERNAME/RepoKit.git`)

Then run these commands:

```bash
# Add remote (replace with your URL)
git remote add origin https://github.com/YOUR_USERNAME/RepoKit.git

# Verify remote
git remote -v

# Rename branch to main (usually already done)
git branch -M main

# Push to GitHub
git push -u origin main
```

### Step 3: Verify

After pushing:
1. Refresh your GitHub repository page
2. You should see all your files and the commit history
3. Go to **Settings** → **General** to verify:
   - Default branch is `main` ✅
4. Go to **Actions** tab to see workflows

---

## 🔐 Authentication

### Using HTTPS (Simpler)

If GitHub prompts for password:
- Use your **GitHub username**
- Use a **Personal Access Token** (not password)

To create a PAT:
1. Go to [GitHub Settings → Developer Settings → Personal Access Tokens](https://github.com/settings/tokens)
2. Click "Generate new token"
3. Select scopes: `repo`, `workflow`
4. Copy token and use as password

### Using SSH (More Secure)

```bash
# Generate SSH key (if you don't have one)
ssh-keygen -t ed25519 -C "your_email@example.com"

# Add to SSH agent
ssh-add ~/.ssh/id_ed25519

# Add public key to GitHub: Settings → SSH and GPG keys → New SSH key
cat ~/.ssh/id_ed25519.pub  # Copy this

# Use SSH remote URL instead:
git remote add origin git@github.com:YOUR_USERNAME/RepoKit.git
```

---

## ✨ Quick Commands

### One-liner (if starting fresh):

```bash
cd /Users/sourav/Desktop/nuget/RepoKit

# Set remote (replace URL)
git remote add origin https://github.com/YOUR_USERNAME/RepoKit.git

# Push to GitHub
git push -u origin main
```

### Alternative: Using GitHub CLI

```bash
# Install GitHub CLI first: https://cli.github.com

# Login to GitHub
gh auth login

# Create repository on GitHub
gh repo create RepoKit --public --source=. --remote=origin --push

# Done! Your code is now on GitHub
```

---

## 🔄 Next: Setup Branch Protection

Once pushed, setup branch protection rules:

### Using GitHub CLI:

```bash
# Configure main branch protection
gh api repos/YOUR_USERNAME/RepoKit/branches/main/protection \
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

### Or Manually:

1. Go to **Settings** → **Branches**
2. Click **"Add rule"**
3. Follow [BRANCH_PROTECTION.md](../BRANCH_PROTECTION.md)

---

## 📝 Setup Secrets

After pushing, configure GitHub Secrets:

1. Go to **Settings** → **Secrets and variables** → **Actions**
2. Add these secrets:

```
NUGET_API_KEY
  Source: https://www.nuget.org/account/apikeys
  Scope: "Push new packages"

SONAR_TOKEN (optional)
  Source: https://sonarcloud.io/account/security

SLACK_WEBHOOK_URL (optional)
  Source: Slack incoming webhooks
```

---

## ✅ Verify Everything

Once pushed, check:

- ✅ Repository visible on GitHub
- ✅ All files present
- ✅ Git history complete
- ✅ Workflows file exist (`.github/workflows/`)
- ✅ README.md displays properly
- ✅ License badge works

---

## 🚀 First CI/CD Run

After setup:

1. **Workflows will test automatically** when you:
   - Push to `main` or `develop`
   - Create a Pull Request
   - Manually trigger from Actions tab

2. **First run may take 15-20 minutes** (multi-OS, multi-.NET version)

3. **Check status:**
   - Go to **Actions** tab
   - Click workflow run
   - View logs to see progress

---

## 💡 Tips

- 📌 **Keep main branch protected** - require PR reviews
- 🔄 **Use develop for integration** - less strict rules
- 🌿 **Create feature branches** - `feature/your-feature`
- ✍️ **Follow commit format** - `feat:`, `fix:`, `docs:`
- 🏷️ **Use semantic versioning** - `v1.0.0`, `v1.0.1`
- 📝 **Update CHANGELOG.md** - before each release

---

## 🆘 Troubleshooting

### ❌ "fatal: remote origin already exists"

```bash
git remote remove origin
git remote add origin https://github.com/YOUR_USERNAME/RepoKit.git
```

### ❌ "Permission denied" (SSH issues)

```bash
# Check if SSH key is working
ssh -T git@github.com

# If not, add key to agent
ssh-add ~/.ssh/id_ed25519
```

### ❌ "Refused to create repository" (GitHub CLI)

```bash
# Make sure you're logged in
gh auth login

# Try again
gh repo create RepoKit --public --source=. --remote=origin --push
```

### ❌ Workflow not running

- Workflows need to complete once before they show as required status checks
- Check `.github/workflows/` folder exists
- Review workflow YAML syntax
- Check Actions tab for errors

---

## 📚 Resources

- [GitHub Docs - About Repositories](https://docs.github.com/en/repositories/creating-and-managing-repositories)
- [GitHub Docs - Branch Protection](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches)
- [GitHub CLI Reference](https://cli.github.com/manual/)

---

**Status:** ✅ Ready to Push  
**Files Staged:** 37  
**Branch:** main  
**Next Action:** Create repository on GitHub & run push command

🚀 **You're almost there! Just need to push to GitHub!**
