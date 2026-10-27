using Spark.Bindings;
using Spark.Bindings.Components;
using SparkArpg.Core;
using SparkArpg.Input;

namespace SparkArpg.Player;

internal sealed class PlayerController
{
    public void Update(PlayerAvatarResult player, GameContext ctx, FrameTiming timing)
    {
        IEngineContext engine = ctx.Engine!;
        IInput input = engine.GetInput();
        CharacterCameraRig rig = player.Rig;

        if (input.IsKeyPressedThisFrame(InputKeys.Escape))
        {
            engine.RequestWindowClose();
        }

        if (input.IsKeyPressedThisFrame(InputKeys.F1))
        {
            input.SetCursorCaptured(!input.IsCursorCaptured());
        }

        if (input.IsCursorCaptured() && timing.FrameIndex > 0)
        {
            rig.AddLook(input.GetMouseDeltaX(), input.GetMouseDeltaY());
        }

        Vector3 posBeforeWalk = rig.CharacterPosition;
        rig.ProcessWalk(input, timing.DeltaTimeSeconds);
        Vector3 wishVel = default;
        if (timing.DeltaTimeSeconds > 1.0e-6f)
        {
            wishVel = new Vector3
            {
                X = (rig.CharacterPosition.X - posBeforeWalk.X) / timing.DeltaTimeSeconds,
                Y = 0,
                Z = (rig.CharacterPosition.Z - posBeforeWalk.Z) / timing.DeltaTimeSeconds,
            };
        }

        rig.CharacterPosition = posBeforeWalk;

        TransformComponent? rootTr = player.Root?.GetOrAddTransform();
        GameWorld? world = ctx.World;

        if (player.Controller is not null && rootTr is not null && world is not null)
        {
            rootTr.Translation = rig.CharacterPosition;
            player.Controller.SetMoveInput(wishVel);
            world.SimulateCharacterControllers3D(
                timing,
                new CharacterController3DSettings
                {
                    GravityY = 0f,
                    MaxFallSpeed = 0f,
                    BroadPhaseCellSize = 4f,
                });
            Vector3 resolved = rootTr.Translation;
            rig.CharacterPosition = new Vector3 { X = resolved.X, Y = rig.GroundY, Z = resolved.Z };
        }
        else if (rootTr is not null && timing.DeltaTimeSeconds > 1.0e-6f)
        {
            rig.CharacterPosition = new Vector3
            {
                X = posBeforeWalk.X + wishVel.X * timing.DeltaTimeSeconds,
                Y = rig.GroundY,
                Z = posBeforeWalk.Z + wishVel.Z * timing.DeltaTimeSeconds,
            };
        }

        bool moving = input.IsKeyDown(InputKeys.W) || input.IsKeyDown(InputKeys.S)
            || input.IsKeyDown(InputKeys.A) || input.IsKeyDown(InputKeys.D);
        bool sprint = moving && (input.IsKeyDown(InputKeys.LeftShift) || input.IsKeyDown(InputKeys.RightShift));

        if (player.UseSkinnedAvatar && player.AnimFsm is not null)
        {
            if (moving)
            {
                player.AnimFsm.ClearManualClip();
            }

            float speed = MathF.Sqrt(wishVel.X * wishVel.X + wishVel.Z * wishVel.Z);
            player.AnimFsm.SetLocomotionAnalogSpeed(speed);
            player.AnimFsm.SetLocomotionInput(moving ? 1 : 0, sprint ? 1 : 0);
        }

        if (input.IsKeyPressedThisFrame(InputKeys.F) && player.AnimFsm is not null)
        {
            player.AnimFsm.RequestAttack();
        }

        rig.ClampCharacterInsideRoom();

        if (rootTr is not null)
        {
            rootTr.Translation = rig.CharacterPosition;
            SparkQuaternion qYaw = GameMath.FromAxisAngleY(rig.CharacterVisualYaw + player.BindYawOffset);
            SparkQuaternion rot = player.UseSkinnedAvatar
                ? GameMath.Normalized(GameMath.Multiply(qYaw, player.BindFix))
                : qYaw;
            rootTr.SetRotation(rot);
        }
    }
}
