---
name: git-workflow
description: >-
  Index skill for the team Git workflow: protected integration branch (usually
  develop), topic branches (feat/*, refactor/*, fix/*), push/PR flow; PRs always
  target the integration branch (not main). OpenSpec propose/apply/archive git
  gates (proposal on the integration branch before apply, archive from it after
  merge). Points to commit-conventions and pull-request-drafting. Open PRs ready
  for review by default (not draft). Use when starting Git work, push to the
  integration branch fails, OpenSpec lifecycle git checks, mapping repo workflow,
  or onboarding agents.
---

# Team Git workflow (skill index)

**Repos:** this workspace follows the Git rules below; adjust branch names and host
to the actual repository.

## When to read this skill

- You are about to **commit, push, or open a PR** and need **branch-protection** context in one place.
- **`git push origin <integration-branch>`** failed (e.g. **GH013** / "Changes must be made through a pull request").
- You are running **OpenSpec** propose, continue, apply, verify, archive, or worktree flows where proposal artifacts, branches, merges, or archive timing affect git history.
- The user asks how **Git skills** fit together.

## Rules (short)

| Topic | Rule |
|-------|------|
| Integration branch | Usually **branch-protected** (often `develop`) — do not rely on pushing new commits directly to it. |
| Where to commit | **Topic branch** from an updated integration branch. |
| Branch names | `feat/`, `fix/`, `refactor/`, `chore/`, `docs/`, `test/` + short-kebab-case (full table in **commit-conventions**). |
| Land work | `git push -u origin <topic-branch>` → open **PR** with **`--base <integration-branch>`** (always; not `main`) → merge into the integration branch. |

## Minimal flow

```bash
git fetch origin && git checkout develop && git pull origin develop
git checkout -b refactor/your-topic
# … stage, commit …
git push -u origin refactor/your-topic
# PR: --head = your topic branch, not develop (unless team says otherwise)
```

If you already committed on the local integration branch: `git branch refactor/your-topic` (captures commits), then `git checkout develop && git reset --hard origin/develop`, then `git checkout refactor/your-topic` — or cherry-pick onto a new branch (see **commit-conventions**).

## OpenSpec git discipline

Every OpenSpec state change must cross the **integration branch** before the next lifecycle phase depends on it.

- Propose/continue artifacts may be drafted on a branch, but must be committed and merged to the **integration branch** before apply starts.
- Apply may run on the integration branch, a topic branch, or a worktree only if that exact proposal change is already available on the **integration branch**.
- Archive may run only from the **integration branch** after implementation is merged back.

Never create commits, branches, or merges unless the user explicitly asks.

### OpenSpec gates

| Moment | Gate |
| --- | --- |
| Before propose | Prefer the integration branch; if not, warn and ask whether to continue intentionally. |
| During continue | Before creating the next artifact, ask the user to commit completed artifact changes or explicitly continue without that checkpoint. |
| After propose | Ask the user to commit proposal artifacts; offer to create a PR branch for review. |
| Before apply | Confirm the proposal change is committed on the **integration branch**; then apply may run from it, a topic branch, or a worktree. |
| Before archive | Stop unless implementation is merged back to the **integration branch** and archive is running from it. |
| After archive | Ask the user to commit archive/spec sync changes. |

### Required checks

**Before apply:**

1. Run `git status --short`.
2. Verify `openspec/changes/<change>/` has no uncommitted proposal files.
3. Verify the proposal change exists on the **integration branch** before applying from any branch/worktree.

Use this language if the proposal has not reached the integration branch:

> I should not apply this yet because the proposal change has not reached `develop`. A proposal can be drafted on a branch, but apply must start only after that proposal state is available on `develop`. Please merge or commit the proposal to `develop` first, then I can apply from `develop`, a topic branch, or a worktree.

**Before archive:**

1. Run `git branch --show-current` and `git status --short`.
2. Stop if not on the **integration branch**.
3. Stop if implementation work has not been merged back to the **integration branch**.

Use this language:

> I should not archive this yet because archive must run from `develop` after implementation is merged back. Verify makes a change eligible to merge; it does not replace the merge.

### OpenSpec red flags

- Applying a proposal that exists only on the current branch/worktree.
- Treating worktree visibility as proof that the proposal reached the **integration branch**.
- Creating the next continue artifact without asking about committing the previous one.
- Archiving from a feature branch or before implementation is merged to the **integration branch**.
- Auto-committing, branching, or merging without explicit user approval.

All of these mean: pause, explain the boundary, and ask the user to make the git state explicit.

## Related skills (read next)

| Skill | Use for |
|-------|---------|
| **commit-conventions** | `[Add]` / `[Fix]` / `[Update]` / `[Refactor]` **subject lines**, full **branch naming** and **WIP-on-integration-branch** handling. |
| **pull-request-drafting** | **PR body** (template, tables, change-based mermaid), **`gh pr create`**, **enterprise `--repo`/`GH_HOST`**, **legacy → new repo** sync merge. |

## GitHub CLI

```bash
gh pr create --base develop --head YOUR_TOPIC_BRANCH --title "[Tag] Subject" --body-file pr-body.md
```

**Do not** pass **`--draft`** unless the user **explicitly** asks for a draft PR; default to **ready for review** (normal `gh pr create` without `--draft`). Always use **`--base <integration-branch>`** (e.g. `develop`) when opening PRs — do not target `main` unless the user explicitly overrides with a documented exception.

## Agent checklist

1. Do not assume pushing directly to the integration branch succeeds when it is branch-protected.
2. Prefer **`feat/*` / `refactor/*` / `fix/*`** before the first commit on new work.
3. **Commit message only** → **commit-conventions**; **PR description / `gh` / sync merge** → **pull-request-drafting**.
4. **OpenSpec apply/archive** → confirm proposal and implementation state on the **integration branch** (see OpenSpec git discipline above).
