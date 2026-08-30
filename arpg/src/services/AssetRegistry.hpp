#pragma once

#include "spark/core/HashMap.hpp"
#include "spark/core/Utf8String.hpp"
#include "spark/memory/SharedPtr.hpp"
#include "spark/scene/assets/GameWorldAssetCache.hpp"

namespace Spark {
class GameWorld;
class Texture2D;
}

namespace Arpg {

class AssetPaths;

/**
 * Factory + cache for glTF props and skinned avatars (avoids duplicate disk I/O).
 * Kenney dungeon props share one atlas texture instance to stay within Spark's scene texture budget.
 */
class AssetRegistry {
public:
    void PrimeSharedTextures(Spark::GameWorld& world, const AssetPaths& paths);

    [[nodiscard]] Spark::GltfAsset LoadProp(Spark::GameWorld& world, const Spark::Utf8String& path);
    [[nodiscard]] Spark::SkinnedGltfAsset LoadSkinnedAvatar(Spark::GameWorld& world, const Spark::Utf8String& path);

private:
    void ApplySharedPropTexture(Spark::GltfAsset& asset) const;

    Spark::SharedPtr<Spark::Texture2D> dungeonAtlas_;
    Spark::HashMap<Spark::Utf8String, Spark::GltfAsset, Spark::Detail::Utf8StringHasher> propCache_;
    Spark::HashMap<Spark::Utf8String, Spark::SkinnedGltfAsset, Spark::Detail::Utf8StringHasher> skinnedCache_;
};

}  // namespace Arpg
