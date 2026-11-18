# OpenNV-R rendering and performance work

## Current renderer constraints

The runtime uses Godot 4.8 with the Forward+ renderer and a threaded rendering
driver. Fallout content is translated into Godot resources and published through
the existing native presentation path. Keep the source record, gameplay state,
and rendered representation separate: a visual optimization must not silently
remove collision, activation, quest, or persistence behavior.

The current runtime already has several important foundations:

- Source-scoped DDS image reuse with bounded retention.
- Shared immutable material/image payloads while keeping material state independent.
- Background NIF/content preparation.
- Incremental near and distant LOD publication.
- A shared upload admission allowance, with per-step and per-batch timing.
- Cached source navigation graphs.
- Existing capture and owned-content audits for selected player and world paths.

Do not replace these mechanisms with parallel caches or a second streaming
pipeline. Extend their ownership, cancellation, retirement, and diagnostics
contracts.

## Rendering observability added

The runtime performance report now records:

- Per-process-frame delta, whole-session min/max/average, and rolling p50/p95/p99.
- Percentage of frames above the 16.67 ms 60 Hz budget.
- Godot render object, primitive, and draw-call counts.
- Godot-reported video, texture, and render-buffer memory monitors.
- Existing process/physics time, node counts, orphan nodes, and static memory.

Godot memory monitors are engine-reported estimates/counters. They are not
guaranteed to equal total dedicated GPU memory or the GTX 1060's driver-visible
VRAM usage. A zero or unavailable monitor must not be interpreted as zero GPU
use. CPU process-frame time is not isolated GPU execution time.

## Work sequence

### 1. Establish a comparable baseline

Use the same New Vegas save, load order, resolution, renderer, graphics settings,
and route. Capture at least three runs each for:

- Settled starting-area view.
- A repeatable exterior traversal.
- A cell/building transition.
- A combat encounter with a consistent number of actors.

Separate cold-start/shader-compilation runs from warmed runs. Record display
resolution, window mode, GPU driver, renderer, and mod stack alongside each
report. Compare medians and p95/p99 rather than a single best FPS result.

### 2. Rendering audit before changing visuals

Trace the production paths for:

- NIF surface and material creation.
- Near and distant LOD publication.
- Texture upload and cache hit/eviction behavior.
- Shadow-casting lights and shadow map costs.
- Terrain and foliage draw calls.
- Visibility ranges and occlusion behavior.
- Per-frame scene-tree processing and render-server submissions.

Use the existing owned audits to compare resource signatures and transforms
before and after each change. Capture representative screenshots and image
metrics; visual changes must not be accepted on FPS alone.

### 3. Low-risk performance improvements

Prioritize measured problems and preserve source semantics:

- Reuse compatible immutable mesh/material resources where ownership permits.
- Instance repeated static geometry only when transforms, material variants,
  visibility, and source identity are compatible.
- Avoid creating scene-tree nodes for presentation-only data that can be
  represented by a lower-overhead render-server resource.
- Reduce redundant surface/material submissions without merging objects that
  need independent activation, collision, or visibility.
- Bound texture and mesh residency by explicit source-owned budgets.
- Keep uploads incremental and cancellation-safe; never assume a per-batch
  allowance can interrupt one expensive native upload.

### 4. Visual quality improvements

Only add quality options after the current renderer's actual feature path is
known and covered by visual fixtures. Candidate controls:

- Resolution scale and upscaling, with a clearly reported active mode.
- Anti-aliasing and anisotropic filtering.
- Shadow quality and shadow distance.
- LOD distance and terrain detail.
- Ambient occlusion and fog quality.
- Texture residency and mip bias.

Profiles should be named Performance, Balanced, and Quality, with a Custom
override. Defaults must be conservative for older GPUs such as the GTX 1060.
Do not enable multiple expensive screen-space effects by default. Preserve the
current neutral/parity renderer path until retail lighting and HDR behavior are
better characterized.

### 5. Acceptance criteria

A rendering change is accepted only when:

1. The relevant managed build and contract tests pass.
2. The same owned source fixtures retain expected geometry, transforms,
   materials, texture bytes, and visibility.
3. Gameplay-relevant references retain collision and activation behavior.
4. Repeated benchmark runs show a reproducible improvement in the targeted
   metric without a material p95/p99 regression elsewhere.
5. Screenshots or image comparisons show no unexplained visual regression.
6. Cache/resource retirement and cancellation tests pass.

Do not claim a performance improvement from synthetic tests alone or from one
uncontrolled before/after run.
