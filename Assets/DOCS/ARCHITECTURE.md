# ARCHITECTURE.MD - AD ASTRA TECHNICAL FOUNDATION

## 1. TECH STACK & ENGINE ENVIRONMENT
- **Engine Version:** Unity 6 (`6000.2.6f2`).
- **Render Pipeline:** Universal Render Pipeline (URP `17.2.0`), custom render pipeline asset: `PC_RPAsset` (`Assets/Settings/PC_RPAsset.asset`).
- **Input Subsystem:** Unity New Input System exclusively (`com.unity.inputsystem: 1.14.2`, `activeInputHandler: 1` in `ProjectSettings.asset`). Legacy Input Manager is completely disabled.
- **Scripting Runtime:** .NET Standard 2.1 / C# 9.0+.
- **Asynchronous Operations:** Unity Coroutines and standard frame-based execution. No external async libraries (`UniTask` is not installed).
- **DI & Frameworks:** Plain Unity component-based architecture. No Zenject, VContainer, or external DI containers. Avoid introducing third-party architectural frameworks unless strictly needed for MVP.

---

## 2. CORE ARCHITECTURAL PRINCIPLES
- **KISS & YAGNI:** AdAstra is an MVP space exploration game. Implement solutions directly and cleanly. Do not introduce multi-layered abstractions, speculative factories, or premature inheritance trees.
- **Academic & Thesis Notes:** Preserve all explanatory comments, `#region EDUCATION NOTE`, and `DO PRACY INŻ.` markers across the codebase. These are deliberate annotations required for the engineering thesis and must not be stripped during refactoring.
- **Compilation Boundaries (asmdef):** The project currently resides in monolithic `Assembly-CSharp`. New modules, domain logic, and architectural layers must be organized using Assembly Definition files (`*.asmdef`) to ensure fast recompilation, testability, and strict dependency boundaries.
- **Data vs Logic:** Prefer `ScriptableObject` assets for static balance data, spaceship configuration, and tunable parameters. Use `MonoBehaviour` solely for runtime actors and scene instances.

---

## 3. GAME LIFECYCLE & INITIALIZATION
- **Entry Point:** The primary playable scene and initial entry point is `Assets/Scenes/SampleScene.unity` (Build Index 0).
- **Game State & Management:** `GameManager` (`Assets/Scripts/GameManagment/GameManager.cs`) is a persistent MonoBehaviour singleton (`DontDestroyOnLoad`).
- **Initialization Discipline:**
  - Gameplay components must NOT use coroutine polling loops (`while (Instance == null || Player == null) yield return null;`) to resolve dependencies.
  - Dependencies must be resolved via serialized references (`[SerializeField]`), explicit initialization callbacks (`Initialize(...)`), or event-driven registration in `Awake`/`Start`.
  - Avoid race conditions: Never assume other scene actors have completed their `Start()` logic without an explicit orchestration contract.

---

## 4. INPUT LAYER SPECIFICATION
- **Abstraction:** Gameplay systems must never read raw hardware devices (`Keyboard.current`, `Mouse.current`, `Gamepad.current`) directly.
- **No String Lookups:** Do not use `playerInput.actions.FindAction("Move")` or string literals at runtime.
- **Strong Typing:** Consume input through a strongly-typed generated C# wrapper (or a project-owned input adapter).
- **Frame Timing:** Read input state in `Update()`. NEVER read hardware or action input in `FixedUpdate()`.

---

## 5. PHYSICS & EXECUTION TIMING
- **Physics Mutations in `FixedUpdate`:** All modifications to `Rigidbody` forces, torques, and velocities (`AddForce`, `AddTorque`, `linearVelocity`) must be executed inside `FixedUpdate`.
- **Force Mode Integration Rule:**
  - When using `ForceMode.Force` or `ForceMode.Acceleration`, DO NOT multiply the force vector by `Time.fixedDeltaTime`. Unity's internal physics integrator automatically scales by the fixed time step.
  - Use `Time.fixedDeltaTime` only when manually integrating positions or calculating damping/lerps inside `FixedUpdate`.
- **Sanitization & Clamping:** All gameplay resource mutators (e.g., fuel consumption, health damage) must clamp values explicitly (`Mathf.Clamp`, `Mathf.Max(0, ...)`).

---

## 6. PREFAB & SCENE DISCIPLINE
- **Prefab-First Integrity:**
  - Critical gameplay components (e.g., `FuelRefillable`, `HealthRefillable`) must reside directly on the prefab asset (`Player.prefab`), not as unapplied overrides on scene instances.
  - Scene objects representing modular prefabs (e.g., `StartingHub`, `IonStorm`) must remain connected prefab instances, not unpacked raw geometry.
- **Dependency Retrieval:**
  - Absolutely do not use `GameObject.Find`, `FindObjectOfType`, or `FindObjectsByType` in gameplay code.
  - Use serialized fields (`[SerializeField]`), component injection, or local hierarchy queries (`GetComponent` / `GetComponentInChildren` in `Awake` or `OnValidate`).

---

## 7. DEFINITION OF DONE (DoD)
Before completing any engineering task:
1. Code compiles with 0 errors and 0 warnings in Unity.
2. Unity Console is inspected and clear of null reference exceptions or missing script warnings (`GUID` nulls).
3. Prefab and scene changes are verified to prevent accidental unapplied overrides or broken links.
4. `Assets/DOCS/SYSTEMS.md` is updated if any system status or action item changed.
5. Inspect `git diff` to ensure no accidental formatting or unintended file changes are committed.
