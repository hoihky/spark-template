#!/usr/bin/env bash
set -euo pipefail

TEMPLATE_ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
SHARP_ROOT="${SPARK_SHARP_ROOT:-$TEMPLATE_ROOT/../spark-sharp}"
BUILD_DIR="${SPARK_SHARP_BUILD_DIR:-$SHARP_ROOT/build}"
# Debug SparkEngine enables Vulkan validation layers and is typically ~15–20 FPS on this scene.
CONFIG="${1:-Release}"

CACHE="$BUILD_DIR/CMakeCache.txt"
if [[ -f "$CACHE" ]] && grep -q 'CMAKE_BUILD_TYPE:STRING=Debug' "$CACHE"; then
  echo "note: spark-sharp native build is Debug (Vulkan validation hurts FPS)." >&2
  echo "      For ~60 FPS: cmake -S \"$SHARP_ROOT\" -B \"$BUILD_DIR\" -DCMAKE_BUILD_TYPE=Release -DSPARK_SHARP_BUILD_SCRIPT_HOST=ON && cmake --build \"$BUILD_DIR\" --target SparkScriptHost" >&2
fi

dotnet build "$TEMPLATE_ROOT/C#/arpg/ArpgCsGame.csproj" -c "$CONFIG" -p:NuGetAudit=false

GAME_DIR="$TEMPLATE_ROOT/C#/arpg/bin/$CONFIG/net8.0"
# SparkScriptHost defaults to HelloCsGame.GameEntry — pass ARPG entry explicitly (argv 3–4).
exec "$SHARP_ROOT/tools/run-spark-script-host.sh" \
  "$BUILD_DIR" \
  "$CONFIG" \
  "$GAME_DIR/ArpgCsGame.runtimeconfig.json" \
  "$GAME_DIR/ArpgCsGame.dll" \
  "SparkArpg.GameEntry, ArpgCsGame" \
  "Initialize"
