#pragma once

#include "core/GameContext.hpp"
#include "player/PlayerAvatar.hpp"
#include "spark/engine/FrameTiming.hpp"

namespace Arpg {

/** Third-person movement, character-controller simulation, and facing for the active player. */
class PlayerController {
public:
    void Update(PlayerAvatarResult& player, GameContext& ctx, const Spark::FrameTiming& timing);
};

}  // namespace Arpg
