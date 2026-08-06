#pragma once

#include "core/GameContext.hpp"
#include "core/GameStateMachine.hpp"
#include "spark/engine/Game.hpp"

namespace Arpg {

/**
 * Root game class: wires Spark::Game lifecycle to the internal state machine.
 * Keeps engine-facing code thin so gameplay lives in states and services.
 */
class ArpgGame final : public Spark::Game {
public:
    void OnAttach(Spark::IEngineContext& context) override;
    void OnDetach() override;
    void OnUpdate(const Spark::FrameTiming& timing, Spark::IEngineContext& context) override;
    void OnRender(Spark::IRenderFrame& frame, Spark::IEngineContext& context) override;

private:
    GameContext context_;
    GameStateMachine stateMachine_;
};

}  // namespace Arpg
