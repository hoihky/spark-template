using Spark.Bindings;

namespace SparkDemoFoundation;

public static class DemoMath
{
    public static SparkQuaternion Identity => new() { w = 1 };
    public const float Pi = MathF.PI;
    public const float TwoPi = MathF.PI * 2f;
    public const float HalfPi = MathF.PI * 0.5f;
    public const float Epsilon = 1.0e-6f;

    public static Vector3 Cross(in Vector3 a, in Vector3 b) =>
        new()
        {
            X = a.Y * b.Z - a.Z * b.Y,
            Y = a.Z * b.X - a.X * b.Z,
            Z = a.X * b.Y - a.Y * b.X,
        };

    public static float Dot(in Vector3 a, in Vector3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

    public static Vector3 Normalized(in Vector3 v)
    {
        float len2 = Dot(v, v);
        if (len2 < Epsilon)
        {
            return v;
        }

        float inv = 1.0f / MathF.Sqrt(len2);
        return new Vector3 { X = v.X * inv, Y = v.Y * inv, Z = v.Z * inv };
    }

    public static SparkQuaternion FromAxisAngleY(float radians)
    {
        float half = radians * 0.5f;
        return new SparkQuaternion
        {
            x = 0,
            y = MathF.Sin(half),
            z = 0,
            w = MathF.Cos(half),
        };
    }

    public static SparkQuaternion FromAxisAngleX(float radians)
    {
        float half = radians * 0.5f;
        return new SparkQuaternion
        {
            x = MathF.Sin(half),
            y = 0,
            z = 0,
            w = MathF.Cos(half),
        };
    }

    public static Matrix4 LookAt(in Vector3 eye, in Vector3 target, in Vector3 up)
    {
        Vector3 f = Normalized(new Vector3
        {
            X = target.X - eye.X,
            Y = target.Y - eye.Y,
            Z = target.Z - eye.Z,
        });
        Vector3 s = Cross(f, up);
        if (Dot(s, s) < Epsilon)
        {
            s = Cross(f, new Vector3 { X = 1, Y = 0, Z = 0 });
        }

        s = Normalized(s);
        Vector3 u = Cross(s, f);
        return new Matrix4
        {
            M00 = s.X,
            M01 = u.X,
            M02 = -f.X,
            M03 = 0,
            M10 = s.Y,
            M11 = u.Y,
            M12 = -f.Y,
            M13 = 0,
            M20 = s.Z,
            M21 = u.Z,
            M22 = -f.Z,
            M23 = 0,
            M30 = -Dot(s, eye),
            M31 = -Dot(u, eye),
            M32 = Dot(f, eye),
            M33 = 1,
        };
    }

    public static float WrapAnglePi(float a)
    {
        while (a > Pi)
        {
            a -= TwoPi;
        }

        while (a < -Pi)
        {
            a += TwoPi;
        }

        return a;
    }
}
