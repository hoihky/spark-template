#pragma once

#include "core/IGameState.hpp"
#include "player/PlayerAvatar.hpp"
#include "player/PlayerController.hpp"
#include "spark/core/Array.hpp"

namespace Arpg {

/** Active gameplay: dungeon level, third-person player, combat-ready animation FSM. */
class PlayState final : public IGameState {
public:
    void OnEnter(GameContext& ctx) override;
    void OnExit(GameContext& ctx) override;
    void OnUpdate(GameContext& ctx, const Spark::FrameTiming& timing) override;
    void OnRender(GameContext& ctx) override;

private:
    void UpdateHud(const Spark::FrameTiming& timing);

    Spark::Array<Spark::GameObject*> levelRoots{};
    Spark::GameObject* skyRoot_ = nullptr;
    PlayerAvatarResult player{};
    PlayerController playerController{};
    float fpsSmoothed = 0.0F;
};

}  // namespace Arpg
