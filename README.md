# Pagmo.NET

![Pagmo.NET](logo_small.png)

**Pagmo.NET** is a C# wrapper for [pagmo2](https://esa.github.io/pagmo2/), a C++ library
providing high-quality metaheuristic and gradient-based optimization routines with multi-island
parallel evolution support.

The wrapper is built with [SWIG 4.4](https://www.swig.org/) and supports Windows x64 and Linux x64.

```
dotnet add package Pagmo.NET --version 1.0.0-beta.2
```

The NuGet package is self-contained — native runtime libraries for Windows x64 and Linux x64
are bundled in `runtimes/`. No additional installation required.

Source archives and individual native bundles are available at
`https://github.com/samthegliderpilot/Pagmo.NET/releases`.

---

## Build requirements (contributors / from source)

### Windows x64

- .NET 10 SDK
- Visual Studio 2022 / Build Tools 2022 (C++ toolchain)
- SWIG 4.4.x (for wrapper regeneration only — pre-generated wrappers are checked in)
- vcpkg with pagmo2: `vcpkg install pagmo2[nlopt,ipopt]:x64-windows-static-md`

To regenerate SWIG wrappers after editing the interface file:

```powershell
pwsh createSwigWrappersAndPlaceThem.ps1
```

SWIG is resolved via `SWIG_EXE` env var, `SWIG_HOME`, or `PATH`.

### Linux x64

The native library statically links pagmo2, Boost.Serialization, TBB, NLopt, and IPOPT
via vcpkg's `x64-linux-static-pic` triplet — `libPagmoWrapper.so` has no system runtime
dependencies beyond the standard C++ runtime.

**One-time setup:**

```bash
# Build tools, SWIG, and .NET
sudo apt install -y cmake build-essential swig

# .NET 10 SDK
sudo tee /etc/apt/preferences.d/dotnet-ubuntu << 'EOF'
Package: dotnet* aspnetcore*
Pin: release o=Ubuntu
Pin-Priority: 1001
EOF
sudo apt update && sudo apt install -y dotnet-sdk-10.0

# PowerShell Core
wget -q https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O ms-prod.deb
sudo dpkg -i ms-prod.deb && sudo apt update && sudo apt install -y powershell

# vcpkg
git clone https://github.com/microsoft/vcpkg ~/vcpkg
~/vcpkg/bootstrap-vcpkg.sh
export VCPKG_ROOT=~/vcpkg   # add to ~/.bashrc or ~/.profile
```

**Build and test:**

```bash
pwsh scripts/build-native.ps1 -Configuration Debug
dotnet test Tests/Tests.Pagmo.NET/Tests.Pagmo.NET.csproj -p:Platform=x64
dotnet run --project Examples/Examples.Pagmo.NET/Examples.Pagmo.NET.csproj -- all
```

> **Note on .NET SDK:** The library targets `net8.0`. Tests and examples target `net10.0`
> because the .NET 8 VsTest runner on Linux only discovers ~198 of 593 tests due to a known
> discovery issue. Consumers can reference the package from any .NET 8+ project.

---

## VS Code workflow

Repo includes tasks and launch configs in `.vscode/`:

- `Pagmo.NET: regenerate SWIG wrappers`
- `Pagmo.NET: build native (Debug x64)`
- `Pagmo.NET: build tests (Debug x64)`
- `Pagmo.NET: build examples (Debug x64)`
- `Pagmo.NET: test (Debug x64)`
- `Pagmo.NET: Run Tests` (launch config)
- `Pagmo.NET: Debug Examples` (launch config)

**Windows requirements:** Visual Studio Build Tools 2022, .NET 10 SDK, PowerShell Core,
vcpkg, VS Code extensions: `ms-dotnettools.csharp`, `ms-dotnettools.csdevkit`,
`ms-vscode.cpptools`, `ms-vscode.powershell`

**Linux requirements:** cmake, build-essential, swig, vcpkg, .NET 10 SDK, PowerShell Core,
same VS Code extensions. Build the native library first before using VS Code tasks.

**C++ include path resolution for `pagmoWrapper.vcxproj`:**
- `PagmoVcpkgInstalledDir` MSBuild property (preferred), or
- `VCPKG_INSTALLED_DIR` environment variable, or
- repo-relative fallback: `$(SolutionDir)..\\vcpkg\\installed`

---

## Managed problem architecture (C# UDP support)

1. User implements `IProblem` / `ManagedProblemBase` in C#
2. A SWIG director adapter (`problem_callback`) forwards calls to managed code
3. Native bridge wraps callback into `managed_problem` (`std::shared_ptr` owned)
4. A real `pagmo::problem` is built from `managed_problem`
5. `population`, `archipelago`, and BFE operator helpers consume that `pagmo::problem`

Ownership lives on the native side via `shared_ptr`, avoiding raw-pointer lifetime bugs.

**Minimal UDP:**
```csharp
public override DoubleVector fitness(DoubleVector x) { ... }
public override PairOfDoubleVectors get_bounds() { ... }
```

**Threading — thread-safe problems:** declare `ThreadSafety.Basic` or `ThreadSafety.Constant`
and the instance is shared directly across threads.

**Threading — cloneable non-thread-safe problems:** implement `IThreadCloneableProblem` and
override `Clone()` on `ManagedProblemBase`. The system creates one exclusive clone per island
(`archipelago`) or per OS thread (`thread_bfe`). The clone reports `ThreadSafety.Basic`
transparently so pagmo's native `thread_island` check passes. Problems returning `null` from
`Clone()` (the default) are still rejected on threaded entrypoints.

---

## FAQ

**Where's SNOPT7?**
SNOPT7 is proprietary and cannot be bundled. Users with a license can build from source:
1. Obtain SNOPT7 headers and compiled shared library from Stanford University.
2. Build pagmo2 with `-DPAGMO_WITH_SNOPT7=ON`.
3. Add `#define PAGMO_WITH_SNOPT7` to `swig/pagmo/config.hpp`.
4. Copy `pagmo/algorithms/snopt7.hpp` into `swig/pagmo/algorithms/`.
5. Run `pwsh scripts/regen-swig.ps1` then `pwsh scripts/build-native.ps1`.
6. Build `Pagmo.NET.csproj` — MSBuild detects the generated `snopt7.cs` automatically.

At runtime, pagmo's `snopt7` loads the solver DLL dynamically:
```csharp
using var solver = new snopt7(screenOutput: false, snopt7LibPath: "path/to/snopt7.dll", minorVersion: 6u);
```
Set `SNOPT7_LIB` to the DLL path to enable the live execution test.

**Is this affiliated with ESA or the pagmo2 team?**
No — Pagmo.NET is an independent .NET binding.

---

## Runnable examples

```bash
dotnet run --project Examples/Examples.Pagmo.NET/Examples.Pagmo.NET.csproj -- all
```

Scenarios: `single`, `archipelago`, `policies`, `maneuver`. Add `--verbose` to print
algorithm logs.

Concept-first walkthroughs in `docs/`:
- `docs/getting-started.md`
- `docs/archipelago-topology-policies.md`

---

## API naming

- Managed extension helpers use C#-style PascalCase.
- Existing snake_case entrypoints are retained for pagmo parity.
- See `.ai/API_NAMING_POLICY.md` for the full policy.
