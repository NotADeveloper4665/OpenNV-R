#!/usr/bin/env bash
set -euo pipefail

repository="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
godot="${1:-${OPENNV_GODOT:-}}"
cd "$repository"

export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1

dotnet build runtime/OpenNV.sln --configuration Release --nologo
dotnet format runtime/OpenNV.sln --verify-no-changes --no-restore --verbosity minimal
dotnet build runtime/OpenNV.sln --configuration Debug --nologo
dotnet run --project contract-tests/FalloutPluginRuntimeProbe/FalloutPluginRuntimeProbe.csproj \
    --configuration Release -- --test-user-config-paths

if [[ -n "$godot" ]]; then
    if [[ ! -x "$godot" ]]; then
        printf 'Godot executable is missing or not executable: %s\n' "$godot" >&2
        exit 1
    fi
    "$godot" --headless --path runtime --editor --quit
fi

git diff --check
printf 'OPENNV_LINUX_RUNTIME_SMOKE_PASS godot=%s\n' "${godot:-not-requested}"
