# C++ templates

Standalone **C++23** game projects that link the [Spark](https://github.com/) engine as `SparkEngine`. Each subdirectory is its own CMake project (open in CLion or configure from the command line).

| Project | Path | Description |
|---------|------|-------------|
| **3D ARPG** | [`arpg/`](arpg/) | Dungeon room, Fox hero, third-person camera, skybox, ECS lighting |

Default engine location when `spark` and `spark-template` are siblings under the same parent:

`SPARK_ROOT` → `../../../spark` (from `C++/arpg`)

Override with `-DSPARK_ROOT=<path>` if your layout differs.
