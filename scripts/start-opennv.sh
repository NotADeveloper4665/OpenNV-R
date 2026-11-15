#!/usr/bin/env bash
set -Eeuo pipefail

ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

# Prefer the private SDK used by this checkout, while still allowing a system SDK.
if [[ -d "$HOME/.dotnet" ]]; then
    export DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}"
    export PATH="$DOTNET_ROOT:$PATH"
fi

if ! command -v dotnet >/dev/null 2>&1; then
    echo "error: .NET SDK not found. Install the SDK pinned by global.json." >&2
    exit 1
fi

EXPECTED_DOTNET="$(sed -n 's/.*"version"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' "$ROOT/global.json" | head -n1)"
ACTUAL_DOTNET="$(dotnet --version)"
if [[ -n "$EXPECTED_DOTNET" && "$ACTUAL_DOTNET" != "$EXPECTED_DOTNET" ]]; then
    echo "error: OpenNV requires .NET SDK $EXPECTED_DOTNET; found $ACTUAL_DOTNET." >&2
    echo "Set DOTNET_ROOT/ PATH to the pinned SDK, or install it." >&2
    exit 1
fi

GODOT="${OPENNV_GODOT:-}"
if [[ -z "$GODOT" ]]; then
    for candidate in \
        "$HOME/Applications/OpenNV-Godot/Godot_v4.8-dev7_mono_linux_x86_64/Godot_v4.8-dev7_mono_linux.x86_64" \
        "$HOME/Applications/OpenNV-Godot/Godot_v4.8-dev7_mono_linux.x86_64"; do
        if [[ -x "$candidate" ]]; then GODOT="$candidate"; break; fi
    done
fi
if [[ -z "$GODOT" ]]; then
    for candidate in godot-mono godot4-mono godot; do
        if command -v "$candidate" >/dev/null 2>&1; then
            GODOT="$(command -v "$candidate")"
            break
        fi
    done
fi
if [[ -z "$GODOT" || ! -x "$GODOT" ]]; then
    echo "error: Godot 4.8-dev7 Mono not found. Set OPENNV_GODOT to its executable." >&2
    exit 1
fi

GODOT_VERSION="$("$GODOT" --version 2>&1 | head -n1)"
if [[ "$GODOT_VERSION" != *"4.8.dev7.mono.official"* ]]; then
    echo "error: expected Godot 4.8-dev7 Mono; found: $GODOT_VERSION" >&2
    exit 1
fi

GAME="${OPENNV_DATA_ROOT:-}"
if [[ -z "$GAME" ]]; then
    candidates=(
        "$HOME/.var/app/com.valvesoftware.Steam/.local/share/Steam/steamapps/common/Fallout New Vegas"
        "$HOME/.local/share/Steam/steamapps/common/Fallout New Vegas"
    )
    for candidate in "${candidates[@]}"; do
        if [[ -f "$candidate/Data/FalloutNV.esm" ]]; then
            GAME="$candidate"
            break
        fi
    done
fi
if [[ -z "$GAME" || ! -f "$GAME/Data/FalloutNV.esm" ]]; then
    echo "error: Fallout: New Vegas installation not found." >&2
    echo "Set OPENNV_DATA_ROOT to the game installation directory (the parent of Data/)." >&2
    exit 1
fi

SAVE_PATH="${OPENNV_SAVE_PATH:-$HOME/.local/share/OpenNV/saves/new-vegas.json}"
mkdir -p "$(dirname -- "$SAVE_PATH")"

echo "Building OpenNV Debug runtime with .NET $ACTUAL_DOTNET..."
dotnet build "$ROOT/runtime/OpenNV.csproj" --configuration Debug

echo "Launching Fallout: New Vegas from: $GAME"
exec "$GODOT" --path "$ROOT/runtime" --xr-mode off -- \
    --data-root "$GAME" \
    --campaign fallout-new-vegas \
    --save-path "$SAVE_PATH" \
    --opening-menu \
    "$@"
