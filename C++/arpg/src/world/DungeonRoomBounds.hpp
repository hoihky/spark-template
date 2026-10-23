#pragma once

#include "spark/math/Vector3.hpp"

namespace Arpg {

/** Axis-aligned interior of the Kenney dungeon room (inside perimeter wall colliders). */
struct DungeonRoomBounds {
    float minX = 0.0F;
    float maxX = 0.0F;
    float minZ = 0.0F;
    float maxZ = 0.0F;
    float minY = 0.0F;
    float maxY = 0.0F;

    [[nodiscard]] static DungeonRoomBounds ForGrid(
            const int gridWidth,
            const int gridDepth,
            const float cellSize,
            const float groundY) noexcept;

    [[nodiscard]] Spark::Vector3 ClampHorizontal(const Spark::Vector3& position, const float margin) const noexcept;

    [[nodiscard]] Spark::Vector3 ConstrainCameraAlongPivot(
            const Spark::Vector3& pivot,
            const Spark::Vector3& idealCamera,
            const float margin) const noexcept;

private:
    [[nodiscard]] bool Contains(const Spark::Vector3& p, const float margin) const noexcept;
    [[nodiscard]] Spark::Vector3 ClampPoint(const Spark::Vector3& p, const float margin) const noexcept;
};

}  // namespace Arpg
