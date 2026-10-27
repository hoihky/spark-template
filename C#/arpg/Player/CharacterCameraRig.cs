using Spark.Bindings;
using SparkArpg.Input;
using SparkArpg.World;

namespace SparkArpg.Player;

internal enum CharacterCameraMode
{
    FirstPerson,
    ThirdPerson,
}

/// <summary>Port of <c>Spark::CharacterCameraRig</c> for third-person ARPG camera and movement.</summary>
internal sealed class CharacterCameraRig
{
    public CharacterCameraMode Mode { get; set; } = CharacterCameraMode.ThirdPerson;

    public Vector3 CharacterPosition { get; set; } = new() { X = 0, Y = 0.02f, Z = 2.5f };
    public float CharacterVisualYaw { get; set; }
    public float CharacterFacingYawOffset { get; set; }
    public SparkQuaternion CharacterRootBindOrientation { get; set; } = new() { w = 1 };
    public Vector3 CharacterLocalForward { get; set; } = new() { X = 0, Y = 0, Z = -1 };

    public float CameraYaw { get; set; }
    public float CameraPitch { get; set; } = -0.15f;

    public float ThirdPersonDistance { get; set; } = 4.25f;
    public float ThirdPersonPivotHeight { get; set; } = 1.55f;
    public float ThirdPersonFocusAhead { get; set; } = 0.35f;
    public float ThirdPersonCameraLift { get; set; } = 0.5f;
    public float FirstPersonEyeHeight { get; set; } = 1.62f;
    public float FirstPersonForwardNudge { get; set; } = 0.22f;

    public float MouseSensitivity { get; set; } = 0.12f;
    public float MoveSpeed { get; set; } = 4.5f;
    public float RunSpeed { get; set; } = 8.5f;
    public float GroundY { get; set; } = 0.02f;

    public DungeonRoomBounds? RoomBounds { get; set; }
    public float RoomCameraMargin { get; set; } = 0.35f;
    public float RoomCharacterMargin { get; set; } = 0.5f;

    public void AddLook(float deltaX, float deltaY)
    {
        float dy = deltaX * MouseSensitivity * 0.01f;
        if (Mode == CharacterCameraMode.ThirdPerson)
        {
            CameraYaw = GameMath.WrapAnglePi(CameraYaw + dy);
            CharacterVisualYaw = GameMath.WrapAnglePi(CharacterVisualYaw + dy);
        }
        else
        {
            CameraYaw = GameMath.WrapAnglePi(CameraYaw + dy);
        }

        CameraPitch -= deltaY * MouseSensitivity * 0.01f;
        const float limFp = 1.54f;
        const float limTpMin = -0.48f;
        const float limTpMax = 0.52f;
        CameraPitch = Mode == CharacterCameraMode.FirstPerson
            ? Math.Clamp(CameraPitch, -limFp, limFp)
            : Math.Clamp(CameraPitch, limTpMin, limTpMax);
    }

    public void ProcessWalk(IInput input, float deltaSeconds)
    {
        if (Mode == CharacterCameraMode.ThirdPerson)
        {
            Vector3 flatF = ThirdPersonFlatForward();
            Vector3 move = default;
            if (input.IsKeyDown(InputKeys.W))
            {
                move = new Vector3 { X = move.X + flatF.X, Y = move.Y + flatF.Y, Z = move.Z + flatF.Z };
            }

            if (input.IsKeyDown(InputKeys.S))
            {
                move = new Vector3 { X = move.X - flatF.X, Y = move.Y - flatF.Y, Z = move.Z - flatF.Z };
            }

            if (GameMath.LengthSquared(move) > GameMath.Epsilon)
            {
                bool sprint = input.IsKeyDown(InputKeys.LeftShift) || input.IsKeyDown(InputKeys.RightShift);
                float speed = (sprint ? RunSpeed : MoveSpeed) * deltaSeconds;
                move = GameMath.Normalized(move);
                move = new Vector3 { X = move.X * speed, Y = move.Y * speed, Z = move.Z * speed };
                CharacterPosition = new Vector3
                {
                    X = CharacterPosition.X + move.X,
                    Y = CharacterPosition.Y + move.Y,
                    Z = CharacterPosition.Z + move.Z,
                };
            }

            CharacterPosition = new Vector3 { X = CharacterPosition.X, Y = GroundY, Z = CharacterPosition.Z };
            return;
        }

        float sy = MathF.Sin(CameraYaw);
        float cy = MathF.Cos(CameraYaw);
        Vector3 flatFwd = new() { X = sy, Y = 0, Z = -cy };
        Vector3 flatR = GameMath.Cross(flatFwd, new Vector3 { X = 0, Y = 1, Z = 0 });
        if (GameMath.LengthSquared(flatR) < GameMath.Epsilon)
        {
            flatR = new Vector3 { X = 1, Y = 0, Z = 0 };
        }
        else
        {
            flatR = GameMath.Normalized(flatR);
        }

        Vector3 moveFp = default;
        if (input.IsKeyDown(InputKeys.W))
        {
            moveFp = new Vector3 { X = moveFp.X + flatFwd.X, Y = moveFp.Y + flatFwd.Y, Z = moveFp.Z + flatFwd.Z };
        }

        if (input.IsKeyDown(InputKeys.S))
        {
            moveFp = new Vector3 { X = moveFp.X - flatFwd.X, Y = moveFp.Y - flatFwd.Y, Z = moveFp.Z - flatFwd.Z };
        }

        if (input.IsKeyDown(InputKeys.D))
        {
            moveFp = new Vector3 { X = moveFp.X + flatR.X, Y = moveFp.Y + flatR.Y, Z = moveFp.Z + flatR.Z };
        }

        if (input.IsKeyDown(InputKeys.A))
        {
            moveFp = new Vector3 { X = moveFp.X - flatR.X, Y = moveFp.Y - flatR.Y, Z = moveFp.Z - flatR.Z };
        }

        if (GameMath.LengthSquared(moveFp) > GameMath.Epsilon)
        {
            bool sprint = input.IsKeyDown(InputKeys.LeftShift) || input.IsKeyDown(InputKeys.RightShift);
            float speed = (sprint ? RunSpeed : MoveSpeed) * deltaSeconds;
            moveFp = GameMath.Normalized(moveFp);
            moveFp = new Vector3 { X = moveFp.X * speed, Y = moveFp.Y * speed, Z = moveFp.Z * speed };
            CharacterPosition = new Vector3
            {
                X = CharacterPosition.X + moveFp.X,
                Y = CharacterPosition.Y + moveFp.Y,
                Z = CharacterPosition.Z + moveFp.Z,
            };
            CharacterVisualYaw = MathF.Atan2(moveFp.X, -moveFp.Z);
        }

        CharacterPosition = new Vector3 { X = CharacterPosition.X, Y = GroundY, Z = CharacterPosition.Z };
    }

    public Vector3 ForwardWorld()
    {
        float cyy = MathF.Cos(CameraYaw);
        float syy = MathF.Sin(CameraYaw);
        float cp = MathF.Cos(CameraPitch);
        float sp = MathF.Sin(CameraPitch);
        return GameMath.Normalized(new Vector3 { X = syy * cp, Y = sp, Z = -cyy * cp });
    }

    public Vector3 FirstPersonEyeWorld()
    {
        float sy = MathF.Sin(CameraYaw);
        float cy = MathF.Cos(CameraYaw);
        Vector3 flatFwd = new() { X = sy, Y = 0, Z = -cy };
        if (GameMath.LengthSquared(flatFwd) < GameMath.Epsilon)
        {
            flatFwd = new Vector3 { X = 0, Y = 0, Z = -1 };
        }
        else
        {
            flatFwd = GameMath.Normalized(flatFwd);
        }

        return new Vector3
        {
            X = CharacterPosition.X + flatFwd.X * FirstPersonForwardNudge,
            Y = CharacterPosition.Y + FirstPersonEyeHeight,
            Z = CharacterPosition.Z + flatFwd.Z * FirstPersonForwardNudge,
        };
    }

    public Vector3 CameraWorldPosition()
    {
        if (Mode == CharacterCameraMode.FirstPerson)
        {
            Vector3 eye = FirstPersonEyeWorld();
            return RoomBounds is { } room
                ? room.ConstrainCameraAlongPivot(CharacterPosition, eye, RoomCameraMargin)
                : eye;
        }

        Vector3 pivot = ThirdPersonPivot();
        Vector3 ideal = ThirdPersonIdealCameraPosition(pivot);
        return RoomBounds is { } bounds
            ? bounds.ConstrainCameraAlongPivot(pivot, ideal, RoomCameraMargin)
            : ideal;
    }

    public void ClampCharacterInsideRoom()
    {
        if (RoomBounds is { } room)
        {
            CharacterPosition = room.ClampHorizontal(CharacterPosition, RoomCharacterMargin);
            CharacterPosition = new Vector3 { X = CharacterPosition.X, Y = GroundY, Z = CharacterPosition.Z };
        }
    }

    public Matrix4 ViewMatrix()
    {
        if (Mode == CharacterCameraMode.FirstPerson)
        {
            Vector3 eye = FirstPersonEyeWorld();
            Vector3 f = ForwardWorld();
            Vector3 worldUp = new() { X = 0, Y = 1, Z = 0 };
            if (MathF.Abs(GameMath.Dot(f, worldUp)) > 0.92f)
            {
                worldUp = new Vector3 { X = 0, Y = 0, Z = 1 };
            }

            return GameMath.LookAt(eye, new Vector3 { X = eye.X + f.X, Y = eye.Y + f.Y, Z = eye.Z + f.Z }, worldUp);
        }

        Vector3 pivot = ThirdPersonPivot();
        Vector3 flatF = ThirdPersonFlatForward();
        Vector3 focus = new()
        {
            X = pivot.X + flatF.X * ThirdPersonFocusAhead,
            Y = pivot.Y + flatF.Y * ThirdPersonFocusAhead,
            Z = pivot.Z + flatF.Z * ThirdPersonFocusAhead,
        };
        Vector3 cam = CameraWorldPosition();
        Vector3 worldUpTp = new() { X = 0, Y = 1, Z = 0 };
        Vector3 toFocus = GameMath.Normalized(new Vector3
        {
            X = focus.X - cam.X,
            Y = focus.Y - cam.Y,
            Z = focus.Z - cam.Z,
        });
        if (MathF.Abs(GameMath.Dot(toFocus, worldUpTp)) > 0.96f)
        {
            worldUpTp = new Vector3 { X = 0, Y = 0, Z = 1 };
        }

        return GameMath.LookAt(cam, focus, worldUpTp);
    }

    private Vector3 ThirdPersonPivot() => new()
    {
        X = CharacterPosition.X,
        Y = CharacterPosition.Y + ThirdPersonPivotHeight,
        Z = CharacterPosition.Z,
    };

    private Vector3 ThirdPersonIdealCameraPosition(Vector3 pivot)
    {
        Vector3 flatF = ThirdPersonFlatForward();
        float cp = MathF.Cos(CameraPitch);
        float sp = MathF.Sin(CameraPitch);
        float horiz = cp * ThirdPersonDistance;
        float vert = sp * ThirdPersonDistance;
        return new Vector3
        {
            X = pivot.X - flatF.X * horiz,
            Y = pivot.Y + vert + ThirdPersonCameraLift,
            Z = pivot.Z - flatF.Z * horiz,
        };
    }

    private Vector3 ThirdPersonFlatForward()
    {
        float yaw = GameMath.WrapAnglePi(CharacterVisualYaw + CharacterFacingYawOffset);
        SparkQuaternion qYaw = GameMath.FromAxisAngleY(yaw);
        SparkQuaternion qTotal = GameMath.Normalized(GameMath.Multiply(qYaw, CharacterRootBindOrientation));
        Vector3 f = GameMath.RotateVector(qTotal, CharacterLocalForward);
        float hx = f.X;
        float hz = f.Z;
        float hLen2 = hx * hx + hz * hz;
        if (hLen2 < 1.0e-12f)
        {
            float sy = MathF.Sin(yaw);
            float cy = MathF.Cos(yaw);
            return new Vector3 { X = -sy, Y = 0, Z = cy };
        }

        float inv = 1.0f / MathF.Sqrt(hLen2);
        return new Vector3 { X = -hx * inv, Y = 0, Z = -hz * inv };
    }
}
