# TimeLiner – Overview

## Purpose

TimeLiner is a Windows desktop application for creating, editing, and analyzing
time-based data using visual timelines.

The application focuses on clarity, direct manipulation, and predictable behavior.
It is designed as a practical tool rather than as a highly automated or large-scale
data processing solution.

---

## Target Audience

This documentation is intended for:

- developers maintaining or extending the TimeLiner code base
- technically experienced users interested in the internal structure
- the original author as a long-term technical reference

End-user workflows and usage instructions are documented separately in the user manual.

---

## Technical Context

TimeLiner is implemented with the following technical foundations:

- Platform: Windows 11 x64
- Runtime: .NET 10 (Windows)
- UI technology: WPF
- UI framework: Fluent Ribbon
- Architectural pattern: MVVM
- Persistence: local file-based storage (CSV, JSON)

The application is a standalone desktop program with no server-side components.
The public repository is source-only: contributors build it with the .NET 10 SDK,
and NuGet restores the required packages. No official binaries or installers are
published. User-specific data is stored separately from build output.

---

## Design Focus

The design of TimeLiner emphasizes:

- a clear separation of concerns between UI, presentation logic, and data models
- maintainable and testable code over aggressive optimization
- explicit handling of known limitations

Performance for very large data sets is not a primary goal and is treated as a
documented architectural trade-off.

---

## Documentation Structure

This folder contains the developer-oriented documentation for TimeLiner:

- [Architecture](architecture.md)
- [Data Model](data-model.md)
- [Persistence](persistence.md)
- [Theme System](theme-system.md)
- [Installation](installation.md)
- [MVVM and Performance](mvvm-and-performance.md)
- [Third-Party Libraries](third-party-libraries.md)
- [Online Help](online-help.md)
- [Build and Release](build-and-release.md)
- [Decisions and Limitations](decisions-and-limitations.md)

---

## Scope

This documentation:

- explains architectural concepts and design decisions
- provides guidance for building and maintaining the application
- records known limitations and trade-offs

It deliberately avoids:

- detailed API or class-level documentation
- duplication of end-user documentation
- exhaustive descriptions of UI elements
