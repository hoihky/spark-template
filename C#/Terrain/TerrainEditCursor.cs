using Spark.Bindings;
using Spark.Bindings.Components;
using SparkDemoFoundation;

namespace SparkDemoTerrain;

/// <summary>Cyan sculpt cursor cube (C++ <c>TerrainEditCursor</c> object).</summary>
internal sealed class TerrainEditCursor
{
    private GameObject? _object;
    private MaterialComponent? _material;

    public void Spawn(GameWorld world, DemoRootCollection roots)
    {
        _object = world.CreateGameObject("TerrainEditCursor");
        roots.Track(_object);
        TransformComponent tr = _object.GetOrAddTransform();
        tr.SetUniformScale(0.34f);
        Hide(tr);
        _object.AddMesh(SparkSceneMeshSlot.SparkSceneMeshSlot_UnitCube, new Vector3 { X = 0.2f, Y = 0.95f, Z = 1.0f });
        _material = _object.AddMaterialLit(null, new Vector3 { X = 1, Y = 1, Z = 1 });
        _material.SetEmissive(new Vector3 { X = 0.18f, Y = 0.92f, Z = 1.0f }, 4.2f);
        _material.Roughness = 0.25f;
    }

    public void UpdateAtHit(Vector3 hit, bool leftDown, bool rightDown)
    {
        if (_object is null || _material is null)
        {
            return;
        }

        TransformComponent tr = _object.GetOrAddTransform();
        tr.Translation = new Vector3 { X = hit.X, Y = hit.Y + TerrainCatalog.CursorHeightOffset, Z = hit.Z };

        if (leftDown)
        {
            _material.SetEmissive(new Vector3 { X = 0.35f, Y = 1.0f, Z = 0.48f }, 5.8f);
        }
        else if (rightDown)
        {
            _material.SetEmissive(new Vector3 { X = 1.0f, Y = 0.36f, Z = 0.2f }, 5.8f);
        }
        else
        {
            _material.SetEmissive(new Vector3 { X = 0.18f, Y = 0.92f, Z = 1.0f }, 4.2f);
        }
    }

    public void HideAt()
    {
        if (_object is null)
        {
            return;
        }

        Hide(_object.GetOrAddTransform());
    }

    private static void Hide(TransformComponent tr) =>
        tr.Translation = new Vector3 { X = 0, Y = TerrainCatalog.HiddenCursorY, Z = 0 };
}
