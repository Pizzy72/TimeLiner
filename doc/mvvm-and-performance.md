# MVVM Usage and Performance Considerations

This document describes how the MVVM pattern is used in TimeLiner and explains the
performance characteristics and limitations that result from this architectural choice.

The intent is to make these trade-offs explicit for future maintenance and development.

---

## MVVM in TimeLiner

TimeLiner follows the Model-View-ViewModel (MVVM) pattern consistently across the user interface.

The main goals of using MVVM are:

- clear separation between UI and application logic
- improved testability of non-visual logic
- predictable data flow between models and views

MVVM is implemented with lightweight application-owned base classes, commands, and services.
No external MVVM framework is required.

---

## Responsibilities of Each Layer

### Models

Models represent the domain data of the application, including:

- timelines
- timeline items
- application and window settings

They are free of UI concerns and can be tested independently.

### ViewModels

ViewModels act as the presentation layer:

- they expose properties for data binding
- they provide commands for user interaction
- they translate domain data into UI-friendly representations

ViewModels do not reference concrete views and do not depend on WPF control types.

### Views

Views are responsible for:

- visual layout
- styling and theming
- binding to ViewModel properties and commands

Code-behind is limited to UI-specific behavior that cannot reasonably be expressed via bindings.

---

## ViewModel Granularity

TimeLiner uses relatively fine-grained ViewModels.

Each major UI element or dialog typically has its own ViewModel. This approach:

- keeps responsibilities small and explicit
- simplifies reasoning about individual UI components
- improves testability

The trade-off is an increased number of bindings and notification events at runtime.

---

## Performance Characteristics

The performance of TimeLiner is largely determined by WPF data binding behavior.

In particular:

- property change notifications are propagated through bindings
- complex visual trees amplify binding overhead
- frequent updates can trigger layout and rendering work

For typical data sizes, this approach performs well and provides a responsive user experience.

---

## Known Performance Limitations

### Scroll update batching

`TimelineItemTextBehavior` batches label-width updates per timeline through the
dispatcher. Moving several item anchors before the dispatcher runs therefore
queues one row update. The update traverses the visual tree and captures obstacle
and anchor geometry once, then shares those values across the row's labels.
Previously, each moving anchor queued an update for every label, and each label
update traversed the row again.

The STA regression test `ScrollBatch_CoalescesLabelUpdates_AndPreservesSpacing`
moves 40 anchors three times before flushing layout and verifies both bounded
dispatcher work and the resulting label widths. The previous implementation
queued 4,800 Loaded-priority operations in this scenario. This measures queued
UI work, not application frame rate or end-to-end scrolling latency.

Further candidates for profiling are the collection-view filters refreshed on
scroll and the remaining delivery of parent property changes to off-screen items.
Label collision comparisons still scale quadratically with the number of items
in a row; batching removes redundant scheduling and visual-tree traversal, but
does not introduce a spatial index.

### Reusing time-scale labels

`TimeScaleView` retains its tick controls while scrolling. Motion within one grid
interval changes only a render translation, without changing label text or
invalidating the scale's layout. Crossing a grid boundary updates the existing
labels. Zoom and viewport changes adjust their width and count, keeping the
controls that are still needed.

Previously, every scroll offset cleared the tick panel, created all labels again,
and changed the control's layout margin. The regression tests in
`TestTimeScaleView` verify control reuse across fractional scrolling, grid
boundaries, large jumps and backward scrolling; unchanged layout within a grid
interval; and correct labels and alignment after zoom and viewport changes.
These checks establish reduced allocation and layout work, not an application
frame-rate measurement.

### Deferring horizontal updates for off-screen rows

Each timeline has a row index maintained when rows are added, inserted, removed
or moved. Its vertical viewport membership can therefore be checked in constant
time, using the same bounds as the existing row filter. Horizontal scrolling
only raises item geometry notifications and collection-view notifications for
visible rows. Hidden rows remember that an update is pending. When scrolling,
resizing or reordering brings them into view, their collection and geometry
bindings are refreshed. Property getters always calculate from current state;
editing and zoom notifications are not suppressed.

This deliberately retains the existing collection filters and event subscriptions.
It reduces downstream notifications, allocation and binding work, but does not
yet remove the parent event dispatch to every item or the full row scan during
vertical scrolling.

`TestScrollPerformance.ExternalFile_ScrollBenchmark` is an opt-in local benchmark.
The input remains outside the repository; no user file is copied or modified.
To reproduce it in PowerShell:

```powershell
$env:TIMELINER_BENCHMARK_FILE = 'C:\path\to\representative.tli'
$env:DOTNET_TieredCompilation = '0'
dotnet test TimeLiner.sln --no-restore -c Release --filter FullyQualifiedName~ExternalFile_ScrollBenchmark --logger 'console;verbosity=detailed'
```

To compare an installed build with compatible view-model constructors and
properties, set `TIMELINER_BENCHMARK_ASSEMBLY` to its `TimeLiner.dll` before
running the same command. Clear the variable to measure the current build.
The installed assembly is loaded in a separate assembly load context with
in-memory settings and a dialog service that rejects unexpected calls. The
installed executable is not launched and its bundled runtime is not used: both
builds run under the test host's .NET/WPF runtime. The test logs the assembly
SHA-256 and runtime version so identical product version labels do not obscure
which binaries were measured. Run each build in a separate test process.

The benchmark runs a fixed WPF binding/layout harness with a 1200-by-600-pixel
viewport, normal 30-pixel rows and the one-second scale. It uses rectangles and
labels bound to the real view models, rather than the complete application UI.
Each sample consists of 120 scroll steps with a layout/dispatcher flush after
each step. Horizontal steps repeat offsets from 0 to 590 pixels; vertical steps
repeat rows 0 to 59. Three warm-up samples precede five recorded samples per
direction. Reported allocations are bytes allocated on the UI thread, not live
heap size; elapsed times include binding/layout and dispatcher processing, not
GPU presentation or application frame rate. Disabling tiered compilation makes
the comparison more stable but differs from normal application execution.

Local comparison against commit `e2af89d`, using
`ProcessesFromStartToSavelog.tli` (945 rows, one time span per row), with 20 rows
visible. Medians of five samples, tiered compilation disabled:

| Direction / metric | Before | After |
| --- | ---: | ---: |
| Horizontal elapsed time / 120 steps | 1956.15 ms | 1708.66 ms |
| Horizontal UI-thread allocations / 120 steps | 269667000 bytes | 149792576 bytes |
| Horizontal item notifications / 120 steps | 453600 | 9600 |
| Vertical elapsed time / 120 steps | 2908.51 ms | 2881.59 ms |
| Vertical UI-thread allocations / 120 steps | 252087152 bytes | 252054272 bytes |

Horizontal time samples before: 1953.56, 1974.98, 1978.70, 1931.98, 1956.15 ms;
after: 2136.98, 1700.19, 1689.02, 2009.72, 1708.66 ms. The median improves by
about 13%, but individual runs still vary. Vertical timing differs by less than
1%; this change does not target that path. An initial comparison with default
tiered compilation showed about 25% fewer horizontal allocation bytes, but
unstable timings during JIT warm-up. The 44% allocation reduction in the table
is specific to the controlled configuration; it is not a general application
speedup claim.

Direct comparison with the user-specified installed build, using the same
controlled settings and the assembly-adapter benchmark in separate processes:

| Metric / 120 steps (median of five) | Installed build | Current build |
| --- | ---: | ---: |
| Horizontal elapsed time | 1995.68 ms | 1716.76 ms |
| Horizontal UI-thread allocations | 269665416 bytes | 149785384 bytes |
| Horizontal item notifications | 453600 | 9600 |
| Vertical elapsed time | 2991.20 ms | 2929.86 ms |
| Vertical UI-thread allocations | 252362032 bytes | 252221192 bytes |

Both assemblies report version 2.15.1.0 and ran under .NET 10.0.11. SHA-256:

- Installed: `DA4B9278DDECBD7BF155677E26A2A03DC71483D6ECD3B65A155226C9A2499D94`
- Current: `1140A26B351E996DAFB4AA196A7D3CF2B323E1D24BA8415FF449912758F909AB`

Horizontal time samples installed: 2066.08, 1988.14, 1976.67, 2001.85, 1995.68 ms;
current: 1730.17, 1732.71, 1714.30, 1716.76, 1716.49 ms. This gives about 14%
less elapsed time, 44% fewer allocation bytes and 98% fewer item notifications
for horizontal scrolling in this harness. The roughly 2% vertical timing
difference is small relative to run-to-run variation. Because both builds use
the same simplified views here, this comparison does not quantify the earlier
text-collision and time-scale view optimizations or whole-application FPS.

### Large data sets

When working with very large data models, performance may degrade noticeably.

Typical scenarios include:

- a large number of timelines
- many timeline items visible at once
- frequent updates to bound properties

These limitations are a direct consequence of:

- the number of active bindings
- the cost of change notification propagation
- WPF layout recalculation

This behavior is expected and not considered a defect.

---

## Design Trade-offs

The use of MVVM in TimeLiner represents a conscious trade-off:

**Advantages**
- clear structure
- maintainable code base
- good test coverage for logic
- low coupling between UI and logic

**Disadvantages**
- reduced scalability for very large data sets
- higher runtime overhead compared to more imperative UI approaches

Given the intended use cases of TimeLiner, this trade-off is considered acceptable.

---

## Possible Alternatives

Alternative approaches could improve performance for large data sets, for example:

- reducing ViewModel granularity
- limiting the number of active bindings
- moving parts of the UI logic closer to the view layer

These approaches would increase complexity and reduce the clarity provided by MVVM.

They have therefore not been pursued.

---

## Summary

MVVM provides a solid and maintainable foundation for TimeLiner.

Performance limitations for large data models are a known and documented consequence of this
choice. The architecture favors clarity and correctness over maximum scalability.
