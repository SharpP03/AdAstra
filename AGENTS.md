# AdAstra

Unity 6 (`6000.2.6f2`, URP) MVP of a 6DoF space exploration game: fly a ship, manage fuel and hull, outrun the Annihilation Wave. It is the author's engineering thesis project. Everything compiles into `Assembly-CSharp` — there are no custom `.asmdef` files and no automated tests, so verification happens in the Unity Editor. Keep solutions simple: no speculative abstractions.

## Starting a task
Before starting a new task or mechanic, ask the user whether to create a branch and a GitHub Projects task. The procedure (issue format, branches, commits, PR, merge) is in `.claude/skills/github-task/SKILL.md`. Scope and decisions are agreed in chat; issues are only for the user's overview.

## Hard rules
- Never commit or push to `main` without the user's explicit consent.
- Keep thesis annotations (`#region EDUCATION NOTE`, `DO PRACY INŻ.`). Update them when the logic they describe changes.
- Every asset has a `.meta` file with a GUID. Never delete or hand-write `.meta` files; when moving or renaming a file, move its `.meta` with it. Class name = file name.
- Change scenes and prefabs through the Unity Editor (MCP tools), not by hand-editing their YAML. If the editor is unavailable and the task needs it, tell the user instead of guessing.
- Do not modify third-party packages: `Assets/AssetStore/`, `Assets/Plugins/`, `Assets/ThridParty/` (the misspelling is the real folder name).
- Do not add new global singletons without the user's approval.

## Project conventions
- `Awake()` configures only the object itself (`GetComponent` on the same GameObject); wiring between objects happens in `Start()`. Never poll for dependencies in coroutine loops.
- `GameManager` is the composition root and game-state orchestrator (menu, playing, paused, game over, victory, reset), not a service locator. Systems own their state.
- Prefer `[SerializeField]` references wired on prefabs; no `GameObject.Find` / `FindAnyObjectByType` in runtime gameplay code.
- Input goes through `Assets/playerInput.inputactions`. `Assets/InputSystem_Actions.*` is Unity's unused default template.
- Discrete state changes are C# `event`s; listeners subscribe in `OnEnable()` and unsubscribe in `OnDisable()`.
- ScriptableObjects hold balance and configuration data (thrust, fuel burn, max hull).
- Core components live on the prefab (`Player.prefab`), not as unapplied scene overrides. Modular scene objects (`StartingHub`, `IonStorm`) stay connected prefab instances.

## Definition of Done
- The project compiles with 0 errors and no new console warnings.
- Play Mode on `Assets/Scenes/SampleScene.unity` runs without exceptions.
- Changes made on scene instances are applied to their prefabs.
- After renaming, moving or deleting a script: grep `*.unity` and `*.prefab` for its old GUID (from its `.meta`). Any hit is a broken reference.
- `git status` / `git diff` show no unintended changes — especially in `ProjectSettings/`, `Packages/packages-lock.json` or scenes you did not touch.

## Documentation
- `Assets/DOCS/GAMEPLAY.md` — game design overview: core loop, mechanics, win and loss conditions. Read it when a task involves gameplay decisions.
- Ignore — material for the user and the thesis, not guidelines: `README.md` (legacy), `TODO.md` (roadmap), `Assets/DOCS/SYSTEMS.md` (mechanics registry), `Assets/DOCS/THESIS_NOTES.md`, `Assets/DOCS/raports/`, `Assets/DOCS/Poster/`.
- Never read generated folders: `Library/`, `Temp/`, `Logs/`, `obj/`, `UserSettings/`, `*.csproj`, `*.sln`.
