using Spark.Bindings;

namespace SparkArpg.Core;

internal sealed class GameStateMachine
{
    private IGameState? _current;

    public void ChangeState(IGameState? next, GameContext ctx)
    {
        _current?.OnExit(ctx);
        _current = next;
        _current?.OnEnter(ctx);
    }

    public void Update(GameContext ctx, FrameTiming timing) => _current?.OnUpdate(ctx, timing);

    public void Render(GameContext ctx) => _current?.OnRender(ctx);
}
