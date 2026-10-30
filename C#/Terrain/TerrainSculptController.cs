using Spark.Bindings;
using Spark.Bindings.Components;
using SparkDemoFoundation;

namespace SparkDemoTerrain;

/// <summary>Ray aim + height sculpt (C++ <c>TerrainDemo::Simulate</c> terrain block).</summary>
internal sealed class TerrainSculptController
{
    private readonly FlyCamera _camera;

    public TerrainSculptController(FlyCamera camera) => _camera = camera;

    public void Simulate(
        FrameTiming timing,
        IEngineContext context,
        GameObject terrain,
        TerrainComponent terrainComp,
        TerrainEditCursor cursor)
    {
        IInput input = context.GetInput();
        if (input.IsKeyPressedThisFrame(InputKeys.R))
        {
            terrainComp.ResetHeightsToProcedural(terrain);
        }

        context.GetFramebufferSize(out int fbW, out int fbH);
        if (!TryBuildAimRay(context, fbW, fbH, out Vector3 rayOrigin, out Vector3 rayDirection))
        {
            cursor.HideAt();
            return;
        }

        bool aimHit = terrainComp.TryRaycastWorld(
            terrain,
            rayOrigin,
            rayDirection,
            TerrainCatalog.RaycastMaxDistance,
            out Vector3 hit);

        bool leftDown = input.IsMouseButtonDown(InputKeys.MouseButtonLeft);
        bool rightDown = input.IsMouseButtonDown(InputKeys.MouseButtonRight);

        if (aimHit)
        {
            cursor.UpdateAtHit(hit, leftDown, rightDown);
            if (leftDown || rightDown)
            {
                float strength = TerrainCatalog.BrushStrengthPerSecond * timing.DeltaTimeSeconds;
                float delta = leftDown ? strength : -strength;
                terrainComp.ApplyHeightBrushWorld(terrain, hit, TerrainCatalog.BrushRadiusWorld, delta);
            }
        }
        else
        {
            cursor.HideAt();
        }
    }

    private bool TryBuildAimRay(IEngineContext context, int fbW, int fbH, out Vector3 rayOrigin, out Vector3 rayDirection)
    {
        rayOrigin = default;
        rayDirection = default;
        IInput input = context.GetInput();

        if (input.IsCursorCaptured())
        {
            rayOrigin = _camera.Position;
            rayDirection = _camera.Forward();
            return DemoMath.Dot(rayDirection, rayDirection) > 1.0e-12f;
        }

        input.GetCursorFramebufferPixels(fbW, fbH, out float mx, out float my);
        Matrix4 view = _camera.ViewMatrix();
        float aspect = fbH > 0 ? (float)fbW / fbH : 1f;
        Matrix4 proj = Matrix4.PerspectiveVulkan(DemoMath.Pi * 60f / 180f, aspect, 0.12f, 900f);
        Matrix4 viewProj = global::Spark.Bindings.SparkMath.Mul(proj, view);
        if (!viewProj.TryInvert(out Matrix4 invVp))
        {
            return false;
        }

        return DemoScreenRay.TryTerrainScreenToWorldRay(fbW, fbH, mx, my, invVp, out rayOrigin, out rayDirection);
    }
}
