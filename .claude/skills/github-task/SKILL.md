---
name: github-task
description: AdAstra GitHub procedure — creating a task (issue on the "AdAstra Kanban" board) and branch, and step-by-step commits. Push, PRs and merges are done by the user. Use when starting a new task or mechanic, or committing a finished step.
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
`feat/`, `fix/`, `refactor/`, `docs/` or `chore/` (setup, tooling, config) + kebab-case description. One branch per task. By default it starts from the current task branch (stacked branches, merged in order); start from `main` only when the user says so.

## 2. Work in steps
Work in coherent increments. After each one, stop and report:
- **What** changed — files, classes, methods/fields.
- **Why** — the cause (bug, physics timing, a rule from `AGENTS.md`).
- **How** — the technical mechanism.

Propose a Conventional Commits message, ask the user to review `git diff`, and wait for confirmation before committing and moving on. Every step must leave the project compiling in Unity with 0 errors.

## 3. Finish the task
The agent does not push, open PRs, merge or close issues — the user does all of that. When the last step is committed:
1. Check the Definition of Done in `AGENTS.md`.
2. Tell the user the task is ready: branch name, the task's commits (`git log --oneline <base>..HEAD`) and anything left open.
3. If the user asks for help with the PR, give a ready title in Conventional Commits style (e.g. `feat: implement docking magnet mechanism`) and the body `ref: <issue link>` — nothing more; the issue holds the description.
