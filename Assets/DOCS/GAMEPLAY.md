# GAMEPLAY.MD - AD ASTRA GAME DESIGN & CORE LOOP

## 1. HIGH CONCEPT & EXPERIENCE GOALS
AdAstra is an MVP space exploration game focused on atmospheric traversal, resource tension, and pilot maneuverability in a fragmented, hazardous sector of deep space.

The player pilots a 6DoF/Arcade spaceship, managing limited fuel and hull integrity while navigating through environmental anomalies toward an extraction point.

---

## 2. CORE GAMEPLAY LOOP
The core loop operates on a tight cycle of risk evaluation, spatial navigation, and resource management:

```
[ Launch from Hub ]
        │
        ▼
[ Navigate Space & Avoid Hazards ] ───► (Asteroid Fields, Ion Storms, Gravity Anomalies)
        │
        ├──► [ Manage Resources ] ────► (Fuel drained by thrusters, Hull damaged by hazards)
        │           │
        │           ▼
        │    [ Refuel / Repair ] ────► (Docking at Refill Stations along the route)
        ▼
[ Reach Extraction Objective ] ──────► (Warp Gate / Sector Boundary)
        │
        ▼
  [ Run Complete / Win ]
```

---

## 3. WIN / LOSE CONDITIONS

### 3.1. Win Condition (Run Complete)
* **Objective:** Reach and enter the target **Warp Gate (Extraction Point)** situated on the perimeter of the sector.
* **Outcome:** Triggers the Victory state / run summary screen, acknowledging successful extraction.

### 3.2. Lose Conditions (Game Over)
* **Hull Destruction (`Health <= 0`):** Hull integrity drops to zero caused by high-velocity collisions with asteroids, ion storm lightning strikes, or gravitational crush.
* **Propulsion Depletion (`Fuel <= 0`):** Fuel is completely exhausted while outside a refill zone, stranding the vessel adrift in space without thrust.
* **Outcome:** Triggers the Game Over state with an option to restart the run immediately at the Starting Hub.

---

## 4. RESOURCE ECONOMICS

| Resource | Primary Drain | Replenishment | Depletion Consequence |
| :--- | :--- | :--- | :--- |
| **Fuel** | Continuous drain during regular flight (`Moving`), 2.2x amplified drain during boost (`Sprinting`). | `RefuelZone` at Stations (e.g., Starting Hub). | Disables engine control; vessel drifts on remaining momentum until lost. |
| **Health (Hull)** | Collisions with floating geometry, periodic electrical strikes in Ion Storms. | `HealthRefillable` repair zones at Stations. | Vessel explodes / structural failure resulting in immediate Game Over. |

---

## 5. ENVIRONMENTAL HAZARDS & SPATIAL ANCHORS

1. **Starting Hub (`StartingHub`):**
   - Safe starting haven containing docking clamps, repair bays, and refuel triggers.
2. **Asteroid Fields (`AsteroidField_01`):**
   - Physical obstacles with randomized mass, tumbling rotation, and impact colliders. Demands spatial awareness and pitch/roll evasion.
3. **Gravity Anomaly (`GravityAnomaly`):**
   - Supermassive spatial singularity that exerts gravitational pull on the ship's `Rigidbody`, creating an orbital velocity vector and edge surge zone.
4. **Ion Storm Hazard (`IonStorm`):**
   - Volumetric nebula zone that periodically generates warning VFX followed by direct electrical hull damage if the vessel remains inside the perimeter.
5. **Extraction Point (`Warp Gate`):**
   - The primary beacon and finish line marking the completion of the sector run.

---

## 6. NAVIGATION & SPATIAL GUIDANCE (OPEN DECISION / TBD)

> [!NOTE]
> **Status: OPEN DESIGN DECISION (TBD during sector blockout)**
> Navigating 3D zero-gravity space without terrain references poses a risk of disorientation. The exact guidance method will be tested in prototype blockouts.

### Evaluated Approaches:
* **Option A — In-World Diegetic Guidance (Recommended):**
  - Prominent visual beacons, pulsating light pillars, or high-intensity emissive silhouettes visible across space at the Warp Gate and Refuel Stations.
  - *Pros:* Maximizes immersion, avoids screen-space UI clutter, fits the atmospheric space aesthetic.
* **Option B — Screen-Space HUD Waypoints:**
  - Minimalist 2D off-screen indicator or compass reticle pointing toward the target objective and nearest refuel hub.
  - *Pros:* Guarantees the player will never become lost regardless of orientation.

---

## 7. MVP SCOPE BOUNDARIES

* **Included in MVP:**
  - Complete flyable 6DoF spaceship model with fuel, health, and dynamic chase camera.
  - Functional hazard loop (Asteroid collision, Ion Storm strikes, Gravity pull).
  - Working Refuel/Repair station interaction.
  - Win/Lose state triggers and run reset cycle.
* **Excluded / Post-MVP:**
  - Combat and enemy combatants (deferred).
  - Procedural galaxy map generation (MVP uses one curated sector blockout).
  - Upgrades, currency, and inventory management.
