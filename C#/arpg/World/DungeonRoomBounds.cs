using Spark.Bindings;

namespace SparkArpg.World;

/// <summary>Axis-aligned interior of the Kenney dungeon room (inside perimeter wall colliders).</summary>
internal readonly struct DungeonRoomBounds
{
    public float MinX { get; init; }
    public float MaxX { get; init; }
    public float MinZ { get; init; }
    public float MaxZ { get; init; }
    public float MinY { get; init; }
    public float MaxY { get; init; }

    public static DungeonRoomBounds ForGrid(int gridWidth, int gridDepth, float cellSize, float groundY)
    {
        float originX = -gridWidth * cellSize * 0.5f + cellSize * 0.5f;
        float originZ = -gridDepth * cellSize * 0.5f + cellSize * 0.5f;
        float minFloorX = originX;
        float maxFloorX = originX + (gridWidth - 1) * cellSize;
        float minFloorZ = originZ;
        float maxFloorZ = originZ + (gridDepth - 1) * cellSize;

        float wallCenterInset = cellSize * 0.5f;
        const float wallColliderHalf = 0.95f;

        return new DungeonRoomBounds
        {
            MinX = minFloorX - wallCenterInset + wallColliderHalf,
            MaxX = maxFloorX + wallCenterInset - wallColliderHalf,
            MinZ = minFloorZ - wallCenterInset + wallColliderHalf,
            MaxZ = maxFloorZ + wallCenterInset - wallColliderHalf,
            MinY = groundY + 0.3f,
            MaxY = groundY + 6.5f,
        };
    }

    public Vector3 ClampHorizontal(Vector3 position, float margin)
    {
        return new Vector3
        {
            X = Math.Clamp(position.X, MinX + margin, MaxX - margin),
            Y = position.Y,
            Z = Math.Clamp(position.Z, MinZ + margin, MaxZ - margin),
        };
    }

    /// <summary>Pulls the camera toward <paramref name="pivot"/> until it lies inside the room.</summary>
    public Vector3 ConstrainCameraAlongPivot(Vector3 pivot, Vector3 idealCamera, float margin)
    {
        if (Contains(idealCamera, margin))
        {
            return idealCamera;
        }

        Vector3 delta = new()
        {
            X = idealCamera.X - pivot.X,
            Y = idealCamera.Y - pivot.Y,
            Z = idealCamera.Z - pivot.Z,
        };

        float lo = 0f;
        float hi = 1f;
        for (int i = 0; i < 20; i++)
        {
            float mid = (lo + hi) * 0.5f;
            Vector3 sample = new()
            {
                X = pivot.X + delta.X * mid,
                Y = pivot.Y + delta.Y * mid,
                Z = pivot.Z + delta.Z * mid,
            };
            if (Contains(sample, margin))
            {
                lo = mid;
            }
            else
            {
                hi = mid;
            }
        }

        Vector3 onRay = new()
        {
            X = pivot.X + delta.X * lo,
            Y = pivot.Y + delta.Y * lo,
            Z = pivot.Z + delta.Z * lo,
        };
        return ClampPoint(onRay, margin);
    }

    private bool Contains(Vector3 p, float margin) =>
        p.X >= MinX + margin && p.X <= MaxX - margin &&
        p.Y >= MinY + margin && p.Y <= MaxY - margin &&
        p.Z >= MinZ + margin && p.Z <= MaxZ - margin;

    private Vector3 ClampPoint(Vector3 p, float margin) => new()
    {
        X = Math.Clamp(p.X, MinX + margin, MaxX - margin),
        Y = Math.Clamp(p.Y, MinY + margin, MaxY - margin),
        Z = Math.Clamp(p.Z, MinZ + margin, MaxZ - margin),
    };
}
