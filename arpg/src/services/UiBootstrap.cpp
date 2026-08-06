#include "services/UiBootstrap.hpp"

#include "spark/config.hpp"
#include "spark/scene/GameWorld.hpp"
#include "spark/text/Font.hpp"

namespace Arpg {

void MountUiFontIfNeeded(Spark::GameWorld& world) {
    if (world.GetUiFont()) {
        return;
    }
    auto uiFont = Spark::MakeShared<Spark::Font>();
    constexpr float kUiFontEmPx = 42.0F;
    bool fontOk = uiFont->TryLoadTrueTypeFromFile(SPARK_UI_FONT_PATH, kUiFontEmPx);
    if (!fontOk) {
        Spark::Utf8String buildTree(SPARK_BUILD_ASSETS_DIR);
        buildTree.AppendUtf8("/fonts/Roboto-Regular.ttf");
        fontOk = uiFont->TryLoadTrueTypeFromFile(buildTree.CStr(), kUiFontEmPx);
    }
    if (!fontOk) {
        Spark::Utf8String srcTree(SPARK_ASSETS_DIR);
        srcTree.AppendUtf8("/fonts/Roboto-Regular.ttf");
        fontOk = uiFont->TryLoadTrueTypeFromFile(srcTree.CStr(), kUiFontEmPx);
    }
    if (fontOk) {
        world.SetUiFont(uiFont);
    }
}

}  // namespace Arpg
