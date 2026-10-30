using Spark.Bindings;

namespace SparkDemoFoundation;

/// <summary>Port of <c>Spark::FlyCamera</c> — WASD + mouse look, Space/Shift vertical.</summary>
public sealed class FlyCamera
{
    public Vector3 Position { get; set; } = new() { X = 0, Y = 4.2f, Z = 16 };
    public float Yaw { get; set; }
    public float Pitch { get; set; }
    public float MoveSpeed { get; set; } = 5f;
    public float MouseSensitivity { get; set; } = 0.12f;

    public void AddLook(float deltaX, float deltaY)
    {
        Yaw += deltaX * MouseSensitivity * 0.01f;
        Pitch -= deltaY * MouseSensitivity * 0.01f;
        const float lim = 1.54f;
        Pitch = Math.Clamp(Pitch, -lim, lim);
    }

    public void ProcessMovement(IInput input, float deltaSeconds)
    {
        Vector3 forward = Forward();
        Vector3 right = DemoMath.Cross(forward, new Vector3 { X = 0, Y = 1, Z = 0 });
        if (DemoMath.Dot(right, right) < DemoMath.Epsilon)
        {
            right = new Vector3 { X = 1, Y = 0, Z = 0 };
        }
        else
        {
            right = DemoMath.Normalized(right);
        }

        float s = MoveSpeed * deltaSeconds;
        if (input.IsKeyDown(InputKeys.W))
        {
            Position = new Vector3 { X = Position.X + forward.X * s, Y = Position.Y + forward.Y * s, Z = Position.Z + forward.Z * s };
        }

        if (input.IsKeyDown(InputKeys.S))
        {
            Position = new Vector3 { X = Position.X - forward.X * s, Y = Position.Y - forward.Y * s, Z = Position.Z - forward.Z * s };
        }

        if (input.IsKeyDown(InputKeys.A))
        {
            Position = new Vector3 { X = Position.X - right.X * s, Y = Position.Y - right.Y * s, Z = Position.Z - right.Z * s };
        }

        if (input.IsKeyDown(InputKeys.D))
        {
            Position = new Vector3 { X = Position.X + right.X * s, Y = Position.Y + right.Y * s, Z = Position.Z + right.Z * s };
        }

        if (input.IsKeyDown(InputKeys.Space))
        {
            Position = new Vector3 { X = Position.X, Y = Position.Y + s, Z = Position.Z };
        }

        if (input.IsKeyDown(InputKeys.LeftShift) || input.IsKeyDown(InputKeys.RightShift))
        {
            Position = new Vector3 { X = Position.X, Y = Position.Y - s, Z = Position.Z };
        }
    }

    public Vector3 Forward()
    {
        float cy = MathF.Cos(Yaw);
        float sy = MathF.Sin(Yaw);
        float cp = MathF.Cos(Pitch);
        float sp = MathF.Sin(Pitch);
        return DemoMath.Normalized(new Vector3 { X = sy * cp, Y = sp, Z = -cy * cp });
    }

    /// <summary>Align yaw/pitch so <see cref="Forward"/> points at <paramref name="target"/> (C++ <c>FlyCamera::SnapLookAt</c>).</summary>
    public void SnapLookAt(Vector3 target)
    {
        Vector3 d = new()
        {
            X = target.X - Position.X,
            Y = target.Y - Position.Y,
            Z = target.Z - Position.Z,
        };
        if (DemoMath.Dot(d, d) < DemoMath.Epsilon)
        {
            return;
        }

        d = DemoMath.Normalized(d);
        Pitch = Math.Clamp(MathF.Asin(Math.Clamp(d.Y, -1f, 1f)), -1.54f, 1.54f);
        Yaw = MathF.Atan2(d.X, -d.Z);
    }

    public Matrix4 ViewMatrix() =>
        DemoMath.LookAt(Position, new Vector3 { X = Position.X + Forward().X, Y = Position.Y + Forward().Y, Z = Position.Z + Forward().Z }, new Vector3 { X = 0, Y = 1, Z = 0 });
}
