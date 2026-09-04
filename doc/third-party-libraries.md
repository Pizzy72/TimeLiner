# Third-Party Libraries

This document describes the external packages used by TimeLiner and their role in
the application or test infrastructure.

## Application

### Fluent.Ribbon

`Fluent.Ribbon` 11.0.2 provides the ribbon-based WPF user interface and its related controls.
It is the only direct NuGet dependency of the application.

Fluent.Ribbon brings the following runtime dependencies transitively:

- `ControlzEx` 7.0.3 for window chrome and theming support
- `Microsoft.Xaml.Behaviors.Wpf` 1.1.135 for Fluent.Ribbon compatibility

These dependencies are retained because they are required by Fluent.Ribbon.
All three packages are licensed under the MIT License according to the metadata
and license file in the versions restored for this project. Their complete
license notices are maintained in [`THIRD-PARTY-NOTICES.txt`](../THIRD-PARTY-NOTICES.txt).
The source repository does not store the packages or their DLLs; NuGet restores
them during the build.

## Test Infrastructure

The test project uses the Microsoft test stack:

- `Microsoft.NET.Test.Sdk` for test execution and Visual Studio integration
- `MSTest.TestAdapter` for test discovery and execution
- `MSTest.TestFramework` for test attributes and assertions

The tests use application-owned test doubles and do not require a mocking framework.
These build- and test-only packages are documented here rather than repeated in
the runtime-dependency notices.

## Platform APIs Used Instead of Packages

The following functionality is implemented with .NET and WPF platform APIs:

- MVVM base classes, commands, and service abstractions
- dialog coordination
- JSON settings persistence using `System.Text.Json`
- CSV parsing and validation

## Summary

Runtime package usage is intentionally limited to Fluent.Ribbon and its required
transitive dependencies. Test-only packages remain to preserve the standard
Visual Studio and `dotnet test` workflow.
