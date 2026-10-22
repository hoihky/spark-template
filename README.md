# Spark Project Templates

This repository is a **collection of standalone game project templates** for learning and building on the [Spark](https://github.com/) game engine. Each template is a complete CMake project that links Spark as a library, implements a focused slice of gameplay, and demonstrates **public Spark APIs** (`Spark::Game`, ECS components, scene submission, physics, animation, and more) without modifying the engine itself.

Use these projects as starting points, reference implementations, or teaching material when exploring how Spark is meant to be used in real game code.

## Repository layout

```
spark-template/
  README.md           ← this file
  C++/                ← C++23 templates (CMake + SparkEngine)
    README.md
    arpg/             ← 3D action RPG starter
  …                   ← more languages / templates later
```

Each template under `C++/` is an **independent project**: open it in CLion (or configure with CMake), set `SPARK_ROOT` to your Spark engine clone, build, and run.

## Available templates

| Template | Path | What it teaches |
|----------|------|-----------------|
| **3D ARPG** | [`C++/arpg/`](C++/arpg/) | `Spark::Game` lifecycle, state machine, glTF dungeon, skybox, ECS lights, skinned hero (Fox), character controller, third-person `CharacterCameraRig`, locomotion FSM, `SubmitStandardLitSceneFromWorld`, asset loading and caching |

See each template’s own `README.md` and `DEVELOPMENT_PLAN.md` (where present) for controls, architecture notes, and a phased roadmap.

## Prerequisites

Shared across all templates:

- **CMake 3.28+** and a **C++23** compiler
- A clone of the **Spark engine** repository (default preset assumes `spark` is a sibling of `spark-template`: `SPARK_ROOT=../../../spark` from `C++/arpg`)
- **Vulkan SDK** for Spark’s renderer (e.g. `glslangValidator` on `PATH` on first configure)

Typical setup:

```bash
# Layout example:
#   projects/spark
#   projects/spark-template/C++/arpg

cd C++/arpg
cmake -S . -B cmake-build-debug -DSPARK_ROOT=../../../spark
cmake --build cmake-build-debug
```

CLion users can open `C++/arpg` directly and enable **CMake presets** from `CMakePresets.json`.

## How templates relate to Spark

| Principle | Description |
|-----------|-------------|
| **Engine boundary** | Game code lives in the template repo; Spark is linked via `SPARK_ROOT`, not vendored as a submodule of game source. |
| **Public API only** | Templates use current Spark headers (e.g. `spark/scene/core/GameWorld.hpp`, `spark/scene/submit/SceneSubmit.hpp`, `spark/scene/material/GltfMaterial.hpp`). |
| **Shared `SparkEngine`** | Templates build Spark as a **shared library** for faster iteration; see each template’s `CMakeLists.txt`. |
| **Upgrade path** | Bump Spark, rebuild the template, fix compile breaks in template code only—see per-template upgrade notes. |

Official engine docs live in the Spark repo: `README.md`, `docs/`, and `docs/programming-guide/`.

## Adding or choosing a template

1. Pick the template that matches the APIs you want to learn (3D lit scenes → `C++/arpg`).
2. Copy or fork the template folder for your own game.
3. Keep `SPARK_ROOT` as a CMake cache variable so the engine path stays portable across machines.

More templates will be added under `C++/` (and other top-level folders) over time.

## License

Follow the license of the Spark engine and any third-party assets bundled in each template (e.g. Kenney dungeon assets in `C++/arpg/assets/`).
