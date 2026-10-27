# Spark template — C# (spark-sharp)

`C#/arpg` is a C# port of the Kenney dungeon ARPG in `C++/arpg`, built on [spark-sharp](https://github.com/your-org/spark-sharp) (`Spark.Scripting` + generated `Spark.Bindings`). No custom native interop sources are required beyond what spark-sharp already ships.

## Prerequisites

- .NET 8 SDK
- Built **spark-sharp** with `SparkScriptHost` (`-DSPARK_SHARP_BUILD_SCRIPT_HOST=ON`)
- **Spark engine** tree with `assets/` (Fox model, sky HDR, UI font), same as the C++ template

## Build the game assembly

```bash
dotnet build C#/arpg/ArpgCsGame.csproj -c Release
```

Kenney props are copied from `C++/arpg/assets` into `bin/Release/net8.0/assets` on build.

### Frame rate

The ARPG scene is GPU-bound when **SparkEngine is built as Debug**: Vulkan Khronos validation is enabled (`NDEBUG` off), which often caps this template around **15–20 FPS**. For **~60 FPS**, build spark-sharp and the game in **Release**:

```bash
cmake -S /path/to/spark-sharp -B /path/to/spark-sharp/build \
  -DCMAKE_BUILD_TYPE=Release -DSPARK_SHARP_BUILD_SCRIPT_HOST=ON
cmake --build /path/to/spark-sharp/build --target SparkScriptHost
dotnet build C#/arpg/ArpgCsGame.csproj -c Release
./C#/tools/run-arpg-cs-game.sh Release
```

`run-arpg-cs-game.sh` defaults to **Release** and prints a reminder if the native CMake cache is still Debug.

## Run

From **spark-sharp** (or use the helper script in this repo):

```bash
export SPARK_ENGINE_ROOT=/path/to/spark
export SPARK_ARPG_ASSETS=/optional/override/to/game/assets   # optional

/path/to/spark-sharp/tools/run-spark-script-host.sh \
  /path/to/spark-sharp/build Release \
  /path/to/spark-template/C#/arpg/bin/Release/net8.0/ArpgCsGame.runtimeconfig.json \
  /path/to/spark-template/C#/arpg/bin/Release/net8.0/ArpgCsGame.dll \
  "SparkArpg.GameEntry, ArpgCsGame" \
  Initialize
```

Or:

```bash
./C#/tools/run-arpg-cs-game.sh
```

The script host working directory must be `SPARK_ENGINE_ROOT` so engine assets resolve.

If you see `load_assembly_and_get_function_pointer failed: -2147024894` (`0x80070002`), the host is still using the default entry type `HelloCsGame.GameEntry`. Pass the managed entry as the 3rd and 4th arguments after the DLL path (see command above).

## Parity notes (vs C++)

| Area | C# behavior |
|------|-------------|
| Third-person camera | `CharacterCameraRig` ported in C# (same math as `Spark::CharacterCameraRig`) |
| Character controller walls | `CharacterController3DComponent.SetMoveInput` + `GameWorld.SimulateCharacterControllers3D` (`Spark.Bindings`) |
| Player visual hierarchy | No `SetParent` — Fox/skinned mesh on the player root via `GameObject.TryAddSkinnedCharacterFromGltf` |
| Sky mesh | `GameWorld.RegisterSkySphereMesh` + `SkyComponent`; HDR equirect when `MaterialComponent.SetBaseColorTexture` succeeds |
| ESC quit | `IEngineContext.RequestWindowClose()` |

Controls match the C++ template: WASD, Shift run, F attack, F1 capture mouse.

## Sharing / version control

Committed sources use **relative** paths only (`ArpgCsGame.csproj` → `../../../spark-sharp`, `run-arpg-cs-game.sh` → repo-relative roots, `CMakePresets.json` → `${sourceDir}`). Do not commit `bin/`, `obj/`, `.idea/`, or `*.DotSettings.user` — they embed local absolute paths and NuGet cache locations.
