using Spark.Bindings;

namespace SparkDemoTerrain;

/// <summary>Constants mirrored from <c>TerrainDemo.cpp</c>.</summary>
internal static class TerrainCatalog
{
    public const float BrushRadiusWorld = 10.0f;
    public const float BrushStrengthPerSecond = 48.0f;
    public const float RaycastMaxDistance = 750.0f;
    public const float CursorHeightOffset = 0.18f;
    public const float HiddenCursorY = -2000.0f;

    public static readonly Vector3 DefaultMeshAlbedo = new() { X = 0.42f, Y = 0.55f, Z = 0.36f };

    public static TerrainGeneratorSettings ProceduralSettings => new()
    {
        SubdivX = 288,
        SubdivZ = 288,
        HalfExtentX = 220.0f,
        HalfExtentZ = 220.0f,
        HeightScale = 26.0f,
        NoiseScale = 0.0135f,
        Octaves = 6,
        Persistence = 0.5f,
        Lacunarity = 2.05f,
        Seed = 0xC047ACEEu,
        WorldUnitsPerTextureRepeat = 440.0f,
    };

    public static readonly Vector3 InitialCameraPosition = new() { X = 0, Y = 62, Z = 228 };
    public static readonly Vector3 InitialLookTarget = new() { X = 0, Y = 14, Z = 0 };
}
