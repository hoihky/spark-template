#pragma once

#include "core/IGameState.hpp"
#include "spark/engine/FrameTiming.hpp"
#include "spark/memory/UniquePtr.hpp"

namespace Arpg {

/**
 * State pattern coordinator: owns the active IGameState and handles transitions.
 */
class GameStateMachine {
public:
    void ChangeState(Spark::UniquePtr<IGameState> next, GameContext& ctx);
    void Update(GameContext& ctx, const Spark::FrameTiming& timing);
    void Render(GameContext& ctx);

private:
    Spark::UniquePtr<IGameState> active;
};

}  // namespace Arpg
