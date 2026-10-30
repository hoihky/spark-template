using Spark.Bindings;
using SparkDemoFoundation;

namespace SparkDemoTerrain;

/// <summary>C# port of Spark shell demo <c>TerrainDemo</c> (catalog: Terrain).</summary>
public sealed class TerrainGame : DemoGameBase
{
    private readonly DemoHelpHud _hud = new();
    private readonly TerrainSceneBuilder _scene = new();
    private TerrainSculptController? _sculpt;

    public override void OnAttach(IEngineContext context)
    {
        base.OnAttach(context);
        GameWorld world = World!;
        context.GetInput().SetCursorCaptured(true);
        world.MountPlatformerUiFont();

        _scene.Build(world, Roots);
        _sculpt = new TerrainSculptController(Camera);

        Camera.Position = TerrainCatalog.InitialCameraPosition;
        Camera.SnapLookAt(TerrainCatalog.InitialLookTarget);

        _hud.Attach(
            world,
            Roots,
            "Terrain (C#)",
            "cyan dot = aim · LMB/RMB sculpt · R reset · F1 mouse");
    }

    public override void OnUpdate(FrameTiming timing, IEngineContext context)
    {
        SceneTimeSeconds = timing.TotalTimeSeconds;
        HandleFlyCameraInput(context, timing);

        _sculpt?.Simulate(timing, context, _scene.Terrain, _scene.TerrainComp, _scene.EditCursor);

        if (context.GetInput().IsKeyPressedThisFrame(InputKeys.Escape))
        {
            context.RequestWindowClose();
        }

        _hud.Update(timing, "Terrain (C#)");
        World!.UpdateGameObjects(timing, context);
    }

    public override void OnRender(IRenderFrame frame, IEngineContext context) =>
        TerrainRenderPass.Submit(World!, context, Camera, SceneTimeSeconds);
}
