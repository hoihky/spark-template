using Spark.Bindings;
using Spark.Bindings.Components;
using SparkDemoFoundation;

namespace SparkDemoSky;

/// <summary>C# port of Spark shell demo <c>SkyDemo</c> (catalog: Sky).</summary>
public sealed class SkyGame : DemoGameBase
{
    private const string SkyMeshKey = "spark/demo/sky_sphere";
    private const string SkyTextureKey = "spark/demo/sky_equirect";

    private readonly DemoHelpHud _hud = new();
    private GameObject? _skyRoot;
    private SkyComponent? _sky;
    private MaterialComponent? _skyMat;
    private int _skyModeIndex;
    private bool _hasEquirect;
    private string _hdrLabel = "procedural fallback";
    private string _groundTextureKey = "spark/demo/ground_baked";

    public override void OnAttach(IEngineContext context)
    {
        base.OnAttach(context);
        GameWorld world = World!;
        context.GetInput().SetCursorCaptured(true);
        world.MountPlatformerUiFont();

        world.RegisterSkySphereMesh(SkyMeshKey, 1f, 32, 64);
        _hasEquirect = world.TryRegisterSkyDemoHdr(out _hdrLabel);
        if (!_hasEquirect)
        {
            _hdrLabel = "procedural fallback";
        }

        if (world.RegisterSkyDemoGround(out _groundTextureKey, out float groundRoughness))
        {
            SpawnGround(world, groundRoughness);
        }
        else
        {
            SpawnGroundFallback(world);
        }

        SpawnReferenceCubes(world);
        _skyRoot = SpawnSky(world);

        Camera.Position = new Vector3 { X = 0, Y = 4.5f, Z = 14f };
        Camera.SnapLookAt(new Vector3 { X = 0, Y = 1.2f, Z = 0 });

        _hud.Attach(world, Roots, "Sky (C#)");
    }

    public override void OnUpdate(FrameTiming timing, IEngineContext context)
    {
        SceneTimeSeconds = timing.TotalTimeSeconds;
        HandleFlyCameraInput(context, timing);

        if (_skyRoot is not null)
        {
            TransformComponent tr = _skyRoot.GetOrAddTransform();
            tr.Translation = Camera.Position;
            tr.SetRotation(new SparkQuaternion { w = 1 });
            tr.SetUniformScale(SkyScaleForMode(_skyModeIndex));
        }

        IInput input = context.GetInput();
        if (input.IsKeyPressedThisFrame(InputKeys.Escape))
        {
            context.RequestWindowClose();
        }

        if (input.IsKeyPressedThisFrame(77))
        {
            _skyModeIndex = (_skyModeIndex + 1) % 3;
            ApplySkyMode();
        }

        _hud.Update(timing, $"Sky (C#) — {ModeLabel()} · {_hdrLabel} — M cycle");
        World!.UpdateGameObjects(timing, context);
    }

    public override void OnRender(IRenderFrame frame, IEngineContext context) => SubmitSkyScene(context);

    private static float SkyScaleForMode(int modeIndex) => modeIndex == 1 ? 96f : 102f;

    private GameObject SpawnSky(GameWorld world)
    {
        GameObject sky = CreateRoot(world, "SkyEnvironment");
        _sky = sky.AddSky(SparkSceneSkyMode.SparkSceneSkyMode_Box);

        MeshComponent mesh = sky.AddMesh(SparkSceneMeshSlot.SparkSceneMeshSlot_Custom, new Vector3 { X = 1, Y = 1, Z = 1 });
        mesh.SetMeshByKey(world, SkyMeshKey);

        _skyMat = sky.AddMaterialLit(null, new Vector3 { X = 1, Y = 1, Z = 1 });
        ApplySkyMode();
        return sky;
    }

    private void ApplySkyMode()
    {
        if (_sky is null || _skyMat is null)
        {
            return;
        }

        SparkSceneSkyMode mode = _skyModeIndex switch
        {
            1 => SparkSceneSkyMode.SparkSceneSkyMode_Dome,
            2 => SparkSceneSkyMode.SparkSceneSkyMode_Plane,
            _ => SparkSceneSkyMode.SparkSceneSkyMode_Box,
        };
        _sky.SetMode(mode);

        if (_hasEquirect)
        {
            _sky.Tint = new Vector3 { X = 1, Y = 1, Z = 1 };
            _skyMat.SetBaseColorTexture(World!, SkyTextureKey);
        }
        else if (_skyModeIndex == 0)
        {
            _sky.Tint = new Vector3 { X = 0.22f, Y = 0.34f, Z = 0.58f };
        }
        else if (_skyModeIndex == 1)
        {
            _sky.Tint = new Vector3 { X = 0.28f, Y = 0.2f, Z = 0.42f };
        }
        else
        {
            _sky.Tint = new Vector3 { X = 1, Y = 1, Z = 1 };
        }
    }

    private string ModeLabel() => _skyModeIndex switch
    {
        1 => "SkyDome",
        2 => "SkyPlane",
        _ => "SkyBox",
    };

    private void SpawnGround(GameWorld world, float roughness)
    {
        GameObject ground = CreateRoot(world, "Ground");
        // Match SkyDemo.cpp: GroundPlane slot + soil material (static engine ground mesh + UV tiling).
        ground.AddMesh(SparkSceneMeshSlot.SparkSceneMeshSlot_GroundPlane, new Vector3 { X = 1, Y = 1, Z = 1 });
        MaterialComponent mat = ground.AddMaterialLit(_groundTextureKey, new Vector3 { X = 1.0f, Y = 0.98f, Z = 0.94f });
        mat.Roughness = roughness;
        mat.Metallic = 0f;
    }

    private void SpawnGroundFallback(GameWorld world)
    {
        GameObject ground = CreateRoot(world, "Ground");
        ground.AddMesh(SparkSceneMeshSlot.SparkSceneMeshSlot_GroundPlane, new Vector3 { X = 1, Y = 1, Z = 1 });
        MaterialComponent mat = ground.AddMaterialLit(null, new Vector3 { X = 1.0f, Y = 0.98f, Z = 0.94f });
        mat.Roughness = 0.91f;
        mat.Metallic = 0f;
    }

    private void SpawnReferenceCubes(GameWorld world)
    {
        SpawnReferenceCube(world, new Vector3 { X = 0, Y = 0.62f, Z = 0 }, 1.25f, new Vector3 { X = 0.78f, Y = 0.52f, Z = 0.28f }, 0.42f, 0.08f);
        SpawnReferenceCube(world, new Vector3 { X = -5.5f, Y = 0.48f, Z = 3.2f }, 0.95f, new Vector3 { X = 0.62f, Y = 0.66f, Z = 0.72f }, 0.55f, 0.18f);
        SpawnReferenceCube(world, new Vector3 { X = 4.8f, Y = 0.56f, Z = -3.6f }, 1.05f, new Vector3 { X = 0.84f, Y = 0.44f, Z = 0.30f }, 0.38f, 0.12f);
    }

    private void SpawnReferenceCube(
        GameWorld world,
        Vector3 pos,
        float uniformScale,
        Vector3 albedo,
        float roughness,
        float metallic)
    {
        GameObject cube = CreateRoot(world, "SkyDemoProp");
        TransformComponent tr = cube.GetOrAddTransform();
        tr.Translation = pos;
        tr.SetUniformScale(uniformScale);
        cube.AddMesh(SparkSceneMeshSlot.SparkSceneMeshSlot_UnitCube, albedo);
        MaterialComponent? mat = cube.GetMaterial();
        if (mat is not null)
        {
            mat.Roughness = roughness;
            mat.Metallic = metallic;
        }
    }
}
