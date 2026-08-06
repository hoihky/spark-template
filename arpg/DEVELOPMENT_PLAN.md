# Spark ARPG — Development Plan

This document is a phased roadmap for growing the template into a full **3D action RPG** on Spark. Each phase adds vertical slices you can play-test before moving on.

## Principles

1. **Engine boundary** — Gameplay uses `Spark::Game`, `GameWorld`, ECS components, and `SceneSubmit` APIs. Spark repo changes are rare; one optional API (`Skeleton::AdoptAnimationClipsFrom`) exists for Quaternius heroes without embedded clips.
2. **Version resilience** — Keep `SPARK_ROOT` as a CMake cache variable. When upgrading Spark:
   - Build this project against the new engine in a branch.
   - Run `SparkArpg` and fix compile breaks (usually renamed headers or component API tweaks).
   - Re-run Spark demos (`SparkDemo`) to see new features you can adopt.
3. **Composition over inheritance** — New behaviors = new `GameComponent` subclasses or small services, not deep `Player` class hierarchies.
4. **Data-driven content** — Level layouts, enemy tables, and item defs should move to JSON/TOML or Spark scene documents as soon as hand-placed code becomes painful.

---

## Phase 0 — Foundation (complete)

**Goal:** Runnable project in CLion, shared `SparkEngine`, playable dungeon room.

**Delivered:**

| Area | Module | Notes |
|------|--------|-------|
| Build | `CMakeLists.txt`, `CMakePresets.json` | Shared `SparkEngine`, assets copied post-build, `SPARK_BUILD_SCRIPT_HOST=OFF` |
| Entry | `ArpgGame`, `GameStateMachine`, `PlayState` | State pattern; `PlayState` owns level + player lifecycle |
| Services | `AssetPaths`, `AssetRegistry`, `UiBootstrap` | Path resolution; glTF cache; shared Kenney dungeon atlas (scene texture budget); Roboto HUD font |
| World | `DungeonLevelBuilder` | **11×11** grid (~22×22 m), perimeter walls, north doorway, plaza props |
| Ground snap | `GroundYOffsetForMesh` | Props rest on `Y = 0` via mesh AABB min |
| Player | `PlayerAvatar` | Fox.glb (Spark repo) with CesiumMan fallback; skinned mesh + locomotion FSM |
| Camera | `CharacterCameraRig` | **Third-person only** (no first-person toggle) |
| Input | `PlayerController` | WASD, run, attack, mouse look, F1 capture, ESC quit |
| Render | `SubmitStandardLitSceneFromWorld` | Far plane 200 m; standard lit dungeon |

**Play-test:** Walk the room, collide with walls, run, attack animation, third-person mouse look.

**Lessons learned (keep in mind for larger levels):**

- Spark’s lit scene path has a **limited unique texture count** (~16). Kenney props all use `dungeon_texture.png`; `AssetRegistry::PrimeSharedTextures` reuses one `Texture2D` instance across props.
- Skinned avatars need a skeleton with **animation clips** (or merged clips via `AdoptAnimationClipsFrom`) for `AnimatorComponent` / skinned draw paths.
- Quaternius `Rogue.glb` in `assets/models/` has skin but **no clips** — use Fox, or merge `Rig_Medium_MovementBasic.glb` in `PlayerAvatar` before switching hero.

---

## Phase 1 — Player polish & content paths

**Goal:** Feel like an ARPG hero in your own art.

| Task | Spark / game APIs | Patterns |
|------|-------------------|----------|
| Custom skinned hero in `assets/models/` | `LoadSkinnedGltf`, `SkinnedMeshComponent`, `AnimatorComponent` | Extend `PlayerAvatar` factory |
| Quaternius / clip-less mesh | `Skeleton::AdoptAnimationClipsFrom` | Merge movement rig at spawn |
| Weapon attach socket | `AttachmentSocket` component, child prop meshes (`sword_1handed.gltf`, etc.) | Composition |
| Dodge / roll | `Character3DAnimFsmComponent`, temporary speed boost + cooldown | State in `PlayerController` or player component |
| Footstep / swing SFX | `SoundCueComponent`, `ProcessSoundCues` | Observer via animation events |
| Point lights on torches | `PointLightComponent` on torch spawns | Factory in `DungeonLevelBuilder` |

**Exit criteria:** Custom character mesh, visible weapon, dodge, basic audio.

---

## Phase 2 — Combat core

**Goal:** Hit things and take damage.

| Task | Implementation hint |
|------|---------------------|
| **Weapon hitbox** | Short-lived `SphereCollider3D` or sweep along attack arc during attack clip |
| **Damage pipeline** | `SignalId::DamageApplied` + `DamageSignalPayload` (see Spark ECS docs) |
| **Health component** | `HealthComponent` on player and enemies; UI bar via `UiCanvasComponent` |
| **Lock-on (optional)** | Ray pick enemy; camera rig orbits toward target |
| **Hit feedback** | Emissive flash on `MaterialComponent`, brief freeze, screen shake via camera offset |

**Patterns:** Command pattern for attack types (`IAttackAction`), strategy for weapon profiles.

**Exit criteria:** Kill a stationary target; player dies when health reaches zero.

---

## Phase 3 — Enemies & AI

**Goal:** Hostile actors that chase and fight.

| Task | Spark APIs |
|------|------------|
| Enemy factory | Mirror `PlayerAvatar`; use prop meshes or new glTF |
| Patrol / chase | `AiAgentComponent`, `SimulateGameAi`, steering |
| Attack telegraph | FSM states: Idle → Windup → Strike → Recover |
| Physics option | `Rigidbody3DComponent` for knockback on heavy hits |

**Content:** Define `EnemyDef` structs (mesh, health, damage, AI params) loaded from data files.

**Exit criteria:** One enemy type spawns, chases, attacks, dies.

---

## Phase 4 — RPG stats & inventory

**Goal:** Numbers that matter.

| System | Suggested design |
|--------|------------------|
| Attributes | STR / DEX / VIT on `CharacterStatsComponent` |
| XP & levels | `ProgressionService` updates stats on kill signals |
| Inventory | `InventoryComponent` + grid UI (`UiCanvasComponent` / Spark retained UI) |
| Equipment | Slots map to `AttachmentSocket` + stat modifiers |
| Loot | `LootTable` rolls on enemy death; spawn `chest.gltf` / `coin.gltf` pickups |

**Patterns:** Data-driven loot tables; observer on `DamageApplied` / custom `EnemyDefeated` signal.

**Exit criteria:** Gain XP, level up, equip sword from chest, stats change damage.

---

## Phase 5 — Quests & world structure

**Goal:** Purpose beyond combat rooms.

| Task | Notes |
|------|-------|
| Quest log | `QuestService` + UI panel; objectives as small structs |
| NPC interact | `Physics3DTriggerEnter` on doorway / NPC collider |
| Multi-room dungeon | Extend `DungeonLevelBuilder` to read room graph from JSON |
| Scene transitions | `GameStateMachine` → new `PlayState` with different level id |
| Save / load | Serialize player stats + quest flags (Spark `SceneDocument` serialization for world props) |

**Exit criteria:** Talk to NPC (UI prompt), complete “clear room” quest, door opens.

---

## Phase 6 — UI, audio, polish

| Area | Spark tools |
|------|-------------|
| HUD | Health, stamina, minimap — `UiCanvasComponent` (replace or augment `TextOverlayComponent`) |
| Pause menu | `PauseState` in state machine |
| Settings | Input remap, volume — `SoundEngine` gain |
| Ambience | Looping dungeon ambience + torch crackle |
| Post | Tune lighting in `SubmitStandardLitSceneFromWorld` args; optional fog volumes |

---

## Phase 7 — Production pipeline

| Task | Why |
|------|-----|
| Spark Editor integration | Author levels visually when `SparkEditor` matures (see Spark `docs/SPARK_EDITOR_PLAN.md`) |
| Async loading | `GameWorldAssetLoader` — stream next room while playing |
| CI build | Script: configure Spark + `SparkArpg`, smoke-run headless if available |
| Modding | Keep `assets/` and data files separate from binaries |

---

## Current module map

```
src/
  game/         ArpgGame
  core/         GameContext, GameStateMachine, IGameState
  services/     AssetPaths, AssetRegistry, UiBootstrap
  world/        DungeonLevelBuilder
  player/       PlayerAvatar, PlayerController
  states/       PlayState
```

**Planned folders (add when a phase needs them):**

```
src/
  combat/       AttackActions, DamageResolver, HitscanUtil
  ai/           EnemyBrain, EnemyFactory
  rpg/          Stats, Inventory, QuestService, LootTable
  ui/           HudController, PauseMenu
  data/         Json loaders for levels, enemies, items
  states/       MainMenuState, PauseState, PlayState
```

---

## Spark upgrade checklist

When pulling a new Spark version:

1. `cmake --build` — fix compile errors in this repo only.
2. Run `SparkDemo` → Character Camera, Maze 3D, Scene Editor 3D for API drift.
3. Check `docs/programming-guide/` for new components (physics, UI, serialization).
4. Re-test: load glTF props, skinned avatar, character controller, `SubmitStandardLitSceneFromWorld`.
5. Update `README.md` if new CMake options affect `SPARK_BUILD_ENGINE_SHARED`.

---

## Asset reference (`assets/models/`)

The Kenney dungeon pack provides modular pieces used by the template:

- **Floors:** `floor_tile_large.gltf`, `floor_wood_large.gltf`
- **Walls:** `wall.gltf`, `wall_doorway.gltf`, `wall_corner.gltf`
- **Texture:** `dungeon_texture.png` (shared via `AssetRegistry`)
- **Props:** `chest.gltf`, `torch_mounted.gltf`, `pillar.gltf`, `table_medium.gltf`, `sword_shield.gltf`
- **Weapons:** `sword_1handed.gltf`, `bow.gltf`, `staff.gltf`, shields, etc.

Quaternius characters (`Rogue.glb`, `Rig_Medium_MovementBasic.glb`, …) are present for Phase 1 hero experiments.

Hero mesh default: Spark engine `Fox.glb` (not in this `assets/` tree — resolved via `AssetPaths::SparkEngineModelPath`).

Combine Kenney pieces in `DungeonLevelBuilder` or replace with data-driven spawn lists as levels grow.
