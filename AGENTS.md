# AGENTS.MD - PROJECT AD ASTRA PROTOCOL

## 0. CORE PHILOSOPHY & SIMPLICITY
- AdAstra is an MVP exploration game. Keep solutions simple, direct, and readable (KISS/YAGNI).
- Do not overengineer. Avoid speculative abstractions, unnecessary generic interfaces, or deep inheritance hierarchies.
- Preserve all academic and thesis notes (e.g., `#region EDUCATION NOTE`, `DO PRACY INŻ.`). Never delete or refactor out explanatory comments meant for the engineering thesis.

## 1. SOURCE OF TRUTH & ARCHITECTURE
- Physical project files and Git history are the sole source of truth. Disregard legacy claims in `README.md`.
- Engine: Unity 6 (6000.2.6f2) URP, New Input System exclusively (`activeInputHandler: 1`).
- Assembly Definitions (`*.asmdef`): Any new module or architectural boundary must use asmdefs. Avoid bloating monolithic `Assembly-CSharp`.
- ScriptableObjects are preferred for static balance data, parameters, and configs. MonoBehaviours are for runtime actors.

## 2. INPUT SUBSYSTEM
- Use Unity's New Input System exclusively.
- Gameplay code MUST NOT directly access hardware devices (`Keyboard.current`, `Mouse.current`, `Gamepad.current`).
- Do not use string-based action lookups such as `actions.FindAction("ActionName")`.
- Gameplay systems consume input via a strongly-typed generated C# wrapper or a dedicated project input adapter.
- Read input state in `Update()`, never in `FixedUpdate()`.

## 3. PHYSICS & EXECUTION TIMING
- All physics mutations (`AddForce`, `AddTorque`, direct velocity modifications) MUST run in `FixedUpdate`.
- When using `ForceMode.Force` or `ForceMode.Acceleration`, DO NOT multiply the force by `Time.fixedDeltaTime` (Unity integrates the time step automatically).
- Use `Time.fixedDeltaTime` only when manually integrating positions/angles or dampening inside `FixedUpdate`.
- Never poll dependencies using coroutine loops like `while (Instance == null) yield return null;`. Use explicit initialization callbacks, events, or direct references.

## 4. REFERENCES & SCENE DISCIPLINE
- Do not use `GameObject.Find`, `FindObjectOfType`, `FindObjectsByType`, or scene-wide scans for gameplay dependencies.
- Prefer serialized references (`[SerializeField]`), initialization contracts, or prefab-local hierarchy queries (`GetComponent` in `Awake`/`OnValidate`).
- Do not introduce new global singletons without explicit approval.
- Prefab Integrity: Critical components (e.g., `FuelRefillable`, `HealthRefillable`) MUST be committed directly onto prefabs (`Player.prefab`), never left as unapplied overrides in `SampleScene.unity`.
- Modular scene objects must remain connected prefab instances, not unpacked raw geometry.

## 5. DOCUMENTATION & LOGS
- Architectural additions or changes to core contracts MUST update the documentation in `Assets/DOCS/`.
- Routine bug fixes and localized tweaks do not require documentation entries.
- Documentation must accurately describe the physical implementation as-is, never speculative designs.

## 6. VALIDATION & DEFINITION OF DONE
- A task is not complete until modified code compiles with 0 errors in Unity.
- Verify the Unity Console has no new errors or warnings caused by the changes.
- Check that modified scenes and prefabs serialize correctly without missing script references (`GUID` nulls).
- Verify affected asmdefs and namespace references compile cleanly.
- Inspect `git diff` before reporting completion to ensure no unintended files or formatting changes were introduced.
