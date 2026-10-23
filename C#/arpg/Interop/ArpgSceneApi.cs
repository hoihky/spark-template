using Spark.Bindings;

namespace SparkArpg.Interop;

internal static class ArpgSceneApi
{
    public const string SkyMeshKey = "arpg/sky_sphere";
    public const string SkyTextureKey = "arpg/sky_equirect";
    public const string DungeonAtlasKey = "arpg/dungeon_atlas";

    public static bool RegisterSkySphereMesh(GameWorld world, float radius = 1f, int lat = 18, int lon = 36)
    {
        unsafe
        {
            return Utf8Path.WithUtf8(SkyMeshKey, keyPtr =>
                Native.spark_world_register_sky_sphere_mesh(
                    ArpgNative.World(world),
                    (sbyte*)keyPtr,
                    radius,
                    lat,
                    lon) != 0);
        }
    }

    public static bool RegisterTextureFromFile(GameWorld world, string filePath, string cacheKey)
    {
        unsafe
        {
            byte[] pathBytes = System.Text.Encoding.UTF8.GetBytes(filePath + "\0");
            byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(cacheKey + "\0");
            fixed (byte* pathPtr = pathBytes)
            fixed (byte* keyPtr = keyBytes)
            {
                return Native.spark_world_register_texture_from_file(
                           ArpgNative.World(world),
                           (sbyte*)pathPtr,
                           (sbyte*)keyPtr)
                       != 0;
            }
        }
    }

    public static bool ApplyGltfPropMaterial(
        GameObject obj,
        GameWorld world,
        string gltfPath,
        string? overrideTextureKeyOrPath)
    {
        unsafe
        {
            byte[] gltfBytes = System.Text.Encoding.UTF8.GetBytes(gltfPath + "\0");
            fixed (byte* gltfPtr = gltfBytes)
            {
                if (string.IsNullOrEmpty(overrideTextureKeyOrPath))
                {
                    return Native.spark_object_apply_gltf_prop_material(
                               ArpgNative.Object(obj),
                               ArpgNative.World(world),
                               (sbyte*)gltfPtr,
                               null)
                           != 0;
                }

                byte[] texBytes = System.Text.Encoding.UTF8.GetBytes(overrideTextureKeyOrPath + "\0");
                fixed (byte* texPtr = texBytes)
                {
                    return Native.spark_object_apply_gltf_prop_material(
                               ArpgNative.Object(obj),
                               ArpgNative.World(world),
                               (sbyte*)gltfPtr,
                               (sbyte*)texPtr)
                           != 0;
                }
            }
        }
    }

    public static bool TryMeasureSkinnedAvatar(
        GameWorld world,
        string gltfPath,
        float targetHeightMeters,
        out float uniformScale,
        out SparkQuaternion bindFix,
        out float facingYawOffset,
        out float fittedHeightMeters)
    {
        uniformScale = 0.04f;
        bindFix = new SparkQuaternion { w = 1 };
        facingYawOffset = 0;
        fittedHeightMeters = targetHeightMeters;

        unsafe
        {
            float scale = 0;
            float yaw = 0;
            float fitted = 0;
            SparkQuaternion bind = default;
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(gltfPath + "\0");
            fixed (byte* ptr = bytes)
            {
                int ok = Native.spark_skinned_gltf_measure_avatar_layout(
                    ArpgNative.World(world),
                    (sbyte*)ptr,
                    targetHeightMeters,
                    &scale,
                    &bind,
                    &yaw,
                    &fitted);
                if (ok == 0)
                {
                    return false;
                }

                uniformScale = scale;
                bindFix = bind;
                facingYawOffset = yaw;
                fittedHeightMeters = fitted;
                return true;
            }
        }
    }
}
