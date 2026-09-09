# Scroll optimization after 32e4d56

## Results

The full WPF application takes between 64% and 92% less time to complete the
measured scrolling sequences. The densest view is substantially faster, but
scrolling is still not consistently smooth.

The comparison covers the source at commit `32e4d56` and the uncommitted working
tree with two changes:

1. During scrolling, each row still updates the geometry of its affected items.
   Its collection is reset only when the visible set changes. This preserves
   existing item templates in most cases.
2. Text anchors unsubscribe their `DependencyPropertyDescriptor` listener when
   unloaded and subscribe again when reloaded. Previously, this listener retained
   removed anchors in memory, including their views and bindings.

The version bump to 2.15.4.0 was already present in the working tree before this work.

## Measurement method

Measured on September 7, 2026, with .NET 10.0.11, WPF rendering tier 2, 125% DPI,
the default light appearance, and visible item names. All 16 rows containing
361 items fit vertically in the window. The timeline viewport measures
1263.6 × 593.75 DIPs.

Each zoom level uses one warm-up run followed by three repetitions of 30 continuous
10-DIP steps. Steps are requested every 16.67 ms at Input priority.
Actual execution is delayed under load. Both versions use the same dense starting
positions and scroll 300 DIPs per run. Tables show the medians of the three
repetitions. For setup and reproduction, see the [README](README.md).

Early diagnostic attempts using scroll changes directly in the rendering callback
and very long runs were discarded. The results below come exclusively from
completed, input-driven runs.

| Zoom level | Total time before → after | Reduction | WPF frame interval p95 before → after |
|---|---:|---:|---:|
| 5 minutes | 33.61 → 5.79 s | 82.8% | 870.99 → 352.42 ms |
| 1 minute | 17.49 → 1.31 s | 92.5% | 455.18 → 79.38 ms |
| 1 second | 2.37 → 0.86 s | 63.7% | 88.67 → 52.81 ms |

Frame intervals are measured between distinct `CompositionTarget.Rendering`
callbacks, not GPU presentations. The p95 column shows the median of the three
individually calculated p95 values. Total times are not FPS measurements. The
baseline varies considerably: 22.51–33.64 s at 5 minutes and 13.77–46.74 s at
1 minute. After the changes, these ranges are 5.66–6.02 s and 1.28–1.36 s,
respectively.

| Zoom level | Process CPU before → after | UI allocations before → after | LayoutUpdated cycles before → after |
|---|---:|---:|---:|
| 5 minutes | 34.55 → 6.50 s | 2278.25 → 543.37 MB | 164 → 122 |
| 1 minute | 17.03 → 1.34 s | 903.36 → 114.13 MB | 364 → 59 |
| 1 second | 2.41 → 0.33 s | 194.30 → 17.86 MB | 60 → 31 |

MB denotes decimal megabytes. Allocations sum all bytes allocated on the UI thread
during a run; they do not measure memory held simultaneously. LayoutUpdated events
count global layout cycles, not individual Measure/Arrange calls.

| Zoom level | Collection resets before → after | Item Loaded events before → after | Text collisions: callback time before → after |
|---|---:|---:|---:|
| 5 minutes | 480 → 67 | 6588 → 1158 | 631.11 → 443.44 ms |
| 1 minute | 480 → 25 | 2383 → 172 | 272.86 → 154.34 ms |
| 1 second | 480 → 2 | 457 → 8 | 46.87 → 20.30 ms |

An intermediate version with reset avoidance alone already achieved total times
of 6.43 / 1.48 / 0.85 s. Preserving views therefore provides the main improvement.
The additional benefit of the listener fix is demonstrated by the GC lifetime
test; its isolated timing benefit cannot be quantified reliably from these
variable runs.

## Regression testing

- 166 regression tests passed, including zoom, visibility, existing scrolling
  tests, and four new tests covering container preservation, both scroll
  directions, collection of unloaded text anchors, and listener resubscription.
- Before the listener fix, the new GC test failed even after three full GC cycles;
  it passes after the fix.
- 15 actual WPF layout states across three zoom levels, including backward
  scrolling, match the baseline: 4627 anchor, obstacle, and text box geometries
  compared. Visibility, position, and widths were compared to 0.001 DIP;
  this is not a pixel comparison.

## Remaining potential

When an item enters or leaves the viewport, the application still resets the
entire affected row. The 1158 item load events and high frame spikes in the
densest view suggest investigating individual collection changes instead of full
row resets next. Text collision detection remains quadratic per row, but here
it takes substantially less time than the rest of UI construction. A more
complex search structure would therefore not be the first approach at this stage.

These measurements cover the three horizontal scrolling sequences. They establish
neither a general FPS guarantee nor improvements for arbitrary files or vertical
scrolling. The changes remain uncommitted for review.

## Raw data

Local, ignored artifacts under `artifacts/scroll-profile`:

- `baseline-1.json`: complete baseline measurement, including individual frame intervals.
- `candidate-1.json`: intermediate version with reset avoidance.
- `final-1.json`: final measurement with both changes.
- `geometry-baseline.json`, `geometry-final.json`: layout comparison.
- `regression.log`: regression suite.

The JSON metadata includes the SHA-256 hash of each profiling build.
