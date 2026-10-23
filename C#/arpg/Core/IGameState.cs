using Spark.Bindings;

namespace SparkArpg.Core;

internal interface IGameState
{
    void OnEnter(GameContext ctx);
    void OnExit(GameContext ctx);
    void OnUpdate(GameContext ctx, FrameTiming timing);
    void OnRender(GameContext ctx);
}
