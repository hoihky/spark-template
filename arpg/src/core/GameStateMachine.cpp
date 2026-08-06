#include "core/GameStateMachine.hpp"

namespace Arpg {

void GameStateMachine::ChangeState(Spark::UniquePtr<IGameState> next, GameContext& ctx) {
    if (active_ != nullptr) {
        active_->OnExit(ctx);
        active_.Reset();
    }
    if (next != nullptr) {
        active_ = Spark::MoveTemp(next);
        active_->OnEnter(ctx);
    }
}

void GameStateMachine::Update(GameContext& ctx, const Spark::FrameTiming& timing) {
    if (active_ != nullptr) {
        active_->OnUpdate(ctx, timing);
    }
}

void GameStateMachine::Render(GameContext& ctx) {
    if (active_ != nullptr) {
        active_->OnRender(ctx);
    }
}

}  // namespace Arpg
