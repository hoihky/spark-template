using System.Reflection;

namespace SparkArpg.Services;

internal sealed class AssetPaths
{
    private const string FoxModelFile = "Fox.glb";

    public string GameAssetsRoot { get; }

    public AssetPaths()
    {
        GameAssetsRoot = ResolveGameAssetsRoot();
    }

    public string ModelPath(string fileName) => Path.Combine(GameAssetsRoot, "models", fileName);

    public string SparkEngineModelPath(string fileName)
    {
        string? engineRoot = Environment.GetEnvironmentVariable("SPARK_ENGINE_ROOT");
        if (!string.IsNullOrEmpty(engineRoot))
        {
            string enginePath = Path.Combine(engineRoot, "assets", "models", fileName);
            if (File.Exists(enginePath))
            {
                return enginePath;
            }
        }

        string cwdPath = Path.Combine("assets", "models", fileName);
        if (File.Exists(cwdPath))
        {
            return cwdPath;
        }

        string[] fallbacks =
        [
            Path.Combine("..", "spark", "assets", "models", fileName),
            Path.Combine("..", "..", "spark", "assets", "models", fileName),
        ];
        foreach (string fb in fallbacks)
        {
            if (File.Exists(fb))
            {
                return fb;
            }
        }

        return cwdPath;
    }

    public string FoxModelPath() => SparkEngineModelPath(FoxModelFile);

    public string SkyTexturePath()
    {
        string? engineRoot = Environment.GetEnvironmentVariable("SPARK_ENGINE_ROOT");
        if (!string.IsNullOrEmpty(engineRoot))
        {
            string p = Path.Combine(engineRoot, "assets", "textures", "sky", "equirect_sky_1k.hdr");
            if (File.Exists(p))
            {
                return p;
            }
        }

        string rel = Path.Combine("assets", "textures", "sky", "equirect_sky_1k.hdr");
        return File.Exists(rel) ? rel : rel;
    }

    private static string ResolveGameAssetsRoot()
    {
        string? env = Environment.GetEnvironmentVariable("SPARK_ARPG_ASSETS");
        if (!string.IsNullOrEmpty(env) && Directory.Exists(env))
        {
            return env;
        }

        string? asmDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (!string.IsNullOrEmpty(asmDir))
        {
            string near = Path.Combine(asmDir, "assets");
            if (HasCompleteDungeonMeshes(near))
            {
                return near;
            }
        }

        if (HasCompleteDungeonMeshes("assets"))
        {
            return "assets";
        }

        return asmDir is null ? "assets" : Path.Combine(asmDir, "assets");
    }

    private static bool HasCompleteDungeonMeshes(string assetsRoot)
    {
        string gltf = Path.Combine(assetsRoot, "models", "floor_tile_large.gltf");
        string bin = Path.Combine(assetsRoot, "models", "floor_tile_large.bin");
        return File.Exists(gltf) && File.Exists(bin);
    }
}
