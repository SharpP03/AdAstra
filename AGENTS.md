# AdAstra

Unity 6 (`6000.2.6f2`, URP) MVP of a 6DoF space exploration game: fly a ship, manage fuel and hull, outrun the Annihilation Wave. It is the author's engineering thesis project. Everything compiles into `Assembly-CSharp` — there are no custom `.asmdef` files and no automated tests, so verification happens in the Unity Editor.

## Starting a task
Before starting a new task or mechanic, ask the user whether to create a branch and a GitHub Projects task. The procedure (issue format, branches, commits, PR, merge) is in `.claude/skills/github-task/SKILL.md`. Scope and decisions are agreed in chat; issues are only for the user's overview.

## Hard rules
- Never commit or push to `main` without the user's explicit consent.
- Keep thesis annotations (`#region EDUCATION NOTE`, `DO PRACY INŻ.`). Update them when the logic they describe changes.
- Every asset has a `.meta` file with a GUID. Never delete or hand-write `.meta` files; when moving or renaming a file, move its `.meta` with it. Class name = file name.
- Change scenes and prefabs through the Unity Editor (MCP tools), not by hand-editing their YAML. If the editor is unavailable and the task needs it, tell the user instead of guessing.
- Do not modify third-party packages: `Assets/AssetStore/`, `Assets/Plugins/`, `Assets/ThridParty/`.
- Do not add new global singletons without the user's approval.
- After changes, check `git status` for unintended edits to `ProjectSettings/`, `Packages/packages-lock.json` or scenes you did not touch.

## Documentation map (`Assets/DOCS/`)
- `ARCHITECTURE.md` — read before writing or changing C# code: lifecycle, input and physics timing, references, events, prefab discipline, Definition of Done.
- `SYSTEMS.md` — status and checklist of each mechanic. Check the entry before working on a mechanic and update it when done.
- `GAMEPLAY.md` — design intent; read when a task involves gameplay decisions.
- Architectural changes must update these docs, which describe the implementation as it is, never planned designs. Routine fixes need no doc entry.
- Ignore: `README.md` (legacy), `TODO.md` (roadmap, not a source of truth), `THESIS_NOTES.md`, `raports/`, `Poster/` — thesis material for the user, not guidelines.
- Never read generated folders: `Library/`, `Temp/`, `Logs/`, `obj/`, `UserSettings/`, `*.csproj`, `*.sln`.
