using Spark.Bindings;

namespace SparkArpg.Interop;

internal static class GameWorldExtensions
{
    public static bool EnsureGltfLoaded(this GameWorld world, string path)
    {
        unsafe
        {
            return Utf8Path.WithUtf8(path, ptr =>
                Native.spark_world_load_gltf(ArpgNative.World(world), (sbyte*)ptr) != 0);
        }
    }

    public static bool EnsureSkinnedGltfLoaded(this GameWorld world, string path)
    {
        unsafe
        {
            return Utf8Path.WithUtf8(path, ptr =>
                Native.spark_world_load_skinned_gltf(ArpgNative.World(world), (sbyte*)ptr) != 0);
        }
    }

    public static bool EnsureTextureLoaded(this GameWorld world, string path)
    {
        unsafe
        {
            return Utf8Path.WithUtf8(path, ptr =>
                Native.spark_world_load_texture(ArpgNative.World(world), (sbyte*)ptr) != 0);
        }
    }

    public static void SimulateCharacterControllers3D(
        this GameWorld world,
        FrameTiming timing,
        float gravityY = 0f,
        float maxFallSpeed = 0f,
        float broadPhaseCellSize = 4f)
    {
        unsafe
        {
            var nativeTiming = new SparkFrameTiming
            {
                deltaTimeSeconds = timing.DeltaTimeSeconds,
                totalTimeSeconds = timing.TotalTimeSeconds,
                frameIndex = timing.FrameIndex,
            };
            var settings = new SparkCharacterController3DSettings
            {
                gravityY = gravityY,
                maxFallSpeed = maxFallSpeed,
                slideIterations = 3,
                sweepBinaryIterations = 8,
                broadPhaseCellSize = broadPhaseCellSize,
            };
            Native.spark_world_simulate_character_controllers_3d(
                ArpgNative.World(world),
                &nativeTiming,
                &settings);
        }
    }
}
