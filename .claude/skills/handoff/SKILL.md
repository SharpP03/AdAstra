---
name: handoff
description: AdAstra handoff between conversations — write a snapshot of the current task at the end of a session, or resume a task from one in a new session. Use when the user asks to write/update a handoff ("zapisz handoff", "przekaż kontekst"), or starts a session with "kontynuujemy #N" / "przeczytaj handoff".
---

# Handoff between conversations

A handoff carries what git cannot: why things were decided, what failed, what to watch out for, and the next step. One file per task, overwritten at the end of each session (a snapshot, not a log).

- Location: `Assets/DOCS/agent_plans/<issue>-<kebab-slug>.md` (gitignored, local only).
- Language: the user's (Polish).
- Durable rules (valid beyond this task) do not belong here — propose them for `CLAUDE.md` / `AGENTS.md` instead.

## Resume (start of a session)
1. Read the handoff, `git log --oneline main..HEAD` and `git status`.
2. Verify claims before acting on them (files, commits, values) — the code may have moved on.
3. Briefly confirm with the user where you start, then work. Do not edit the handoff during the session.

## Write (only when the user asks, usually at the end of a session)
Overwrite the file with the current state, max ~60 lines:

```markdown
# #<issue> <title> — handoff

**Cel:** <one or two sentences>. Issue: <link>. Branch: `<branch>` (<base / stacking, if any>).

## Status
- [x] Krok 1 — <what> (commit `<hash>`).
- [ ] **Krok 2 — <what>.** Gotowe gdy: <verifiable condition>.

## Decyzje i powody
- <decision> — <why>.

## Pułapki
- <non-obvious trap, failed approach, editor quirk>.

## Otwarte pytania
- <question for the user, if any>.
```

Rules:
- Do not repeat what `git log`, the code, `CLAUDE.md` or `AGENTS.md` already say.
- Steps are coherent, testable increments with a "Gotowe gdy" condition; finished steps get their commit hash.
- When the task is merged, the user decides whether the file is deleted.
