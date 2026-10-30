#!/usr/bin/env bash
# Run a C# Spark demo via SparkScriptHost.
# Usage: ./C#/tools/run-cs-demo.sh <DemoFolderName> [Debug|Release]
# Example: ./C#/tools/run-cs-demo.sh Basic3D
set -euo pipefail

if [[ $# -lt 1 ]]; then
  echo "Usage: $0 <DemoFolderName> [Release|Debug]" >&2
  echo "  DemoFolderName: Basic3D | Sky | arpg (under C#/<name>)" >&2
  exit 1
fi

DEMO="$1"
CONFIG="${2:-Release}"
TEMPLATE_ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
SHARP_ROOT="${SPARK_SHARP_ROOT:-$TEMPLATE_ROOT/../spark-sharp}"
# SparkScriptHost lives under build/; SparkInterop must be rebuilt in the same tree:
#   cmake --build "$SHARP_ROOT/build" --target SparkInterop
BUILD_DIR="${SPARK_SHARP_BUILD_DIR:-$SHARP_ROOT/build}"

INTEROP_DYLIB="$BUILD_DIR/native/libSparkInterop.dylib"
if [[ "$(uname -s)" == "Darwin" ]]; then
  INTEROP_DYLIB="$BUILD_DIR/native/libSparkInterop.dylib"
elif [[ "$(uname -s)" == "Linux" ]]; then
  INTEROP_DYLIB="$BUILD_DIR/native/libSparkInterop.so"
fi
interop_has_demo_exports() {
  local dylib="$1"
  [[ -f "$dylib" ]] || return 1
  # Avoid `nm | grep -q` under pipefail: grep -q closes the pipe and nm exits 141, failing the check.
  local sym
  sym="$(nm -gU "$dylib" 2>/dev/null)" || return 1
  [[ "$sym" == *spark_demo_try_register_sky_hdr* ]]
}

if [[ -f "$INTEROP_DYLIB" ]] && ! interop_has_demo_exports "$INTEROP_DYLIB"; then
  echo "SparkInterop in $BUILD_DIR is stale (missing demo interop exports)." >&2
  echo "Rebuild in the same directory as SparkScriptHost:" >&2
  echo "  cmake --build \"$BUILD_DIR\" --target SparkInterop" >&2
  exit 4
fi

case "$DEMO" in
  Basic3D)
    CSPROJ="$TEMPLATE_ROOT/C#/Basic3D/Basic3DDemo.csproj"
    ENTRY_NS="SparkDemoBasic3D"
    ASM="Basic3DDemo"
    ;;
  Terrain)
    CSPROJ="$TEMPLATE_ROOT/C#/Terrain/TerrainDemo.csproj"
    ENTRY_NS="SparkDemoTerrain"
    ASM="TerrainDemo"
    ;;
  Sky)
    CSPROJ="$TEMPLATE_ROOT/C#/Sky/SkyDemo.csproj"
    ENTRY_NS="SparkDemoSky"
    ASM="SkyDemo"
    ;;
  arpg)
    CSPROJ="$TEMPLATE_ROOT/C#/arpg/ArpgCsGame.csproj"
    ENTRY_NS="SparkArpg"
    ASM="ArpgCsGame"
    ;;
  *)
    CSPROJ="$TEMPLATE_ROOT/C#/Demo/DEMO.csproj"
    if [[ ! -f "$CSPROJ" ]]; then
      CSPROJ="$TEMPLATE_ROOT/C#/${DEMO}/${DEMO}Demo.csproj"
    fi
    if [[ ! -f "$CSPROJ" ]]; then
      echo "Unknown demo '$DEMO' (no csproj). See C#/DEMO_CATALOG.md" >&2
      exit 2
    fi
    ENTRY_NS="${DEMO}"
    ASM="$(basename "$CSPROJ" .csproj)"
    ;;
esac

dotnet build "$CSPROJ" -c "$CONFIG" -p:NuGetAudit=false

GAME_DIR="$(dirname "$CSPROJ")/bin/$CONFIG/net8.0"
RUNTIME="$GAME_DIR/${ASM}.runtimeconfig.json"
DLL="$GAME_DIR/${ASM}.dll"

if [[ "$DEMO" != "arpg" ]]; then
  ENTRY_TYPE="${ENTRY_NS}.GameEntry, ${ASM}"
else
  ENTRY_TYPE="SparkArpg.GameEntry, ArpgCsGame"
fi

exec "$SHARP_ROOT/tools/run-spark-script-host.sh" \
  "$BUILD_DIR" \
  "$CONFIG" \
  "$RUNTIME" \
  "$DLL" \
  "$ENTRY_TYPE" \
  Initialize
