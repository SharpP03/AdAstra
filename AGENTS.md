# AGENTS.MD - PROJECT AD ASTRA PROTOCOL

## 0. CORE PHILOSOPHY & SIMPLICITY
- AdAstra is an MVP exploration game. Keep solutions simple, direct, and readable (KISS/YAGNI).
- Do not overengineer. Avoid speculative abstractions, unnecessary generic interfaces, or deep inheritance hierarchies.
- Preserve all academic and thesis notes (e.g., `#region EDUCATION NOTE`, `DO PRACY INŻ.`). Keep them accurate if modifying the associated implementation; do not delete them.

## 1. SOURCE OF TRUTH & ARCHITECTURE
- Physical project files are the source of truth for the current implementation; use Git history for architectural context. Disregard legacy claims in `README.md`.
- Taski i Issues (GitHub Projects) służą wyłącznie do wglądu i orientacji użytkownika – nie są źródłem prawdy ani sztywnym wyznacznikiem. Bieżący zakres i decyzje ustalamy bezpośrednio w konwersacji na czacie.
- Engine: Unity 6 (6000.2.6f2) URP, New Input System exclusively (`activeInputHandler: 1`).
- Use ScriptableObjects for shared balance data, configurations, and reusable definitions. Keep instance-specific state and local tweaks directly on MonoBehaviours.

## 2. INPUT SUBSYSTEM
- Use Unity's New Input System exclusively.
- Gameplay code MUST NOT directly access hardware devices (`Keyboard.current`, `Mouse.current`, `Gamepad.current`).
- Prefer generated C# wrappers or cache `InputAction` references during initialization (`Awake`/`Start`). Avoid string-based lookups (`actions.FindAction(...)`) in per-frame updates.
- Read input state and buffer transitions in `Update()`. Apply resulting physics forces in `FixedUpdate()`. Avoid reading transient button presses directly in `FixedUpdate()`.

## 3. PHYSICS & EXECUTION TIMING
- All physics mutations (`AddForce`, `AddTorque`, direct velocity modifications) MUST run in `FixedUpdate`.
- When using `ForceMode.Force` or `ForceMode.Acceleration`, DO NOT multiply the vector by `Time.fixedDeltaTime` (Unity integrates the timestep automatically). Use `Time.fixedDeltaTime` for manual rate calculations, smoothing, or custom integration inside `FixedUpdate`.
- Never poll dependencies using coroutine loops like `while (Instance == null) yield return null;`. Use explicit initialization callbacks, events, or direct references.

## 4. REFERENCES & SCENE DISCIPLINE
- Avoid scene-wide searches (`GameObject.Find`, `FindAnyObjectByType`) in gameplay runtime code. Prefer serialized references (`[SerializeField]`), initialization callbacks, or local queries (`GetComponent` in `Awake`). Use `OnValidate` strictly for editor-time data validation.
- Do not introduce new global singletons without explicit approval.
- Prefab Integrity: Core components and baseline configurations must be committed directly to prefabs (`Player.prefab`), not left as unapplied component additions in scenes. Scene overrides are reserved for intentional, instance-specific values.
- Modular scene objects must remain connected prefab instances, not unpacked raw geometry.

## 5. DOCUMENTATION & LOGS
- Przed rozpoczęciem każdego nowego zadania / mechaniki postępuj ściśle według procedury zdefiniowanej w `WORKFLOW.md` (zapytaj o utworzenie brancha i taska w GitHub Projects).
- Przed rozpoczęciem prac nad dowolną mechaniką sprawdź wpis w `Assets/DOCS/SYSTEMS.md`, a po zakończeniu prac zaktualizuj jego checklistę oraz status.
- Architectural additions or changes to core contracts MUST update the documentation in `Assets/DOCS/`.
- Routine bug fixes and localized tweaks do not require documentation entries.
- Documentation must accurately describe the physical implementation as-is, never speculative designs.

## 6. VALIDATION & DEFINITION OF DONE
- A task is not complete until modified code compiles with 0 errors in Unity.
- Verify the Unity Console has no new errors or warnings caused by the changes.
- Check that modified scenes and prefabs serialize correctly without missing script references (`GUID` nulls).
- Verify affected namespace references and scripts compile cleanly.
- Inspect `git diff` before reporting completion to ensure no unintended files or formatting changes were introduced.

## 7. AGENT SKILLS & TOOLING
- Przed rozpoczęciem prac nad zadaniem gamedevowym skorzystaj ze skilla `router`, aby dobrać i przeczytać odpowiednie skille dopasowane do bieżącej mechaniki.
- Przy projektowaniu granic modułów, klas i interfejsów stosuj zasady ze skilla `codebase-design`.
- **Unity MCP:** Podczas zadań wymagających bezpośredniej interakcji z edytorem Unity (weryfikacja obiektów w scenie, konfiguracja prefabów, odczyt logów z konsoli Unity, uruchamianie testów), korzystaj z narzędzi `unityMCP`. Jeśli narzędzia MCP są niedostępne lub Unity Editor jest wyłączony, a bezpośrednia inspekcja/edycja jest kluczowa dla zadania, **poinformuj o tym użytkownika** zamiast zgadywać lub ryzykownie modyfikować pliki YAML scen/prefabów.
