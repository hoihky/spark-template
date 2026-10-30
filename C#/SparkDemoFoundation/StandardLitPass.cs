using Spark.Bindings;

namespace SparkDemoFoundation;

/// <summary>Common 3D demo lighting + <see cref="GameWorld.SubmitStandardLit"/> setup.</summary>
public static class StandardLitPass
{
    public static readonly Vector3 ThreeDDemoSunDirection =
        DemoMath.Normalized(new Vector3 { X = 0.35f, Y = 0.88f, Z = 0.32f });
    public static readonly Vector3 ThreeDDemoSunColor = new() { X = 1.0f, Y = 0.95f, Z = 0.88f };
    public const float ThreeDDemoSunIntensity = 0.95f;
    public static readonly Vector3 ThreeDDemoAmbient = new() { X = 0.10f, Y = 0.11f, Z = 0.14f };

    public static readonly Vector3 SkyDemoSunDirection =
        DemoMath.Normalized(new Vector3 { X = 0.42f, Y = 0.82f, Z = 0.28f });
    public static readonly Vector3 SkyDemoSunColor = new() { X = 1.0f, Y = 0.97f, Z = 0.90f };
    public const float SkyDemoSunIntensity = 1.0f;
    public static readonly Vector3 SkyDemoAmbient = new() { X = 0.13f, Y = 0.14f, Z = 0.17f };

    public static readonly Vector3 DefaultSunDirection = ThreeDDemoSunDirection;
    public static readonly Vector3 DefaultSunColor = ThreeDDemoSunColor;
    public const float DefaultSunIntensity = ThreeDDemoSunIntensity;
    public static readonly Vector3 DefaultAmbient = ThreeDDemoAmbient;

    public static void Submit(
        GameWorld world,
        IEngineContext context,
        in FlyCamera camera,
        float sceneTimeSeconds,
        float verticalFovDegrees = 60f,
        float nearZ = 0.12f,
        float farZ = 400f,
        bool enableParticles = false,
        bool ssaoEnabled = false,
        Vector3? sunDirection = null,
        Vector3? sunColor = null,
        float? sunIntensity = null,
        Vector3? ambientColor = null)
    {
        Vector3 lightDir = sunDirection ?? DefaultSunDirection;
        Vector3 lightCol = sunColor ?? DefaultSunColor;
        float lightIntensity = sunIntensity ?? DefaultSunIntensity;
        Vector3 ambient = ambientColor ?? DefaultAmbient;

        context.GetFramebufferSize(out int fbW, out int fbH);
        float aspect = fbH > 0 ? (float)fbW / fbH : 1f;
        Matrix4 proj = Matrix4.PerspectiveVulkan(verticalFovDegrees * DemoMath.Pi / 180f, aspect, nearZ, farZ);
        Matrix4 view = camera.ViewMatrix();
        Matrix4 viewProj = global::Spark.Bindings.SparkMath.Mul(proj, view);

        Vector3 forward = camera.Forward();
        Vector3 pr = DemoMath.Normalized(DemoMath.Cross(new Vector3 { X = 0, Y = 1, Z = 0 }, forward));
        if (DemoMath.Dot(pr, pr) < 1.0e-8f)
        {
            pr = DemoMath.Normalized(DemoMath.Cross(new Vector3 { X = 1, Y = 0, Z = 0 }, forward));
        }

        Vector3 pu = DemoMath.Normalized(DemoMath.Cross(forward, pr));

        world.SubmitStandardLit(
            context,
            viewProj,
            camera.Position,
            lightDir,
            lightCol,
            lightIntensity,
            ambient,
            enableParticles,
            pr,
            pu,
            sceneTimeSeconds,
            SparkSpriteSortMode.SparkSpriteSortMode_SortOrderOnly,
            ssaoEnabled);
    }
}
