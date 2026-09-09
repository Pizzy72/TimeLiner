# WPF scroll profiling

This optional runner starts the full application through `App.Run()`:
main window, ribbon, actual item templates, time scale, and text collision detection.
It is not part of the normal build or automated test suite.

## Setup

`ScrollProfiler.targets` adds a profiling entry point and enables `SCROLL_PROFILE`.
This compiles additional counters for natural text width measurements;
these counters and timings are absent from normal application builds. Application
code and all WPF resources are compiled from the source version under investigation.
At startup, user settings are replaced with defaults and in-memory persistence.
The input file is read only; its name and text contents are not included in the
JSON report.

Measurement uses a visible 1280 × 900 DIP window and three zoom levels:
5 minutes, 1 minute, and 1 second. A dense location is selected computationally
for each zoom level. One warm-up run is followed by three measured runs, each
with 30 horizontal steps of 10 DIPs. A DispatcherTimer at `Input` priority requests
steps every 16.67 ms. A busy UI thread executes them later; the actual total
duration is recorded.

Recorded metrics include process CPU time, UI-thread allocations, collection
resets, item Loaded events, global WPF LayoutUpdated events, and the duration of
`TimelineItemTextBehavior` dispatcher callbacks. Callback identification uses
the private WPF field `DispatcherOperation._method` and checks for its presence
at startup. A runtime change may require an adjustment.

`TextWidthCalls` counts width queries; `TextWidthMeasurements` counts actual
`FormattedText` measurements created. `TextWidthMs` records their construction
and width calculation, excluding cache lookups. Older source versions without
these optional counters report `null` for them. Both versions must contain the
same profiling probes to compare their contributions directly. Counter values
are read outside the measurement interval.

Frame times are intervals between distinct `CompositionTarget.Rendering` callbacks.
They measure WPF UI cadence, **not GPU presentation times**. LayoutUpdated counters
do not count individual Measure/Arrange calls. Allocations are cumulative bytes,
not peak memory usage. Run measurements sequentially; do not run builds, tests,
or UI automation concurrently with them.

## Running

Use PowerShell in the repository directory. `TIMELINER_BENCHMARK_FILE` must point
to a representative local file. Store output in the ignored directory
`artifacts/scroll-profile`.

```powershell
$root = (Get-Location).Path
$targets = Join-Path $root 'Test/ScrollProfiler/ScrollProfiler.targets'
dotnet build Source/TimeLiner/TimeLiner.csproj -c Release -p:Platform=x64 `
  "-p:CustomAfterMicrosoftCommonTargets=$targets" -p:StartupObject=Program `
  -p:OutputType=Exe -o artifacts/scroll-profile/app-current
$env:TIMELINER_PROFILE_OUTPUT = Join-Path $root 'artifacts/scroll-profile/current.json'
dotnet artifacts/scroll-profile/app-current/TimeLiner.dll
```

For the baseline, extract the desired commit using `git archive` into a separate
directory under `artifacts`. Then build its `Source/TimeLiner/TimeLiner.csproj`
using the same targets path and a separate output directory.
Do not reset the active checkout.

With `TIMELINER_PROFILE_VERIFY=1`, the runner records geometric states instead of
timings at three zoom levels and five scroll positions each, including backward
scrolling. The comparison covers anchors, obstacles, and automatically constrained
text boxes: position, visibility, and width, rounded to 0.001 DIP.
Skip the first metadata record when comparing the JSON files.
This check compares layout geometry, not rasterized pixels.

```powershell
$env:TIMELINER_PROFILE_VERIFY = '1'
$env:TIMELINER_PROFILE_OUTPUT = Join-Path $root 'artifacts/scroll-profile/geometry-current.json'
dotnet artifacts/scroll-profile/app-current/TimeLiner.dll
Remove-Item Env:TIMELINER_PROFILE_VERIFY
```

After profiling builds, run the normal Release build or tests without the
additional MSBuild properties. Do not use files from profiling output directories
for normal application packages.
