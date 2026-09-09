# Text width cache after 04a63d2

## Decision

The bounded cache is retained: it substantially reduces repeated text measurement,
CPU time, and allocations. The dense scrolling sequence takes 15.3% less time in
the standard run. An improvement in p95 frame intervals, however, is **not
consistently demonstrated**. No further changes to collision search or layout
were made in this step. The changes remain uncommitted for now.

## Change and validation

Each TextBlock stores at most its last natural text width. Before reusing it,
the cache checks the text, font family, style, weight, stretch, size, flow direction,
DPI, and current UI culture. Measurements are not reused for mutable culture
information or anonymous composite font families. There is no global collection
of label text or TextBlock references.

180 regression tests pass. New tests compare a previously measured text box after
changes to its inputs with a fresh text box, including culture and DPI changes.
A GC test checks that a text box with a cache can be collected.
15 actual WPF layout states with 4627 element geometries match the baseline,
including backward scrolling. This is not a pixel comparison.

## Measurement method

The baseline is `04a63d2`, with only profiling counters added. Both builds contain
the same counters for width queries, actual FormattedText measurements, and their
duration. They are guarded by `SCROLL_PROFILE` and absent from normal builds.
The runner reads them outside the measured interval; older source versions
without counters remain supported and report `null` for them.

Unchanged [WPF measurement setup](README.md): complete main window, 16 rows,
361 items, three zoom levels, one warm-up run and three repetitions per level,
with 30 steps of 10 DIPs each. .NET 10.0.11, 125% DPI, rendering tier 2.
All table values are medians of the three repetitions.

## Standard run

| Zoom | Total time before → after | Process CPU before → after | UI allocations before → after |
|---|---:|---:|---:|
| 5 minutes | 2.304 → 1.950 s | 3.719 → 3.000 s | 158.45 → 135.78 MB |
| 1 minute | 0.895 → 0.841 s | 1.281 → 1.172 s | 55.76 → 48.45 MB |
| 1 second | 0.816 → 0.872 s | 0.234 → 0.359 s | 15.74 → 15.05 MB |

| Zoom | Actual text measurements before → after | Text measurement time before → after | Collision callback time before → after |
|---|---:|---:|---:|
| 5 minutes | 6108 → 29 | 429.75 → 5.29 ms | 648.51 → 208.25 ms |
| 1 minute | 1903 → 2 | 110.00 → 0.58 ms | 160.66 → 32.35 ms |
| 1 second | 168 → 0 | 11.58 → 0.00 ms | 17.39 → 8.67 ms |

The number of width queries remains unchanged. The cache avoids the expensive
part of each query. Text measurement time includes FormattedText construction
and calculation of the natural width, but excludes checking the cache key.

| Zoom | WPF frame interval p95 before → after | LayoutUpdated cycles before → after |
|---|---:|---:|
| 5 minutes | 63.18 → 75.36 ms | 87 → 87 |
| 1 minute | 39.46 → 43.12 ms | 46 → 46 |
| 1 second | 53.88 → 58.62 ms | 31 → 31 |

The p95 values are higher in the standard run. Less work therefore does not
automatically improve frame spikes here. At 5 minutes, the number of frame
intervals exceeding 25 ms also falls from 36 to 31 per sequence. The number and
distribution of rendering callbacks change with the run duration; these metrics
describe different aspects and must not be treated as interchangeable.

## Control run without tiered JIT compilation

Because the frame results were mixed, both variants were also run with
`DOTNET_TieredCompilation=0`. This setting applied only to the respective
measurement processes and does not change normal application settings.
It provides a diagnostic comparison, not a replacement for the standard run.

| Zoom | Total time before → after | WPF frame interval p95 before → after |
|---|---:|---:|
| 5 minutes | 2.913 → 2.270 s | 91.57 → 82.84 ms |
| 1 minute | 1.150 → 0.959 s | 47.87 → 48.17 ms |
| 1 second | 0.933 → 0.875 s | 55.02 → 49.68 ms |

At 5 minutes, process CPU time falls from 3.203 to 2.672 s and collision time
from 704.18 to 171.10 ms in this run. The reduction in computational work is
therefore reproducible. Differences between the standard and control runs do
not allow frame variability to be attributed conclusively to JIT compilation.
No reliable timing improvement is claimed for the lightly loaded one-second view.

Frame intervals come from `CompositionTarget.Rendering`, not GPU presentations.
Each p95 is the median of three individual p95 values. MB denotes decimal,
cumulative UI-thread allocations, not memory held simultaneously.

## Raw data

The ignored directory `artifacts/text-width` contains `baseline.json`,
`final.json`, `baseline-controlled.json`, `final-controlled.json`, the initial
cache trial `candidate.json`, `geometry-baseline.json`, `geometry-final.json`,
and `regression.log`. The JSON metadata includes the profiling build hashes.
