#include "player/PlayerController.hpp"

#include "spark/ecs/GameObject.hpp"
#include "spark/ecs/components/animation/Character3DAnimFsmComponent.hpp"
#include "spark/ecs/components/core/TransformComponent.hpp"
#include "spark/ecs/components/physics/3d/CharacterController3DComponent.hpp"
#include "spark/engine/IEngineContext.hpp"
#include "spark/engine/IInput.hpp"
#include "spark/physics/CharacterController3D.hpp"
#include "spark/render/platform/Window.hpp"

#include <GLFW/glfw3.h>

namespace Arpg {

void PlayerController::Update(
        PlayerAvatarResult& player,
        GameContext& ctx,
        const Spark::FrameTiming& timing) {
    Spark::IInput& in = ctx.engine->GetInput();
    Spark::CharacterCameraRig& rig = player.rig;

    if (in.IsKeyPressedThisFrame(GLFW_KEY_ESCAPE)) {
        glfwSetWindowShouldClose(ctx.engine->GetWindow().Handle(), GLFW_TRUE);
    }
    if (in.IsKeyPressedThisFrame(GLFW_KEY_F1)) {
        in.SetCursorCaptured(!in.IsCursorCaptured());
    }

    if (in.IsCursorCaptured() && timing.frameIndex > 0U) {
        rig.AddLook(in.GetMouseDeltaX(), in.GetMouseDeltaY());
    }

    const Spark::Vector3 posBeforeWalk = rig.characterPosition;
    rig.ProcessWalk(in, timing.deltaTimeSeconds);
    Spark::Vector3 wishVel = Spark::Vector3::Zero;
    if (timing.deltaTimeSeconds > 1.0e-6F) {
        wishVel = (rig.characterPosition - posBeforeWalk) * (1.0F / timing.deltaTimeSeconds);
        wishVel.y = 0.0F;
    }
    rig.characterPosition = posBeforeWalk;

    Spark::TransformComponent* rootTr =
            player.root != nullptr ? player.root->GetComponent<Spark::TransformComponent>() : nullptr;

    if (player.controller != nullptr && rootTr != nullptr) {
        rootTr->SetTranslation(rig.characterPosition);
        player.controller->SetMoveInput(wishVel);
        Spark::CharacterController3DSettings ccSettings{};
        ccSettings.gravityY = 0.0F;
        ccSettings.maxFallSpeed = 0.0F;
        ccSettings.broadPhaseCellSize = 4.0F;
        Spark::SimulateCharacterControllers3D(*ctx.world, timing, ccSettings);
        const Spark::Vector3 resolved = rootTr->GetLocalTransform().translation;
        rig.characterPosition.x = resolved.x;
        rig.characterPosition.y = rig.groundY;
        rig.characterPosition.z = resolved.z;
    } else if (timing.deltaTimeSeconds > 1.0e-6F) {
        rig.characterPosition.x = posBeforeWalk.x + wishVel.x * timing.deltaTimeSeconds;
        rig.characterPosition.z = posBeforeWalk.z + wishVel.z * timing.deltaTimeSeconds;
        rig.characterPosition.y = rig.groundY;
    }

    const bool moving =
            in.IsKeyDown(GLFW_KEY_W) || in.IsKeyDown(GLFW_KEY_S) || in.IsKeyDown(GLFW_KEY_A) || in.IsKeyDown(GLFW_KEY_D);
    const bool sprint =
            moving && (in.IsKeyDown(GLFW_KEY_LEFT_SHIFT) || in.IsKeyDown(GLFW_KEY_RIGHT_SHIFT));

    if (player.useSkinnedAvatar && player.animFsm != nullptr) {
        if (moving) {
            player.animFsm->ClearManualClip();
        }
        player.animFsm->SetLocomotionInput(moving, sprint);
    }

    if (in.IsKeyPressedThisFrame(GLFW_KEY_F) && player.animFsm != nullptr) {
        player.animFsm->RequestAttack();
    }

    if (rootTr != nullptr) {
        rootTr->SetTranslation(rig.characterPosition);
        const Spark::Quaternion qYaw =
                Spark::Quaternion::FromAxisAngle(Spark::Vector3::UnitY, rig.characterVisualYaw + player.bindYawOffset);
        rootTr->SetRotation(player.useSkinnedAvatar ? (qYaw * player.bindFix).Normalized() : qYaw);
    }
}

}  // namespace Arpg
