# Spark ARPG Template

A **3D action RPG** starter project that links the [Spark](https://github.com/) game engine as a **shared library** (`SparkEngine`). Gameplay code lives entirely in this repository; the engine is referenced via `SPARK_ROOT`.

## Layout

```
arpg/
  CMakeLists.txt          # Links SparkEngine shared lib; copies assets to build output
  CMakePresets.json       # CLion / CMake presets (debug, release)
  assets/models/          # Kenney dungeon glTF props (floors, walls, chests, weapons)
  src/
    main.cpp
    game/ArpgGame.*       # Spark::Game entry; owns state machine
    core/                 # State pattern, GameContext (service locator)
    services/             # AssetPaths, AssetRegistry, UiBootstrap
    world/                # DungeonLevelBuilder (level factory)
    player/               # PlayerAvatar, PlayerController
    states/PlayState.*    # Active gameplay state
```

## Open in CLion

1. **File → Open** → select this `arpg` folder.
2. Enable **Load CMake presets from CMakePresets.json** (Settings → CMake).
3. Select the **debug** profile; build directory: `cmake-build-debug`.
4. If Spark is not at `../../spark`, set cache variable `SPARK_ROOT` to your engine clone.
5. **Build** target `SparkArpg`, then **Run**.

Requires **CMake 3.28+**, **C++23**, and the **Vulkan SDK** (`glslangValidator` on `PATH` when Spark is first configured).

CMake sets `SPARK_BUILD_DEMO=OFF` and `SPARK_BUILD_INTEROP=OFF` so only `SparkArpg` and shared `SparkEngine` are built. `SPARK_BUILD_ENGINE_DEMOS=ON` is required because current Spark links demo shell symbols into `SparkEngine`.

Game assets are resolved at runtime from `<executable>/assets` (copied on each build). No compile-time absolute asset paths.

## Configure manually

```bash
cmake -S . -B cmake-build-debug -DSPARK_ROOT=../../spark
cmake --build cmake-build-debug
```

If Spark is not a sibling of `spark-template`, set `SPARK_ROOT` to a **relative or absolute** path to your engine clone.

Assets are copied beside the executable on each build (`cmake-build-debug/assets` when using the default preset).

## Controls

| Input | Action |
|--------|--------|
| **WASD** | Move (third-person: W/S along facing; mouse turns) |
| **Shift** | Run |
| **F** | Attack animation |
| **F1** | Toggle mouse capture |
| **ESC** | Quit |

## Architecture (design patterns)

| Pattern | Where | Purpose |
|---------|--------|---------|
| **State** | `IGameState`, `GameStateMachine`, `PlayState` | Swap menu / play / pause without tangling `ArpgGame` |
| **Service locator** | `GameContext` | Shared world, engine, asset services for states |
| **Factory** | `DungeonLevelBuilder`, `PlayerAvatar` | Spawn level and player from data + assets |
| **Registry / cache** | `AssetRegistry` | Avoid reloading glTF; stable keys for GPU assets |
| **Composition (ECS)** | Spark `GameObject` + components | Gameplay as components, not inheritance trees |

Spark may release new versions over time. This project depends only on **public Spark headers** (e.g. `spark/scene/core/GameWorld.hpp`, `spark/scene/submit/SceneSubmit.hpp`, `spark/scene/material/GltfMaterial.hpp`) and `SPARK_ROOT` — not engine internals. See `DEVELOPMENT_PLAN.md` for upgrade strategy and feature roadmap.

## Player character

Hero mesh: Spark engine **Fox.glb** (`assets/models/Fox.glb` in the Spark repo, downloaded at configure). **CesiumMan** is used only if Fox fails to load. Third-person camera is always active.

## World size

The dungeon is a **11×11** tile grid (~**22×22 m**) with perimeter walls and a handful of props. Props are snapped so each mesh’s lowest point rests on the floor.

## Further reading

- Spark engine: `SPARK_ROOT/README.md` and `SPARK_ROOT/docs/`
- Step-by-step ARPG roadmap: `DEVELOPMENT_PLAN.md`
