# IThreadCloneableProblem — Per-Thread Problem Cloning

## Motivation

Managed `IProblem` / `ManagedProblemBase` implementations default to `ThreadSafety.None`, which causes `archipelago.push_back_island()` and `thread_bfe.Operator()` to throw. The only way to use a managed problem on those paths today is to declare `ThreadSafety.Basic` or `ThreadSafety.Constant` — meaning the problem must be safe to call concurrently from multiple threads.

This design adds an opt-in cloning contract: a problem that cannot be used concurrently but can produce independent copies of itself provides `Clone()`. The system creates one clone per island or per OS thread, so each uses its own exclusive instance. This is a **pure managed-side change** — no native C++ modifications, no SWIG regeneration.

---

## Interface

```csharp
public interface IThreadCloneableProblem : IProblem
{
    /// <summary>
    /// Returns a fully independent copy of this problem for exclusive use on a
    /// single thread or island. Must never return null.
    /// </summary>
    IProblem Clone();
}
```

---

## ManagedProblemBase — opt-in via override

`ManagedProblemBase` implements `IThreadCloneableProblem` with a default `Clone()` returning `null` (opt-out). Subclasses override to opt in:

```csharp
public abstract partial class ManagedProblemBase : IProblem, IThreadCloneableProblem
{
    public virtual IProblem Clone() => null;
}
```

A `null` return means "I don't support cloning" — the existing `ThrowIfNotThreadSafe` guard fires as before.

**Example consumer:**

```csharp
public class MyExpensiveProblem : ManagedProblemBase
{
    private readonly SomeMutableState _state = new();

    public override DoubleVector fitness(DoubleVector x) { /* uses _state */ }
    public override PairOfDoubleVectors get_bounds() => ...;
    public override ThreadSafety get_thread_safety() => ThreadSafety.None;

    public override IProblem Clone() => new MyExpensiveProblem(); // fresh state
}
```

---

## Archipelago: clone per island

`WithManagedProblem` (in `archipelago.cs`) changes from `private static` to `private` (instance). When the problem is `IThreadCloneableProblem` with a non-null `Clone()` result, the clone is used as the effective problem for that island.

The clone is added to a `_managedProblemCloneRoots` field (`List<IProblem>`) on the archipelago partial class. This keeps the clone — and therefore its `ProblemCallbackAdapter` GCHandle — alive for the lifetime of the archipelago. No changes to `NativeInterop` are needed.

Guards:
- `Clone()` returning `null` → falls through to existing `ThrowIfNotThreadSafe`
- `Clone()` returning `this` (same instance) → `InvalidOperationException` with descriptive message

---

## thread_bfe: clone per OS thread via `managed_thread_bfe`

A new pure-managed evaluator (`BatchEvaluators/managed_thread_bfe.cs`) uses `Parallel.For` with `ThreadLocal<IProblem>` to give each OS thread its own clone. After evaluation, all clones are disposed.

`BfeBridge.BatchEvaluate` in `bfe.cs` gains a guard before `ThrowIfNotThreadSafe`:

```csharp
if (requiresParallelSafety
    && problem.get_thread_safety() == ThreadSafety.None
    && problem is IThreadCloneableProblem cloneable)
{
    using var bfe = new managed_thread_bfe();
    return bfe.Operator(cloneable, batchX);
}
```

Thread-safe problems that also happen to implement `IThreadCloneableProblem` continue through the native `thread_bfe` path (the guard only fires for `ThreadSafety.None`).

---

## Files to Change / Create

| File | Change |
|---|---|
| `Pagmo.NET/pagmoExtensions/Problems/IThreadCloneableProblem.cs` | **New** — interface |
| `Pagmo.NET/pagmoExtensions/Problems/ManagedProblemBase.cs` | Add `IThreadCloneableProblem` to implements; add `virtual Clone() => null` |
| `Pagmo.NET/pagmoExtensions/Problems/IProblemThreadingExtensions.cs` | Improve error message to hint at `Clone()` when interface present but null |
| `Pagmo.NET/pagmoExtensions/archipelago.cs` | `_managedProblemCloneRoots` field; `WithManagedProblem` → instance method + clone path |
| `Pagmo.NET/pagmoExtensions/BatchEvaluators/bfe.cs` | `BfeBridge.BatchEvaluate` — add cloneable guard before thread-safety check |
| `Pagmo.NET/pagmoExtensions/BatchEvaluators/managed_thread_bfe.cs` | **New** — `Parallel.For` + `ThreadLocal<IProblem>` evaluator |
| `Tests/.../Test_thread_cloneable_problem.cs` | **New** — test cases |

**No changes** to `problem.h`, `managed_bridge.cpp`, any `.i` SWIG file, or `NativeInterop.cs`.

---

## Test Cases

1. `CloneableNonThreadSafeProblemCanPushBackToArchipelago` — push_back_island + evolve/wait_check succeeds
2. `EachIslandGetsItsOwnClone` — Interlocked clone counter; after N push_back_island calls, counter == N
3. `CloneableNonThreadSafeProblemCanUseThreadBfe` — `thread_bfe.Operator` succeeds and returns correct fitness shape
4. `ThreadBfeClonesAreDisposedAfterEvaluation` — disposed flag in Clone; all clones disposed after Operator returns
5. `NullCloneThrowsOnArchipelago` — `IThreadCloneableProblem` with `Clone()` → null → `InvalidOperationException`
6. `NullCloneThrowsOnThreadBfe` — same for thread_bfe path
7. `SameInstanceCloneThrowsOnArchipelago` — `Clone()` returning `this` → `InvalidOperationException`
8. `NonCloneableNonThreadSafeStillThrowsOnArchipelago` — existing rejection path unchanged
9. `ThreadSafeProblemSkipsCloningOnArchipelago` — no Clone() calls when `ThreadSafety.Basic`
