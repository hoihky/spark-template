#pragma once

#include "services/AssetPaths.hpp"
#include "services/AssetRegistry.hpp"

namespace Spark {
class GameWorld;
class IEngineContext;
}

namespace Arpg {

/**
 * Service locator for per-session game services (world pointer, asset loading, timing).
 * Passed to states and builders so they do not depend on ArpgGame directly.
 */
struct GameContext {
    Spark::GameWorld* world = nullptr;
    Spark::IEngineContext* engine = nullptr;
    AssetPaths paths;
    AssetRegistry assets;
    float sceneTimeSeconds = 0.0F;
};

}  // namespace Arpg
