using Spark.Bindings;
using Spark.Bindings.Components;
using Spark.Scripting;

namespace SparkDemoFoundation;

/// <summary>Minimal FPS overlay (mirrors demo help HUD pattern).</summary>
public sealed class DemoHelpHud
{
    private TextOverlayComponent? _text;
    private float _fpsSmoothed;
    private int _displayedFps = -1;
    private string _controlHints = "F1 mouse · ESC quit";

    public void Attach(GameWorld world, DemoRootCollection roots, string titleLine, string? controlHints = null)
    {
        if (controlHints is not null)
        {
            _controlHints = controlHints;
        }

        GameObject hud = world.CreateGameObject("DemoHud");
        roots.Track(hud);
        _text = hud.AddTextOverlay();
        _text.SetScreenPosition(12, 12);
        _text.FontSizePixels = 18;
        _text.Color = new Vector3 { X = 0.92f, Y = 0.94f, Z = 1.0f };
        _text.SetText($"{titleLine} — {_controlHints}");
    }

    public void Update(FrameTiming timing, string titleLine)
    {
        if (_text is null)
        {
            return;
        }

        float dt = timing.DeltaTimeSeconds;
        float instant = dt > 1.0e-6f ? 1.0f / dt : 0;
        _fpsSmoothed = timing.FrameIndex < 2 ? instant : _fpsSmoothed * 0.88f + instant * 0.12f;
        int fps = (int)MathF.Round(_fpsSmoothed);
        if (fps == _displayedFps)
        {
            return;
        }

        _displayedFps = fps;
        _text.SetText($"{titleLine} — {fps} FPS — {_controlHints}");
    }
}
