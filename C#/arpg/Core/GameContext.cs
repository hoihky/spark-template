using Spark.Bindings;
using SparkArpg.Services;

namespace SparkArpg.Core;

internal sealed class GameContext
{
    public GameWorld? World { get; set; }
    public IEngineContext? Engine { get; set; }
    public float SceneTimeSeconds { get; set; }
    public AssetPaths Paths { get; } = new();
    public AssetRegistry Assets { get; } = new();
}
