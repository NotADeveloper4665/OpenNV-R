# Installation and development

OpenNV currently pins .NET SDK 10.0.401 and Godot 4.8-dev7 Mono. Godot
4.8-dev7 is a development snapshot, not a stable release. Use the matching
Mono build; the standard Godot editor does not include the C# runtime.

## Windows

Install PowerShell 7 and the pinned .NET SDK. Node.js 22 is only needed for
the retained JavaScript compatibility tests.

Verify and launch:

```powershell
. .\scripts\ManagedToolchain.ps1
Initialize-OpenNVManagedToolchain
dotnet build .\runtime\OpenNV.sln -c Release
npm test --prefix .\desktop
.\scripts\Test-GodotRuntime.ps1 -Godot 'C:\Path\To\Godot_console.exe'
.\scripts\Start-OpenNV.ps1
```

## Linux

Install the pinned .NET SDK and Godot 4.8-dev7 Mono. The Linux development
launcher detects the Flatpak Steam New Vegas installation and the private
Godot location used by the Fedora development setup:

```bash
bash scripts/start-opennv.sh
```

Override detection when needed:

```bash
OPENNV_GODOT=/path/to/Godot_v4.8-dev7_mono_linux.x86_64 \
OPENNV_DATA_ROOT="/path/to/Fallout New Vegas" \
bash scripts/start-opennv.sh
```

The launcher builds the Debug C# runtime, then starts the direct New Vegas
diagnostic route. `OPENNV_SAVE_PATH` can override the default save location
under `~/.local/share/OpenNV/saves/`. Additional runtime arguments can be
passed after the script name.

Select only a legally owned installation. Retail files remain read-only and
are not copied into the repository. The launcher does not convert or package
Bethesda assets.
