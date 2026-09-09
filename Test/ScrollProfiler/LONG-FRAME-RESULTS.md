# Long frame investigation after 1fc89a3

## Findings

The dense view's long intervals are dominated by WPF MediaContext render callbacks,
which include layout and event processing. GC pauses contribute materially to some
spikes. Text collision callbacks are a smaller part of these intervals. This
investigation adds diagnostics only; it does not change scrolling or hide labels.

Across the three dense-view runs, 67.4% of the observed interval time falls inside
WPF render callbacks, 17.7% inside dispatcher timers, and 11.6% inside text collision
callbacks. The remainder is other dispatcher work or time outside observed
operations. These are elapsed-time shares, not CPU samples or GPU timings.

The lightly populated one-second view has a different pattern: 86.4% of its
observed interval time lies outside the recorded dispatcher operations. Optimizing
the collision callback cannot explain or eliminate those gaps on this evidence.
Waiting, scheduling, native message handling, and rendering cadence have not been
separated by this probe.

## Measurement

Measured on September 8, 2026, using the full WPF application at `1fc89a3` with
optional profiling additions. The existing representative workload contains
16 rows and 361 items. The viewport is 1263.6 × 593.75 DIPs, with 125% DPI,
rendering tier 2, .NET 10.0.11, and default tiered compilation.

The [existing runner](README.md) uses one warm-up and three measured runs per
zoom level, each with 30 horizontal steps of 10 DIPs requested at Input priority
every 16.67 ms. User settings remain in memory. No concurrent builds or tests
were run during measurement.

The final detailed trace contains 430 rendering intervals: 185 at five minutes,
129 at one minute, and 116 at one second. Intervals are delimited by distinct
`CompositionTarget.Rendering` callbacks, not GPU presentation events. The time
before the first callback is excluded from interval-level totals.

## Individual intervals

The following are actual individual intervals from `trace-gc.json`, not medians.
Run numbers are zero-based; interval numbers are one-based within a run.

| Zoom / run / interval | Interval | WPF render callbacks | Collision callbacks | Dispatcher timers | Reported GC pause delta | Layout cycles | Item loads |
|---|---:|---:|---:|---:|---:|---:|---:|
| 5 minutes / 1 / 50 | 97.34 ms | 82.39 ms | 5.23 ms | 8.83 ms | 26.32 ms | 2 | 10 |
| 5 minutes / 1 / 31 | 79.47 ms | 64.34 ms | 5.88 ms | 8.21 ms | 16.09 ms | 1 | 8 |
| 5 minutes / 2 / 8 | 72.35 ms | 58.22 ms | 5.64 ms | 7.50 ms | 26.85 ms | 5 | 0 |
| 1 second / 0 / 21 | 60.55 ms | 1.94 ms | 0.14 ms | 0.85 ms | 0.00 ms | 1 | 0 |

GC pauses overlap the elapsed dispatcher spans and must **not** be added to them.
`GC.GetTotalPauseDuration()` is sampled at rendering boundaries; it does not give
exact pause start/end times or identify the allocating call stack. Likewise,
item loads and layout counts establish co-occurrence, not causation.

In the longest interval, the actual scroll setter accounts for most of the
8.83 ms timer span. About 0.84 ms lies outside observed dispatcher operations;
the remaining approximately 0.05 ms is other dispatcher work. The one-second
example instead contains 57.59 ms outside the observed operations, with no
reported GC pause increase.

Of the dense view's 43 intervals exceeding 50 ms, 36 have a positive GC pause
delta and 24 include item loads. The 72.35 ms example shows that new containers
are not a necessary condition for a long interval: it has no loads, five layout
cycles, and a 26.85 ms reported GC pause delta. A single rendering interval can
overlap several WPF render operations.

The application's text LayoutUpdated handler executes 57,246 times across the
dense-view interval samples, but its measured body totals only 1.44 ms. This
counter does not measure WPF's complete layout engine or the deferred collision
work. It does not support optimizing that handler as the main bottleneck.

## Observation overhead and variability

The final build was run both with and without detailed tracing. Each table entry
is the median of three runs; p95 is the median of the individual run percentiles.

| Zoom | Total time without → with tracing | Frame p95 without → with tracing |
|---|---:|---:|
| 5 minutes | 1.707 → 1.811 s | 63.41 → 67.80 ms |
| 1 minute | 0.893 → 0.887 s | 47.18 → 48.03 ms |
| 1 second | 0.869 → 0.926 s | 60.41 → 57.20 ms |

The dense-view duration is 6.1% higher in this comparison. The ranges overlap:
1.652–1.845 s without tracing and 1.710–1.886 s with tracing. Earlier diagnostic
builds and control processes also showed variability. These results neither
establish zero overhead nor isolate a fixed overhead percentage. Detailed timings
are diagnostic evidence, not a performance improvement claim.

The trace preallocates sample lists and resolves callback names after measurement.
It still adds reflection, counter sampling, dictionary operations, and some
allocations. Both control and detailed runs contain the same profiling-only
text measurement and text layout probes. Normal builds contain none of these
probes or the profiling entry point.

## Recommended next step

If the remaining dense-view spikes warrant further work, collect CPU/allocation
stacks around MediaContext render callbacks and the scroll setter. Separate WPF
Measure/Arrange, template creation, binding updates, text formatting/rendering,
and allocation sources before choosing a production change. The current trace
cannot distinguish those costs inside WPF.

Reducing repeated layout or allocation work is a better-supported investigation
than further optimizing the collision search alone. Hiding labels may also affect
WPF layout and rendering costs, but the 5.23 ms collision portion of the longest
interval is not evidence that hiding text would remove the other 82.39 ms.
That proposal still requires a separate controlled experiment and a usability
check when labels return.

No production optimization is implemented in this step. Changes remain
uncommitted for review.

## Validation and artifacts

- Normal Release build and all 180 non-performance regression tests pass.
- The analyzer was checked with a synthetic fixture covering operations crossing
  frame boundaries, nested operations, and fractional unassigned time.
- All 430 real intervals partition into render, collision, timer, other dispatcher,
  and unattributed time within 0.000001 ms.
- Source changes outside the optional profiler are guarded by `SCROLL_PROFILE`.

Local ignored artifacts are under `artifacts/long-frames`:

- `trace-gc.json`, `control-gc.json`: final detailed and control runs, with build
  hashes in the JSON metadata.
- `frames-gc.csv`: per-interval analysis from `Analyze-Frames.ps1`.
- `trace.json`, `control.json`, `trace-final.json`, `control-final.json`: earlier
  diagnostic runs before the final GC-pause probe.
- `analyzer-fixture.json`, `analyzer-fixture.csv`: analyzer validation fixture.
- `regression.log`: normal-build regression results.

The earlier `trace.json` omits unfinished operation tails; the final format
includes these up to the observation cutoff and reports `TruncatedOperations`.
Use the final `trace-gc.json` for the interval attribution above.
