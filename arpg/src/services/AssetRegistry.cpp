#include "services/AssetRegistry.hpp"

#include "services/AssetPaths.hpp"
#include "spark/scene/core/GameWorld.hpp"
#include "spark/scene/texture/Texture2D.hpp"

namespace Arpg {

namespace {

constexpr const char* kDungeonAtlasKey = "arpg/dungeon_atlas";

}  // namespace

void AssetRegistry::PrimeSharedTextures(Spark::GameWorld& world, const AssetPaths& paths) {
    if (dungeonAtlas_) {
        return;
    }
    const Spark::Utf8String atlasPath = paths.ModelPath("dungeon_texture.png");
    Spark::SharedPtr<Spark::Texture2D> tex = world.LoadTexture(atlasPath.CStr());
    if (!tex) {
        Spark::Texture2D decoded;
        if (Spark::Texture2D::TryLoadFromFile(atlasPath.CStr(), decoded)) {
            tex = Spark::MakeShared<Spark::Texture2D>(Spark::MoveTemp(decoded));
        }
    }
    if (tex) {
        tex->GetName() = Spark::Utf8String("DungeonAtlas");
        world.RegisterTexture(tex, kDungeonAtlasKey);
        dungeonAtlas_ = tex;
    }
}

void AssetRegistry::ApplySharedPropTexture(Spark::GltfAsset& asset) const {
    if (!dungeonAtlas_) {
        return;
    }
    if (asset.baseColorTexture) {
        asset.baseColorTexture = dungeonAtlas_;
    }
    if (asset.material.baseColor) {
        asset.material.baseColor = dungeonAtlas_;
    }
    for (std::size_t i = 0; i < asset.materials.GetSize(); ++i) {
        if (asset.materials[i].baseColor) {
            asset.materials[i].baseColor = dungeonAtlas_;
        }
    }
}

Spark::GltfAsset AssetRegistry::LoadProp(Spark::GameWorld& world, const Spark::Utf8String& path) {
    if (Spark::GltfAsset* found = propCache_.Find(path)) {
        Spark::GltfAsset asset = *found;
        ApplySharedPropTexture(asset);
        return asset;
    }
    Spark::GltfAsset asset = world.LoadGltf(path.CStr());
    ApplySharedPropTexture(asset);
    propCache_.Add(path, asset);
    return asset;
}

Spark::SkinnedGltfAsset AssetRegistry::LoadSkinnedAvatar(Spark::GameWorld& world, const Spark::Utf8String& path) {
    if (Spark::SkinnedGltfAsset* found = skinnedCache_.Find(path)) {
        return *found;
    }
    Spark::SkinnedGltfAsset asset = world.LoadSkinnedGltf(path.CStr());
    if (asset.baseColorTexture) {
        world.RegisterTexture(asset.baseColorTexture, path.CStr());
    }
    skinnedCache_.Add(path, asset);
    return asset;
}

}  // namespace Arpg
