#include "game/ArpgGame.hpp"

#include "states/PlayState.hpp"
#include "spark/audio/SoundSubsystem.hpp"

namespace Arpg {

void ArpgGame::OnAttach(Spark::IEngineContext& ctx) {
    context.world = &GetWorld();
    context.engine = &ctx;
    Spark::UniquePtr<IGameState> playState(Spark::MakeUnique<PlayState>().Release());
    stateMachine.ChangeState(Spark::MoveTemp(playState), context);
}

void ArpgGame::OnDetach() {
    stateMachine.ChangeState(Spark::UniquePtr<IGameState>{}, context);
    context.world = nullptr;
    context.engine = nullptr;
}

void ArpgGame::OnUpdate(const Spark::FrameTiming& timing, Spark::IEngineContext& ctx) {
    stateMachine.Update(context, timing);
    Spark::ProcessSoundCues(GetWorld(), ctx);
}

void ArpgGame::OnRender(Spark::IRenderFrame& /*frame*/, Spark::IEngineContext& /*context*/) {
    stateMachine.Render(context);
}

}  // namespace Arpg
