# OpenNV runtime performance baseline

## Capture a New Vegas session

Build the managed runtime, then launch it with an explicit performance report path.
The report is written when the runtime exits:

```bash
cd ~/Games/Development/OpenNV-R
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$HOME/.dotnet:$HOME/.local/bin:$PATH"
dotnet build runtime/OpenNV.csproj -c Debug

GODOT="$HOME/Applications/OpenNV-Godot/Godot_v4.8-dev7_mono_linux_x86_64/Godot_v4.8-dev7_mono_linux.x86_64"
"$GODOT" --path "$PWD/runtime" -- --launcher --launcher-game newvegas \
  --perf-report user://diagnostics/newvegas-baseline.json
```

Play a repeatable route before exiting: allow the initial load to settle, walk
through the same exterior area, enter and leave a building, and perform a
repeatable combat encounter if the current runtime supports it. Record whether
the run included first-use shader compilation, background downloads, or other
unusual system activity. Do not compare runs with different routes or content
stacks as if they were controlled benchmarks.

The report is written under Godot's per-user data directory. Its absolute path
is printed in the `OPENNV_PERF_REPORT` log line.

## Frame-time metrics

The report's `metrics.frameTimeMilliseconds` entry records every observed
process-frame delta, independently of the slower configurable monitor sampling
interval. It includes:

- Minimum, maximum, and average frame time across the complete observed session.
- p50, p95, and p99 over a bounded rolling window of the latest 4,096 frames.
- The count and percentage of frames exceeding the 16.67 ms 60 Hz frame budget.

The rolling percentile window is deliberately bounded so a long session cannot
grow diagnostic memory without limit. Whole-session min/max/average and
60 Hz-budget counts remain cumulative. These are CPU-side process-frame
intervals, not isolated GPU execution timings or proof of displayed/presented
frame pacing.

Existing periodic metrics remain sampled at the configured interval and include
FPS, process and physics-process time, node counts, orphan node counts, static
memory, rendered objects, and rendered primitives.

## Benchmark discipline

1. Use the same game installation, mod stack, save, resolution, renderer, and
   graphics settings for compared runs.
2. Keep the same route and actions, including a consistent warm-up period.
3. Capture at least three runs for each candidate and compare medians, not the
   single best run.
4. Keep loading, traversal, and combat segments distinguishable in notes; the
   current report is session-wide and does not yet label gameplay phases.
5. Treat frame-time percentiles as observations, not pass/fail thresholds.
6. Do not infer GPU bottlenecks from CPU frame time. Add GPU-specific profiling
   only when the active renderer exposes trustworthy measurements.

The observer adds a fixed 4,096-value frame-time ring buffer. Percentiles are
calculated when the report is written, not on every frame.
