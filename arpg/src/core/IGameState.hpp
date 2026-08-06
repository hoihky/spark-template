#pragma once

#include "core/GameContext.hpp"
#include "spark/engine/FrameTiming.hpp"

namespace Arpg {

/** State pattern: each high-level mode (play, menu, pause) implements this interface. */
class IGameState {
public:
    virtual ~IGameState() = default;

    virtual void OnEnter(GameContext& ctx) = 0;
    virtual void OnExit(GameContext& ctx) = 0;
    virtual void OnUpdate(GameContext& ctx, const Spark::FrameTiming& timing) = 0;
    virtual void OnRender(GameContext& ctx) = 0;
};

}  // namespace Arpg
