using Spark.Bindings;

namespace SparkArpg.Services;

internal sealed class AssetRegistry
{
    private readonly HashSet<string> _loadedGltf = new(StringComparer.Ordinal);

    public void PrimeSharedTextures(GameWorld world, AssetPaths paths)
    {
        string atlasPath = Path.GetFullPath(paths.ModelPath("dungeon_texture.png"));
        if (File.Exists(atlasPath))
        {
            world.LoadTexture(atlasPath);
            world.RegisterTextureFromFile(atlasPath, ArpgAssetKeys.DungeonAtlasKey);
        }
    }

    public bool EnsurePropMesh(GameWorld world, string modelPath)
    {
        string full = Path.GetFullPath(modelPath);
        if (_loadedGltf.Contains(full))
        {
            return true;
        }

        if (!world.LoadGltf(full))
        {
            return false;
        }

        _loadedGltf.Add(full);
        return true;
    }
}
