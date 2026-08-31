# Binary Release Audit — TimeLiner 2.15.0

Audit date: 2026-08-31

Target: Windows 11 x64

Branch: `release/binary-distribution`
Status: **READY WITH MINOR CHANGES**

## Distribution model

The selected model is **self-contained `win-x64`**, pinned to .NET runtime
version 10.0.11.

A framework-dependent comparison publish was also built successfully. It
contained 11 files and occupied 5,578,963 bytes. It is much smaller and receives
runtime servicing through the separately installed .NET runtime, but requires a
compatible Windows Desktop Runtime on the target computer.

The selected self-contained publish contains 480 files and occupies 184,631,787
bytes. It is substantially larger and runtime security updates require a new
TimeLiner publish and distribution. For this small desktop application, the
predictable installation experience without a separate .NET prerequisite is the
deciding advantage. The runtime version, license, and notices are therefore
pinned and validated by the build.

Official references:

- https://learn.microsoft.com/en-us/dotnet/core/deploying/
- https://github.com/dotnet/core/blob/main/license-information.md
- https://github.com/dotnet/runtime/blob/main/docs/project/licensing-assets.md
- https://dotnet.microsoft.com/en-us/dotnet_library_license.htm

## Distributed components

| Component | Version | Source in the publish result |
|---|---:|---|
| TimeLiner | 2.15.0.0 | Application assembly and Windows executable |
| Fluent.Ribbon | 11.0.2 | Direct NuGet dependency; `Fluent.dll` |
| ControlzEx | 7.0.3 | Transitive NuGet dependency; `ControlzEx.dll` |
| Microsoft.Xaml.Behaviors.Wpf | 1.1.135 | Transitive NuGet dependency; `Microsoft.Xaml.Behaviors.dll` |
| Microsoft.NETCore.App.Runtime.win-x64 | 10.0.11 | Self-contained .NET runtime files |
| Microsoft.WindowsDesktop.App.Runtime.win-x64 | 10.0.11 | Self-contained WPF/Windows Desktop runtime files |

`Microsoft.AspNetCore.App.Runtime.win-x64` is present in restore assets but no
ASP.NET Core assembly is present in the application publish result. Test SDK,
MSTest, testhost, coverage, and test-data files are not distributed.

The package versions above were checked against the resolved project assets,
the exact restored packages, and the resulting `TimeLiner.deps.json`. Reference
package pages:

- https://www.nuget.org/packages/Fluent.Ribbon/11.0.2
- https://www.nuget.org/packages/ControlzEx/7.0.3
- https://www.nuget.org/packages/Microsoft.Xaml.Behaviors.Wpf/1.1.135

## License and notice provenance

| Distributed file | Exact source | SHA-256 |
|---|---|---|
| `LICENSE` | Repository root; TimeLiner MIT license | `671790ACA7510184BB513E8957832C84B35661CC43085BAD053288AE7B20BA59` |
| `THIRD-PARTY-NOTICES.txt` | Repository root; version-specific texts checked against the restored NuGet packages and their package metadata | `532C5084F61C4A5695C06F4F9DFBA9E620E27753DE8FBA631B9753EE18E07333` |
| `DOTNET-LICENSE.txt` | `$(NetCoreRoot)LICENSE.txt` from the installed Windows .NET 10.0.11 distribution; Microsoft .NET Library License | `7F6839A61CE892B79C6549E2DC5A81FDBD240A0B260F8881216B45B7FDA8B45D` |
| `DOTNET-THIRD-PARTY-NOTICES.txt` | `microsoft.netcore.app.runtime.win-x64/10.0.11/THIRD-PARTY-NOTICES.TXT` from the exact restored runtime pack | `6D15E10A101C6BFFF2AB4429ED061BF76C456FC4B23AD6B03E0D0F8377148A21` |

No runtime license text is downloaded from an unversioned upstream branch.
Self-contained publishing fails if the runtime version is not explicit, the
selected runtime is unavailable, or either runtime legal file is missing. The
installer has equivalent compile-time checks and installs all four files. For
the license page it uses an automatically generated UTF-8 BOM copy under
`artifacts/installer/`, avoiding ANSI misinterpretation without modifying the
Microsoft file distributed in the publish payload.

## Complete publish inventory

Publish directory: `artifacts/publish`

The following mutually exclusive groups account for all 480 files:

| Location and contents | Files | Bytes/notes |
|---|---:|---|
| Publish root: application, dependency, and runtime DLLs | 250 | `TimeLiner.dll`; the three NuGet dependency assemblies; 246 .NET and Windows Desktop runtime assemblies |
| Publish root: executables | 2 | `TimeLiner.exe`, `createdump.exe` |
| Publish root: runtime manifests | 2 | `TimeLiner.deps.json`, `TimeLiner.runtimeconfig.json` |
| Publish root: legal files | 4 | `LICENSE`, `THIRD-PARTY-NOTICES.txt`, `DOTNET-LICENSE.txt`, `DOTNET-THIRD-PARTY-NOTICES.txt` |
| Satellite resource directories | 221 | 17 resource DLLs in each of `cs`, `de`, `es`, `fr`, `it`, `ja`, `ko`, `pl`, `pt-BR`, `ru`, `tr`, `zh-Hans`, and `zh-Hant` |
| `Help` directory | 1 | `TimeLinerHelp.pdf`, 1,080,312 bytes |
| **Total** | **480** | **184,631,787 bytes** |

Extension cross-check: 471 DLL, 2 EXE, 2 JSON, 3 TXT, 1 PDF, and 1 extensionless
license file. There are no PDB, XML documentation, ZIP, MSI, NUPKG, user, test,
or test-data files in the publish directory.

## Installer inventory

Installer: `Setup/Output/TimeLiner_2.15.0.0.exe`

- Size: 55,137,151 bytes
- SHA-256: `2F0695B807EC55C184B35461FD8A21D5D8B1AAAA7C3025A798BCEAFD83232839`
- Payload: all 480 publish files, recursively, with the explicit exclusion of
  `*.xml` and `*.pdb` (neither exists in the current publish output)
- Generated with Inno Setup compiler 6.2.1
- The four required legal files are compile-time prerequisites and payload files
- The .NET distribution license is shown by the installer before installation;
  its display-only UTF-8 BOM copy preserves the French characters

## Verification

| Check | Result |
|---|---|
| Initial branch and synchronization | Clean `main`; `main` and `origin/main` at the same commit; audit branch created locally |
| Clean | Successful, 0 warnings, 0 errors |
| Tool restore | Successful (`Invoke-Build` 5.14.22, Inno Setup tool 6.2.1) |
| NuGet restore | Successful with forced, non-cached restore |
| Release/x64 build | Successful, 0 warnings, 0 errors |
| Complete tests | 151 passed, 0 failed, 0 skipped |
| Framework-dependent comparison publish | Successful |
| Self-contained `win-x64` publish | Successful, runtime 10.0.11 |
| Installer build | Successful, 0 warnings, 0 errors |
| Self-contained launch smoke test | Process remained running and opened `New - TimeLiner` while `DOTNET_ROOT` and `DOTNET_ROOT_X64` pointed to a nonexistent directory |
| Required legal files and hashes | Present and matched their sources |
| PDB/test/generated-build-file scan | No findings |
| Secret/private-key/private-company scan | No findings |
| Local absolute path scan | No findings after disabling Release debug information |
| `git diff --check` | Passed |

URLs found in the two notice files are expected public upstream and license
references. No private URLs were found.

## Remaining issues and blockers

There is no blocker to retaining and reviewing the locally prepared binary
distribution. Before a public binary release, perform one installation and
launch test on a clean Windows 11 x64 machine or VM without a separately
installed .NET runtime. The local self-contained smoke test is strong evidence,
but it is not a substitute for that clean-machine acceptance test.

The installer is not code-signed. Signing is not a licensing or functional
blocker, but the publisher should make an explicit signing decision before a
public download because an unsigned installer can produce Windows reputation
warnings.

No commit, tag, remote change, push, GitHub release, or publication was made.
