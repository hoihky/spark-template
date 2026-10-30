using Spark.Bindings;
using Spark.Bindings.Components;
using SparkDemoFoundation;

namespace SparkDemoTerrain;

/// <summary>Builds terrain, marker, edit cursor, and point lights (C++ <c>TerrainDemo::Load</c>).</summary>
internal sealed class TerrainSceneBuilder
{
    public GameObject Terrain { get; private set; } = null!;
    public TerrainComponent TerrainComp { get; private set; } = null!;
    public TerrainEditCursor EditCursor { get; private set; } = null!;

    public void Build(GameWorld world, DemoRootCollection roots)
    {
        world.RegisterTerrainDemoGround();

        Terrain = CreateNamed(world, roots, "ProceduralTerrain");
        TerrainComp = Terrain.AddTerrain(TerrainCatalog.ProceduralSettings, TerrainCatalog.DefaultMeshAlbedo);
        MaterialComponent terrainMat = Terrain.AddMaterialLit(
            TerrainDemoTextureKeys.Ground,
            new Vector3 { X = 1, Y = 1, Z = 1 });
        terrainMat.Metallic = 0.02f;
        terrainMat.Roughness = 0.94f;

        SpawnMarker(world, roots);
        EditCursor = new TerrainEditCursor();
        EditCursor.Spawn(world, roots);

        SpawnPointLight(
            world,
            roots,
            new Vector3 { X = 92, Y = 52, Z = 78 },
            new Vector3 { X = 0.4f, Y = 0.75f, Z = 1.0f },
            7.0f,
            260.0f);
        SpawnPointLight(
            world,
            roots,
            new Vector3 { X = -105, Y = 34, Z = -72 },
            new Vector3 { X = 1.0f, Y = 0.55f, Z = 0.28f },
            5.8f,
            240.0f);
    }

    private static GameObject CreateNamed(GameWorld world, DemoRootCollection roots, string name)
    {
        GameObject go = world.CreateGameObject(name);
        roots.Track(go);
        go.GetOrAddTransform();
        return go;
    }

    private static void SpawnMarker(GameWorld world, DemoRootCollection roots)
    {
        GameObject marker = CreateNamed(world, roots, "TerrainMarker");
        TransformComponent tr = marker.GetOrAddTransform();
        tr.Translation = new Vector3 { X = 55, Y = 9, Z = 72 };
        tr.SetUniformScale(0.9f);
        marker.AddMesh(SparkSceneMeshSlot.SparkSceneMeshSlot_UnitCube, new Vector3 { X = 0.85f, Y = 0.38f, Z = 0.22f });
        MaterialComponent em = marker.AddMaterialLit(null, new Vector3 { X = 1, Y = 1, Z = 1 });
        em.SetEmissive(new Vector3 { X = 0.95f, Y = 0.42f, Z = 0.12f }, 2.1f);
        em.Roughness = 0.38f;
    }

    private static void SpawnPointLight(
        GameWorld world,
        DemoRootCollection roots,
        Vector3 position,
        Vector3 color,
        float intensity,
        float range)
    {
        GameObject light = CreateNamed(world, roots, "TerrainPointLight");
        light.GetOrAddTransform().Translation = position;
        PointLightComponent pl = light.AddPointLight(color, intensity, range);
        pl.SetCastsShadow(1);
    }
}
