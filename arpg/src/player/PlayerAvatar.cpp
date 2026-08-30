#include "player/PlayerAvatar.hpp"

#include "spark/ecs/GameObject.hpp"
#include "spark/scene/core/GameWorld.hpp"

#include "spark/ecs/components/animation/AnimatorComponent.hpp"
#include "spark/ecs/components/animation/Character3DAnimFsmComponent.hpp"
#include "spark/ecs/components/core/TransformComponent.hpp"
#include "spark/ecs/components/physics/3d/CharacterController3DComponent.hpp"
#include "spark/ecs/components/rendering/MaterialComponent.hpp"
#include "spark/ecs/components/rendering/SkinnedMeshComponent.hpp"
#include "spark/ecs/components/rendering/TextOverlayComponent.hpp"
#include "spark/math/Constants.hpp"
#include "spark/scene/material/GltfMaterial.hpp"

#include <algorithm>
#include <cmath>

namespace Arpg {

namespace {

void RemoveFromOwnedRoots(Spark::Array<Spark::GameObject*>& ownedRoots, Spark::GameObject* go) {
    for (std::size_t i = 0; i < ownedRoots.GetSize(); ++i) {
        if (ownedRoots[i] == go) {
            ownedRoots[i] = ownedRoots.GetLast();
            ownedRoots.PopBack();
            break;
        }
    }
}

void TuneThirdPersonRigForHeight(Spark::CharacterCameraRig& rig, const float heightM) {
    rig.mode = Spark::CharacterCameraMode::ThirdPerson;
    rig.firstPersonEyeHeight = std::clamp(heightM * 0.88F, 1.4F, 1.95F);
    rig.firstPersonForwardNudge = std::clamp(heightM * 0.08F, 0.14F, 0.42F);
    rig.thirdPersonPivotHeight = std::clamp(heightM * 0.88F, 1.32F, 1.9F);
    rig.thirdPersonFocusAhead = std::clamp(heightM * 0.05F, 0.12F, 0.38F);
    rig.thirdPersonCameraLift = std::clamp(heightM * 0.12F, 0.35F, 0.68F);
    rig.thirdPersonDistance = std::clamp(heightM * 2.4F, 4.0F, 6.5F);
}

}  // namespace

PlayerAvatarResult PlayerAvatar::Spawn(
        GameContext& ctx,
        Spark::Array<Spark::GameObject*>& ownedRoots,
        const Spark::Vector3& spawnPosition) {
    PlayerAvatarResult result{};
    Spark::GameWorld& world = *ctx.world;

    result.rig.characterPosition = spawnPosition;
    result.rig.groundY = spawnPosition.y;
    result.rig.characterVisualYaw = 0.0F;
    result.rig.cameraYaw = 0.0F;
    result.rig.cameraPitch = -0.15F;
    result.rig.moveSpeed = 4.8F;
    result.rig.runSpeed = 8.5F;

    Spark::GameObject* root = world.CreateGameObject();
    root->GetName() = Spark::Utf8String("Player");
    Spark::TransformComponent* rootTr = root->AddComponent<Spark::TransformComponent>();
    rootTr->SetTranslation(spawnPosition);

    Spark::CharacterController3DComponent* controller =
            root->AddComponent<Spark::CharacterController3DComponent>(0.38F, Spark::Vector3{0.0F, 0.38F, 0.0F});
    controller->SetStepOffset(0.35F);
    controller->SetSlopeLimitDegrees(50.0F);

    Spark::Utf8String foxPath = ctx.paths.SparkEngineModelPath(kSparkFoxModelFile);
    Spark::SkinnedGltfAsset avatar = ctx.assets.LoadSkinnedAvatar(world, foxPath);
    bool usedFox = avatar.mesh && avatar.skeleton;
    if (!usedFox) {
        Spark::Utf8String cesiumPath = ctx.paths.SparkEngineModelPath("CesiumMan.glb");
        avatar = ctx.assets.LoadSkinnedAvatar(world, cesiumPath);
    }

    if (avatar.mesh && avatar.skeleton) {
        result.useSkinnedAvatar = avatar.skeleton->GetClipCount() > 0;
        result.bindFix = avatar.bindUpAlignment.Normalized();
        result.bindYawOffset = usedFox ? -Spark::HalfPi : avatar.bindFacingYawOffset;
        result.rig.characterFacingYawOffset = result.bindYawOffset;
        result.rig.characterRootBindOrientation = result.bindFix;

        Spark::Vector3 alignedMin{};
        Spark::Vector3 alignedMax{};
        float minAlignY = 0.0F;
        bool haveBounds = false;
        const Spark::Array<Spark::SkinnedMesh::Vertex>& sv = avatar.mesh->GetVertices();
        if (!sv.IsEmpty()) {
            haveBounds = true;
            bool first = true;
            for (std::size_t vi = 0; vi < sv.GetSize(); ++vi) {
                const Spark::Vector3 p = result.bindFix.RotateVector(sv[vi].position);
                if (first) {
                    alignedMin = p;
                    alignedMax = p;
                    minAlignY = p.y;
                    first = false;
                } else {
                    alignedMin.x = std::min(alignedMin.x, p.x);
                    alignedMin.y = std::min(alignedMin.y, p.y);
                    alignedMin.z = std::min(alignedMin.z, p.z);
                    alignedMax.x = std::max(alignedMax.x, p.x);
                    alignedMax.y = std::max(alignedMax.y, p.y);
                    alignedMax.z = std::max(alignedMax.z, p.z);
                    minAlignY = std::min(minAlignY, p.y);
                }
            }
        }

        constexpr float kTargetHeightM = 1.75F;
        float scale = 0.04F;
        if (haveBounds) {
            const float h = std::max(1.0e-4F, alignedMax.y - alignedMin.y);
            scale = kTargetHeightM / h;
        }

        Spark::Vector3 visualLocalOffset{};
        if (haveBounds) {
            visualLocalOffset = {
                    -(alignedMin.x + alignedMax.x) * 0.5F * scale,
                    -minAlignY * scale,
                    -(alignedMin.z + alignedMax.z) * 0.5F * scale};
            TuneThirdPersonRigForHeight(result.rig, (alignedMax.y - alignedMin.y) * scale);
        } else {
            TuneThirdPersonRigForHeight(result.rig, kTargetHeightM);
        }

        Spark::GameObject* visual = world.CreateGameObject();
        visual->GetName() = Spark::Utf8String("PlayerVisual");
        visual->SetParent(root);
        Spark::TransformComponent* vtr = visual->AddComponent<Spark::TransformComponent>();
        vtr->SetUniformScale(scale);
        vtr->SetTranslation(visualLocalOffset);
        vtr->SetRotation(Spark::Quaternion::Identity);

        visual->AddComponent<Spark::SkinnedMeshComponent>(avatar.mesh);
        if (avatar.material.HasAnyTexture() || avatar.baseColorTexture) {
            if (Spark::MaterialComponent* mat = visual->AddComponent<Spark::MaterialComponent>()) {
                Spark::ApplyGltfMaterialDesc(*mat, avatar.material);
                mat->SetMetallic(0.05F);
                mat->SetRoughness(0.65F);
            }
        }

        Spark::Character3DAnimFsmComponent* fsm = visual->AddComponent<Spark::Character3DAnimFsmComponent>();
        Spark::AnimatorComponent* animator =
                visual->AddComponent<Spark::AnimatorComponent>(avatar.skeleton, avatar.walkClipIndex, 1.0F);
        if (result.useSkinnedAvatar) {
            fsm->ConfigureLocomotionFromSkeleton(*avatar.skeleton, avatar.walkClipIndex);
            fsm->SetWalkSpeedThreshold(0.35F);
            fsm->SetRunSpeedThreshold(6.0F);
        }

        result.visual = visual;
        result.animFsm = fsm;
        result.animator = animator;
    }

    Spark::GameObject* hud = world.CreateGameObject();
    hud->GetName() = Spark::Utf8String("Hud");
    Spark::TextOverlayComponent* hudText = hud->AddComponent<Spark::TextOverlayComponent>();
    hudText->SetScreenPosition(14.0F, 14.0F);
    hudText->SetFontSizePixels(20.0F);
    hudText->SetColor({0.92F, 0.94F, 1.0F});
    hudText->SetText(Spark::Utf8String("ARPG — WASD · Shift run · F attack · F1 mouse · ESC quit"));
    ownedRoots.PushBack(hud);

    result.hudText = hudText;
    result.heroLabel = usedFox ? Spark::Utf8String("Fox") : Spark::Utf8String("CesiumMan");
    result.root = root;
    result.controller = controller;
    ownedRoots.PushBack(root);

    return result;
}

void PlayerAvatar::Destroy(
        GameContext& ctx,
        PlayerAvatarResult& avatar,
        Spark::Array<Spark::GameObject*>& ownedRoots) {
    Spark::GameWorld& world = *ctx.world;
    if (avatar.hudText != nullptr) {
        if (Spark::GameObject* hud = avatar.hudText->GetOwner()) {
            world.DestroyGameObject(hud);
            RemoveFromOwnedRoots(ownedRoots, hud);
        }
    }
    if (avatar.root != nullptr) {
        world.DestroyGameObject(avatar.root);
        RemoveFromOwnedRoots(ownedRoots, avatar.root);
    }
    avatar = PlayerAvatarResult{};
}

}  // namespace Arpg
