#include "world/DungeonLevelBuilder.hpp"

#include "world/DungeonRoomBounds.hpp"

#include "spark/ecs/GameObject.hpp"
#include "spark/scene/core/GameWorld.hpp"

#include "spark/ecs/components/core/TransformComponent.hpp"
#include "spark/ecs/components/lighting/DirectionalLightComponent.hpp"
#include "spark/ecs/components/lighting/PointLightComponent.hpp"
#include "spark/ecs/components/physics/3d/BoxCollider3DComponent.hpp"
#include "spark/ecs/components/rendering/MaterialComponent.hpp"
#include "spark/ecs/components/rendering/MeshComponent.hpp"
#include "spark/ecs/components/rendering/SkyComponent.hpp"
#include "spark/config.hpp"
#include "spark/math/Constants.hpp"
#include "spark/math/Quaternion.hpp"
#include "spark/scene/material/GltfMaterial.hpp"
#include "spark/scene/mesh/Mesh.hpp"
#include "spark/scene/texture/Texture2D.hpp"

namespace Arpg {

namespace {

constexpr float kCellSize = 2.0F;
constexpr int kGridWidth = 11;
constexpr int kGridDepth = 11;
constexpr float kGroundY = 0.0F;
constexpr float kSkySphereScale = 72.0F;

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

    const bool hasMaterial = asset.material.HasAnyTexture() || asset.baseColorTexture;
    const Spark::Vector3 meshAlbedo =
            hasMaterial ? Spark::Vector3::One : Spark::Vector3{0.55F, 0.52F, 0.48F};
    go->AddComponent<Spark::MeshComponent>(asset.mesh, Spark::SceneMeshSlot::Custom, meshAlbedo);
    if (hasMaterial) {
        if (Spark::MaterialComponent* mat = go->AddComponent<Spark::MaterialComponent>()) {
            Spark::ApplyGltfMaterialDesc(*mat, asset.material);
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

Spark::GameObject* DungeonLevelBuilder::SpawnPointLight(
        GameContext& ctx,
        Spark::Array<Spark::GameObject*>& roots,
        const Spark::Vector3& position,
        const Spark::Vector3& color,
        const float intensity,
        const float range,
        const char* debugName) {
    Spark::GameWorld& world = *ctx.world;
    Spark::GameObject* light = world.CreateGameObject();
    light->GetName() = Spark::Utf8String(debugName);
    Spark::TransformComponent* tr = light->AddComponent<Spark::TransformComponent>();
    tr->SetTranslation(position);
    light->AddComponent<Spark::PointLightComponent>(color, intensity, range)->SetCastsShadow(false);
    roots.PushBack(light);
    return light;
}

void DungeonLevelBuilder::SpawnSkybox(
        GameContext& ctx,
        Spark::Array<Spark::GameObject*>& roots,
        DungeonBuildResult& result) {
    Spark::GameWorld& world = *ctx.world;

    Spark::SharedPtr<Spark::Mesh> skyMesh = Spark::MakeShared<Spark::Mesh>(Spark::Utf8String("DungeonSky"));
    *skyMesh = Spark::Mesh::CreateSkySphere(1.0F, 18, 36);

    Spark::SharedPtr<Spark::Texture2D> skyTex;
    Spark::Texture2D skyDecoded;
    if (Spark::Texture2D::TryLoadFromFile(SPARK_SKY_TEXTURE_PATH, skyDecoded)) {
        skyTex = Spark::MakeShared<Spark::Texture2D>(Spark::MoveTemp(skyDecoded));
        world.RegisterTexture(skyTex, "arpg/sky_equirect");
    } else {
        Spark::Utf8String altSky(SPARK_ASSETS_DIR);
        altSky.AppendUtf8("/textures/sky/equirect_sky_1k.hdr");
        if (Spark::Texture2D::TryLoadFromFile(altSky.CStr(), skyDecoded)) {
            skyTex = Spark::MakeShared<Spark::Texture2D>(Spark::MoveTemp(skyDecoded));
            world.RegisterTexture(skyTex, "arpg/sky_equirect");
        }
    }

    Spark::GameObject* sky = world.CreateGameObject();
    sky->GetName() = Spark::Utf8String("Sky");
    Spark::TransformComponent* tr = sky->AddComponent<Spark::TransformComponent>();
    tr->SetUniformScale(kSkySphereScale);

    Spark::SkyComponent* skyComp = sky->AddComponent<Spark::SkyComponent>(Spark::SceneSkyMode::Box);
    Spark::MeshComponent* skyMc = sky->AddComponent<Spark::MeshComponent>(skyMesh, Spark::Vector3::One);
    Spark::MaterialComponent* skyMat = sky->AddComponent<Spark::MaterialComponent>();

    if (skyTex) {
        skyComp->SetTint(Spark::Vector3::One);
        skyMc->SetAlbedo(Spark::Vector3::One);
        skyMat->SetBaseColorTexture(skyTex);
    } else {
        const Spark::Vector3 skyTint{0.18F, 0.26F, 0.48F};
        skyComp->SetTint(skyTint);
        skyMc->SetAlbedo(skyTint);
    }

    roots.PushBack(sky);
    result.skyRoot = sky;
}

void DungeonLevelBuilder::SpawnSceneLights(
        GameContext& ctx,
        Spark::Array<Spark::GameObject*>& roots,
        const Spark::Vector3& center) {
    Spark::GameWorld& world = *ctx.world;

    Spark::GameObject* sun = world.CreateGameObject();
    sun->GetName() = Spark::Utf8String("Sun");
    Spark::TransformComponent* sunTr = sun->AddComponent<Spark::TransformComponent>();
    const Spark::Vector3 sunDir = Spark::Vector3{0.35F, 0.82F, 0.28F}.Normalized();
    sunTr->SetRotation(Spark::Quaternion::FromShortestArc(Spark::Vector3::UnitZ, sunDir));
    sun->AddComponent<Spark::DirectionalLightComponent>(Spark::Vector3{1.0F, 0.96F, 0.90F}, 1.05F)
            ->SetCastsShadow(false);
    roots.PushBack(sun);

    SpawnPointLight(
            ctx,
            roots,
            {center.x - 4.0F, 2.4F, center.z + 3.5F},
            {1.0F, 0.58F, 0.22F},
            4.2F,
            9.0F,
            "TorchLight");
    SpawnPointLight(
            ctx,
            roots,
            {center.x + 2.5F, 2.0F, center.z + 3.0F},
            {0.95F, 0.72F, 0.38F},
            2.8F,
            7.5F,
            "ChestLight");
    SpawnPointLight(
            ctx,
            roots,
            {center.x, 5.5F, center.z},
            {0.82F, 0.88F, 1.0F},
            2.2F,
            16.0F,
            "HallFill");
    SpawnPointLight(
            ctx,
            roots,
            {center.x - 8.0F, 3.0F, center.z - 6.0F},
            {0.45F, 0.62F, 1.0F},
            2.0F,
            14.0F,
            "CornerCoolA");
    SpawnPointLight(
            ctx,
            roots,
            {center.x + 8.0F, 3.0F, center.z + 6.0F},
            {0.55F, 0.70F, 0.95F},
            1.8F,
            14.0F,
            "CornerCoolB");
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
    SpawnSkybox(ctx, result.roots, result);
    SpawnSceneLights(ctx, result.roots, center);

    result.playerSpawn = center;
    result.playerSpawn.y = 0.02F;
    result.roomBounds = DungeonRoomBounds::ForGrid(kGridWidth, kGridDepth, kCellSize, kGroundY);
    return result;
}

}  // namespace Arpg
