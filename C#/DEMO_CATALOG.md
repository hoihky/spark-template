# Spark engine demos — C# ports

Each row matches a **Spark shell demo** from `spark/src/spark/demo/DemoCatalog.cpp`.  
One folder per demo under `C#/<Name>/`, same layout as [`C#/arpg`](arpg/) (local `.csproj`, `GameEntry.cs`, `*Game.cs`).

Shared helpers live in [`SparkDemoFoundation`](SparkDemoFoundation/) — prefer these over copy-paste when adding demos:

| Type | Use |
|------|-----|
| `DemoGameBase` | `SparkGame` + root tracking + fly camera + standard lit submit |
| `FlyCamera` | 3D demo camera (WASD, mouse, Space/Shift) |
| `StandardLitPass` | `GameWorld.SubmitStandardLit` with demo sun/ambient |
| `SparkEngineAssets` | Resolve `assets/…` under `SPARK_ENGINE_ROOT` |
| `DemoMath` | Vector / look-at helpers (distinct from `Spark.Bindings.SparkMath`) |
| `DemoHelpHud` | FPS + title overlay |
| `InputKeys` | GLFW key constants |

## Run

```bash
export SPARK_ENGINE_ROOT=/path/to/spark   # must contain assets/
./C#/tools/run-cs-demo.sh Basic3D
./C#/tools/run-cs-demo.sh Sky
./C#/tools/run-cs-demo.sh Terrain
./C#/tools/run-arpg-cs-game.sh           # game template (not a shell demo)
```

Every demo registers via `[ModuleInitializer]` → `GameBootstrap.Factory` and exports `GameEntry.Initialize` for `SparkScriptHost`.

## Port status

| # | C++ demo | Folder | Status | Notes |
|---|----------|--------|--------|-------|
| — | *(game template)* | [`arpg`](arpg/) | **Done** | Full ARPG, not shell catalog |
| 1 | Basic 3D (`ThreeDDemo`) | [`Basic3D`](Basic3D/) | **Done** | glTF async spawn, point lights, fox FSM |
| 2 | Sky (`SkyDemo`) | [`Sky`](Sky/) | **Done** | HDR sky, M cycles box/dome/plane |
| 3 | VFX Showcase | `VfxShowcase` | Planned | Needs retained UI + `.sparkvfx` save |
| 4 | Terrain (`TerrainDemo`) | [`Terrain`](Terrain/) | **Done** | Sculpt brush, outdoor TOD submit, soil texture |
| 5 | Character camera | `CharacterCamera` | Planned | Port `CharacterCameraRig` + skinned hero |
| 6 | 2D platformer | `Platformer2D` | Planned | `RegisterPlatformer2dDemoTextures` |
| 7 | Broad-phase 2D | `BroadPhase2D` | Planned | 2D physics + joints |
| 8 | 3D maze | `Maze3D` | Planned | AI + procedural maze |
| 9 | 3D physics balls | `PhysicsBall3D` | Planned | ImGui tuning (optional) |
| 10 | Scene editor 3D | `SceneEditor3D` | Planned | Retained UI |
| 11 | Tetris 2D | `Tetris2D` | Planned | 2D composite |
| 12 | Match-3 | `Connect3` | Planned | Grid gameplay |
| 13 | Space Invaders 2D | `SpaceInvaders2D` | Planned | Arcade |
| 14 | Toon shading | `ToonShading` | Planned | Material variants |
| 15 | Steering 3D | `SteeringShowcase3D` | Planned | AI steering |
| 16 | Material ball | `MaterialShowcase3D` | Planned | PBR live tuning |
| 17 | Time of day | `TimeOfDay` | Planned | Volumes + CSM |
| 18 | 2D render layers | `RenderLayers2D` | Planned | Layer masks |
| 19 | Tilemap showcase | `TilemapShowcase2D` | Planned | TMX + pathfinding |
| 20 | Dear ImGui | `ImGuiShowcase` | Planned | Editor UI |
| 21 | glTF PBR showcase | `GltfSamples3D` | Planned | IBL studio |
| 22 | Model viewer 3D | `ModelViewer3D` | Planned | TAB cycle models |
| 23 | Water lake | `WaterLake` | Planned | Ocean + terrain |
| 25 | 2D VFX | `VfxShowcase2D` | Planned | Sprite VFX |
| 26 | 2D product path | `GameFlow2DProductPath` | Planned | GameFlow + save |
| 27 | Foliage wind | `FoliageWind` | Planned | Wind UBO |
| 28 | Grass field terrain | `GrassFieldTerrain` | Planned | GPU grass |
| 29 | Physics 3D showcase | `PhysicsShowcase3D` | Planned | Triggers, CCD |
| 30 | Spark-pix 2D | `SparkPixShowcase2D` | Planned | Atlas manifest |

## Adding the next demo

1. Copy [`Basic3D/Basic3DDemo.csproj`](Basic3D/Basic3DDemo.csproj) → `C#/<Name>/`.
2. Implement `<Name>Game : DemoGameBase` mirroring the C++ demo’s `Load` / `Simulate` / `Render` using **Spark.Bindings** only.
3. Add `GameEntry.cs` with namespace `SparkDemo<Name>`.
4. Register in `C#/tools/run-cs-demo.sh` and update this catalog.
5. Prefer `SubmitStandardLit` when the C++ demo used `SubmitStandardLitSceneFromWorld`; document gaps when C++ used custom `SceneRenderParams` fills.

## API patterns (learn from samples)

```csharp
// Lifecycle
public sealed class MyGame : DemoGameBase {
  public override void OnAttach(IEngineContext ctx) {
    base.OnAttach(ctx);
    GameWorld w = World!;
    GameObject go = CreateRoot(w, "Entity");
    go.GetOrAddTransform().Translation = new Vector3 { X = 0, Y = 1, Z = 0 };
    go.AddMesh(SparkSceneMeshSlot.SparkSceneMeshSlot_UnitCube, new Vector3 { X = 1, Y = 1, Z = 1 });
  }
  public override void OnUpdate(FrameTiming t, IEngineContext ctx) {
    SceneTimeSeconds = t.TotalTimeSeconds;
    HandleFlyCameraInput(ctx, t);
    World!.UpdateGameObjects(t, ctx);
  }
  public override void OnRender(IRenderFrame f, IEngineContext ctx) => SubmitStandardScene(ctx);
}
```

```csharp
// glTF (async — retry spawn until SetMeshByKey succeeds)
world.LoadGltf(SparkEngineAssets.ModelPath("DamagedHelmet.glb"));
// Skinned character
world.LoadSkinnedGltf(path);
fox.TryAddSkinnedCharacterFromGltf(world, path, out _, out var anim, out _);
```

See [`C#/README.md`](README.md) for build prerequisites (Release native host, `SPARK_ENGINE_ROOT`).
