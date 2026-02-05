# Git Version Control & Workflow Guide

This document outlines the industry-standard version control practices, branching strategies, and workflows adopted for this project. All contributors are expected to follow these guidelines to ensure collaboration efficiency and code stability.

---

## 1. Remote Server Configuration & Multi-Remote Sync

In this project, we manage synchronization between **Local Development**, **Staging/Dev Server (Origin)**, and **Production Server (Office)**.

| Remote Name | Environment | Server/URL |
| :--- | :--- | :--- |
| `origin` | **Development / Staging** | `https://github.com/shuvo-art/ERP_SYSTEM` |
| `office` | **Production** | *(Your Office/Production URL)* |

### Set up remotes:
```bash
# Add origin if not present
git remote add origin https://github.com/shuvo-art/ERP_SYSTEM

# Add production remote
git remote add office <production-repo-url>

# Verify remotes
git remote -v
```

---

## 2. Authentication

We recommend using **SSH** for secure and convenient authentication with GitHub.

**Setting up SSH:**
1. Generate an SSH key pair:
   ```bash
   ssh-keygen -t ed25519 -C "your-email@example.com"
   ```
2. Add the public key (`~/.ssh/id_ed25519.pub`) to your GitHub account under **Settings > SSH and GPG keys**.
3. Verify connection:
   ```bash
   ssh -T git@github.com
   ```

---

## 3. Branching Strategy

We follow a **Feature Branch Workflow**, strictly isolating feature development from stable branches.

### Branch Types
- **`master` (or `main`)**:
  - The single source of truth.
  - Contains production-ready code.
  - **Never** push directly to `master`.
- **Feature Branches**:
  - Used for new features, bug fixes, or refactoring.
  - **Naming Convention**: `category/description`
    - `feat/user-auth`
    - `fix/login-page-error`
    - `refactor/database-schema`
    - `docs/update-readme`
  - **Do NOT** use personal names (e.g., `shuvo`) for branch names.

---

## 4. The Development Life Cycle

### Step 1: Feature Development
Never code directly on `master`. Always create a feature branch from the latest `master`.
```bash
git checkout master
git pull origin master
git checkout -b feat/your-feature-name
```

### Step 2: Continuous Integration (Push to Dev/Origin)
When your feature is ready, push it to `origin` for testing or code review.
```bash
git add .
git commit -m "feat(scope): detailed description of changes"
git push origin feat/your-feature-name
```

---

## 5. Synchronization & Deployment Flow

### Phase A: Synchronize Staging (Origin/Dev)
Once the feature is tested in its branch, merge it into the local master and push to the **Dev Server (`origin`)**.

1. **Merge to local master:**
   ```bash
   git checkout master
   git merge feat/your-feature-name
   ```
2. **Push to Development Server:**
   ```bash
   git push origin master
   ```
3. **Delete the local feature branch (Cleanup):**
   ```bash
   git branch -d feat/your-feature-name
   ```

### Phase B: Synchronize Production (Office/Prod)
After the code is verified on the Development Server (`origin/master`), promote it to **Production (`office/master`)**.

1. **Ensure local master is up-to-date:**
   ```bash
   git checkout master
   git pull origin master
   ```
2. **Push to Production Server:**
   ```bash
   git push office master
   ```

---

## 6. Core Git Commands

### Staging & Committing
- **Stage files**: `git add .`
- **Commit**: `git commit -m "feat: implement user login controller"`
  - *Use prefixes: `feat:`, `fix:`, `chore:`, `docs:`.*

### Undo Changes
- **Soft Reset** (Keep changes staged): `git reset --soft HEAD~1`
- **Hard Reset** (Destroy changes): `git reset --hard HEAD~1`
- **Revert** (Safe undo for history): `git revert <commit-hash>`

### Stashing
```bash
git stash              # Save changes
git stash list         # View entries
git stash pop          # Apply latest
```

---

## 7. Advanced Operations

### Rebasing
Used to keep a clean history by moving your feature branch changes on top of the latest `master`.
```bash
git checkout feature/my-feature
git fetch origin master
git rebase origin/master
```

### Emergency Fixes (Hotfixes)
If a bug is found in Production (`office`):
1. Branch off `master`.
2. Apply the fix.
3. Merge to `local master`.
4. Push to `origin master` (Test first).
5. Push to `office master` (Apply to Prod).

---

## 8. Summary Tracking Command
To verify that all three (Local, Origin, Office) are in sync:
```bash
git fetch --all
git branch -vv
```
*Your `master` should ideally show as `[origin/master]` and be at the same commit hash as `office/master`.*

---

## 9. Collaboration & Pull Requests
1. **Push your branch**: `git push -u origin feature/my-feature`
2. **Create a Pull Request (PR)** on GitHub.
3. **Code Review**: Team reviews and addresses comments.
4. **Merge**: Once approved, merge using **Merge Commit** or **Squash & Merge**.
