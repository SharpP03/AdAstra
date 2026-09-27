# TODO.md - AD ASTRA GAMEPLAY ROADMAP (MVP)

> [!NOTE]
> **Dokument poglądowy:** Niniejszy plik pełni rolę pomocniczą i poglądową – **nie stanowi ostatecznego źródła prawdy**. Wszelkie kluczowe kwestie architektoniczne, mechaniki i szczegóły implementacyjne należy doprecyzowywać bezpośrednio w konwersacji na czacie oraz weryfikować ze stanem kodu i dokumentacją w `Assets/DOCS/`.

Operational roadmap of core gameplay mechanics to transition AdAstra from a flight prototype into a playable roguelike survival loop.

---

## 🚀 PRIORITY 1: Core Tension & Escape (Heart of Gameplay)

### 1. Front Anihilacji / The Annihilation Wave
* **Status:** `IMPLEMENTED`
* **Path:** `Assets/Scripts/Hazards/AnnihilationWave/`
* **Concept:** A linear energy boundary moving forward along the Z-axis (from $Z = -200$ to $Z = 4000$) at constant speed.
* **Scope:**
  - [x] Implement `AnnihilationWaveController.cs` (advances along Z at configurable base speed).
  - [x] **Warning Zone (50-100m ahead):** Threat states (`Safe`, `Warning`, `Critical`, `Engulfed`), distance calculation and event channel for HUD.
  - [x] **Kill/Damage Zone:** Rapid damage over time (`IDamageable.TakeDamage`) when engulfed by the wave.
  - [x] Visual representation: Transparent/emissive double-sided URP plane and reusable prefab advancing through space.
* **Future Polish / Creative Concepts (Post-MVP / Polish):**
  - *Cosmic Crescent / Funnel:* Curved bowl/funnel mesh surrounding player's rear & flanks to eliminate flat planar look.
  - *Skybox Bleed / Space Tear:* Rear skybox shader dissolution/blackout representing cosmic collapse.
  - *Ionized Corridor:* Side radiation boundaries enforcing a diegetic flight corridor.
  - *Singularity Drag:* Magnetic drift/RCS interference on extreme lateral deviation.

---

## ⚓ PRIORITY 2: Tactical Pitstop & Fuel Economy

### 2. Dead-Stop Refuel & Magnes Dokujący (Docking Magnet)
* **Status:** `PARTIALLY IMPLEMENTED (UPGRADE NEEDED)`
* **Path:** `Assets/Scripts/Locations/StartingHub/RefuelZone.cs`
* **Concept:** Upgrades `RefuelZone` from an instant trigger pickup into a tense, tactical docking procedure under pressure.
* **Scope:**
  - [ ] **Dead-Stop Threshold:** Detect when player velocity inside the zone drops below threshold (`rb.linearVelocity.magnitude < stopVelocityThreshold`).
  - [ ] **Docking Magnet:** Dampen momentum, gently align/pull ship towards docking clamp, lock physics movement during refuel.
  - [ ] **Progressive Refueling:** Replenish fuel/health over time (e.g. 20-30 units/sec) while docked.
  - [ ] **Undock / Release:** Release lock once full or when player holds sprint/emergency release.

---

## 💎 PRIORITY 3: Risk vs Reward Exploration

### 3. Rdzenie Danych (Data Cores / Risk vs Reward Pickups)
* **Status:** `DESCRIBED / UNIMPLEMENTED`
* **Path:** `Assets/Scripts/Pickups/DataCore.cs`
* **Concept:** Floating containers placed inside high-danger zones (inside Ion Storms, close orbits of Gravity Anomalies).
* **Scope:**
  - [ ] Create `DataCore` pickup prefab with visual floating bob/spin and collider trigger.
  - [ ] On collection: immediate resource refill (+25% fuel or +25 hull health via `IRefillable`) and increment run data core counter.
  - [ ] Place instances strategically in `SampleScene.unity` inside the Ion Storm and Gravity Anomaly perimeter.

---

## 🧭 PRIORITY 4: 3D Spatial Navigation

### 4. HUD Waypoint Tracker (3D Direction & Distance)
* **Status:** `DESCRIBED / UNIMPLEMENTED`
* **Path:** `Assets/Scripts/UI/WaypointTracker.cs`
* **Concept:** Screen-space indicators guiding player towards objectives across vast 3D space.
* **Scope:**
  - [ ] Project world target positions (Next Station, Warp Gate) to screen coordinates.
  - [ ] Show off-screen edge pointers when target is behind or outside camera FOV.
  - [ ] Display distance in meters to target and distance to incoming Annihilation Wave.

---

## 🏁 PRIORITY 5: Loop Closure & Extraction

### 5. Sector Terminus & Extraction (Warp Gate)
* **Status:** `DESCRIBED / UNIMPLEMENTED`
* **Path:** `Assets/Scripts/Locations/WarpGate/`
* **Concept:** Warp Gate placed at $Z = 4000$ in `SampleScene.unity` acting as the victory condition.
* **Scope:**
  - [ ] Place Warp Gate trigger at sector terminus.
  - [ ] On entry: transition `GameState` to `Victory` in `GameManager`.
  - [ ] Summary screen showing escape time, remaining resources, and collected Data Cores.
