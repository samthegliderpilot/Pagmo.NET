using System;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;
using pagmo;

namespace Tests.Pagmo.NET.Interop;

[TestFixture]
public class Test_thread_cloneable_problem
{
    // ── fixture problems ──────────────────────────────────────────────────────

    private sealed class CloneableProblem : ManagedProblemBase
    {
        public static int CloneCallCount;

        private readonly bool _returnNullClone;
        private readonly bool _returnSameInstance;

        public CloneableProblem(bool returnNullClone = false, bool returnSameInstance = false)
        {
            _returnNullClone = returnNullClone;
            _returnSameInstance = returnSameInstance;
        }

        public override DoubleVector fitness(DoubleVector x) => Vec(x[0] * x[0]);
        public override PairOfDoubleVectors get_bounds() => Bounds(new[] { -5.0 }, new[] { 5.0 });
        public override string get_name() => "CloneableProblem";
        public override ThreadSafety get_thread_safety() => ThreadSafety.None;

        public override IProblem Clone()
        {
            if (_returnNullClone) return null;
            if (_returnSameInstance) return this;
            Interlocked.Increment(ref CloneCallCount);
            return new CloneableProblem();
        }
    }

    private sealed class NullCloneOnlyProblem : ManagedProblemBase, IThreadCloneableProblem
    {
        public override DoubleVector fitness(DoubleVector x) => Vec(x[0] * x[0]);
        public override PairOfDoubleVectors get_bounds() => Bounds(new[] { -5.0 }, new[] { 5.0 });
        public override string get_name() => "NullCloneOnlyProblem";
        public override ThreadSafety get_thread_safety() => ThreadSafety.None;
        public override IProblem Clone() => null;
    }

    private sealed class NonCloneableNonThreadSafeProblem : ManagedProblemBase
    {
        public override DoubleVector fitness(DoubleVector x) => Vec(x[0] * x[0]);
        public override PairOfDoubleVectors get_bounds() => Bounds(new[] { -5.0 }, new[] { 5.0 });
        public override ThreadSafety get_thread_safety() => ThreadSafety.None;
    }

    private sealed class ThreadSafeProblem : ManagedProblemBase
    {
        public static int CloneCallCount;

        public override DoubleVector fitness(DoubleVector x) => Vec(x[0] * x[0]);
        public override PairOfDoubleVectors get_bounds() => Bounds(new[] { -5.0 }, new[] { 5.0 });
        public override ThreadSafety get_thread_safety() => ThreadSafety.Basic;
        public override IProblem Clone() { Interlocked.Increment(ref CloneCallCount); return new ThreadSafeProblem(); }
    }

    [SetUp]
    public void ResetCounters()
    {
        CloneableProblem.CloneCallCount = 0;
        ThreadSafeProblem.CloneCallCount = 0;
    }

    // ── archipelago tests ─────────────────────────────────────────────────────

    [Test]
    public void CloneableNonThreadSafeProblemCanPushBackToArchipelago()
    {
        using var archi = new archipelago();
        using var problem = new CloneableProblem();
        using IAlgorithm algo = new de(5u);

        Assert.DoesNotThrow(() => archi.push_back_island(algo, problem, 10u, seed: 1u));
        archi.evolve(1u);
        archi.wait_check();
    }

    [Test]
    public void EachIslandGetsItsOwnClone()
    {
        const int islandCount = 4;
        using var archi = new archipelago();
        using var problem = new CloneableProblem();

        for (int i = 0; i < islandCount; i++)
        {
            using IAlgorithm algo = new de(5u);
            archi.push_back_island(algo, problem, 10u, seed: (uint)(20 + i));
        }

        Assert.That(CloneableProblem.CloneCallCount, Is.EqualTo(islandCount),
            "each push_back_island should produce exactly one clone");
    }

    [Test]
    public void NullCloneThrowsOnArchipelago()
    {
        using var archi = new archipelago();
        using var problem = new NullCloneOnlyProblem();
        using IAlgorithm algo = new de(5u);

        Assert.Throws<InvalidOperationException>(() =>
            archi.push_back_island(algo, problem, 10u, seed: 1u));
    }

    [Test]
    public void SameInstanceCloneThrowsOnArchipelago()
    {
        using var archi = new archipelago();
        using var problem = new CloneableProblem(returnSameInstance: true);
        using IAlgorithm algo = new de(5u);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            archi.push_back_island(algo, problem, 10u, seed: 1u));
        Assert.That(ex.Message, Does.Contain("same instance"));
    }

    [Test]
    public void NonCloneableNonThreadSafeStillThrowsOnArchipelago()
    {
        using var archi = new archipelago();
        using var problem = new NonCloneableNonThreadSafeProblem();
        using IAlgorithm algo = new de(5u);

        Assert.Throws<InvalidOperationException>(() =>
            archi.push_back_island(algo, problem, 10u, seed: 1u));
    }

    [Test]
    public void ThreadSafeProblemSkipsCloningOnArchipelago()
    {
        using var archi = new archipelago();
        using var problem = new ThreadSafeProblem();
        using IAlgorithm algo = new de(5u);

        archi.push_back_island(algo, problem, 10u, seed: 1u);

        Assert.That(ThreadSafeProblem.CloneCallCount, Is.EqualTo(0),
            "thread-safe problems should not be cloned — they are used directly");
    }

    // ── thread_bfe tests ──────────────────────────────────────────────────────

    [Test]
    public void CloneableNonThreadSafeProblemCanUseThreadBfe()
    {
        using var problem = new CloneableProblem();
        using var bfe = new thread_bfe();
        using var batchX = new DoubleVector(new[] { 1.0, 2.0, 3.0 });

        using var result = bfe.Operator(problem, batchX);

        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result[0], Is.EqualTo(1.0).Within(1e-12));
        Assert.That(result[1], Is.EqualTo(4.0).Within(1e-12));
        Assert.That(result[2], Is.EqualTo(9.0).Within(1e-12));
    }

    [Test]
    public void NullCloneThrowsOnThreadBfe()
    {
        using var problem = new NullCloneOnlyProblem();
        using var bfe = new thread_bfe();
        using var batchX = new DoubleVector(new[] { 1.0, 2.0 });

        Assert.Throws<InvalidOperationException>(() => bfe.Operator(problem, batchX));
    }

    [Test]
    public void ThreadBfeClonesAreDisposedAfterEvaluation()
    {
        int disposedCount = 0;

        var trackingProblem = new TrackingCloneableProblem(() => disposedCount++);
        using var bfe = new thread_bfe();
        using var batchX = new DoubleVector(new[] { 1.0, 2.0, 3.0, 4.0 });

        bfe.Operator(trackingProblem, batchX).Dispose();

        Assert.That(disposedCount, Is.GreaterThan(0),
            "at least one clone should have been disposed after batch evaluation");
    }

    private sealed class TrackingCloneableProblem : ManagedProblemBase
    {
        private readonly Action _onDispose;

        public TrackingCloneableProblem(Action onDispose = null) { _onDispose = onDispose; }

        public override DoubleVector fitness(DoubleVector x) => Vec(x[0] * x[0]);
        public override PairOfDoubleVectors get_bounds() => Bounds(new[] { -5.0 }, new[] { 5.0 });
        public override ThreadSafety get_thread_safety() => ThreadSafety.None;

        public override IProblem Clone() => new TrackingCloneableProblem(_onDispose);

        public override void Dispose() { _onDispose?.Invoke(); base.Dispose(); }
    }

    // ── stress test ───────────────────────────────────────────────────────────

    /// <summary>
    /// Runs a cloneable non-thread-safe problem under sustained parallel evolution to
    /// surface any GC-rooting, lifetime, or clone-isolation issues that only appear
    /// under load. Default is 5 s (CI-friendly). Bump <c>stressDurationSeconds</c>
    /// in the method body for a longer soak.
    /// </summary>
    [Test]
    [Timeout(int.MaxValue)]
    public void StressTest_CloneableProblemsRemainStableUnderExtendedParallelEvolution()
    {
        double stressDurationSeconds = 5.0; // increase and rebuild for a longer soak
        const int IslandCount = 8;
        const uint PopSize = 20u;
        const uint EvolvesPerBatch = 3u;

        using var archi = new archipelago();
        using var baseProblem = new StressProblem();

        for (int i = 0; i < IslandCount; i++)
        {
            using IAlgorithm algo = new de(20u);
            archi.push_back_island(algo, baseProblem, PopSize, seed: (uint)(100 + i));
        }

        Assert.That(StressProblem.TotalClones, Is.EqualTo(IslandCount),
            "each island should receive exactly one clone");

        var sw = Stopwatch.StartNew();
        int batches = 0;

        while (sw.Elapsed.TotalSeconds < stressDurationSeconds)
        {
            archi.evolve(EvolvesPerBatch);
            archi.wait_check(); // throws if any island threw — catches native crashes too

            batches++;

            // Periodically stress the GC to try to surface any rooting bugs:
            // if a clone or its callback adapter is incorrectly collectible, this
            // will cause a use-after-free in the native callback.
            if (batches % 10 == 0)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }

        Assert.That(batches, Is.GreaterThan(0));
        Assert.That(StressProblem.TotalEvaluations, Is.GreaterThan(0),
            "fitness should have been evaluated at least once");
    }

    private sealed class StressProblem : ManagedProblemBase
    {
        public static int TotalClones;
        public static long TotalEvaluations;

        // Per-instance mutable state — corruption here would indicate shared state between clones.
        private int _instanceEvaluations;

        public override DoubleVector fitness(DoubleVector x)
        {
            // Increment both per-instance and global counters.
            Interlocked.Increment(ref _instanceEvaluations);
            Interlocked.Increment(ref TotalEvaluations);

            // Negate the sum so the optimizer always wants larger values — no convergence
            // within the wide bounds, so evolution keeps running for the full duration.
            double f = 0;
            for (int i = 0; i < x.Count; i++) f -= x[i];
            return Vec(f);
        }

        private static readonly double[] Lower = [-1e6, -1e6, -1e6];
        private static readonly double[] Upper = [1e6, 1e6, 1e6];

        public override PairOfDoubleVectors get_bounds() => Bounds(Lower, Upper);

        public override string get_name() => "StressProblem";
        public override ThreadSafety get_thread_safety() => ThreadSafety.None;

        public override IProblem Clone()
        {
            Interlocked.Increment(ref TotalClones);
            return new StressProblem();
        }
    }
}
