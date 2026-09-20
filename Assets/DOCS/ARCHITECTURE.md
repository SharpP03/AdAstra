# ARCHITECTURE.MD - AD ASTRA TECHNICAL FOUNDATION

## 1. TECH STACK & ENGINE ENVIRONMENT
* **Engine Version:** Unity 6 (`6000.2.6f2`).
* **Render Pipeline:** Universal Render Pipeline (URP `17.2.0`), custom pipeline asset: `PC_RPAsset` (`Assets/Settings/PC_RPAsset.asset`).
* **Input Subsystem:** Unity New Input System exclusively (`com.unity.inputsystem: 1.14.2`, `activeInputHandler: 1` in `ProjectSettings.asset`). Legacy Input Manager is completely disabled. Canonical action asset: `Assets/playerInput.inputactions`.
* **Scripting Runtime:** .NET Standard 2.1 / C# 9.0+.
* **Asynchronous Operations:** Coroutines and standard Unity frame execution. Heavy external frameworks (e.g., `UniTask`) are avoided to maintain MVP simplicity.
* **DI & Architectural Frameworks:** Plain component-based Unity architecture. No heavy external IoC/DI containers (Zenject, VContainer). Dependencies are resolved via direct serialized references or explicit initialization contracts.

---

## 2. CORE ARCHITECTURAL PRINCIPLES

### 2.1. KISS & YAGNI (Keep It Simple / You Aren't Gonna Need It)
AdAstra is an MVP exploration game. Implement solutions directly and cleanly. Avoid speculative abstractions, premature generic interfaces, deep inheritance hierarchies, and enterprise patterns.

### 2.2. Academic & Thesis Notes
Preserve all explanatory comments, `#region EDUCATION NOTE`, and `DO PRACY INŻ.` markers across the codebase. These are deliberate pedagogical annotations required for the engineering thesis and must not be stripped or refactored out.

### 2.3. Compilation Boundaries (`asmdef`)
Keep project namespaces and folder structures clean. Assembly Definition files (`*.asmdef`) should be introduced at real module boundaries (e.g., test suites, shared utilities, or isolated domain systems), rather than mandating an asmdef for every single script during early MVP prototyping.

### 2.4. Data vs Logic (ScriptableObjects)
* Use `ScriptableObject` assets strictly for **static balance data, spaceship configuration, and tunable parameters** (e.g., thruster force, max health, fuel burn rates).
* Do **not** use ScriptableObjects as mutable shared runtime state containers for this MVP.
* Use `MonoBehaviour` solely for runtime actors, physics entities, and scene instances.

---

## 3. LIFECYCLE & INITIALIZATION CONTRACT

To eliminate initialization race conditions and eradicate coroutine polling loops (`while (Instance == null) yield return null;`), all gameplay actors adhere to a strict 4-phase frame contract:

```
┌─────────────────────────────────────────────────────────────┐
│ 1. Awake()                                                  │
│    Self-configuration ONLY: GetComponent on same GameObject,│
│    data structure allocation. DO NOT query other actors.   │
└──────────────────────────────┬──────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────┐
│ 2. Initialize(...) / Start()                                │
│    Inter-object wiring: Resolve dependencies, connect to    │
│    observable events, and broadcast initial state.          │
└──────────────────────────────┬──────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────┐
│ 3. Runtime Loop                                             │
│    • Update()      ──► Read/cache input, evaluate state     │
│    • FixedUpdate() ──► Rigidbody forces, physics mutation   │
│    • LateUpdate()  ──► Camera tracking, visual smoothing    │
└─────────────────────────────────────────────────────────────┘
```

### 3.1. GameManager Role
`GameManager` serves as the **Composition Root & High-Level State Orchestrator** (managing states like `Playing`, `Paused`, `GameOver`, and run resets). It is **not** an all-knowing god object or global service locator. Systems manage their own local state.

### 3.2. Dependency Resolution Rules
* Prefer explicit `[SerializeField]` references wired on prefabs.
* For components on the same GameObject, query via `GetComponent` in `Awake()`.
* Avoid scene-wide scans (`GameObject.Find`, `FindObjectOfType`) in runtime gameplay code (acceptable in custom editor utilities).
* Never use coroutine polling to wait for dependencies.

---

## 4. INPUT & TIMING SPECIFICATION

* **Timing Separation:**
  * **`Update()`:** Read and cache all player input (`playerInput.actions`) and advance gameplay timers.
  * **`FixedUpdate()`:** Apply all physical movements, forces, and torques.
  * **`LateUpdate()`:** Follow targets with the camera, apply cosmetic smoothing (lerps), and update procedural transforms.
* **Hardware Abstraction:** Gameplay systems should not query raw hardware devices (`Keyboard.current`, `Mouse.current`, `Gamepad.current`) directly; consume actions through the New Input System asset (`playerInput.inputactions`).
* **Strong Typing:** Avoid string-based action queries (`actions.FindAction("Move")`) in hot paths; generate and use strongly typed C# action wrappers.

---

## 5. PHYSICS & SIMULATION RULES

* **Execution in `FixedUpdate`:** All modifications to `Rigidbody` forces, torques, and velocities (`AddForce`, `AddTorque`, `linearVelocity`) must be executed inside `FixedUpdate`.
* **Force Integration Rule:**
  * When using `ForceMode.Force` or `ForceMode.Acceleration`, **DO NOT** multiply the force vector by `Time.fixedDeltaTime` (Unity's physics integrator automatically multiplies by the physics delta time).
  * Use `Time.fixedDeltaTime` only when manually integrating custom positions or calculating manual damping inside `FixedUpdate`.
* **Resource Bounds:** All mutating operations on resources (Fuel, Health) must explicitly clamp values (`Mathf.Clamp`, `Mathf.Max(0, ...)`).

---

## 6. EVENT & COMMUNICATION ARCHITECTURE

* **Decoupling Pattern:** Discrete state changes (damage taken, fuel refilled, ship destroyed) use **native C# instance events** (`public event System.Action<float, float> OnHealthChanged;`).
* **Subscription Lifecycle:**
  * Listeners (such as `UI_Manager` or VFX controllers) subscribe to events in `OnEnable()` and unsubscribe in `OnDisable()` to prevent memory leaks and dangling references.
* **Pragmatic Polling:** Event-driven updates are preferred for state transitions. Polling in `Update()` is discouraged for discrete events, but permissible when tracking continuously changing smooth values or where polling provides substantial simplicity without measurable overhead.

---

## 7. PERFORMANCE BUDGET & OPTIMIZATION RULE

### 7.1. Principle: "Profile → Optimize"
* Write clean, readable, and idiomatic C# code first.
* Avoid premature optimization: do not implement object pooling or complex manual `NonAlloc` buffers before profiling indicates a measurable bottleneck.
* Use the **Unity Profiler** (`Ctrl+7`) to measure frame time and identify allocations before altering architecture.

### 7.2. Target Budget (MVP Baseline)
* **Frame Rate:** Stable 60 FPS on standard desktop hardware (< 16.6 ms frame time).
* **Garbage Collection:** Near-zero GC allocations in `Update()` and `FixedUpdate()` hot loops (avoid string formatting, per-frame LINQ, and unnecessary array allocations).

---

## 8. PREFAB & SCENE DISCIPLINE

* **Prefab-First Integrity:**
  * Critical gameplay components (e.g., `FuelRefillable`, `HealthRefillable`) must reside directly on the prefab asset (`Player.prefab`), never left as unapplied overrides on scene instances.
  * Modular scene objects (e.g., `StartingHub`, `IonStorm`) must remain connected prefab instances, not unpacked raw geometry.
* **Component Cohesion:** Components providing interface implementations (e.g., `IRefillable`, `IDamageable`) must declare required dependencies via `[RequireComponent(...)]`.

---

## 9. DEFINITION OF DONE (DoD)

Before marking any engineering task as complete:
1. **Compilation & Console:** The project compiles with 0 errors and 0 new warnings in Unity.
2. **Prefab Integrity:** All modified components on scene instances are applied to their corresponding prefab assets (`.prefab`).
3. **PlayMode Sanity Check:** The primary scene (`SampleScene.unity`) is executed in PlayMode to verify intended gameplay behavior with no `NullReferenceException` in the console.
4. **Git Verification:** `git diff` is inspected to ensure no accidental formatting changes or untracked meta files are introduced.
