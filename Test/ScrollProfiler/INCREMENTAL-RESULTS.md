# Incremental visibility after c147575

## Results

Incremental updates improve the dense view in particular. Total time for its
scrolling sequence falls by 59.4%, and the p95 WPF frame interval by 80.8%.
Scrolling no longer causes collection resets. Timing in the one-second view
remains practically unchanged within measurement variability.

The baseline is the clean commit `c147575`. The changes remain uncommitted for
review; the previous step is documented in [RESULTS.md](RESULTS.md).

## Changes

- An `ObservableCollection` holds visible items. Items are removed individually
  when they leave the viewport and inserted in model order when they enter.
  Surviving WPF containers are preserved. This order also matters for overlapping
  markers.
- The stable, dispatcher-bound CollectionView is created when the view first
  accesses it, rather than when the ViewModel is constructed.
- A text anchor remembers its host while loaded. On unload, it schedules the
  host's text layout again and clears the host reference. This restores the
  available width of a stationary label after its neighbor is removed.
  Listeners are still unsubscribed.

Visibility calculation and the collision search itself remain unchanged.

## Measurement

The full WPF application, with its actual main window, ribbon, time scale, and
item templates. Unchanged [profiling runner](README.md), .NET 10.0.11, rendering
tier 2, 125% DPI, 16 rows, and 361 items. Timeline viewport: 1263.6 × 593.75 DIPs.

Each zoom level uses one warm-up run and three repetitions of 30 horizontal
10-DIP steps. Starting positions and scrolling distances are identical between
builds. The input timer requests steps every 16.67 ms; a busy dispatcher executes
them later. Tables show medians from `baseline.json` and the final `final.json`.

| Zoom level | Total time before → after | WPF frame interval p95 before → after | UI allocations before → after |
|---|---:|---:|---:|
| 5 minutes | 5.878 → 2.388 s | 337.96 → 64.89 ms | 541.31 → 159.27 MB |
| 1 minute | 1.317 → 0.889 s | 79.75 → 46.50 ms | 113.95 → 55.73 MB |
| 1 second | 0.814 → 0.822 s | 54.15 → 54.76 ms | 17.84 → 15.75 MB |

WPF frame intervals are intervals between distinct `CompositionTarget.Rendering`
callbacks, not GPU presentation times. The p95 column contains the median of the
three individually calculated p95 values. Allocations are cumulative UI-thread
bytes in decimal MB, not peak memory usage.

| Zoom level | Collection resets before → after | Item Loaded events before → after | LayoutUpdated cycles before → after |
|---|---:|---:|---:|
| 5 minutes | 67 → 0 | 1158 → 45 | 112 → 87 |
| 1 minute | 25 → 0 | 172 → 7 | 59 → 46 |
| 1 second | 2 → 0 | 8 → 2 | 31 → 31 |

| Zoom level | Process CPU before → after | Text collisions: callback time before → after |
|---|---:|---:|
| 5 minutes | 6.641 → 3.797 s | 471.77 → 671.93 ms |
| 1 minute | 1.438 → 1.234 s | 168.08 → 146.69 ms |
| 1 second | 0.266 → 0.250 s | 22.88 → 20.45 ms |

The dense view mainly benefits from avoiding template reconstruction. Its measured
collision time increases despite an unchanged callback count, so this step does
not accelerate the collision search itself. The cause of the higher cost per
callback was not isolated here.

At 5 minutes, total time ranges from 5.50–6.00 s before and 2.23–2.43 s after.
At 1 minute, the ranges are 1.24–1.35 s and 0.856–0.917 s. In the one-second
view, they overlap: 0.773–0.874 s and 0.810–0.861 s.

An additional baseline process and further intermediate-version processes were
used to investigate initially slightly higher CPU/frame values in the one-second
view. They do not confirm a clear timing improvement there. Neither a speedup
nor a meaningful regression is inferred from the small timing differences in
this view.

## Regression testing

- 168 tests passed. The existing viewport boundary-crossing test now checks
  targeted Add/Remove events and preservation of the other WPF containers in
  both scroll directions.
- Additional checks cover model order, identical start times, zoom, resizing,
  compact grid, editing, deletion, and undo/redo.
- The new test for removing a neighboring text item failed before the host
  update and passes with it. Existing GC and resubscription tests for unloaded
  anchors also pass.
- 15 actual WPF states with a total of 4627 anchor, obstacle, and text box
  geometries match the baseline: visibility, position, and width to 0.001 DIP,
  including backward scrolling. This is not a pixel comparison.

## Remaining potential

The densest view still does not achieve a steady 60 Hz. With row resets eliminated,
text collision detection accounts for a larger share of the work. A further step
should therefore isolate its cost, including repeated natural text width
measurement. Neither a different search structure nor a text width cache was
introduced in this step.

## Raw data

Local, ignored artifacts under `artifacts/incremental-scroll`:

- `baseline.json`, `final.json`: main comparison, individual measurements, and frame intervals.
- `baseline-repeat.json`: additional baseline process.
- `candidate.json`, `candidate-repeat.json`: measurements before deferred
  CollectionView creation.
- `geometry-baseline.json`, `geometry-final.json`: complete geometry comparison.
- `regression.log`: final regression suite.

The JSON metadata includes the SHA-256 hashes of the measured profiling builds.
