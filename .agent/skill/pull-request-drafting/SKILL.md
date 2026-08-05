---
name: pull-request-drafting
description: >-
  Draft pull requests with the team template, title tags, reviewer-first
  descriptions, GitHub CLI, and change-based diagrams (class, sequence, flowchart,
  state, C4-inspired container). Pick diagram type from what changed — layer
  boundary, async flow, contracts, guards, modes, external integration — not PR
  size; see diagram-guide.md. Base the integration branch (usually develop), not
  main; ready-for-review by default. Topic branches; legacy repo sync merge recipe.
  Index: git-workflow.
---

# Pull request drafting

## When to use

- The user wants a **GitHub PR title and body** with the team conventions.
- The user asks to **open a PR**, **draft PR text**, **improve PR description**, **reviewer-first body**, **merge a legacy branch into the new remote**, or align with **`.github/pull_request_template.md`**.

## Workflow

0. **Branch (before any commit or push)** — The **integration branch (often `develop`) is usually branch-protected**; pushing directly is rejected. **Do not commit on it** intending to push there. Create a **topic branch first** (`git checkout -b refactor/short-name` or `feat/…` / `fix/…` — see **commit-conventions** § Branch workflow), commit there, then `git push -u origin <branch>` and open the PR. If the user already committed on the local integration branch, create a branch at `HEAD` and reset the integration branch to its remote (or cherry-pick onto a new branch).
1. **Read the template** in the repo: `.github/pull_request_template.md`. Match its sections (Description, Related Issue, Checklist, optional Screenshots).
2. **Title**: Start with one tag from the template: `[Add]`, `[Fix]`, `[Update]`, or `[Refactor]`, followed by an **imperative**, concise subject (e.g. `[Fix] Register device proximity registration and BLE serial addressing`).
3. **Description (reviewer-first)** — See **§ Description depth** below. Do **not** paste a long **commit list**; that duplicates `git log` / the PR "Commits" tab. Prefer a short narrative plus **tables of types** (new/renamed/removed classes, modules, entry points).
4. **Diagrams (change-based)** — Read **[diagram-guide.md](diagram-guide.md)** before drawing. Summary:
   - **Classify by change signal** in the diff (not LOC or file count): hygiene only, layer boundary, async/ordering, contract surface, branching/guards, state/mode, external integration, persistence path, non-obvious fix, UI presentation.
   - **Map signal → diagram type:**
     - **Layer boundary** → layer `flowchart` (`:presentation` → `:domain` → `:data` → external).
     - **Async / ordering** → `sequenceDiagram` (sync, stream, pagination, Flow chain).
     - **Contract surface** → `classDiagram` (≤12 types; skip if types table is enough).
     - **Branching / guards** → control-flow `flowchart`.
     - **State / mode** → `stateDiagram-v2`.
     - **External integration** → container `flowchart`; add `sequenceDiagram` for happy path if needed.
     - **Hygiene only** or **UI presentation** (no logic) → **no Mermaid** (screenshot for UI).
   - **At most 2** diagrams per PR — only for **distinct** reviewer questions not answered by prose + tables. Link ADR/OpenSpec instead of a third diagram.
   - Place under headings named for the question (**### Architecture**, **### Flow**, **### Contracts**, **### States**, **### Fix**); verify rendering (**§ Diagrams on GitHub**).
5. **Related Issue**: use `Fix #id` when this closes a GitHub issue; otherwise leave a placeholder or omit.
6. **Checklist**: copy from `.github/pull_request_template.md`; leave boxes unchecked unless the user confirms.
7. **Security**: Do **not** commit or paste secrets, tokens, or internal-only URLs into PR bodies; use placeholders.
8. **GitHub CLI** (optional): If `gh` is installed and authenticated, suggest:

   ```bash
   gh pr create --title "[Tag] Subject" --body-file pr-body.md --base develop --head YOUR_TOPIC_BRANCH
   ```

   Always pass **`--base <integration-branch>`** (not `main`). **`--head`** must be a **pushed topic branch** (e.g. `refactor/cleanup`, `feat/foo`); it is **not** the integration branch when that branch is protected — you cannot push commits to it directly.

   **Enterprise (self-hosted GitHub) — required for `gh`:** Without the host in `--repo`, `gh` may call `api.github.com` and return **401**. Always pass the full repo slug:

   ```bash
   gh pr create --repo <host>/<org>/<repo> --base develop --head YOUR_TOPIC_BRANCH --title "[Tag] Subject" --body-file pr-body.md
   ```

   Alternative: `GH_HOST=<host>` for that shell session, then `gh pr create ...` as usual.

   **Draft PRs:** Do **not** use **`--draft`** by default. Create **ready-for-review** PRs unless the user explicitly requests a draft.

## Description depth (reviewer-first PR bodies)

Goal: a reviewer should answer **"what do I need to read and what behavior changed?"** without replaying commits.

### Lead with (short)

1. **Motivation** — 1–3 sentences: problem or goal (e.g. align notifications with domain layer, fix crash on X).
2. **Scope** — what is in / **out of scope** (stops scope creep debates).
3. **User-visible or API behavior** — what changes for app, callers, or backend contract (if any).

### Prefer tables over commit bullets

| Instead of… | Use… |
|-------------|------|
| Pasting 8 commit subjects | "See **Commits** tab" or one line: "8 commits; split by layer for history." |
| Listing every touched file | Group by **module** and **role** (see templates below). |
| Vague "refactored messaging" | Table: **type → name → module → one-line purpose**; mark **new / renamed / removed**. |

**Module inventory (typical Android multi-module; adjust to the repo)**

| Module | Changes (summary) |
|--------|-------------------|
| `:domain` | … |
| `:data` | … |
| `:presentation` | … |
| `:core` | … |

**Types & entry points (adjust columns to the PR)**

| Kind | Name | Module | Note |
|------|------|--------|------|
| Use case | `ExampleUseCase` | `:domain` | New |
| Repository (interface) | `ExampleRepository` | `:domain` | Renamed from … |
| Repository (impl) | `ExampleRepositoryImpl` | `:data` | … |
| DI module / binding | `XxxModule` | `:data` / `:presentation` | Binds … |
| Service / Activity | `NotificationMessagingService` | `:presentation` | Delegates to … |
| ViewModel | `FooViewModel` | `:presentation` | Now uses … |

Keep tables **scannable** (≤ ~15 rows per table); for huge PRs, summarize and add: "Full list in **Files changed**."

### Diagram placement (change-based)

Follow **[diagram-guide.md](diagram-guide.md)** for the **change-signal router**, templates, and anti-patterns.

**Body order:** motivation → scope → behavior → **tables** → **diagrams** (0–2, only if a change signal requires them) → test plan → OpenSpec/ADR (optional).

**Quick examples (signal → diagram):**

| Change in this PR | Diagram |
|-------------------|---------|
| New `ChatHistoryRepository` + sync from API into Room | Layer `flowchart` + `sequenceDiagram` |
| Badge fetch/clear ordering only | `sequenceDiagram` or control-flow `flowchart` |
| Renamed use case, same behavior | None (table is enough) |
| String/color tweak | None (screenshot if UI) |
| Wrong guard caused double-fetch fix | Before/after `flowchart` |

**One diagram = one reviewer question.** If you cannot name the question, omit the diagram.

### OpenSpec / spec-driven changes (secondary)

When the PR includes **OpenSpec** work (e.g. change **archived** under `openspec/changes/archive/…`, or **deltas merged** into `openspec/specs/<capability>/spec.md`):

- **Do not lead** **## Description** with archive paths, change folder names, or a long list of merged spec files — that is **process metadata**, not the main review story.
- **Preferred order**: **Motivation → scope → user-visible behavior → module / types tables → Mermaid** (when required by **Workflow §4**) — then, if still useful, a short **OpenSpec** subsection **below** the diagram (e.g. `### OpenSpec` with 1–2 lines: what was archived; which canonical specs under `openspec/specs/` changed).
- **Omit** the OpenSpec subsection when it adds no reviewer value (trivial hygiene only), or fold a **single sentence** into **Scope** instead of a dedicated block.

### Diagrams on GitHub (when you paste Mermaid)

GitHub only renders fenced blocks labeled **`mermaid`** (tables are not diagrams).

- One fenced block = one diagram; never nest fences inside the PR body.
- Supported types: `flowchart`, `sequenceDiagram`, `classDiagram`, `stateDiagram-v2`.
- Syntax hygiene (node IDs, `subgraph id [Label]`, reserved words): see **diagram-guide.md § GitHub Mermaid hygiene**.
- After `gh pr create` / `gh pr edit`, **open the PR in the browser** — gray code block means simplify or fix the fence.
- [Creating diagrams](https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/creating-diagrams)

### Test & verification (concrete)

- State **commands run** (e.g. `gradlew :presentation:assembleDebug`) and **manual checks** if UI.
- If not tested, say so honestly; don't check template boxes the team didn't do.

### Risk & rollout

- **Breaking** renames, DI graph changes, manifest, Proguard, or **local-only files** (e.g. a gitignored `google-services.json` — document where to place it).
- **Follow-ups** as a short list (optional WorkManager, tests, docs).

### UI / design PRs

- **Screenshots** or short screen recording; light / dark if relevant.
- **Flow**: "Settings → X → Y".

### When commit history *is* worth mentioning

- **Merge commits**, **revert chains**, or **intentional multi-phase** work: one sentence + link to compare range is enough.

### Brainstorm prompts (before writing)

Use these to classify **change signals** and decide diagrams:

- **Which change signals apply?** (layer boundary, async, contracts, guards, modes, external, persistence, fix-only, hygiene, UI-only)
- **What is the one-line reviewer question** for each candidate diagram?
- **Who is the primary reviewer** (platform, feature owner, security)? What will they search first?
- **What contracts changed** (DTO shapes, deep links, push payload, repository interfaces)?
- **What must work after merge** (smoke path, offline, context switch)?
- **Can prose + tables answer it?** If yes, skip the diagram.

## Agent checklist (when drafting)

1. **Branch**: Confirm work is on a **topic branch** (`feat/*`, `refactor/*`, `fix/*`, …) that was **pushed** to origin; do not assume pushing to the integration branch will succeed when it is protected.
2. **`gh pr create`**: **`--base <integration-branch>`**; **no `--draft`** unless the user asked for a draft.
3. Title: one template tag + imperative subject (see **commit-conventions** skill for subject style).
4. Description: motivation → scope → behavior; **module summary table** + **types/entry-points table**; no long commit list.
5. **Diagrams:** Apply **Workflow §4** + **[diagram-guide.md](diagram-guide.md)** — classify **change signals** (not PR size); add 0–2 Mermaid blocks only when a signal maps to a diagram and tables/prose are insufficient; confirm GitHub rendering (**§ Diagrams on GitHub**).
6. **OpenSpec:** If the PR includes OpenSpec archive/merge, follow **§ OpenSpec / spec-driven changes (secondary)** — place after diagrams (optional `### OpenSpec`), do not lead **## Description** with archive paths; omit the subsection when it adds no reviewer value.
7. **Verification**: real commands / honest gaps.
8. **Risks & local setup** (secrets, gitignored config files, env flags) if applicable.
9. Template sections present: Description, Related Issue, Checklist, Screenshots only when UI.

## Legacy → new repo sync merge (easy path)

Use when the remote repo has been migrated and commits on a legacy branch (e.g. `legacy/develop`) are not yet on the new remote's integration branch. Goal: one branch + PR into the integration branch on the new remote.

### One-time remotes

- **`origin`** → new repo URL
- **`legacy`** → legacy repo URL

```bash
git remote add legacy <legacy-url>   # if missing
```

### Steps (copy-paste friendly)

1. **Clean tree** on the branch you are leaving: stash or commit WIP so checkout/merge is safe.
2. **Fetch both** remotes: `git fetch origin` and `git fetch legacy`.
3. **Inspect gap** (optional): `git log --oneline origin/develop..legacy/develop`
4. **Branch from new integration branch:** pick a sync branch name (example: `sync/legacy-develop`):

   ```bash
   git checkout -B sync/legacy-develop origin/develop
   ```

5. **Merge legacy branch**, preferring **incoming** (`legacy`) on conflicts — matches "port missing product code from old repo" without hand-resolving dozens of files:

   ```bash
   git merge legacy/develop -X theirs -m "Merge legacy/develop into sync branch"
   ```

   - **`-X theirs`** here means: for conflict hunks, keep the version from **`legacy/develop`** (the branch being merged in). Use manual merge or `-X ours` only if the team explicitly wants to keep **new-repo** sides instead.
6. **Push:** `git push -u origin sync/legacy-develop`. If **403** appears, the account lacks **write** access to the new repo (org/team access).
7. **Open PR** with **enterprise `--repo`** (see **§ Workflow → GitHub CLI** above), e.g. base `develop`, head your sync branch.

### If step 5 conflicts remain

Rare with `-X theirs`, but if it happens: `git status`, then per file `git checkout --theirs -- PATH` or `git checkout --ours -- PATH`, then `git add` and `git merge --continue`.

### If histories should not favor legacy

Skip `-X theirs`; run plain `git merge legacy/develop` and resolve conflicts intentionally, or cherry-pick specific SHAs from `git log origin/develop..legacy/develop`.

## Repo-specific notes

- **Integration branch (often `develop`) is branch-protected** in many repos: land changes via **PR from a topic branch** (`refactor/*`, `feat/*`, `fix/*`, …). See **commit-conventions** § Branch workflow and **git-workflow** for the index.
- Commit message conventions may be documented under `.cursor/rules/` (e.g. `commit-messages.mdc`); PR title tags should stay consistent with `.github/pull_request_template.md`.
- A **paste-ready body** can be maintained as `pr-body.md` in the project root when the user prepares a PR locally (not required to commit).

## Optional companion files (personal skill folder)

- `examples.md` — one anonymized past PR body as a pattern (prefer an example that uses **inventory tables** + short narrative).
- `reference.md` — links to the PR template path and commit-message rule in the repo.
