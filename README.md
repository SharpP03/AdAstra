# AdAstra 

> Procedural roguelike about survival, exploration, and progression through a fragmented space.

AdAstra is a Unity (C#) game project focused on systemic gameplay: traveling between dynamically generated locations, managing limited resources, and making decisions under constant risk.

The player progresses toward the **galaxy center**, navigating an unstable network of paths filled with threats, opportunities, and unknown events.

---

## Design Pillars

- **Exploration over linear progression**  
  No obvious paths. Player chooses direction with limited information.

- **System-driven gameplay**  
  Mechanics interact (fuel, enemies, environment, events).

- **Risk vs Reward**  
  Safer routes = slower progress. Dangerous routes = faster gains.

- **Replayability**  
  Each run generates a different world layout and encounter flow.

---

## Core Gameplay Loop
- Navigate between nodes (locations)
- Handle events (combat, anomalies, traders)
- Consume fuel while traveling
- Find upgrades and resources
- Move closer to final objective (galaxy center)

---

## World Structure

The world is generated as a **semi-structured sequence of interconnected locations with controlled randomness**.
Location types are distributed using **alternating patterns**:
- Hostile
- Friendly
- Neutral
- Misc (events, anomalies)


### Location Types

**Asteroids**
- Collision hazards
- Resource drops on destruction

**Stations**
- Refuel
- Upgrade technology
- Restore shields

**High Gravity Planets**
- Constant pull toward center
- Environmental hazard
- High-risk resource zones
- Escape sequence mechanic (planned)

**Destroyed Flagship Chunks**
- Enemy-heavy areas
- Provide navigation hints (progress direction)

---

## World Generation

- Semi-structured path generation (not fully random)
- Controlled randomness with distribution rules
- Alternating node types to maintain gameplay rhythm

### Additional Systems

- Increasing danger away from main route
- Non-linear navigation (no obvious correct path)
- Random events:
  - Traders
  - Enemy patrols
  - Anomalies (teleport, etc.)

---

## Combat & Enemies

### Enemy Behavior (current)

Enemies are built around simple, aggressive AI.

They detect the player at range and engage directly, closing distance while tracking the target. 
The current design focuses on creating constant pressure rather than complex behaviors.

Attack mechanics are currently in development.

### Architecture

- Component-based system (composition)
- Modular subsystems (e.g. events, weapons, controllers)

This allows:
- Easy creation of enemy variants
- Reusable behaviors
- Scalable design

---

## Systems

### Fuel

- Core limiting resource
- Determines how far player can travel
- Refueling via stations or events

### Navigation

- Indirect guidance (no markers)
- Direction hints through world elements
- Player-driven exploration

### Events

- Randomized encounters during travel:
  - Traders
  - Combat situations
  - Environmental hazards
  - Anomalies

---

## Technical Overview

- Engine: Unity
- Language: C#
- Architecture:
  - Component-based entities
  - Modular systems (enemy, weapons, events)
  - Physics-driven interactions (planned)

---

## Current State

Prototype phase.

Implemented:
- Basic movement & enemy behavior
- Initial world structure
- Early systems (fuel, locations)

In progress:
- Combat system (shooting & damage)
- Procedural generation core
- Event system

---

## Roadmap

- [ ] Full combat system (shooting, damage, feedback)
- [ ] Enemy variety
- [ ] Weapon system expansion
- [ ] Event system expansion
- [ ] UI / player feedback
- [ ] World generation balancing
- [ ] Final Goal (Galaxy center)
- [ ] Navigation hints
- [ ] Location based events (resources, refils)
- [ ] Performance optimization (object pooling)

---
