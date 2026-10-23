using System.Text;
using Spark.Bindings;

namespace SparkArpg.Interop;

internal static class GameObjectExtensions
{
    public static void AddBoxCollider3D(
        this GameObject obj,
        in Vector3 halfExtents,
        in Vector3 offset = default)
    {
        unsafe
        {
            var he = halfExtents.ToNative();
            var off = offset.ToNative();
            Native.spark_object_add_box_collider_3d(ArpgNative.Object(obj), &he, &off);
        }
    }

    public static void AddMaterialLit(this GameObject obj, string? textureKeyOrPath, in Vector3 tint)
    {
        unsafe
        {
            var c = tint.ToNative();
            if (textureKeyOrPath is null)
            {
                Native.spark_object_add_material_lit(ArpgNative.Object(obj), null, &c);
                return;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(textureKeyOrPath + "\0");
            fixed (byte* ptr = bytes)
            {
                Native.spark_object_add_material_lit(ArpgNative.Object(obj), (sbyte*)ptr, &c);
            }
        }
    }

    public static void AddSky(this GameObject obj, SparkSceneSkyMode mode)
    {
        unsafe
        {
            Native.spark_object_add_sky(ArpgNative.Object(obj), mode);
        }
    }

    public static bool TryAddSkinnedCharacterFromGltf(this GameObject obj, GameWorld world, string gltfPath)
    {
        unsafe
        {
            SparkGameComponent* skinned = null;
            SparkGameComponent* animator = null;
            SparkGameComponent* material = null;
            byte[] bytes = Encoding.UTF8.GetBytes(gltfPath + "\0");
            fixed (byte* ptr = bytes)
            {
                return Native.spark_object_add_skinned_character_from_gltf(
                           ArpgNative.Object(obj),
                           ArpgNative.World(world),
                           (sbyte*)ptr,
                           &skinned,
                           &animator,
                           &material)
                       != 0;
            }
        }
    }
}
