using Spark.Bindings;
using SparkArpg.Interop;

namespace SparkArpg.Services;

internal sealed class AssetRegistry
{
    private readonly HashSet<string> _loadedGltf = new(StringComparer.Ordinal);

    public void PrimeSharedTextures(GameWorld world, AssetPaths paths)
    {
        string atlasPath = Path.GetFullPath(paths.ModelPath("dungeon_texture.png"));
        if (File.Exists(atlasPath))
        {
            world.EnsureTextureLoaded(atlasPath);
            ArpgSceneApi.RegisterTextureFromFile(world, atlasPath, ArpgSceneApi.DungeonAtlasKey);
        }
    }

    public bool EnsurePropMesh(GameWorld world, string modelPath)
    {
        string full = Path.GetFullPath(modelPath);
        if (_loadedGltf.Contains(full))
        {
            return true;
        }

        if (!world.EnsureGltfLoaded(full))
        {
            return false;
        }

        _loadedGltf.Add(full);
        return true;
    }
}
