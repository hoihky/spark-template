#include "player/DungeonRoomCamera.hpp"

#include <cmath>

namespace Arpg {

Spark::Vector3 CameraWorldPositionInRoom(
        const Spark::CharacterCameraRig& rig,
        const DungeonRoomBounds& room) noexcept {
    const Spark::Vector3 ideal = rig.CameraWorldPosition();
    if (rig.mode == Spark::CharacterCameraMode::FirstPerson) {
        return room.ConstrainCameraAlongPivot(rig.characterPosition, ideal, kRoomCameraMargin);
    }

    const Spark::Vector3 pivot{
            rig.characterPosition.x,
            rig.characterPosition.y + rig.thirdPersonPivotHeight,
            rig.characterPosition.z,
    };
    return room.ConstrainCameraAlongPivot(pivot, ideal, kRoomCameraMargin);
}

Spark::Matrix4 ViewMatrixInRoom(
        const Spark::CharacterCameraRig& rig,
        const DungeonRoomBounds& room) noexcept {
    if (rig.mode == Spark::CharacterCameraMode::FirstPerson) {
        const Spark::Vector3 eye = CameraWorldPositionInRoom(rig, room);
        const Spark::Vector3 f = rig.ForwardWorld();
        Spark::Vector3 worldUp = Spark::Vector3::UnitY;
        if (std::fabs(Spark::Vector3::Dot(f, worldUp)) > 0.92F) {
            worldUp = Spark::Vector3::UnitZ;
        }
        return Spark::Matrix4::LookAt(eye, eye + f, worldUp);
    }

    const Spark::Vector3 pivot{
            rig.characterPosition.x,
            rig.characterPosition.y + rig.thirdPersonPivotHeight,
            rig.characterPosition.z,
    };
    const float yaw = rig.characterVisualYaw + rig.characterFacingYawOffset;
    const Spark::Quaternion qYaw = Spark::Quaternion::FromAxisAngle(Spark::Vector3::UnitY, yaw);
    const Spark::Quaternion qTotal = (qYaw * rig.characterRootBindOrientation).Normalized();
    Spark::Vector3 f = qTotal * rig.characterLocalForward;
    const float hx = f.x;
    const float hz = f.z;
    const float hLen2 = hx * hx + hz * hz;
    Spark::Vector3 flatF{};
    if (hLen2 < 1.0e-12F) {
        const float sy = std::sin(yaw);
        const float cy = std::cos(yaw);
        flatF = {-sy, 0.0F, cy};
    } else {
        const float inv = 1.0F / std::sqrt(hLen2);
        flatF = {-hx * inv, 0.0F, -hz * inv};
    }

    const Spark::Vector3 focus{
            pivot.x + flatF.x * rig.thirdPersonFocusAhead,
            pivot.y + flatF.y * rig.thirdPersonFocusAhead,
            pivot.z + flatF.z * rig.thirdPersonFocusAhead,
    };
    const Spark::Vector3 cam = CameraWorldPositionInRoom(rig, room);
    Spark::Vector3 worldUp = Spark::Vector3::UnitY;
    const Spark::Vector3 toFocus = (focus - cam).Normalized();
    if (std::fabs(Spark::Vector3::Dot(toFocus, worldUp)) > 0.96F) {
        worldUp = Spark::Vector3::UnitZ;
    }
    return Spark::Matrix4::LookAt(cam, focus, worldUp);
}

void ClampCharacterInsideRoom(Spark::CharacterCameraRig& rig, const DungeonRoomBounds& room) noexcept {
    rig.characterPosition = room.ClampHorizontal(rig.characterPosition, kRoomCharacterMargin);
    rig.characterPosition.y = rig.groundY;
}

}  // namespace Arpg
