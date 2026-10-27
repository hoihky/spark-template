using Spark.Bindings;
using Spark.Bindings.Components;
using SparkArpg.Core;
using SparkArpg.Services;

namespace SparkArpg.World;

internal sealed class DungeonBuildResult
{
    public List<GameObject> Roots { get; } = [];
    public Vector3 PlayerSpawn { get; set; }
    public GameObject? SkyRoot { get; set; }
    public DungeonRoomBounds RoomBounds { get; set; }
}

internal static class DungeonLevelBuilder
{
    private const float CellSize = 2.0f;
    private const int GridWidth = 11;
    private const int GridDepth = 11;
    private const float GroundY = 0.0f;
    private const float SkySphereScale = 72.0f;

    public static DungeonBuildResult Build(GameContext ctx)
    {
        GameWorld world = ctx.World!;
        ctx.Assets.PrimeSharedTextures(world, ctx.Paths);

        var result = new DungeonBuildResult();

        for (int gz = 0; gz < GridDepth; gz++)
        {
            for (int gx = 0; gx < GridWidth; gx++)
            {
                SpawnFloorTile(ctx, result.Roots, gx, gz);
            }
        }

        Vector3 corner00 = GridToWorld(0, 0);
        Vector3 center = GridToWorld(GridWidth / 2, GridDepth / 2);

        SpawnPerimeterWalls(ctx, result.Roots, corner00, center);
        SpawnPlazaProps(ctx, result.Roots, center);
        result.SkyRoot = SpawnSkybox(ctx, result.Roots);
        SpawnSceneLights(ctx, result.Roots, center);

        result.PlayerSpawn = new Vector3 { X = center.X, Y = 0.02f, Z = center.Z };
        result.RoomBounds = DungeonRoomBounds.ForGrid(GridWidth, GridDepth, CellSize, GroundY);
        return result;
    }

    private static Vector3 GridToWorld(int gx, int gz)
    {
        float originX = -GridWidth * CellSize * 0.5f + CellSize * 0.5f;
        float originZ = -GridDepth * CellSize * 0.5f + CellSize * 0.5f;
        return new Vector3
        {
            X = originX + gx * CellSize,
            Y = GroundY,
            Z = originZ + gz * CellSize,
        };
    }

    private static GameObject? SpawnProp(
        GameContext ctx,
        List<GameObject> roots,
        string modelFile,
        Vector3 translation,
        float yawRadians,
        float uniformScale,
        bool addWallCollider,
        string debugName)
    {
        GameWorld world = ctx.World!;
        string path = Path.GetFullPath(ctx.Paths.ModelPath(modelFile));
        if (!ctx.Assets.EnsurePropMesh(world, path))
        {
            return null;
        }

        GameObject go = world.CreateGameObject(debugName);
        TransformComponent tr = go.GetOrAddTransform();
        tr.Translation = new Vector3 { X = translation.X, Y = GroundY, Z = translation.Z };
        tr.SetUniformScale(uniformScale);
        tr.SetRotation(GameMath.FromAxisAngleY(yawRadians));

        Vector3 meshAlbedo = new() { X = 1, Y = 1, Z = 1 };
        MeshComponent mesh = go.AddMesh(SparkSceneMeshSlot.SparkSceneMeshSlot_Custom, meshAlbedo);
        mesh.SetMeshByKey(world, path);

        string atlasPath = Path.GetFullPath(ctx.Paths.ModelPath("dungeon_texture.png"));
        go.ApplyGltfPropMaterial(
            world,
            path,
            File.Exists(atlasPath) ? ArpgAssetKeys.DungeonAtlasKey : null);

        if (addWallCollider)
        {
            Vector3 half = new() { X = 0.95f * uniformScale, Y = 1.0f * uniformScale, Z = 0.95f * uniformScale };
            go.AddBoxCollider3D(half);
        }

        roots.Add(go);
        return go;
    }

    private static void SpawnFloorTile(GameContext ctx, List<GameObject> roots, int gridX, int gridZ)
    {
        Vector3 pos = GridToWorld(gridX, gridZ);
        SpawnProp(ctx, roots, "floor_tile_large.gltf", pos, 0, 1, false, "FloorTile");
    }

    private static void SpawnPerimeterWalls(GameContext ctx, List<GameObject> roots, Vector3 corner00, Vector3 center)
    {
        float wallYawNorth = 0;
        float wallYawEast = GameMath.HalfPi;
        float wallYawSouth = GameMath.Pi;
        float wallYawWest = -GameMath.HalfPi;

        for (int i = 0; i < GridWidth; i++)
        {
            Vector3 north = GridToWorld(i, 0);
            Vector3 south = GridToWorld(i, GridDepth - 1);
            SpawnProp(
                ctx,
                roots,
                "wall.gltf",
                new Vector3 { X = north.X, Y = GroundY, Z = north.Z - CellSize * 0.5f },
                wallYawNorth,
                1,
                true,
                "WallNorth");
            SpawnProp(
                ctx,
                roots,
                "wall.gltf",
                new Vector3 { X = south.X, Y = GroundY, Z = south.Z + CellSize * 0.5f },
                wallYawSouth,
                1,
                true,
                "WallSouth");
        }

        for (int j = 0; j < GridDepth; j++)
        {
            Vector3 west = GridToWorld(0, j);
            Vector3 east = GridToWorld(GridWidth - 1, j);
            SpawnProp(
                ctx,
                roots,
                "wall.gltf",
                new Vector3 { X = west.X - CellSize * 0.5f, Y = GroundY, Z = west.Z },
                wallYawWest,
                1,
                true,
                "WallWest");
            SpawnProp(
                ctx,
                roots,
                "wall.gltf",
                new Vector3 { X = east.X + CellSize * 0.5f, Y = GroundY, Z = east.Z },
                wallYawEast,
                1,
                true,
                "WallEast");
        }

        SpawnProp(
            ctx,
            roots,
            "wall_doorway.gltf",
            new Vector3 { X = center.X, Y = GroundY, Z = corner00.Z - CellSize * 0.5f },
            0,
            1,
            false,
            "DoorwayNorth");
    }

    private static void SpawnPlazaProps(GameContext ctx, List<GameObject> roots, Vector3 center)
    {
        SpawnProp(ctx, roots, "pillar.gltf", new Vector3 { X = center.X - 3, Y = GroundY, Z = center.Z }, 0, 1, true, "PillarA");
        SpawnProp(
            ctx,
            roots,
            "pillar_decorated.gltf",
            new Vector3 { X = center.X + 3, Y = GroundY, Z = center.Z - 2 },
            GameMath.HalfPi * 0.5f,
            1,
            true,
            "PillarB");
        SpawnProp(
            ctx,
            roots,
            "chest.gltf",
            new Vector3 { X = center.X + 2.5f, Y = GroundY, Z = center.Z + 3 },
            -GameMath.HalfPi,
            1,
            false,
            "Chest");
        SpawnProp(
            ctx,
            roots,
            "torch_mounted.gltf",
            new Vector3 { X = center.X - 4, Y = GroundY, Z = center.Z + 3.5f },
            GameMath.HalfPi,
            1,
            false,
            "Torch");
        SpawnProp(
            ctx,
            roots,
            "table_medium.gltf",
            new Vector3 { X = center.X, Y = GroundY, Z = center.Z + 2 },
            0,
            1,
            true,
            "Table");
        SpawnProp(
            ctx,
            roots,
            "sword_shield.gltf",
            new Vector3 { X = center.X + 0.5f, Y = GroundY, Z = center.Z + 2 },
            GameMath.HalfPi,
            0.85f,
            false,
            "SwordShieldProp");
    }

    private static GameObject SpawnPointLight(
        GameContext ctx,
        List<GameObject> roots,
        Vector3 position,
        Vector3 color,
        float intensity,
        float range,
        string debugName)
    {
        GameWorld world = ctx.World!;
        GameObject light = world.CreateGameObject(debugName);
        light.GetOrAddTransform().Translation = position;
        light.AddPointLight(color, intensity, range);
        roots.Add(light);
        return light;
    }

    private static GameObject SpawnSkybox(GameContext ctx, List<GameObject> roots)
    {
        GameWorld world = ctx.World!;
        world.RegisterSkySphereMesh(ArpgAssetKeys.SkyMeshKey);

        GameObject sky = world.CreateGameObject("Sky");
        sky.GetOrAddTransform().SetUniformScale(SkySphereScale);
        sky.AddSky(SparkSceneSkyMode.SparkSceneSkyMode_Box);

        SkyComponent? skyComp = sky.GetSky();
        string skyTexPath = Path.GetFullPath(ctx.Paths.SkyTexturePath());
        bool haveSkyTex = File.Exists(skyTexPath)
            && world.RegisterTextureFromFile(skyTexPath, ArpgAssetKeys.SkyTextureKey);

        MeshComponent skyMesh = sky.AddMesh(SparkSceneMeshSlot.SparkSceneMeshSlot_Custom, new Vector3 { X = 1, Y = 1, Z = 1 });
        skyMesh.SetMeshByKey(world, ArpgAssetKeys.SkyMeshKey);

        MaterialComponent? skyMat = sky.GetMaterial();
        if (skyMat is null)
        {
            sky.AddMaterialLit(null, new Vector3 { X = 1, Y = 1, Z = 1 });
            skyMat = sky.GetMaterial();
        }

        if (haveSkyTex && skyMat is not null && skyMat.SetBaseColorTexture(world, ArpgAssetKeys.SkyTextureKey))
        {
            if (skyComp is not null)
            {
                skyComp.Tint = new Vector3 { X = 1, Y = 1, Z = 1 };
            }

            skyMesh.Albedo = new Vector3 { X = 1, Y = 1, Z = 1 };
        }
        else
        {
            Vector3 skyTint = new() { X = 0.18f, Y = 0.26f, Z = 0.48f };
            if (skyComp is not null)
            {
                skyComp.Tint = skyTint;
            }

            skyMesh.Albedo = skyTint;
        }

        roots.Add(sky);
        return sky;
    }

    private static void SpawnSceneLights(GameContext ctx, List<GameObject> roots, Vector3 center)
    {
        GameWorld world = ctx.World!;

        GameObject sun = world.CreateGameObject("Sun");
        Vector3 sunDir = GameMath.Normalized(new Vector3 { X = 0.35f, Y = 0.82f, Z = 0.28f });
        sun.GetOrAddTransform().SetRotation(
            GameMath.FromShortestArc(new Vector3 { X = 0, Y = 0, Z = 1 }, sunDir));
        sun.AddDirectionalLight(new Vector3 { X = 1.0f, Y = 0.96f, Z = 0.90f }, 1.05f);
        roots.Add(sun);

        SpawnPointLight(ctx, roots, new Vector3 { X = center.X - 4, Y = 2.4f, Z = center.Z + 3.5f }, new Vector3 { X = 1, Y = 0.58f, Z = 0.22f }, 4.2f, 9, "TorchLight");
        SpawnPointLight(ctx, roots, new Vector3 { X = center.X + 2.5f, Y = 2, Z = center.Z + 3 }, new Vector3 { X = 0.95f, Y = 0.72f, Z = 0.38f }, 2.8f, 7.5f, "ChestLight");
        SpawnPointLight(ctx, roots, new Vector3 { X = center.X, Y = 5.5f, Z = center.Z }, new Vector3 { X = 0.82f, Y = 0.88f, Z = 1 }, 2.2f, 16, "HallFill");
        SpawnPointLight(ctx, roots, new Vector3 { X = center.X - 8, Y = 3, Z = center.Z - 6 }, new Vector3 { X = 0.45f, Y = 0.62f, Z = 1 }, 2, 14, "CornerCoolA");
        SpawnPointLight(ctx, roots, new Vector3 { X = center.X + 8, Y = 3, Z = center.Z + 6 }, new Vector3 { X = 0.55f, Y = 0.70f, Z = 0.95f }, 1.8f, 14, "CornerCoolB");
    }
}
