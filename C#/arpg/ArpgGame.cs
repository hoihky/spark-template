using Spark.Bindings;
using Spark.Scripting;
using SparkArpg.Core;
using SparkArpg.States;

namespace SparkArpg;

public sealed class ArpgGame : SparkGame
{
    private readonly GameContext _context = new();
    private readonly GameStateMachine _stateMachine = new();

    public override void OnAttach(IEngineContext context)
    {
        base.OnAttach(context);
        _context.World = World;
        _context.Engine = context;
        _stateMachine.ChangeState(new PlayState(), _context);
    }

    public override void OnDetach()
    {
        _stateMachine.ChangeState(null, _context);
        _context.World = null;
        _context.Engine = null;
        base.OnDetach();
    }

    public override void OnUpdate(FrameTiming timing, IEngineContext context)
    {
        _stateMachine.Update(_context, timing);
        World?.ProcessSoundCues(context);
    }

    public override void OnRender(IRenderFrame frame, IEngineContext context)
    {
        _stateMachine.Render(_context);
    }
}
