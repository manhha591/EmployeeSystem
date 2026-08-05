---
name: commit-conventions
description: >-
  Formats Git commit subject lines and aligns them with the team PR title tags
  ([Add], [Fix], [Update], [Refactor]). Covers branch workflow when the
  integration branch (usually develop) is protected (use feat/*, refactor/*, ...).
  Use when writing commits, amending messages, splitting commits, or when the user
  asks for team-style commit messages. Complements the repo
  .github/pull_request_template.md and the pull-request-drafting skill for PR
  descriptions. Index: git-workflow.
---

# Commit message conventions

## When to use

- Drafting or rewriting **Git commit** first lines, or any workspace where **`.github/pull_request_template.md`** defines the same title tags.
- The user asks for **commit message format**, **conventional tags**, or **hand-written commits** matching the team PR style.

## Title tags (first line of commit and of PR title)

Use **exactly one** tag at the start of the subject:

| Tag | When |
|-----|------|
| `[Add]` | Introduce a new feature. |
| `[Fix]` | Resolve a bug. |
| `[Update]` | Refine or improve an existing feature. |
| `[Refactor]` | Update code style, refactor structure, or remove unused code. |

**Format (subject line only):**

```text
[Tag] Short subject (imperative mood)
```

**Examples:**

- `[Add] Observe message stream use cases for home screen`
- `[Fix] Session address normalization for colon format`
- `[Update] Align minSdk to 31 across library modules`
- `[Refactor] Move shared helpers from util to core.util package`

## Alignment with the repository

1. If **`.github/pull_request_template.md`** exists in the workspace, treat its **PR Title Tags** block as authoritative; commit subjects should use the **same tags** so `git log` matches PR titles.
2. For **PR title + full body** (Description, Related Issue, Checklist, Screenshots), use the **pull-request-drafting** skill or read the template directly.
3. For a **compact map** of Git rules and which skill to open next, use **git-workflow**.

## Branch workflow (protected integration branch)

In many repos the **integration branch (often `develop`) is branch-protected**: pushing to it is rejected with a rule such as **"Changes must be made through a pull request"** (e.g. `GH013`).

**Do not** make project commits on a local integration branch with the intent to push it. Those pushes fail; work must land via a **topic branch** and a **PR**.

**Recommended flow**

1. Sync: `git fetch origin && git checkout develop && git pull origin develop` (or `git reset --hard origin/develop` if the branch should match remote exactly and you have no local-only commits to keep).
2. **Branch off the integration branch** before committing:
   ```bash
   git checkout -b refactor/short-topic-name
   ```
3. **Commit only on the topic branch**, then `git push -u origin <branch>` and open a **ready-for-review** PR with **base the integration branch** (always, not `main`; do **not** use `gh pr create --draft` unless the user asks for a draft).

**Topic branch naming (prefix + short-kebab-case)**

| Prefix | Use for |
|--------|---------|
| `feat/` | New feature or user-visible capability |
| `fix/` | Bugfix |
| `refactor/` | Structure, layering, or cleanup (behavior unchanged or internal-only) |
| `chore/` | Tooling, build, CI, repo hygiene |
| `docs/` | Documentation only |
| `test/` | Tests only |

Examples: `refactor/module-architecture`, `feat/settings-sync`, `fix/device-registration`.

If the user is already on the integration branch with uncommitted changes, create the branch **before** the first commit: `git checkout -b refactor/…` (Git moves the WIP onto the new branch).

## Related Issue line (optional body line)

When the fix maps to a tracked issue, a body line may reference it (same as PR template):

- GitHub: `Fix #xx` with the issue id.

## Edge cases

- **Merge commits**: Keep the default merge message; do not force the tag format on merges.
- **History note**: Older commits may show `[Remove]`; for **new** work, prefer **`[Refactor]`** when the change is structural cleanup or removal of unused code, if the template applies.

## Agent checklist

1. If the remote enforces PR-only updates: **never push to the integration branch**; use a **`feat/*` / `fix/*` / `refactor/*` / …** branch and open a PR.
2. Pick the single best tag from the table (not multiple tags on one line).
3. Use **imperative mood** (*Add*, *Fix*, *Update*, *Refactor* implied by tag; subject is a command: "Add X", not "Added X").
4. Keep the first line **≤ ~72 characters** when practical; put details in the body.
5. For multi-commit branches, use **one tag per commit** that matches that commit's scope (do not label a small fix commit `[Refactor]` just because the branch theme is refactor).
