#include "core/GameStateMachine.hpp"

namespace Arpg {

void GameStateMachine::ChangeState(Spark::UniquePtr<IGameState> next, GameContext& ctx) {
    if (active != nullptr) {
        active->OnExit(ctx);
        active.Reset();
    }
    if (next != nullptr) {
        active = Spark::MoveTemp(next);
        active->OnEnter(ctx);
    }
}

void GameStateMachine::Update(GameContext& ctx, const Spark::FrameTiming& timing) {
    if (active != nullptr) {
        active->OnUpdate(ctx, timing);
    }
}

void GameStateMachine::Render(GameContext& ctx) {
    if (active != nullptr) {
        active->OnRender(ctx);
    }
}

}  // namespace Arpg
