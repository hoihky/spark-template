using Spark.Bindings;
using Spark.Bindings.Components;
using SparkArpg.Core;
using SparkArpg.Interop;

namespace SparkArpg.Player;

internal static class PlayerAvatar
{
    private const float TargetHeightM = 1.75f;

    private static void TuneThirdPersonRig(CharacterCameraRig rig, float heightM)
    {
        rig.ThirdPersonPivotHeight = Math.Clamp(heightM * 0.88f, 1.32f, 1.9f);
        rig.FirstPersonEyeHeight = Math.Clamp(heightM * 0.88f, 1.4f, 1.95f);
        rig.FirstPersonForwardNudge = Math.Clamp(heightM * 0.08f, 0.14f, 0.42f);
        rig.ThirdPersonFocusAhead = Math.Clamp(heightM * 0.05f, 0.12f, 0.38f);
        rig.ThirdPersonCameraLift = Math.Clamp(heightM * 0.12f, 0.35f, 0.68f);
        rig.ThirdPersonDistance = Math.Clamp(heightM * 2.4f, 4.0f, 6.5f);
    }

    public static PlayerAvatarResult Spawn(GameContext ctx, List<GameObject> ownedRoots, Vector3 spawnPosition)
    {
        var result = new PlayerAvatarResult();
        GameWorld world = ctx.World!;

        result.Rig.CharacterPosition = spawnPosition;
        result.Rig.GroundY = spawnPosition.Y;
        result.Rig.MoveSpeed = 4.8f;
        result.Rig.RunSpeed = 8.5f;

        string foxPath = Path.GetFullPath(ctx.Paths.FoxModelPath());
        string avatarPath = foxPath;
        bool usedFox = File.Exists(foxPath);
        if (!usedFox)
        {
            avatarPath = Path.GetFullPath(ctx.Paths.SparkEngineModelPath("CesiumMan.glb"));
        }

        world.EnsureSkinnedGltfLoaded(avatarPath);
        float uniformScale = 0.04f;
        SparkQuaternion bindFix = new() { w = 1 };
        float bindYaw = 0;
        float fittedHeight = TargetHeightM;
        if (ArpgSceneApi.TryMeasureSkinnedAvatar(world, avatarPath, TargetHeightM, out uniformScale, out bindFix, out bindYaw, out fittedHeight))
        {
            TuneThirdPersonRig(result.Rig, fittedHeight);
        }
        else
        {
            TuneThirdPersonRig(result.Rig, TargetHeightM);
        }

        if (usedFox)
        {
            bindYaw = -ArpgMath.HalfPi;
        }

        GameObject root = world.CreateGameObject("Player");
        root.GetOrAddTransform().Translation = spawnPosition;

        root.TryAddSkinnedCharacterFromGltf(world, avatarPath);
        root.GetOrAddTransform().SetUniformScale(uniformScale);

        // C++ keeps the capsule on an unscaled root and scales only the visual child. With skinned mesh on the
        // same object, uniform scale shrinks the physics sphere in world space unless we compensate here.
        CharacterController3DComponent? controller = root.GetOrAddCharacterController3D();
        if (controller is not null)
        {
            float invScale = uniformScale > 1.0e-6f ? 1.0f / uniformScale : 1.0f;
            const float worldRadius = 0.42f;
            const float worldCenterY = 0.38f;
            const float worldStep = 0.35f;
            const float worldSkin = 0.02f;
            controller.Radius = worldRadius * invScale;
            controller.CenterOffset = new Vector3 { X = 0, Y = worldCenterY * invScale, Z = 0 };
            controller.StepOffset = worldStep * invScale;
            controller.SkinWidth = worldSkin * invScale;
            controller.GravityScale = 0;
        }

        result.BindFix = bindFix;
        result.BindYawOffset = bindYaw;
        result.Rig.CharacterFacingYawOffset = bindYaw;
        result.Rig.CharacterRootBindOrientation = bindFix;

        MaterialComponent? mat = root.GetMaterial();
        if (mat is not null)
        {
            mat.Metallic = 0.05f;
            mat.Roughness = 0.65f;
        }

        Character3DAnimFsmComponent? fsm = root.GetCharacter3DAnimFsm();
        AnimatorComponent? animator = root.GetAnimator();
        if (fsm is not null && animator is not null)
        {
            result.UseSkinnedAvatar = animator.GetClipCount() > 0;
            fsm.ConfigureLocomotionFromSkeleton(animator, 0);
            fsm.SetLocomotionDrivingEnabled(1);
            fsm.WalkSpeedThreshold = 0.35f;
            fsm.RunSpeedThreshold = 6.0f;
        }

        GameObject hud = world.CreateGameObject("Hud");
        TextOverlayComponent hudText = hud.AddTextOverlay();
        hudText.SetScreenPosition(14, 14);
        hudText.FontSizePixels = 20;
        hudText.Color = new Vector3 { X = 0.92f, Y = 0.94f, Z = 1.0f };
        hudText.SetText("ARPG — WASD · Shift run · F attack · F1 mouse · ESC quit");
        ownedRoots.Add(hud);

        result.HudRoot = hud;
        result.HudText = hudText;
        result.HeroLabel = usedFox ? "Fox" : "CesiumMan";
        result.Root = root;
        result.Controller = controller;
        result.AnimFsm = fsm;
        ownedRoots.Add(root);

        return result;
    }

    public static void Destroy(GameContext ctx, PlayerAvatarResult avatar, List<GameObject> ownedRoots)
    {
        GameWorld world = ctx.World!;
        if (avatar.HudRoot is not null)
        {
            world.DestroyGameObject(avatar.HudRoot);
            ownedRoots.Remove(avatar.HudRoot);
        }

        if (avatar.Root is not null)
        {
            world.DestroyGameObject(avatar.Root);
            ownedRoots.Remove(avatar.Root);
        }

        avatar.Root = null;
        avatar.Controller = null;
        avatar.AnimFsm = null;
        avatar.HudRoot = null;
        avatar.HudText = null;
    }
}
