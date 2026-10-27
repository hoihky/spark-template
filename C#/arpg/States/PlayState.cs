using Spark.Bindings;
using Spark.Bindings.Components;
using SparkArpg.Core;
using SparkArpg.Player;
using SparkArpg.World;

namespace SparkArpg.States;

internal sealed class PlayState : IGameState
{
    private readonly List<GameObject> _levelRoots = [];
    private readonly PlayerController _playerController = new();
    private GameObject? _skyRoot;
    private PlayerAvatarResult _player = new();
    private float _fpsSmoothed;
    private int _hudFpsDisplayed = -1;
    private float _hudTextRefreshTimer;

    public void OnEnter(GameContext ctx)
    {
        GameWorld world = ctx.World!;
        world.MountPlatformerUiFont();
        ctx.Engine!.GetInput().SetCursorCaptured(true);

        DungeonBuildResult level = DungeonLevelBuilder.Build(ctx);
        _levelRoots.AddRange(level.Roots);
        _player = PlayerAvatar.Spawn(ctx, _levelRoots, level.PlayerSpawn);
        _player.Rig.RoomBounds = level.RoomBounds;
        _skyRoot = level.SkyRoot;
    }

    public void OnExit(GameContext ctx)
    {
        PlayerAvatar.Destroy(ctx, _player, _levelRoots);
        GameWorld world = ctx.World!;
        foreach (GameObject root in _levelRoots)
        {
            world.DestroyGameObject(root);
        }

        _levelRoots.Clear();
        _skyRoot = null;
        _player = new PlayerAvatarResult();
    }

    public void OnUpdate(GameContext ctx, FrameTiming timing)
    {
        ctx.SceneTimeSeconds = timing.TotalTimeSeconds;
        _playerController.Update(_player, ctx, timing);

        if (_skyRoot is not null)
        {
            TransformComponent skyTr = _skyRoot.GetOrAddTransform();
            Vector3 cam = _player.Rig.CameraWorldPosition();
            skyTr.Translation = cam;
            skyTr.SetUniformScale(72);
        }

        UpdateHud(timing);
        ctx.World!.UpdateGameObjects(timing, ctx.Engine!);
    }

    public void OnRender(GameContext ctx)
    {
        IEngineContext engine = ctx.Engine!;
        engine.GetFramebufferSize(out int fbW, out int fbH);
        float aspect = fbH > 0 ? (float)fbW / fbH : 1f;

        Matrix4 proj = Matrix4.PerspectiveVulkan(70f * MathF.PI / 180f, aspect, 0.1f, 200f);
        Matrix4 view = _player.Rig.ViewMatrix();
        Matrix4 viewProj = global::Spark.Bindings.SparkMath.Mul(proj, view);
        Vector3 camPos = _player.Rig.CameraWorldPosition();

        Vector3 forward = _player.Rig.ForwardWorld();
        Vector3 pr = GameMath.Normalized(GameMath.Cross(new Vector3 { X = 0, Y = 1, Z = 0 }, forward));
        if (GameMath.LengthSquared(pr) < 1.0e-8f)
        {
            pr = GameMath.Normalized(GameMath.Cross(new Vector3 { X = 1, Y = 0, Z = 0 }, forward));
        }

        Vector3 pu = GameMath.Normalized(GameMath.Cross(forward, pr));
        Vector3 sunDir = GameMath.Normalized(new Vector3 { X = 0.35f, Y = 0.82f, Z = 0.28f });

        ctx.World!.SubmitStandardLit(
            engine,
            viewProj,
            camPos,
            sunDir,
            new Vector3 { X = 1.0f, Y = 0.96f, Z = 0.90f },
            1.05f,
            new Vector3 { X = 0.18f, Y = 0.20f, Z = 0.26f },
            false,
            pr,
            pu,
            ctx.SceneTimeSeconds,
            SparkSpriteSortMode.SparkSpriteSortMode_SortOrderOnly);
    }

    private void UpdateHud(FrameTiming timing)
    {
        if (_player.HudText is null)
        {
            return;
        }

        float dt = timing.DeltaTimeSeconds;
        float instant = dt > 1.0e-6f ? 1.0f / dt : 0;
        if (timing.FrameIndex < 2)
        {
            _fpsSmoothed = instant;
        }
        else
        {
            _fpsSmoothed = _fpsSmoothed * 0.88f + instant * 0.12f;
        }

        _hudTextRefreshTimer += dt;
        int fpsDisplay = (int)MathF.Round(_fpsSmoothed);
        if (fpsDisplay == _hudFpsDisplayed && _hudTextRefreshTimer < 0.5f)
        {
            return;
        }

        _hudFpsDisplayed = fpsDisplay;
        _hudTextRefreshTimer = 0;

        string hero = string.IsNullOrEmpty(_player.HeroLabel) ? "hero" : _player.HeroLabel;
        _player.HudText.SetText(
            $"ARPG — {fpsDisplay} FPS — 3rd person · {hero} — WASD · Shift run · F attack · F1 mouse · ESC quit");
    }
}
