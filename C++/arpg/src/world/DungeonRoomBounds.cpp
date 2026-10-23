#include "world/DungeonRoomBounds.hpp"

#include <algorithm>
#include <cmath>

namespace Arpg {

DungeonRoomBounds DungeonRoomBounds::ForGrid(
        const int gridWidth,
        const int gridDepth,
        const float cellSize,
        const float groundY) noexcept {
    const float originX = -static_cast<float>(gridWidth) * cellSize * 0.5F + cellSize * 0.5F;
    const float originZ = -static_cast<float>(gridDepth) * cellSize * 0.5F + cellSize * 0.5F;
    const float minFloorX = originX;
    const float maxFloorX = originX + static_cast<float>(gridWidth - 1) * cellSize;
    const float minFloorZ = originZ;
    const float maxFloorZ = originZ + static_cast<float>(gridDepth - 1) * cellSize;

    const float wallCenterInset = cellSize * 0.5F;
    constexpr float kWallColliderHalf = 0.95F;

    DungeonRoomBounds b{};
    b.minX = minFloorX - wallCenterInset + kWallColliderHalf;
    b.maxX = maxFloorX + wallCenterInset - kWallColliderHalf;
    b.minZ = minFloorZ - wallCenterInset + kWallColliderHalf;
    b.maxZ = maxFloorZ + wallCenterInset - kWallColliderHalf;
    b.minY = groundY + 0.3F;
    b.maxY = groundY + 6.5F;
    return b;
}

Spark::Vector3 DungeonRoomBounds::ClampHorizontal(const Spark::Vector3& position, const float margin) const noexcept {
    return {
            std::clamp(position.x, minX + margin, maxX - margin),
            position.y,
            std::clamp(position.z, minZ + margin, maxZ - margin),
    };
}

bool DungeonRoomBounds::Contains(const Spark::Vector3& p, const float margin) const noexcept {
    return p.x >= minX + margin && p.x <= maxX - margin && p.y >= minY + margin && p.y <= maxY - margin &&
           p.z >= minZ + margin && p.z <= maxZ - margin;
}

Spark::Vector3 DungeonRoomBounds::ClampPoint(const Spark::Vector3& p, const float margin) const noexcept {
    return {
            std::clamp(p.x, minX + margin, maxX - margin),
            std::clamp(p.y, minY + margin, maxY - margin),
            std::clamp(p.z, minZ + margin, maxZ - margin),
    };
}

Spark::Vector3 DungeonRoomBounds::ConstrainCameraAlongPivot(
        const Spark::Vector3& pivot,
        const Spark::Vector3& idealCamera,
        const float margin) const noexcept {
    if (Contains(idealCamera, margin)) {
        return idealCamera;
    }

    const Spark::Vector3 delta{
            idealCamera.x - pivot.x,
            idealCamera.y - pivot.y,
            idealCamera.z - pivot.z,
    };

    float lo = 0.0F;
    float hi = 1.0F;
    for (int i = 0; i < 20; ++i) {
        const float mid = (lo + hi) * 0.5F;
        const Spark::Vector3 sample{
                pivot.x + delta.x * mid,
                pivot.y + delta.y * mid,
                pivot.z + delta.z * mid,
        };
        if (Contains(sample, margin)) {
            lo = mid;
        } else {
            hi = mid;
        }
    }

    const Spark::Vector3 onRay{
            pivot.x + delta.x * lo,
            pivot.y + delta.y * lo,
            pivot.z + delta.z * lo,
    };
    return ClampPoint(onRay, margin);
}

}  // namespace Arpg
