using Spark.Bindings;
using Spark.Bindings.Components;
using Spark.Scripting;

namespace SparkDemoFoundation;

/// <summary>
/// Base for Spark shell demos: owns <see cref="GameWorld"/>, tracks roots, fly camera input, standard lit submit.
/// </summary>
public abstract class DemoGameBase : SparkGame
{
    protected readonly DemoRootCollection Roots = new();
    protected readonly FlyCamera Camera = new();
    protected float SceneTimeSeconds;

    protected GameObject CreateRoot(GameWorld world, string name)
    {
        GameObject go = world.CreateGameObject(name);
        Roots.Track(go);
        return go;
    }

    protected void HandleFlyCameraInput(IEngineContext context, FrameTiming timing)
    {
        IInput input = context.GetInput();
        if (input.IsKeyPressedThisFrame(InputKeys.F1))
        {
            input.SetCursorCaptured(!input.IsCursorCaptured());
        }

        if (input.IsCursorCaptured() && timing.FrameIndex > 0)
        {
            Camera.AddLook(input.GetMouseDeltaX(), input.GetMouseDeltaY());
            Camera.ProcessMovement(input, timing.DeltaTimeSeconds);
        }
    }

    public override void OnDetach()
    {
        if (World is not null)
        {
            Roots.DestroyAll(World);
        }

        base.OnDetach();
    }

    protected void SubmitStandardScene(
        IEngineContext context,
        bool enableParticles = false,
        bool ssaoEnabled = true) =>
        StandardLitPass.Submit(
            World!,
            context,
            Camera,
            SceneTimeSeconds,
            enableParticles: enableParticles,
            ssaoEnabled: ssaoEnabled);

    protected void SubmitSkyScene(IEngineContext context) =>
        StandardLitPass.Submit(
            World!,
            context,
            Camera,
            SceneTimeSeconds,
            verticalFovDegrees: 60f,
            ssaoEnabled: true,
            sunDirection: StandardLitPass.SkyDemoSunDirection,
            sunColor: StandardLitPass.SkyDemoSunColor,
            sunIntensity: StandardLitPass.SkyDemoSunIntensity,
            ambientColor: StandardLitPass.SkyDemoAmbient);
}
