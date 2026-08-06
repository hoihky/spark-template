#include "game/ArpgGame.hpp"

#include "states/PlayState.hpp"
#include "spark/audio/SoundSubsystem.hpp"

namespace Arpg {

void ArpgGame::OnAttach(Spark::IEngineContext& context) {
    context_.world = &GetWorld();
    context_.engine = &context;
    Spark::UniquePtr<IGameState> playState(Spark::MakeUnique<PlayState>().Release());
    stateMachine_.ChangeState(Spark::MoveTemp(playState), context_);
}

void ArpgGame::OnDetach() {
    stateMachine_.ChangeState(Spark::UniquePtr<IGameState>{}, context_);
    context_.world = nullptr;
    context_.engine = nullptr;
}

void ArpgGame::OnUpdate(const Spark::FrameTiming& timing, Spark::IEngineContext& context) {
    stateMachine_.Update(context_, timing);
    Spark::ProcessSoundCues(GetWorld(), context);
}

void ArpgGame::OnRender(Spark::IRenderFrame& /*frame*/, Spark::IEngineContext& /*context*/) {
    stateMachine_.Render(context_);
}

}  // namespace Arpg
