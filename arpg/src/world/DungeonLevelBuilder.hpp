#pragma once

#include "core/GameContext.hpp"
#include "spark/core/Array.hpp"
#include "spark/math/Vector3.hpp"

namespace Spark {
class GameObject;
}

namespace Arpg {

struct DungeonBuildResult {
    Spark::Array<Spark::GameObject*> roots;
    Spark::Vector3 playerSpawn{0.0F, 0.0F, 0.0F};
};

/**
 * Factory that assembles a modular dungeon room from Kenney glTF props in assets/models/.
 * Walls receive static box colliders for the character controller.
 */
class DungeonLevelBuilder {
public:
    [[nodiscard]] DungeonBuildResult Build(GameContext& ctx);

private:
    Spark::GameObject* SpawnProp(
            GameContext& ctx,
            Spark::Array<Spark::GameObject*>& roots,
            const char* modelFile,
            const Spark::Vector3& translation,
            float yawRadians,
            float uniformScale,
            bool addWallCollider,
            const char* debugName);

    Spark::GameObject* SpawnFloorTile(
            GameContext& ctx,
            Spark::Array<Spark::GameObject*>& roots,
            int gridX,
            int gridZ);

    void SpawnPerimeterWalls(
            GameContext& ctx,
            Spark::Array<Spark::GameObject*>& roots,
            const Spark::Vector3& corner00,
            const Spark::Vector3& center);

    void SpawnPlazaProps(
            GameContext& ctx,
            Spark::Array<Spark::GameObject*>& roots,
            const Spark::Vector3& center);
};

}  // namespace Arpg
