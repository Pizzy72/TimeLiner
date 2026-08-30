# Build, Test, and Release

This document describes how to build, test, and optionally package TimeLiner from the command line.
It is intended as a practical reference for developers who work on the project intermittently.

The current public release is source-only. Publish and installer automation is
retained for local use and possible future work; its output is not an official release artifact.

---

## Prerequisites

The following tools are required:

- Windows 11 x64
- .NET 10 SDK (including the Windows Desktop SDK)
- PowerShell
- Invoke-Build (as a .NET local tool)
- Inno Setup (for installer creation)

The exact versions are not strictly pinned but should be recent enough to support the project files.

---

## One-Time Setup

Before building the project for the first time, restore local .NET tools:

```powershell
dotnet tool restore
```

This installs Invoke-Build as defined in the repository configuration.

---

## Solution Structure

Relevant files for build and release:

- `TimeLiner.sln` – Visual Studio solution
- `TimeLiner.build.ps1` – Invoke-Build script
- `Setup/TimeLiner.iss` – Inno Setup script
- `Setup/Output/` – generated installer output
- `artifacts/publish/` – generated self-contained application files

All build-related commands are executed from the repository root unless stated otherwise.

---

## Build

To build the application:

```powershell
dotnet ib build
```

This performs an SDK-style build of the solution using `dotnet build`.

The output binaries are generated in the usual build directories of the project.

---

## Test

To run automated tests:

```powershell
dotnet ib test
```

This executes the test project located in the `Test` folder using `dotnet test`.

Tests focus on:
- domain models
- ViewModels
- parsing and validation logic

UI elements are intentionally excluded from automated testing.

---

## Optional Local Packaging

To create a release build suitable for packaging:

```powershell
dotnet ib release
```

This step includes:
- building the application in release configuration
- running all automated tests
- publishing a self-contained `win-x64` distribution
- creating the Inno Setup installer

The publish output includes the TimeLiner MIT license and runtime-dependency
notices. Before any future public binary release, the exact .NET runtime license
and third-party notice requirements must be reviewed and implemented for the
runtime and packaging method in use.

The exact behavior is defined in `TimeLiner.build.ps1`.

---

### Installer Creation

The Windows installer is created using Inno Setup.

Relevant files:
- Script: `Setup/TimeLiner.iss`
- Output directory: `Setup/Output`

The installer can be generated either:
- as part of the release process
- or manually by opening the `.iss` file in Inno Setup and building it

The resulting installer executable is written to the output directory.

Installation and runtime requirements are described in [Installation](installation.md).

---

## Notes and Common Pitfalls

- Always run `dotnet tool restore` after cloning the repository or updating tools.
- Invoke-Build commands must be executed from the repository root.
- Installer creation requires Inno Setup to be installed locally.
- Installer generation uses the self-contained files from `artifacts/publish/`.
- Publishing fails if the project license or runtime-dependency notices cannot be found.

---

## Summary

Typical workflow after a fresh checkout:

```powershell
dotnet tool restore
dotnet ib build
dotnet ib test
dotnet ib release
```

The final `release` command is optional local packaging and is not required for
the current source-only public release.
