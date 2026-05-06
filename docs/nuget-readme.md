# Pagmo.NET

![Pagmo.NET](https://raw.githubusercontent.com/samthegliderpilot/Pagmo.NET/main/logo_small.png)

**Pagmo.NET** is a C# wrapper for [pagmo2](https://esa.github.io/pagmo2/), a C++ library
providing high-quality metaheuristic and gradient-based optimization algorithms with
multi-island parallel evolution support.

Supported platforms: **Windows x64**, **Linux x64**, and **macOS** (arm64 + x86_64
universal binary) — native runtime libraries are bundled in the package. No additional
installation required.

## Installation

```
dotnet add package Pagmo.NET --version 1.0.0-beta.2
```

## Quickstart

```csharp
using pagmo;

public sealed class SphereProblem : ManagedProblemBase
{
    public override PairOfDoubleVectors get_bounds() => Bounds(
        new[] { -5.0, -5.0 },
        new[] { 5.0, 5.0 });

    public override DoubleVector fitness(DoubleVector x)
        => Vec(x[0] * x[0] + x[1] * x[1]);

    public override ThreadSafety get_thread_safety() => ThreadSafety.Basic;
}

using IAlgorithm algo = new de(100u);
using var udp = new SphereProblem();
using var isl = island.Create(algo, udp, popSize: 64u, seed: 2u);
isl.evolve(1u);
isl.wait_check();

using var result = isl.get_population();
using var bestX = result.champion_x();
using var bestF = result.champion_f();
```

Use `island.Create(...)` for single-island runs and `archipelago.push_back_island(...)`
for multi-island parallel search.

## Defining custom problems

Implement `IProblem` or inherit from `ManagedProblemBase`. Only two methods are required:

```csharp
public override DoubleVector fitness(DoubleVector x) { ... }
public override PairOfDoubleVectors get_bounds() { ... }
```

Optional capabilities — gradients, hessians, batch evaluation, seed hooks, metadata —
have safe defaults and can be overridden as needed. `ManagedProblemBase` includes helpers
`Vec(...)` and `Bounds(lower, upper)` for concise authoring.

## Threading

**Thread-safe problems** — declare `ThreadSafety.Basic` and the problem instance is shared
across threads directly:

```csharp
public override ThreadSafety get_thread_safety() => ThreadSafety.Basic;
```

**Non-thread-safe problems that can be cloned** — implement `IThreadCloneableProblem` and
override `Clone()` on `ManagedProblemBase`. Each island or OS thread receives its own
exclusive copy, so the problem never needs to be concurrency-safe:

```csharp
public override ThreadSafety get_thread_safety() => ThreadSafety.None;

public override IProblem Clone() => new MyProblem(); // fresh independent copy
```

Problems declaring `ThreadSafety.None` without a working `Clone()` are rejected on
threaded paths (`archipelago.push_back_island`, `thread_bfe`) with a descriptive error.

## Runnable examples

```bash
dotnet run --project Examples/Examples.Pagmo.NET/Examples.Pagmo.NET.csproj -- all
```

Scenarios: `single`, `archipelago`, `policies`, `maneuver`. Add `--verbose` to print
algorithm logs after each scenario.

## What's included

| Feature | Notes |
|---|---|
| All major pagmo algorithms | `de`, `sade`, `pso`, `cmaes`, `nsga2`, `moead`, and more |
| All built-in benchmark problems | `rosenbrock`, `rastrigin`, `dtlz`, `wfg`, and more |
| Multi-island archipelago | Topology, migration, and policy wiring |
| Custom C# problems (`IProblem` / `ManagedProblemBase`) | Full lifecycle with exception-safe callbacks |
| Per-thread problem cloning (`IThreadCloneableProblem`) | Non-thread-safe problems can participate in threaded paths via `Clone()` |
| Custom C# algorithms (`IAlgorithm`) | Participates in island/archipelago type-erased flows |
| NLopt | Statically linked — available out of the box |
| IPOPT | Statically linked — available out of the box |
| SNOPT7 | Not included (proprietary license; build from source with your own license) |
| macOS (arm64 + x86_64) | Supported — universal binary; NLopt included, IPOPT coming |
| x86 / ARM | Not supported in v1 |
| .NET Framework | Not supported |

## FAQ

**Why .NET and C#?**
The .NET ecosystem is underappreciated for scientific computing. C# performance is
competitive with C++ for many workloads, and .NET's open-source trajectory makes it
a strong foundation for numerical work. Pagmo.NET lets teams use pagmo's optimization
power without a Python runtime dependency.

**Why SWIG?**
SWIG handles the repetitive wrapping work across a large API surface. The `.i` interface
file is also a useful starting point for bindings in other languages.

**Where's IPOPT?**
Included and statically linked on both Windows and Linux.
`OptionalSolverAvailability.IsIpoptAvailable` returns `true` out of the box.

**Where's SNOPT7?**
SNOPT7 is proprietary and cannot be bundled. Users with a SNOPT7 license can build
Pagmo.NET from source with SNOPT7 support — see the
[repository README](https://github.com/samthegliderpilot/Pagmo.NET) for instructions.

**Is this affiliated with ESA or the pagmo2 team?**
No — Pagmo.NET is an independent .NET binding, not affiliated with ESA or the pagmo2
developers.

## Links

- [Source repository](https://github.com/samthegliderpilot/Pagmo.NET)
- [pagmo2 documentation](https://esa.github.io/pagmo2/)
- [Release notes](https://github.com/samthegliderpilot/Pagmo.NET/releases)
