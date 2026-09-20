# SYSTEMS.MD - AD ASTRA SYSTEM REGISTRY

Operational subsystem registry for project **AdAstra**. Serves as the single source of truth for implementation status, dependencies, MVP scope, and remaining technical tasks.

---

## System Status Legend

| Status | Definition |
| :--- | :--- |
| **`IMPLEMENTED`** | Fully functional in the repository; may require minor tuning or optimization. |
| **`PARTIALLY IMPLEMENTED`** | Partially working in code/scene, but has logic gaps, bugs, or missing integrations. |
| **`DESCRIBED / UNIMPLEMENTED`** | Concept and requirements defined, but physical code does not yet exist. |
| **`DEFERRED / OUT OF MVP`** | Postponed; intentionally out of current MVP scope. |
| **`UNDER REVIEW`** | Open architectural or design question requiring a decision. |

---

## Subsystem Registry

### 1. Spaceship Flight & Physics (Spaceship Controller)
* **Status:** `PARTIALLY IMPLEMENTED`
* **Scope:** `MVP`
* **Path:** `Assets/Scripts/Player/Spaceship/Player_Spaceship.cs`
* **Dependencies:** `Rigidbody`, `FuelSystem`, `PlayerInput` (`Assets/playerInput.inputactions`)
* **Description:** 6DoF/Arcade flight model driven by `Rigidbody` forces and the New Input System. Handles translation, pitch/yaw mouse rotation, roll keys, and sprint boost.
* **Action Items:**
  - [ ] Fix logic bug: `MoveInput` property in `Player_Spaceship.cs` is never assigned (remains `(0, 0)`), which disables the camera tilt effect.
  - [ ] Move mouse input reading from `FixedUpdate()` to `Update()` to eliminate input jitter.
  - [ ] Replace runtime string lookups (`actions.FindAction(...)`) with strongly typed C# action wrappers.

---

### 2. Resource: Fuel System
* **Status:** `IMPLEMENTED`
* **Scope:** `MVP`
* **Path:** `Assets/Scripts/Player/Spaceship/FuelSystem.cs`
* **Dependencies:** `Player_Spaceship`
* **Description:** Tracks current and max fuel capacity. Moving and sprinting burn fuel at proportional rates. Zero fuel disables player propulsion.
* **Action Items:**
  - [ ] Replace coroutine polling loop (`WaitForPlayer()`) with direct reference or initialization in `Awake()`.
  - [ ] Expose `event Action<float, float> OnFuelChanged` to allow event-driven HUD updates.

---

### 3. Resource: Health & Damage System
* **Status:** `PARTIALLY IMPLEMENTED`
* **Scope:** `MVP`
* **Path:** `Assets/Scripts/Player/Spaceship/HealthSystem.cs`, `Assets/Scripts/Interfaces/IDamageable.cs`
* **Dependencies:** None (self-contained, implements `IDamageable`)
* **Description:** Manages spaceship hull integrity. Implements `IDamageable` to receive environmental damage (e.g. from Ion Storm discharges).
* **Action Items:**
  - [ ] Add lower clamping (`Mathf.Max(0, ...)`) to prevent negative health.
  - [ ] Implement `event Action<float, float> OnHealthChanged` and `event Action OnDied`.
  - [ ] Connect `OnDied` to trigger the Game Over sequence in `GameManager`.

---

### 4. Dynamic Chase Camera
* **Status:** `PARTIALLY IMPLEMENTED`
* **Scope:** `MVP`
* **Path:** `Assets/Scripts/Player/DynamicCamera.cs`
* **Dependencies:** `Camera`, `Transform` (Target), `Player_Spaceship`
* **Description:** TPP camera tracking the spaceship using `SmoothDamp` in `LateUpdate()`. Expands FOV and pulls back during sprint, responds to ship roll and lateral tilt.
* **Action Items:**
  - [ ] Eliminate `Start()` initialization race condition (resolve player reference via explicit target injection or serialized field instead of relying on `GameManager.Instance.Player` during `Start`).
  - [ ] Verify tilt effect functionality after fixing `Player_Spaceship.MoveInput`.

---

### 5. Engine Thruster FX
* **Status:** `IMPLEMENTED`
* **Scope:** `MVP`
* **Path:** `Assets/Scripts/Player/Spaceship/PlayerThrusterFXController.cs`
* **Dependencies:** `ParticleSystem` list, `Player_Spaceship`
* **Description:** Interpolates particle emission rate, start speed, color, and Z-velocity based on player movement state (`Standstill`, `Moving`, `Sprinting`).
* **Action Items:**
  - [ ] Remove `WaitForPlayer()` coroutine loop in favor of explicit `Awake()`/`Start()` reference assignment.

---

### 6. User Interface & HUD
* **Status:** `PARTIALLY IMPLEMENTED`
* **Scope:** `MVP`
* **Path:** `Assets/Scripts/Player/UI_Manager.cs`
* **Dependencies:** `FuelSystem`, `HealthSystem`, Unity UI (`Slider`)
* **Description:** Displays fuel and health bars using uGUI sliders.
* **Action Items:**
  - [ ] Replace per-frame `Update()` polling with event subscriptions (`OnFuelChanged`, `OnHealthChanged`) adhering to the C# event standard.
  - [ ] Add minimal Win / Game Over screens with restart button.

---

### 7. Refill & Repair Stations
* **Status:** `PARTIALLY IMPLEMENTED`
* **Scope:** `MVP`
* **Path:** `Assets/Scripts/Locations/StartingHub/`, `Assets/Scripts/Refill/`
* **Dependencies:** `IRefillable`, `Collider` triggers
* **Description:** `RefuelZone` detects the player via `RefuelChildCollider` and replenishes fuel or health via the `IRefillable` contract.
* **Action Items:**
  - [ ] **Critical:** Add `FuelRefillable` and `HealthRefillable` components directly to `Player.prefab` (currently present only as unapplied overrides on the scene instance).
  - [ ] Reconnect any unpacked station geometry on `SampleScene.unity` to the `StartingHub.prefab` asset.

---

### 8. Hazard: Gravity Anomaly
* **Status:** `IMPLEMENTED`
* **Scope:** `MVP`
* **Path:** `Assets/Scripts/Locations/GravityAnomaly/GravityAnomallyController.cs`, `Assets/Prefabs/Locations/GravityAnomaly/GravityAnomaly.prefab`
* **Dependencies:** `Physics`, `Rigidbody`
* **Description:** Attracts objects with `Rigidbody`, generates tangential force to form an orbital trajectory, features an outer surge zone, and dampens escaping vessels.
* **Action Items:**
  - [ ] Fix class name typo (`GravityAnomallyController` -> `GravityAnomalyController`).
  - [ ] Profile physics performance in scene context before considering allocation optimizations.

---

### 9. Hazard: Ion Storm (Fog Event)
* **Status:** `IMPLEMENTED`
* **Scope:** `MVP`
* **Path:** `Assets/Scripts/Locations/IonStorm/FogEventController.cs`, `Assets/Prefabs/Locations/IonStorm/IonStorm.prefab`
* **Dependencies:** `Collider` trigger, `IDamageable`, Particle VFX prefabs
* **Description:** Trigger perimeter executing a periodic strike loop: spawns telegraph warning VFX, then inflicts damage via `IDamageable` if the target remains within the zone.
* **Action Items:**
  - [ ] Verify prefab connection of the scene instance to `IonStorm.prefab`.

---

### 10. Asteroid Field Generator
* **Status:** `IMPLEMENTED`
* **Scope:** `MVP`
* **Path:** `Assets/Scripts/Locations/AsteroidField_01/AsteroidFieldGenerator.cs`, `Assets/Prefabs/Locations/AsteroidField/AsteroidField_01.prefab`
* **Dependencies:** Asteroid prefabs (`Asteroid_no_*.prefab`)
* **Description:** Instantiates randomized asteroids in a spherical volume, assigning scale variations, colliders, rigidbodies, and random angular velocity impulses.
* **Action Items:**
  - [ ] Tune density and boundary parameters for the MVP sector blockout.

---

### 11. Combat & Enemy AI
* **Status:** `DEFERRED / OUT OF MVP`
* **Scope:** `POST-MVP`
* **Path:** *Deferred*
* **Dependencies:** None
* **Description:** Combat AI architecture was removed in commit `6277380`. Combat mechanics (hostile ships, turrets, projectile weapons) are officially deferred to focus exclusively on flight traversal, resource tension, and environmental hazards for the initial MVP release.
* **Action Items:**
  - [ ] Re-evaluate scope post-MVP if combat encounters are approved.

---

### 12. Game Lifecycle & State Management
* **Status:** `PARTIALLY IMPLEMENTED`
* **Scope:** `MVP`
* **Path:** `Assets/Scripts/GameManagment/GameManager.cs`, `Assets/Scripts/GameManagment/GameState.cs`
* **Dependencies:** `Player_Spaceship`, `UI_Manager`
* **Description:** `GameManager` acts as the Composition Root and high-level state manager. `GameState` enum exists (`MainMenu`, `Playing`, `Paused`, `GameOver`).
* **Action Items:**
  - [ ] Implement active state transitions (e.g., `Playing` -> `GameOver` on death/fuel exhaustion, `Playing` -> `Victory` on reaching Warp Gate).
  - [ ] Add scene reload / run restart function triggered by UI restart button.

---

### 13. World Flow, Objective & Navigation
* **Status:** `DESCRIBED / UNIMPLEMENTED`
* **Scope:** `MVP`
* **Path:** Refer to `Assets/DOCS/GAMEPLAY.md`
* **Dependencies:** `GameManager`, Warp Gate prefab / trigger
* **Description:** Defined in `Assets/DOCS/GAMEPLAY.md`. The player departs the Starting Hub and must reach an extraction objective (Warp Gate). Navigation guidance method is currently an Open Decision (TBD during sector blockout).
* **Action Items:**
  - [ ] Place or block out a Warp Gate extraction trigger at the end of the sector.
  - [ ] Implement visual in-world beacon at the extraction point.

---

### 14. Persistence / Save System
* **Status:** `DEFERRED / OUT OF MVP`
* **Scope:** `POST-MVP`
* **Path:** *None*
* **Description:** AdAstra MVP is structured as an arcade/exploration run completed in a single session (5–10 minutes). Full state serialization across sessions is deferred.
