#include "states/PlayState.hpp"

#include "services/UiBootstrap.hpp"
#include "world/DungeonLevelBuilder.hpp"
#include "spark/engine/IEngineContext.hpp"
#include "spark/engine/IInput.hpp"
#include "spark/ecs/components/rendering/TextOverlayComponent.hpp"
#include "spark/render/platform/Window.hpp"
#include "spark/scene/SceneSubmit.hpp"
#include "spark/scene/GameWorld.hpp"

#include <GLFW/glfw3.h>
#include <format>

namespace Arpg {

void PlayState::OnEnter(GameContext& ctx) {
    MountUiFontIfNeeded(*ctx.world);
    ctx.engine->GetInput().SetCursorCaptured(true);

    DungeonLevelBuilder levelBuilder;
    const DungeonBuildResult level = levelBuilder.Build(ctx);
    for (std::size_t i = 0; i < level.roots.GetSize(); ++i) {
        levelRoots.PushBack(level.roots[i]);
    }

    PlayerAvatar avatarFactory;
    player = avatarFactory.Spawn(ctx, levelRoots, level.playerSpawn);

    glfwSetWindowTitle(ctx.engine->GetWindow().Handle(), "Spark ARPG template");
}

void PlayState::OnExit(GameContext& ctx) {
    PlayerAvatar avatarFactory;
    avatarFactory.Destroy(ctx, player, levelRoots);

    Spark::GameWorld& world = *ctx.world;
    for (std::size_t i = 0; i < levelRoots.GetSize(); ++i) {
        if (levelRoots[i] != nullptr) {
            world.DestroyGameObject(levelRoots[i]);
        }
    }
    levelRoots.Clear();
}

void PlayState::UpdateHud(const Spark::FrameTiming& timing) {
    if (player.hudText == nullptr) {
        return;
    }
    const float dt = timing.deltaTimeSeconds;
    const float instant = (dt > 1.0e-6F) ? (1.0F / dt) : 0.0F;
    if (timing.frameIndex < 2U) {
        fpsSmoothed = instant;
    } else {
        fpsSmoothed = fpsSmoothed * 0.88F + instant * 0.12F;
    }

    const char* heroLabel = player.heroLabel.IsEmpty() ? "hero" : player.heroLabel.CStr();
    player.hudText->SetText(Spark::Utf8String(
            std::format(
                    "ARPG — {:.0f} FPS — 3rd person · {} — WASD · Shift run · F attack · F1 mouse · ESC quit",
                    static_cast<double>(fpsSmoothed),
                    heroLabel)
                    .c_str()));
}

void PlayState::OnUpdate(GameContext& ctx, const Spark::FrameTiming& timing) {
    ctx.sceneTimeSeconds = timing.totalTimeSeconds;
    playerController.Update(player, ctx, timing);
    UpdateHud(timing);
    ctx.world->UpdateGameObjects(timing, *ctx.engine);
}

void PlayState::OnRender(GameContext& ctx) {
    Spark::IEngineContext& engine = *ctx.engine;
    int fbW = 0;
    int fbH = 0;
    engine.GetFramebufferSize(fbW, fbH);
    const float aspect = (fbH > 0) ? static_cast<float>(fbW) / static_cast<float>(fbH) : 1.0F;

    const Spark::Matrix4 proj =
            Spark::Matrix4::PerspectiveVulkan(Spark::DegreesToRadians(70.0F), aspect, 0.1F, 200.0F);
    const Spark::Matrix4 view = player.rig.ViewMatrix();
    const Spark::Matrix4 viewProj = proj * view;
    const Spark::Vector3 camPos = player.rig.CameraWorldPosition();

    Spark::Vector3 pr = Spark::Vector3::Cross(Spark::Vector3{0.0F, 1.0F, 0.0F}, player.rig.ForwardWorld()).Normalized();
    if (pr.LengthSquared() < 1.0e-8F) {
        pr = Spark::Vector3::Cross(Spark::Vector3{1.0F, 0.0F, 0.0F}, player.rig.ForwardWorld()).Normalized();
    }
    const Spark::Vector3 pu = Spark::Vector3::Cross(player.rig.ForwardWorld(), pr).Normalized();

    Spark::SubmitStandardLitSceneFromWorld(
            *ctx.world,
            engine,
            viewProj,
            camPos,
            Spark::Vector3{0.35F, 0.82F, 0.28F}.Normalized(),
            Spark::Vector3{1.0F, 0.96F, 0.90F},
            1.1F,
            Spark::Vector3{0.12F, 0.13F, 0.16F},
            false,
            pr,
            pu,
            ctx.sceneTimeSeconds,
            Spark::SceneSpriteSortMode::SortOrderOnly);
}

}  // namespace Arpg
