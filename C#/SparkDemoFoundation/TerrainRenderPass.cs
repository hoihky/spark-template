using Spark.Bindings;

namespace SparkDemoFoundation;

/// <summary>Outdoor time-of-day submit matching C++ <c>TerrainDemo::Render</c>.</summary>
public static class TerrainRenderPass
{
    public static readonly Vector3 SunDirection = DemoMath.Normalized(new Vector3 { X = 0.38f, Y = 0.82f, Z = 0.35f });
    public static readonly Vector3 SunColor = new() { X = 1.0f, Y = 0.97f, Z = 0.9f };
    public const float SunIntensity = 0.95f;
    public static readonly Vector3 Ambient = new() { X = 0.10f, Y = 0.12f, Z = 0.14f };

    public static void Submit(
        GameWorld world,
        IEngineContext context,
        in FlyCamera camera,
        float sceneTimeSeconds,
        float verticalFovDegrees = 60f,
        float nearZ = 0.12f,
        float farZ = 900f)
    {
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

        world.SubmitTerrainDemo(
            context,
            viewProj,
            camera.Position,
            SunDirection,
            SunColor,
            SunIntensity,
            Ambient,
            enableParticles: false,
            pr,
            pu,
            sceneTimeSeconds,
            ssaoEnabled: true);
    }
}
