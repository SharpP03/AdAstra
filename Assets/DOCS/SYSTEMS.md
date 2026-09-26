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
  - [x] Fix logic bug: `MoveInput` property in `Player_Spaceship.cs` is never assigned (remains `(0, 0)`), which disables the camera tilt effect.
  - [x] Move mouse input reading from `FixedUpdate()` to `Update()` to eliminate input jitter.
  - [ ] Replace runtime string lookups (`actions.FindAction(...)`) with strongly typed C# action wrappers.

---

### 2. Resource: Fuel System
* **Status:** `IMPLEMENTED`
* **Scope:** `MVP`
* **Path:** `Assets/Scripts/Player/Spaceship/FuelSystem.cs`
* **Dependencies:** `Player_Spaceship`
* **Description:** Tracks current and max fuel capacity. Moving and sprinting burn fuel at proportional rates. Zero fuel disables player propulsion.
* **Action Items:**
  - [x] Replace coroutine polling loop (`WaitForPlayer()`) with direct reference or initialization in `Awake()`.
  - [ ] Expose `event Action<float, float> OnFuelChanged` to allow event-driven HUD updates.

---

### 3. Resource: Health & Damage System
* **Status:** `PARTIALLY IMPLEMENTED`
* **Scope:** `MVP`
* **Path:** `Assets/Scripts/Player/Spaceship/HealthSystem.cs`, `Assets/Scripts/Interfaces/IDamageable.cs`
* **Dependencies:** None (self-contained, implements `IDamageable`)
* **Description:** Manages spaceship hull integrity. Implements `IDamageable` to receive environmental damage (e.g. from Ion Storm discharges).
* **Action Items:**
  - [x] Add lower clamping (`Mathf.Max(0, ...)`) to prevent negative health.
  - [x] Implement `event Action<float, float> OnHealthChanged` and `event Action OnDied`.
  - [ ] Connect `OnDied` to trigger the Game Over sequence in `GameManager`.

---

### 4. Dynamic Chase Camera
* **Status:** `IMPLEMENTED`
* **Scope:** `MVP`
* **Path:** `Assets/Scripts/Player/DynamicCamera.cs`
* **Dependencies:** `Camera`, `Transform` (Target), `Player_Spaceship`
* **Description:** TPP camera tracking the spaceship using `SmoothDamp` in `LateUpdate()`. Expands FOV and pulls back during sprint, responds to ship roll and lateral tilt.
* **Action Items:**
  - [x] Eliminate `Start()` initialization race condition (resolve player reference via explicit target injection or serialized field instead of relying on `GameManager.Instance.Player` during `Start`).
  - [x] Verify tilt effect functionality after fixing `Player_Spaceship.MoveInput`.

---

### 5. Engine Thruster FX
* **Status:** `IMPLEMENTED`
* **Scope:** `MVP`
* **Path:** `Assets/Scripts/Player/Spaceship/PlayerThrusterFXController.cs`
* **Dependencies:** `ParticleSystem` list, `Player_Spaceship`
* **Description:** Interpolates particle emission rate, start speed, color, and Z-velocity based on player movement state (`Standstill`, `Moving`, `Sprinting`).
* **Action Items:**
  - [x] Remove `WaitForPlayer()` coroutine loop in favor of explicit `Awake()`/`Start()` reference assignment.

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
* **Dependencies:** `IRefillable`, `Collider` triggers, `Rigidbody`
* **Description:** Outposts providing fuel or hull restoration. In MVP, stations require a **Dead-Stop Refuel & Docking Magnet** procedure: when the spaceship decelerates below a threshold velocity inside `RefuelZone`, a docking magnet locks the vessel in place while resources replenish progressively over time under the threat of the approaching Annihilation Wave.
* **Action Items:**
  - [x] **Critical:** Add `FuelRefillable` and `HealthRefillable` components directly to `Player.prefab` (currently present only as unapplied overrides on the scene instance).
  - [ ] Reconnect any unpacked station geometry on `SampleScene.unity` to the `StartingHub.prefab` asset.
  - [ ] Upgrade `RefuelZone.cs` from immediate trigger-enter to the Dead-Stop detection and docking magnet lock sequence.

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
* **Description:** `GameManager` acts as the Composition Root and high-level state manager. `GameState` enum tracks states (`MainMenu`, `Playing`, `Paused`, `GameOver`, `Victory`).
* **Action Items:**
  - [ ] Handle fuel depletion: `Fuel == 0` initiates **Dead-Stick** flight state (disabling main engine thrust and RCS attitude torque; no immediate GameOver).
  - [ ] Implement active state transitions: `Playing` -> `GameOver` on hull destruction (`Health <= 0`) or Annihilation Wave consumption; `Playing` -> `Victory` on entering Warp Gate.
  - [ ] Add scene reload / run restart function triggered by UI restart button.

---

### 13. World Flow, Objective & Navigation
* **Status:** `DESCRIBED / UNIMPLEMENTED`
* **Scope:** `MVP`
* **Path:** `SampleScene.unity`, `Assets/Scripts/UI/`
* **Dependencies:** `GameManager`, Warp Gate prefab / trigger, `UI_Manager`
* **Description:** Linear sector corridor test blockout (from $Z=0$ to $Z=4000$) in `SampleScene.unity`. Navigation is guided via screen-space HUD Waypoints showing direction and distance in meters to the active objective (Intermediate Station, then Warp Gate), supplemented by high-intensity diegetic light beacons. Full procedural corridor generation is deferred to POST-MVP.
* **Action Items:**
  - [ ] Block out hand-crafted test corridor in `SampleScene.unity`.
  - [ ] Implement HUD Waypoint directional tracker with distance readout.
  - [ ] Place Warp Gate extraction trigger at sector terminus ($Z=4000$).

---

### 14. Persistence / Meta-Progression Storage
* **Status:** `DESCRIBED / UNIMPLEMENTED`
* **Scope:** `MVP (LIGHTWEIGHT)`
* **Path:** `Assets/Scripts/Storage/` (planned)
* **Dependencies:** `UnityEngine.PlayerPrefs`
* **Description:** Lightweight persistence using `PlayerPrefs` to store `BankedDataCores` and unlocked ship variant IDs across play sessions. Full world-state and mid-run serialization is deferred to POST-MVP.
* **Action Items:**
  - [ ] Implement lightweight `MetaProgressionStorage` helper using `PlayerPrefs`.

---

### 15. Hazard: Annihilation Wave (Front Anihilacji)
* **Status:** `DESCRIBED / UNIMPLEMENTED`
* **Scope:** `MVP`
* **Path:** `Assets/Scripts/Environment/AnnihilationWave/` (planned)
* **Dependencies:** `IDamageable`, `Player_Spaceship`, `UI_Manager`
* **Description:** Linear energy plane advancing at constant speed along the Z axis behind the player. Features a warning perimeter (50–100 m) with HUD alarm/tint and proximity meter, and a destructive edge inflicting rapid damage per second (`IDamageable.TakeDamage`, Rapid DPS), permitting desperate last-second sprint escapes.
* **Action Items:**
  - [ ] Implement constant Z-velocity trigger controller.
  - [ ] Calculate distance to player via `Vector3.Dot` and feed to HUD.
  - [ ] Apply periodic Rapid DPS on contact via `IDamageable`.

---

### 16. Collectible: Data Cores (Rdzenie Danych)
* **Status:** `DESCRIBED / UNIMPLEMENTED`
* **Scope:** `MVP`
* **Path:** `Assets/Scripts/Pickups/` (planned)
* **Dependencies:** `Collider` trigger, `FuelSystem`, `HealthSystem`, `GameManager`
* **Description:** Floating salvage containers located in high-risk zones (inside Ion Storms, orbital paths of Gravity Anomalies). Dual-use design: immediately restores resources (+25% fuel or hull repair) upon pickup during a run, and banks as meta-currency upon Warp Gate extraction.
* **Action Items:**
  - [ ] Create pickup prefab with rotating model and trigger collider.
  - [ ] Implement resource recovery and bank registration with `GameManager`.

---

### 17. Meta-Progression: Ship Variants (Warianty Statku)
* **Status:** `DESCRIBED / UNIMPLEMENTED`
* **Scope:** `MVP`
* **Path:** `Assets/Scripts/Player/Spaceship/` (planned ScriptableObject config)
* **Dependencies:** `Player_Spaceship`, `FuelSystem`, `HealthSystem`, `MetaProgressionStorage`
* **Description:** Allows selection between predefined ship archetypes in the Starting Hub (e.g. Light Scout, Heavy Freighter) with differing mass, thrust, fuel capacity, and hull integrity. Note: Code implementation of ship variants is deferred until explicitly commanded by the user.
* **Action Items:**
  - [ ] Extract ship attributes into `SpaceshipConfigSO` (ScriptableObject).
  - [ ] Create minimal ship selection UI in Starting Hub.

---

### 18. Emergency Distress Signal (Sygnał SOS)
* **Status:** `UNDER REVIEW`
* **Scope:** `POST-MVP`
* **Path:** *Deferred*
* **Dependencies:** `FuelSystem`, `UI_Manager`
* **Description:** Optional distress beacon mechanic during powerless inertia drift. In MVP, powerless drift naturally ends in collision or absorption by the Annihilation Wave.

