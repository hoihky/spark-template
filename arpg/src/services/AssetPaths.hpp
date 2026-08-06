#pragma once

#include "spark/core/Utf8String.hpp"

namespace Arpg {

/**
 * Resolves filesystem paths for game and engine assets.
 * Game content is copied to <executable>/assets at build time; engine models use SPARK_ROOT at compile time.
 */
class AssetPaths {
public:
    AssetPaths();

    [[nodiscard]] const Spark::Utf8String& GetGameAssetsRoot() const noexcept { return gameAssetsRoot_; }

    /** e.g. "floor_tile_large.gltf" → absolute or relative path under game assets/models/. */
    [[nodiscard]] Spark::Utf8String ModelPath(const char* fileName) const;

    /** Spark engine bundled models (CesiumMan, Fox, etc.) — path depends on engine install. */
    [[nodiscard]] Spark::Utf8String SparkEngineModelPath(const char* fileName) const;

private:
    Spark::Utf8String gameAssetsRoot_;
    Spark::Utf8String sparkAssetsRoot_;
};

}  // namespace Arpg
