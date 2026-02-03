# Git Version Control & Workflow Guide

This document outlines the industry-standard version control practices, branching strategies, and workflows adopted for this project. All contributors are expected to follow these guidelines to ensure collaboration efficiency and code stability.

## 1. Prerequisites Use

Before contributing, ensure you have Git installed and configured.

### Authentication
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

*Alternatively, you can use a Personal Access Token (PAT) with HTTPS, though SSH is preferred for ease of use.*

---

## 2. Branching Strategy

We follow a **Feature Branch Workflow** (often adapting elements of Git Flow or GitHub Flow).

### Branch Types
- **`main` (or `master`)**:
  - The single source of truth.
  - Contains production-ready code.
  - **Never** push directly to `main`.
- **`develop`** (Optional, depending on project scale):
  - Integration branch for testing before merging to main.
- **Feature Branches**:
  - Used for new features, bug fixes, or refactoring.
  - **Naming Convention**: `category/description`
    - `feature/user-auth`
    - `fix/login-page-error`
    - `refactor/database-schema`
    - `docs/update-readme`
  - **Do NOT** use personal names (e.g., `shuvo`, `john-doe`) for branch names.

### The Lifecycle of a Branch
1. **Create** a branch from `main`:
   ```bash
   git checkout main
   git pull origin main
   git checkout -b feature/new-awesome-feature
   ```
2. **Work** on your changes.
3. **Push** the branch to the remote repository.
4. **Open a Pull Request (PR)** targeting `main`.

---

## 3. Core Git Commands

### Repository Basics
- **Initialize**: `git init` (if starting from scratch)
- **Clone**: `git clone <repo-url>`
- **Status**: `git status` (Check staged/unstaged changes)
- **Log**: `git log --oneline --graph` (View commit history)

### Staging & Committing
- **Stage files**:
  ```bash
  git add <filename>      # Stage specific file
  git add .               # Stage all changes
  ```
- **Commit**:
  ```bash
  git commit -m "feat: implement user login controller"
  ```
  *Use specific prefixes like `feat:`, `fix:`, `chore:`, `docs:` in commit messages.*

### Synchronization
- **Fetch**: `git fetch --all` (Download remote changes without merging)
- **Pull**: `git pull origin main` (Fetch and merge)
- **Push**: `git push origin <branch-name>`

---

## 4. Advanced Operations

### Stashing
Temporarily save changes without committing, useful when switching contexts.
```bash
git stash              # Save changes
git stash list         # View stashed entries
git stash pop          # Apply and remove latest stash
```

### Undo Changes
- **Soft Reset** (Undo commit, keep changes staged):
  ```bash
  git reset --soft HEAD~1
  ```
- **Mixed Reset** (Undo commit, unstage changes, keep file contents):
  ```bash
  git reset --mixed HEAD~1
  ```
- **Hard Reset** (Undo commit, destroy all changes - **Dangerous**):
  ```bash
  git reset --hard HEAD~1
  ```
- **Revert** (Create a new commit that undoes a previous one - **Safe for public history**):
  ```bash
  git revert <commit-hash>
  ```

### Rebasing
Used to keep a clean history by moving your feature branch changes on top of the latest `main`.
```bash
git checkout feature/my-feature
git fetch origin main
git rebase origin/main
```
*Note: Never rebase shared branches (like `main`) as it rewrites history.*

### Cherry-Picking
Applying a specific commit from one branch to another.
```bash
git cherry-pick <commit-hash>
```

---

## 5. Collaboration & Pull Requests

1. **Push your branch**: `git push -u origin feature/my-feature`
2. **Create a Pull Request (PR)** on GitHub/GitLab.
   - **Title**: Clear and descriptive (e.g., "Add JWT Authentication").
   - **Description**: Explain *what* changed and *why*. List steps to test.
3. **Code Review**: Team members review the code. Address comments and push fixes to the same branch.
4. **Merge**: Once approved, merge the PR into `main`.

### Merging vs Rebasing in PRs
- **Merge Commit**: Preserves history of the feature branch. Good for tracing complete features.
- **Squash & Merge**: Combines all branch commits into one. Keeps `main` history clean. (Preferred for small features)

---

## 6. Training & Onboarding Tasks

New to the project? Try these steps to get familiar with our workflow:

1.  **Fork & Clone**: Fork this repo and clone it locally.
2.  **Branching**: Create a branch `onboarding/your-name-intro`.
3.  **Changes**: Add your details to a contributors file.
4.  **Commit & Push**: Commit with a message "docs: add contributor details". Push to origin.
5.  **PR**: Open a dummy Pull Request to practice the flow.

---

## Why Branching Strategies Matter?
- **Isolation**: Features and bugs are isolated; breaking changes in a feature don't affect `main`.
- **Parallel Development**: Multiple developers can work on different features simultaneously.
- **Code Review**: PRs facilitate review before code enters production, reducing bugs.
- **Clean History**: Well-managed branches and commits make it easier to track when features were introduced or bugs appeared.
