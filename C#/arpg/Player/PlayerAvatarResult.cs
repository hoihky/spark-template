using Spark.Bindings;
using Spark.Bindings.Components;

namespace SparkArpg.Player;

internal sealed class PlayerAvatarResult
{
    public GameObject? Root { get; set; }
    public CharacterController3DComponent? Controller { get; set; }
    public Character3DAnimFsmComponent? AnimFsm { get; set; }
    public GameObject? HudRoot { get; set; }
    public TextOverlayComponent? HudText { get; set; }
    public CharacterCameraRig Rig { get; } = new();
    public bool UseSkinnedAvatar { get; set; }
    public float BindYawOffset { get; set; }
    public SparkQuaternion BindFix { get; set; } = new() { w = 1 };
    public string HeroLabel { get; set; } = "hero";
}
