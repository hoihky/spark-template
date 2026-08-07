# Spark Project Templates

This repository is a **collection of standalone game project templates** for learning and building on the [Spark](https://github.com/) game engine. Each template is a complete CMake project that links Spark as a library, implements a focused slice of gameplay, and demonstrates **public Spark APIs** (`Spark::Game`, ECS components, scene submission, physics, animation, and more) without modifying the engine itself.

Use these projects as starting points, reference implementations, or teaching material when exploring how Spark is meant to be used in real C++ game code.

## Repository layout

```
spark-template/
  README.md           ← this file
  arpg/               ← 3D action RPG starter (dungeon, character controller, third-person camera)
  …                   ← more templates planned (2D platformer, FPS, UI-focused, etc.)
```

Each subdirectory is an **independent project**: open it in CLion (or configure with CMake), set `SPARK_ROOT` to your Spark engine clone, build, and run.

## Available templates

| Template | Path | What it teaches |
|----------|------|-----------------|
| **3D ARPG** | [`arpg/`](arpg/) | `Spark::Game` lifecycle, state machine, glTF level props, skinned hero (Fox), character controller, third-person `CharacterCameraRig`, locomotion FSM, `SubmitStandardLitSceneFromWorld`, asset loading and caching |

See each template’s own `README.md` and `DEVELOPMENT_PLAN.md` (where present) for controls, architecture notes, and a phased roadmap.

## Prerequisites

Shared across all templates:

- **CMake 3.28+** and a C++23 compiler
- A clone of the **Spark engine** repository (sibling path `../spark` is the default in CMake presets)
- **Vulkan SDK** for Spark’s renderer (e.g. `glslangValidator` on `PATH` on first configure)

Typical setup:

```bash
# Example: clone Spark next to this repo
#   projects/spark
#   projects/spark-template/

cd arpg
cmake -S . -B cmake-build-debug -DSPARK_ROOT=../../spark
cmake --build cmake-build-debug
```

CLion users can open a template folder directly and enable **CMake presets** from `CMakePresets.json`.

## How templates relate to Spark

| Principle | Description |
|-----------|-------------|
| **Engine boundary** | Game code lives in the template repo; Spark is linked via `SPARK_ROOT`, not vendored as a submodule of game source. |
| **Public API only** | Templates depend on headers under `spark/include` (engine, scene, ecs, render, physics, animation). |
| **Shared `SparkEngine`** | Templates build Spark as a **shared library** for faster iteration; see each template’s `CMakeLists.txt`. |
| **Upgrade path** | Bump Spark, rebuild the template, fix compile breaks in template code only—see per-template upgrade notes. |

Official engine docs live in the Spark repo: `README.md`, `docs/`, and `docs/programming-guide/`.

## Adding or choosing a template

1. Pick the template that matches the APIs you want to learn (3D lit scenes → `arpg`; future 2D templates will highlight sprites, tilemaps, and `Camera2D`).
2. Copy or fork the template folder for your own game.
3. Keep `SPARK_ROOT` as a CMake cache variable so the engine path stays portable across machines.

More templates will be added over time to cover other genres and Spark features (2D platformer, top-down ARPG, audio-heavy demos, UI/canvas, networking hooks, etc.).

## License

Follow the license of the Spark engine and any third-party assets bundled in each template (e.g. Kenney dungeon assets in `arpg/assets/`).
