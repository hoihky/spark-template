#pragma once

#include "world/DungeonRoomBounds.hpp"
#include "spark/scene/camera/CharacterCameraRig.hpp"

namespace Arpg {

constexpr float kRoomCameraMargin = 0.35F;
constexpr float kRoomCharacterMargin = 0.5F;

[[nodiscard]] Spark::Vector3 CameraWorldPositionInRoom(
        const Spark::CharacterCameraRig& rig,
        const DungeonRoomBounds& room) noexcept;

[[nodiscard]] Spark::Matrix4 ViewMatrixInRoom(
        const Spark::CharacterCameraRig& rig,
        const DungeonRoomBounds& room) noexcept;

void ClampCharacterInsideRoom(Spark::CharacterCameraRig& rig, const DungeonRoomBounds& room) noexcept;

}  // namespace Arpg
