using Spark.Bindings;
using Spark.Bindings.Components;
using SparkDemoFoundation;

namespace SparkDemoBasic3D;

/// <summary>C# port of Spark shell demo <c>ThreeDDemo</c> (catalog: Basic 3D).</summary>
public sealed class Basic3DGame : DemoGameBase
{
    private const float CubeScale = 1.0f;
    private const float CubeSpinRadPerSec = DemoMath.TwoPi * 0.35f;
    private const string CheckerKey = "spark/demo/checker";
    private const float GroundClearance = 0.12f;
    private const float HeroTargetMaxExtent = 2.4f;
    private const float HeroDefaultScale = 2.8f;
    private const float ChairTargetMaxExtent = 2.15f;
    private const float ChairDefaultScale = 1.9f;

    private readonly DemoHelpHud _hud = new();
    private readonly HashSet<string> _gltfRequested = new(StringComparer.Ordinal);

    private GameObject? _spinCube;
    private float _cubeYaw;
    private bool _foxSpawned;
    private bool _helmetSpawned;
    private bool _chairSpawned;
    private string _brickTextureKey = "spark/demo/bricks";

    private string _helmetPath = "";
    private string _chairPath = "";
    private string _foxPath = "";

    public override void OnAttach(IEngineContext context)
    {
        base.OnAttach(context);
        GameWorld world = World!;
        context.GetInput().SetCursorCaptured(true);
        world.MountPlatformerUiFont();

        if (!world.RegisterThreeDDemoTextures(out _brickTextureKey))
        {
            world.RegisterCheckerboardTexture(
                CheckerKey,
                256,
                32,
                new Vector3 { X = 0.92f, Y = 0.86f, Z = 0.72f },
                new Vector3 { X = 0.25f, Y = 0.35f, Z = 0.55f });
        }

        _helmetPath = SparkEngineAssets.ModelPath("DamagedHelmet.glb");
        _chairPath = SparkEngineAssets.ModelPath("SheenChair.glb");
        _foxPath = SparkEngineAssets.ModelPath("Fox.glb");
        RequestGltf(_helmetPath);
        RequestGltf(_chairPath);
        RequestSkinnedGltf(_foxPath);

        SpawnGround(world);
        SpawnPrimitives(world);
        SpawnPointLights(world);

        Camera.Position = new Vector3 { X = 10.5f, Y = 6.5f, Z = 16f };
        Camera.SnapLookAt(new Vector3 { X = 1.1f, Y = 1.15f, Z = 2f });

        _hud.Attach(world, Roots, "Basic 3D (C#)");
    }

    public override void OnUpdate(FrameTiming timing, IEngineContext context)
    {
        SceneTimeSeconds = timing.TotalTimeSeconds;
        HandleFlyCameraInput(context, timing);
        PollGltfSpawns(World!);

        if (_spinCube is not null)
        {
            _cubeYaw = (_cubeYaw + timing.DeltaTimeSeconds * CubeSpinRadPerSec) % DemoMath.TwoPi;
            TransformComponent tr = _spinCube.GetOrAddTransform();
            tr.SetRotation(DemoMath.FromAxisAngleY(_cubeYaw));
        }

        if (context.GetInput().IsKeyPressedThisFrame(InputKeys.Escape))
        {
            context.RequestWindowClose();
        }

        _hud.Update(timing, "Basic 3D (C#)");
        World!.UpdateGameObjects(timing, context);
    }

    public override void OnRender(IRenderFrame frame, IEngineContext context) => SubmitStandardScene(context, ssaoEnabled: true);

    private void RequestGltf(string path)
    {
        if (_gltfRequested.Add(path))
        {
            World!.LoadGltf(path);
        }
    }

    private void RequestSkinnedGltf(string path)
    {
        if (_gltfRequested.Add(path))
        {
            World!.LoadSkinnedGltf(path);
        }
    }

    private void PollGltfSpawns(GameWorld world)
    {
        if (!_helmetSpawned)
        {
            if (!world.TryMeasureRigidGltfLayout(
                    _helmetPath,
                    HeroTargetMaxExtent,
                    HeroDefaultScale,
                    out float heroScale,
                    out float heroMinY))
            {
                return;
            }

            float heroY = -heroMinY * heroScale + GroundClearance;
            _helmetSpawned = TrySpawnRigidGltf(
                world,
                _helmetPath,
                "DamagedHelmet",
                new Vector3 { X = 6f, Y = heroY, Z = 0f },
                new SparkQuaternion { w = 1 },
                heroScale);
        }

        if (!_chairSpawned)
        {
            if (!world.TryMeasureRigidGltfLayout(
                    _chairPath,
                    ChairTargetMaxExtent,
                    ChairDefaultScale,
                    out float chairScale,
                    out float chairMinY))
            {
                return;
            }

            float chairY = -chairMinY * chairScale + GroundClearance;
            _chairSpawned = TrySpawnRigidGltf(
                world,
                _chairPath,
                "SheenChair",
                new Vector3 { X = -4.25f, Y = chairY, Z = 4.0f },
                DemoMath.FromAxisAngleY(DemoMath.Pi * 1.35f),
                chairScale);
        }

        if (!_foxSpawned)
        {
            _foxSpawned = TrySpawnFox(world, _foxPath);
        }
    }

    private bool TrySpawnRigidGltf(
        GameWorld world,
        string path,
        string name,
        Vector3 translation,
        SparkQuaternion rotation,
        float uniformScale)
    {
        GameObject probe = world.CreateGameObject("_gltf_probe");
        MeshComponent mesh = probe.AddMesh(SparkSceneMeshSlot.SparkSceneMeshSlot_Custom, new Vector3 { X = 1, Y = 1, Z = 1 });
        if (!mesh.SetMeshByKey(world, path))
        {
            world.DestroyGameObject(probe);
            return false;
        }

        world.DestroyGameObject(probe);

        GameObject go = CreateRoot(world, name);
        TransformComponent tr = go.GetOrAddTransform();
        tr.Translation = translation;
        tr.SetRotation(rotation);
        tr.SetUniformScale(uniformScale);
        if (!go.BindRigidGltfMesh(world, path))
        {
            world.DestroyGameObject(go);
            return false;
        }

        return true;
    }

    private bool TrySpawnFox(GameWorld world, string path)
    {
        GameObject fox = CreateRoot(world, "FoxWalk");
        TransformComponent tr = fox.GetOrAddTransform();
        tr.SetUniformScale(0.032f);
        tr.Translation = new Vector3 { X = 1.8f, Y = 0, Z = 7.25f };
        tr.SetRotation(DemoMath.FromAxisAngleY(DemoMath.HalfPi * 0.5f));
        if (!fox.TryAddSkinnedCharacterFromGltf(world, path, out _, out AnimatorComponent? animator, out _))
        {
            world.DestroyGameObject(fox);
            return false;
        }

        fox.BindSkinnedGltfMesh(world, path);
        Character3DAnimFsmComponent? fsm = fox.GetCharacter3DAnimFsm();
        if (fsm is not null && animator is not null && animator.GetClipCount() > 0)
        {
            fsm.ConfigureLocomotionFromSkeleton(animator, 0);
            fsm.SetLocomotionDrivingEnabled(1);
            fsm.WalkSpeedThreshold = 0.25f;
        }

        return true;
    }

    private void SpawnGround(GameWorld world)
    {
        GameObject ground = CreateRoot(world, "Ground");
        ground.AddMesh(
            SparkSceneMeshSlot.SparkSceneMeshSlot_GroundPlane,
            new Vector3 { X = 0.58f, Y = 0.6f, Z = 0.64f });
    }

    private void SpawnPrimitives(GameWorld world)
    {
        _spinCube = CreateRoot(world, "Cube");
        TransformComponent cubeTr = _spinCube.GetOrAddTransform();
        cubeTr.Translation = new Vector3 { X = 0, Y = CubeScale, Z = 0 };
        cubeTr.SetUniformScale(CubeScale);
        _spinCube.AddMesh(SparkSceneMeshSlot.SparkSceneMeshSlot_UnitCube, new Vector3 { X = 0.75f, Y = 0.42f, Z = 0.32f });
        MaterialComponent? emissive = _spinCube.GetMaterial();
        if (emissive is null)
        {
            emissive = _spinCube.AddMaterialLit(null, new Vector3 { X = 0.75f, Y = 0.42f, Z = 0.32f });
        }

        emissive.SetEmissive(new Vector3 { X = 0.95f, Y = 0.48f, Z = 0.1f }, 2.4f);
        emissive.Roughness = 0.35f;

        GameObject textured = CreateRoot(world, "TexturedCube");
        TransformComponent texTr = textured.GetOrAddTransform();
        texTr.Translation = new Vector3 { X = 3, Y = CubeScale, Z = 0 };
        texTr.SetUniformScale(CubeScale);
        textured.AddMesh(SparkSceneMeshSlot.SparkSceneMeshSlot_UnitCube, new Vector3 { X = 1, Y = 1, Z = 1 });
        MaterialComponent texMat = textured.AddMaterialLit(CheckerKey, new Vector3 { X = 1, Y = 1, Z = 1 });
        texMat.Metallic = 0.04f;
        texMat.Roughness = 0.22f;

        GameObject brickCube = CreateRoot(world, "BrickCube");
        TransformComponent brickTr = brickCube.GetOrAddTransform();
        brickTr.Translation = new Vector3 { X = -3, Y = CubeScale, Z = 0 };
        brickTr.SetUniformScale(CubeScale);
        brickCube.AddMesh(SparkSceneMeshSlot.SparkSceneMeshSlot_UnitCube, new Vector3 { X = 1, Y = 1, Z = 1 });
        MaterialComponent brickMat = brickCube.AddMaterialLit(_brickTextureKey, new Vector3 { X = 1, Y = 1, Z = 1 });
        brickMat.Metallic = 0f;
        brickMat.Roughness = 0.88f;

        GameObject marker = CreateRoot(world, "BillboardMarker");
        TransformComponent markTr = marker.GetOrAddTransform();
        markTr.Translation = new Vector3 { X = 0, Y = 1.65f, Z = 3.2f };
        marker.AddMesh(SparkSceneMeshSlot.SparkSceneMeshSlot_UnitCube, new Vector3 { X = 0.2f, Y = 0.95f, Z = 0.35f });
        MaterialComponent markMat = marker.AddMaterialEmissive(new Vector3 { X = 0.35f, Y = 1.0f, Z = 0.45f }, 3.5f);
        markMat.Roughness = 0.2f;
        BillboardComponent? bb = marker.GetOrAddBillboard();
        bb?.SetEnabled(1);
        if (bb is not null)
        {
            bb.Mode = 1;
        }
    }

    private void SpawnPointLights(GameWorld world)
    {
        AddPoint(world, new Vector3 { X = 2.2f, Y = 3.8f, Z = 5.5f }, new Vector3 { X = 0.35f, Y = 0.72f, Z = 1.0f }, 4.5f, 19f);
        AddPoint(world, new Vector3 { X = -6.5f, Y = 2.2f, Z = -1.5f }, new Vector3 { X = 1.0f, Y = 0.32f, Z = 0.18f }, 3.8f, 15f);
        AddPoint(world, new Vector3 { X = 0, Y = 5.8f, Z = -4.2f }, new Vector3 { X = 0.85f, Y = 0.92f, Z = 0.45f }, 3.0f, 23f);
    }

    private void AddPoint(GameWorld world, Vector3 pos, Vector3 color, float intensity, float range)
    {
        GameObject light = CreateRoot(world, "PointLight");
        light.GetOrAddTransform().Translation = pos;
        PointLightComponent pl = light.AddPointLight(color, intensity, range);
        pl.SetCastsShadow(1);
    }
}
