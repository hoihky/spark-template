#pragma once

#include "core/GameContext.hpp"
#include "spark/core/Array.hpp"
#include "spark/math/Quaternion.hpp"
#include "spark/core/Utf8String.hpp"
#include "spark/math/Vector3.hpp"
#include "spark/scene/camera/CharacterCameraRig.hpp"

namespace Spark {
class GameObject;
class CharacterController3DComponent;
class Character3DAnimFsmComponent;
class AnimatorComponent;
class TextOverlayComponent;
}

namespace Arpg {

struct PlayerAvatarResult {
    Spark::GameObject* root = nullptr;
    Spark::GameObject* visual = nullptr;
    Spark::CharacterController3DComponent* controller = nullptr;
    Spark::Character3DAnimFsmComponent* animFsm = nullptr;
    Spark::AnimatorComponent* animator = nullptr;
    Spark::TextOverlayComponent* hudText = nullptr;
    Spark::CharacterCameraRig rig{};
    Spark::Quaternion bindFix{Spark::Quaternion::Identity};
    Spark::Utf8String heroLabel;
    float bindYawOffset = 0.0F;
    bool useSkinnedAvatar = false;
};

/**
 * Factory for the player entity: character controller root + skinned visual child.
 * Default hero: Spark engine Khronos Fox.glb (mesh + locomotion clips).
 */
class PlayerAvatar {
public:
    static constexpr const char* kSparkFoxModelFile = "Fox.glb";
    [[nodiscard]] PlayerAvatarResult Spawn(
            GameContext& ctx,
            Spark::Array<Spark::GameObject*>& ownedRoots,
            const Spark::Vector3& spawnPosition);

    void Destroy(GameContext& ctx, PlayerAvatarResult& avatar, Spark::Array<Spark::GameObject*>& ownedRoots);
};

}  // namespace Arpg
