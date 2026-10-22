#pragma once

namespace Spark {
class GameWorld;
}

namespace Arpg {

/** Loads Roboto into <c>GameWorld</c> for <c>TextOverlayComponent</c> when not already set. */
void MountUiFontIfNeeded(Spark::GameWorld& world);

}  // namespace Arpg
