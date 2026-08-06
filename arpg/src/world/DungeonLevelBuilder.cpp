#include "world/DungeonLevelBuilder.hpp"

#include "spark/ecs/GameObject.hpp"
#include "spark/scene/GameWorld.hpp"

#include "spark/ecs/components/core/TransformComponent.hpp"
#include "spark/ecs/components/physics/3d/BoxCollider3DComponent.hpp"
#include "spark/ecs/components/rendering/MaterialComponent.hpp"
#include "spark/ecs/components/rendering/MeshComponent.hpp"
#include "spark/math/Constants.hpp"
#include "spark/math/Quaternion.hpp"
#include "spark/scene/Mesh.hpp"

namespace Arpg {

namespace {

constexpr float kCellSize = 2.0F;
constexpr int kGridWidth = 11;
constexpr int kGridDepth = 11;
constexpr float kGroundY = 0.0F;

[[nodiscard]] Spark::Vector3 GridToWorld(const int gx, const int gz) noexcept {
    const float originX = -static_cast<float>(kGridWidth) * kCellSize * 0.5F + kCellSize * 0.5F;
    const float originZ = -static_cast<float>(kGridDepth) * kCellSize * 0.5F + kCellSize * 0.5F;
    return Spark::Vector3{
            originX + static_cast<float>(gx) * kCellSize,
            kGroundY,
            originZ + static_cast<float>(gz) * kCellSize};
}

/** Lifts props so the mesh AABB minimum Y sits on @ref kGroundY after uniform scale. */
[[nodiscard]] float GroundYOffsetForMesh(const Spark::Mesh& mesh, const float uniformScale) noexcept {
    Spark::Vector3 boundsMin{};
    Spark::Vector3 boundsMax{};
    if (!mesh.TryComputeAxisAlignedBounds(boundsMin, boundsMax)) {
        return 0.0F;
    }
    return -boundsMin.y * uniformScale;
}

}  // namespace

Spark::GameObject* DungeonLevelBuilder::SpawnProp(
        GameContext& ctx,
        Spark::Array<Spark::GameObject*>& roots,
        const char* modelFile,
        const Spark::Vector3& translation,
        const float yawRadians,
        const float uniformScale,
        const bool addWallCollider,
        const char* debugName) {
    Spark::GameWorld& world = *ctx.world;
    const Spark::Utf8String path = ctx.paths.ModelPath(modelFile);
    const Spark::GltfAsset asset = ctx.assets.LoadProp(world, path);
    if (!asset.mesh) {
        return nullptr;
    }

    Spark::GameObject* go = world.CreateGameObject();
    go->GetName() = Spark::Utf8String(debugName);

    const float groundLift = GroundYOffsetForMesh(*asset.mesh, uniformScale);
    Spark::Vector3 groundedPos = translation;
    groundedPos.y = kGroundY + groundLift;

    Spark::TransformComponent* tr = go->AddComponent<Spark::TransformComponent>();
    tr->SetTranslation(groundedPos);
    tr->SetUniformScale(uniformScale);
    tr->SetRotation(Spark::Quaternion::FromAxisAngle(Spark::Vector3::UnitY, yawRadians));

    const Spark::Vector3 meshAlbedo =
            asset.baseColorTexture ? Spark::Vector3::One : Spark::Vector3{0.55F, 0.52F, 0.48F};
    go->AddComponent<Spark::MeshComponent>(asset.mesh, Spark::SceneMeshSlot::Custom, meshAlbedo);
    if (asset.baseColorTexture) {
        if (Spark::MaterialComponent* mat = go->AddComponent<Spark::MaterialComponent>(
                    asset.baseColorTexture, Spark::Vector3::One)) {
            mat->SetMetallic(0.05F);
            mat->SetRoughness(0.82F);
        }
    } else if (Spark::MaterialComponent* mat = go->AddComponent<Spark::MaterialComponent>()) {
        mat->SetTint(meshAlbedo);
        mat->SetMetallic(0.08F);
        mat->SetRoughness(0.78F);
    }

    if (addWallCollider) {
        Spark::Vector3 half{0.95F, 1.0F, 0.95F};
        half.x *= uniformScale;
        half.y *= uniformScale;
        half.z *= uniformScale;
        go->AddComponent<Spark::BoxCollider3DComponent>(half);
    }

    roots.PushBack(go);
    return go;
}

Spark::GameObject* DungeonLevelBuilder::SpawnFloorTile(
        GameContext& ctx,
        Spark::Array<Spark::GameObject*>& roots,
        const int gridX,
        const int gridZ) {
    const Spark::Vector3 pos = GridToWorld(gridX, gridZ);
    return SpawnProp(
            ctx,
            roots,
            "floor_tile_large.gltf",
            pos,
            0.0F,
            1.0F,
            false,
            "FloorTile");
}

void DungeonLevelBuilder::SpawnPerimeterWalls(
        GameContext& ctx,
        Spark::Array<Spark::GameObject*>& roots,
        const Spark::Vector3& corner00,
        const Spark::Vector3& center) {
    const float wallYawNorth = 0.0F;
    const float wallYawEast = Spark::HalfPi;
    const float wallYawSouth = Spark::Pi;
    const float wallYawWest = -Spark::HalfPi;

    for (int i = 0; i < kGridWidth; ++i) {
        const Spark::Vector3 north = GridToWorld(i, 0);
        const Spark::Vector3 south = GridToWorld(i, kGridDepth - 1);
        SpawnProp(
                ctx,
                roots,
                "wall.gltf",
                {north.x, kGroundY, north.z - kCellSize * 0.5F},
                wallYawNorth,
                1.0F,
                true,
                "WallNorth");
        SpawnProp(
                ctx,
                roots,
                "wall.gltf",
                {south.x, kGroundY, south.z + kCellSize * 0.5F},
                wallYawSouth,
                1.0F,
                true,
                "WallSouth");
    }
    for (int j = 0; j < kGridDepth; ++j) {
        const Spark::Vector3 west = GridToWorld(0, j);
        const Spark::Vector3 east = GridToWorld(kGridWidth - 1, j);
        SpawnProp(
                ctx,
                roots,
                "wall.gltf",
                {west.x - kCellSize * 0.5F, kGroundY, west.z},
                wallYawWest,
                1.0F,
                true,
                "WallWest");
        SpawnProp(
                ctx,
                roots,
                "wall.gltf",
                {east.x + kCellSize * 0.5F, kGroundY, east.z},
                wallYawEast,
                1.0F,
                true,
                "WallEast");
    }

    SpawnProp(
            ctx,
            roots,
            "wall_doorway.gltf",
            {center.x, kGroundY, corner00.z - kCellSize * 0.5F},
            0.0F,
            1.0F,
            false,
            "DoorwayNorth");
}

void DungeonLevelBuilder::SpawnPlazaProps(
        GameContext& ctx,
        Spark::Array<Spark::GameObject*>& roots,
        const Spark::Vector3& center) {
    SpawnProp(ctx, roots, "pillar.gltf", {center.x - 3.0F, kGroundY, center.z}, 0.0F, 1.0F, true, "PillarA");
    SpawnProp(
            ctx,
            roots,
            "pillar_decorated.gltf",
            {center.x + 3.0F, kGroundY, center.z - 2.0F},
            Spark::HalfPi * 0.5F,
            1.0F,
            true,
            "PillarB");
    SpawnProp(
            ctx,
            roots,
            "chest.gltf",
            {center.x + 2.5F, kGroundY, center.z + 3.0F},
            -Spark::HalfPi,
            1.0F,
            false,
            "Chest");
    SpawnProp(
            ctx,
            roots,
            "torch_mounted.gltf",
            {center.x - 4.0F, kGroundY, center.z + 3.5F},
            Spark::HalfPi,
            1.0F,
            false,
            "Torch");
    SpawnProp(
            ctx,
            roots,
            "table_medium.gltf",
            {center.x, kGroundY, center.z + 2.0F},
            0.0F,
            1.0F,
            true,
            "Table");
    SpawnProp(
            ctx,
            roots,
            "sword_shield.gltf",
            {center.x + 0.5F, kGroundY, center.z + 2.0F},
            Spark::HalfPi,
            0.85F,
            false,
            "SwordShieldProp");
}

DungeonBuildResult DungeonLevelBuilder::Build(GameContext& ctx) {
    ctx.assets.PrimeSharedTextures(*ctx.world, ctx.paths);

    DungeonBuildResult result{};

    for (int gz = 0; gz < kGridDepth; ++gz) {
        for (int gx = 0; gx < kGridWidth; ++gx) {
            SpawnFloorTile(ctx, result.roots, gx, gz);
        }
    }

    const Spark::Vector3 corner00 = GridToWorld(0, 0);
    const Spark::Vector3 center = GridToWorld(kGridWidth / 2, kGridDepth / 2);

    SpawnPerimeterWalls(ctx, result.roots, corner00, center);
    SpawnPlazaProps(ctx, result.roots, center);

    result.playerSpawn = center;
    result.playerSpawn.y = 0.02F;
    return result;
}

}  // namespace Arpg
