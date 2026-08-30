# Building and Running TimeLiner

TimeLiner is currently released as source code only. There are no official
precompiled executables, installers, portable packages, or binary releases.

## Requirements

- Windows 11 x64
- .NET 10 SDK, including the Windows Desktop SDK
- access to the configured NuGet sources

## Build and Run

From the repository root:

```powershell
dotnet restore TimeLiner.sln
dotnet build TimeLiner.sln --configuration Release --property:Platform=x64
dotnet run --project Source/TimeLiner/TimeLiner.csproj --configuration Release --property:Platform=x64
```

Visual Studio can also restore, build, and start the `TimeLiner` project.

The repository retains optional publish and installer automation for local use
and possible future releases. Those outputs are not part of the current public
release and are not supported distribution artifacts.

## User Data and Settings

User-specific data is stored separately from build output, normally under:

```
%APPDATA%\TimeLiner\
```

This includes application preferences, window state and geometry, and theme
selection. Deleting or rebuilding application output does not reset these
settings. To reset the application state completely, remove the corresponding
user-data directory after closing TimeLiner.
