---
name: github-task
description: AdAstra GitHub procedure — creating a task (issue on the "AdAstra Kanban" board) and branch, step-by-step commits, opening a PR, squash merge and closing the issue. Use when starting a new task or mechanic, committing a finished step, or when the user asks to open a PR or merge.
---

# GitHub task procedure

Board: https://github.com/users/SharpP03/projects/3/views/1 ("AdAstra Kanban"). Repo owner: `SharpP03`.

## 1. Start
Ask: "Do you want to create a new branch and a GitHub Projects task?" (ask in the user's language).
- **No** → work on the current branch as agreed in chat. Follow section 2; no issue or PR.
- **Yes** → create the issue, then the branch.

### Issue
- **Granularity:** one coherent feature, fix or subsystem change that can be tested and reviewed on its own. No micro-tasks (one line, one file) unless it is an isolated hotfix. No monster tasks bundling independent subsystems. Group related work — e.g. "Player flight controls and dynamic camera", not five tasks for input, forces, damping and camera.
- **English only.** Short, domain-focused title — never a changelog or file list.
- **Body** — `Scope` items are verifiable behaviors; file and class names are secondary:
  ```markdown
  ## Objective
  <problem, design intent or bug>

  ## Scope
  - [ ] <verifiable behavior / deliverable>

  Created by <Model Name> on SharpP03's behalf
  ```
- Write the body to a temporary file and pass it with `--body-file` (avoids PowerShell quoting issues):
  `gh issue create --title "<title>" --body-file <file> --project "AdAstra Kanban" --assignee "@me"`
- A permission error on `--project` means the token lacks the `project` scope — ask the user to run `gh auth refresh -s project`.

### Branch
From an up-to-date `main`: `feat/`, `fix/`, `refactor/`, `docs/` or `chore/` (setup, tooling, config) + kebab-case description.

## 2. Work in steps
Work in coherent increments. After each one, stop and report:
- **What** changed — files, classes, methods/fields.
- **Why** — the cause (bug, physics timing, a rule from `AGENTS.md`).
- **How** — the technical mechanism.

Propose a Conventional Commits message, ask the user to review `git diff`, and wait for confirmation before committing and moving on. Every step must leave the project compiling in Unity with 0 errors.

## 3. Pull request
Before opening: the Definition of Done in `AGENTS.md` is met.
- Title: Conventional Commits style, e.g. `feat: implement docking magnet mechanism`.
- Body: only `ref: <issue link>` — no summary, no "Generated with" footer; the issue holds the description.
- Push the branch, `gh pr create --title "<title>" --body-file <file>`, then give the user the PR title and link.

## 4. Merge and close
Only after the user agrees the task is done, ask: "Do you want to squash merge into main?" On yes:
1. `gh pr merge --squash`, then switch to `main` and pull.
2. Close the issue: `gh issue close <n> --comment "Resolved and merged in PR #<pr>"` (this moves the card to Done). For a draft board item without an issue, set its status to Done with `gh project item-edit` or ask the user to move it.
3. Tell the user the merge is done and the task closed.
