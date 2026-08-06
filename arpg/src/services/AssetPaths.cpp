#include "services/AssetPaths.hpp"

#include "spark/config.hpp"

#include <filesystem>
#include <vector>

#include <cstdint>

#if defined(_WIN32)
#include <windows.h>
#elif defined(__APPLE__)
#include <mach-o/dyld.h>
#include <climits>
#include <unistd.h>
#elif defined(__linux__)
#include <climits>
#include <unistd.h>
#endif

namespace Arpg {

namespace {

[[nodiscard]] Spark::Utf8String PathToUtf8(const std::filesystem::path& p) {
    const std::string s = p.string();
    return Spark::Utf8String(s.c_str());
}

[[nodiscard]] bool DirectoryExists(const std::filesystem::path& p) {
    return std::filesystem::exists(p) && std::filesystem::is_directory(p);
}

[[nodiscard]] std::filesystem::path ExecutableDirectory() {
#if defined(_WIN32)
    char buf[MAX_PATH];
    const DWORD n = GetModuleFileNameA(nullptr, buf, MAX_PATH);
    if (n > 0 && n < MAX_PATH) {
        return std::filesystem::path(buf).parent_path();
    }
#elif defined(__APPLE__)
    char buf[PATH_MAX];
    std::uint32_t size = static_cast<std::uint32_t>(sizeof(buf));
    if (_NSGetExecutablePath(buf, &size) == 0) {
        char resolved[PATH_MAX];
        if (realpath(buf, resolved) != nullptr) {
            return std::filesystem::path(resolved).parent_path();
        }
        return std::filesystem::path(buf).parent_path();
    }
#elif defined(__linux__)
    char buf[PATH_MAX];
    const ssize_t n = readlink("/proc/self/exe", buf, sizeof(buf) - 1);
    if (n > 0) {
        buf[n] = '\0';
        return std::filesystem::path(buf).parent_path();
    }
#endif
    return std::filesystem::current_path();
}

/** Kenney glTF props need sibling .bin buffers; gltf alone is not enough to spawn meshes. */
[[nodiscard]] bool HasCompleteDungeonMeshes(const std::filesystem::path& assetsRoot) {
    const std::filesystem::path modelsDir = assetsRoot / "models";
    const std::filesystem::path gltf = modelsDir / "floor_tile_large.gltf";
    const std::filesystem::path bin = modelsDir / "floor_tile_large.bin";
    return std::filesystem::exists(gltf) && std::filesystem::exists(bin);
}

[[nodiscard]] Spark::Utf8String ResolveGameAssetsRoot() {
    std::vector<std::filesystem::path> candidates;
    candidates.push_back(ExecutableDirectory() / "assets");
    candidates.push_back(std::filesystem::path("assets"));
#ifdef ARPG_ASSETS_ROOT
    candidates.push_back(std::filesystem::path(ARPG_ASSETS_ROOT));
#endif

    for (const std::filesystem::path& candidate : candidates) {
        if (HasCompleteDungeonMeshes(candidate)) {
            return PathToUtf8(candidate);
        }
    }

    for (const std::filesystem::path& candidate : candidates) {
        if (DirectoryExists(candidate)) {
            return PathToUtf8(candidate);
        }
    }

    return Spark::Utf8String("assets");
}

}  // namespace

AssetPaths::AssetPaths() {
    gameAssetsRoot_ = ResolveGameAssetsRoot();

    sparkAssetsRoot_ = Spark::Utf8String("../../spark/assets");
    if (!DirectoryExists(std::filesystem::path(sparkAssetsRoot_.CStr()))) {
        sparkAssetsRoot_ = Spark::Utf8String("../spark/assets");
    }
}

Spark::Utf8String AssetPaths::ModelPath(const char* fileName) const {
    Spark::Utf8String out(gameAssetsRoot_);
    out.AppendUtf8("/models/");
    out.AppendUtf8(fileName);
    return out;
}

Spark::Utf8String AssetPaths::SparkEngineModelPath(const char* fileName) const {
    Spark::Utf8String out(SPARK_ASSETS_DIR);
    out.AppendUtf8("/models/");
    out.AppendUtf8(fileName);
    if (std::filesystem::exists(std::filesystem::path(out.CStr()))) {
        return out;
    }
    Spark::Utf8String fallback(sparkAssetsRoot_);
    fallback.AppendUtf8("/models/");
    fallback.AppendUtf8(fileName);
    return fallback;
}

}  // namespace Arpg
