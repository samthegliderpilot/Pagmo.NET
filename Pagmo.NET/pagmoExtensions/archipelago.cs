using System;
using System.Collections.Generic;

namespace pagmo
{
    /// <summary>
    /// Managed convenience surface for archipelago construction and island insertion from managed types.
    /// </summary>
    public partial class archipelago
    {
        // Keeps clones alive for the lifetime of this archipelago so their
        // ProblemCallbackAdapter GCHandles (keyed on the clone in NativeInterop)
        // remain reachable while the island's native callback is in use.
        private readonly List<IProblem> _managedProblemCloneRoots = new();

        private ulong WithManagedProblem(IProblem problem, Func<problem, ulong> action)
        {
            if (problem == null) throw new ArgumentNullException(nameof(problem));

            if (problem.get_thread_safety() == ThreadSafety.None
                && problem is IThreadCloneableProblem cloneable)
            {
                var clone = cloneable.Clone();
                if (clone != null)
                {
                    if (ReferenceEquals(clone, problem))
                        throw new InvalidOperationException(
                            $"'{problem.get_name()}.Clone()' returned the same instance. Clone() must return an independent copy.");
                    // Wrap the clone so it reports ThreadSafety.Basic — the clone is exclusively
                    // owned by this island so single-threaded access is guaranteed, satisfying
                    // thread_island's native thread-safety check.
                    var adapter = new ExclusiveCloneAdapter(clone);
                    _managedProblemCloneRoots.Add(adapter);
                    using var wrappedClone = new problem(adapter);
                    return action(wrappedClone);
                }
            }

            problem.ThrowIfNotThreadSafe();
            using var wrappedProblem = new problem(problem);
            return action(wrappedProblem);
        }

        // Wraps a clone and re-declares thread safety as Basic since the clone is
        // exclusively owned by one island and will only be accessed from one thread.
        private sealed class ExclusiveCloneAdapter : ManagedProblemBase
        {
            private readonly IProblem _inner;
            public ExclusiveCloneAdapter(IProblem inner) { _inner = inner; }

            public override DoubleVector fitness(DoubleVector x) => _inner.fitness(x);
            public override PairOfDoubleVectors get_bounds() => _inner.get_bounds();
            public override ThreadSafety get_thread_safety() => ThreadSafety.Basic;
            public override string get_name() => _inner.get_name();
            public override string get_extra_info() => _inner.get_extra_info();
            public override uint get_nobj() => _inner.get_nobj();
            public override uint get_nec() => _inner.get_nec();
            public override uint get_nic() => _inner.get_nic();
            public override uint get_nix() => _inner.get_nix();
            public override bool has_gradient() => _inner.has_gradient();
            public override DoubleVector gradient(DoubleVector x) => _inner.gradient(x);
            public override bool has_gradient_sparsity() => _inner.has_gradient_sparsity();
            public override SparsityPattern gradient_sparsity() => _inner.gradient_sparsity();
            public override bool has_hessians() => _inner.has_hessians();
            public override VectorOfVectorOfDoubles hessians(DoubleVector x) => _inner.hessians(x);
            public override bool has_hessians_sparsity() => _inner.has_hessians_sparsity();
            public override VectorOfSparsityPattern hessians_sparsity() => _inner.hessians_sparsity();
            public override bool has_batch_fitness() => _inner.has_batch_fitness();
            public override DoubleVector batch_fitness(DoubleVector dvs) => _inner.batch_fitness(dvs);
            public override bool has_set_seed() => _inner.has_set_seed();
            public override void set_seed(uint seed) => _inner.set_seed(seed);
            public override void Dispose() { _inner.Dispose(); base.Dispose(); }
        }

        private static uint ToNativePopulationSize(ulong populationSize)
        {
            return SizeTInterop.ToNativeUInt32(populationSize, "popSize");
        }

        private static void ValidatePolicyPair(object replacementPolicy, object selectionPolicy, string replacementPolicyName, string selectionPolicyName)
        {
            var replacementProvided = replacementPolicy != null;
            var selectionProvided = selectionPolicy != null;
            if (replacementProvided == selectionProvided)
            {
                return;
            }

            throw new ArgumentException($"Both {replacementPolicyName} and {selectionPolicyName} must be provided together.");
        }

        private ulong PushBackCore(
            algorithm algorithm,
            IProblem problem,
            ulong populationSize,
            uint seed,
            bfe evaluator = null,
            thread_island islandType = null,
            fair_replace replacementPolicy = null,
            select_best selectionPolicy = null,
            r_policy wrappedReplacementPolicy = null,
            s_policy wrappedSelectionPolicy = null)
        {
            var nativePopulationSize = ToNativePopulationSize(populationSize);

            return WithManagedProblem(problem, wrappedProblem =>
            {
                var hasFairPolicies = replacementPolicy != null;
                var hasWrappedPolicies = wrappedReplacementPolicy != null;

                if (!hasFairPolicies && !hasWrappedPolicies)
                {
                    if (islandType == null)
                    {
                        return evaluator == null
                            ? push_back_island(algorithm, wrappedProblem, nativePopulationSize, seed)
                            : push_back_island(algorithm, wrappedProblem, evaluator, nativePopulationSize, seed);
                    }

                    return evaluator == null
                        ? push_back_island(islandType, algorithm, wrappedProblem, nativePopulationSize, seed)
                        : push_back_island(islandType, algorithm, wrappedProblem, evaluator, nativePopulationSize, seed);
                }

                if (hasFairPolicies)
                {
                    if (islandType == null)
                    {
                        return evaluator == null
                            ? push_back_island(algorithm, wrappedProblem, nativePopulationSize, replacementPolicy, selectionPolicy, seed)
                            : push_back_island(algorithm, wrappedProblem, evaluator, nativePopulationSize, replacementPolicy, selectionPolicy, seed);
                    }

                    return evaluator == null
                        ? push_back_island(islandType, algorithm, wrappedProblem, nativePopulationSize, replacementPolicy, selectionPolicy, seed)
                        : push_back_island(islandType, algorithm, wrappedProblem, evaluator, nativePopulationSize, replacementPolicy, selectionPolicy, seed);
                }

                if (islandType == null)
                {
                    return evaluator == null
                        ? push_back_island(algorithm, wrappedProblem, nativePopulationSize, wrappedReplacementPolicy, wrappedSelectionPolicy, seed)
                        : push_back_island(algorithm, wrappedProblem, evaluator, nativePopulationSize, wrappedReplacementPolicy, wrappedSelectionPolicy, seed);
                }

                return evaluator == null
                    ? push_back_island(islandType, algorithm, wrappedProblem, nativePopulationSize, wrappedReplacementPolicy, wrappedSelectionPolicy, seed)
                    : push_back_island(islandType, algorithm, wrappedProblem, evaluator, nativePopulationSize, wrappedReplacementPolicy, wrappedSelectionPolicy, seed);
            });
        }

        private ulong PushBackWithManagedPolicies(
            algorithm algorithm,
            IProblem problem,
            ulong populationSize,
            uint seed,
            RPolicyCallback replacementPolicy,
            SPolicyCallback selectionPolicy,
            bfe evaluator = null,
            thread_island islandType = null)
        {
            using var wrappedReplacementPolicy = new r_policy(replacementPolicy);
            using var wrappedSelectionPolicy = new s_policy(selectionPolicy);
            return PushBackCore(
                algorithm,
                problem,
                populationSize,
                seed,
                evaluator,
                islandType,
                wrappedReplacementPolicy: wrappedReplacementPolicy,
                wrappedSelectionPolicy: wrappedSelectionPolicy);
        }

        private ulong PushBackNormalizedAlgorithm(
            IAlgorithm algorithm,
            IProblem problem,
            ulong populationSize,
            uint seed,
            bfe evaluator = null,
            thread_island islandType = null,
            fair_replace replacementPolicy = null,
            select_best selectionPolicy = null)
        {
            using var normalized = AlgorithmInterop.NormalizeToTypeErased(algorithm);
            return PushBackCore(normalized, problem, populationSize, seed, evaluator, islandType, replacementPolicy, selectionPolicy);
        }

        /// <summary>
        /// Returns a snapshot of migration log entries.
        /// </summary>
        public IReadOnlyList<MigrationEntry> MigrationLog
        {
            get
            {
                using var native = get_migration_log_entries();
                var list = new List<MigrationEntry>(native.Count);
                for (var i = 0; i < native.Count; ++i)
                {
                    list.Add(native[i]);
                }

                return list;
            }
        }

        /// <summary>
        /// Returns a snapshot of the migrants database.
        /// </summary>
        public IReadOnlyList<IndividualsGroup> MigrantsDb
        {
            get
            {
                using var native = get_migrants_db();
                var list = new List<IndividualsGroup>(native.Count);
                for (var i = 0; i < native.Count; ++i)
                {
                    list.Add(native[i]);
                }

                return list;
            }
        }

        /// <summary>
        /// Returns a copy of an island by index.
        /// </summary>
        public island GetIslandCopy(ulong index)
        {
            return get_island_copy(SizeTInterop.ToNativeUInt32(index, nameof(index)));
        }

        /// <summary>
        /// Adds an island from a type-erased algorithm and managed problem.
        /// </summary>
        public ulong push_back_island(algorithm algorithm, IProblem problem, ulong populationSize, uint seed = 0u, thread_island islandType = null)
        {
            return PushBackCore(algorithm, problem, populationSize, seed, islandType: islandType);
        }

        /// <summary>
        /// Adds an island with managed replacement/selection policies.
        /// </summary>
        public ulong push_back_island(algorithm algorithm, IProblem problem, ulong populationSize, RPolicyCallback replacementPolicy, SPolicyCallback selectionPolicy, uint seed = 0u, thread_island islandType = null)
        {
            ValidatePolicyPair(replacementPolicy, selectionPolicy, nameof(replacementPolicy), nameof(selectionPolicy));
            return PushBackWithManagedPolicies(algorithm, problem, populationSize, seed, replacementPolicy, selectionPolicy, islandType: islandType);
        }

        /// <summary>
        /// Adds an island with built-in replacement/selection policies.
        /// </summary>
        public ulong push_back_island(algorithm algorithm, IProblem problem, ulong populationSize, fair_replace replacementPolicy, select_best selectionPolicy, uint seed = 0u, thread_island islandType = null)
        {
            ValidatePolicyPair(replacementPolicy, selectionPolicy, nameof(replacementPolicy), nameof(selectionPolicy));
            return PushBackCore(algorithm, problem, populationSize, seed, islandType: islandType, replacementPolicy: replacementPolicy, selectionPolicy: selectionPolicy);
        }

        /// <summary>
        /// Adds an island with an explicit batch fitness evaluator.
        /// </summary>
        public ulong push_back_island(algorithm algorithm, IProblem problem, bfe evaluator, ulong populationSize, uint seed = 0u, thread_island islandType = null)
        {
            return PushBackCore(algorithm, problem, populationSize, seed, evaluator, islandType);
        }

        /// <summary>
        /// Adds an island with explicit evaluator and managed policies.
        /// </summary>
        public ulong push_back_island(algorithm algorithm, IProblem problem, bfe evaluator, ulong populationSize, RPolicyCallback replacementPolicy, SPolicyCallback selectionPolicy, uint seed = 0u, thread_island islandType = null)
        {
            ValidatePolicyPair(replacementPolicy, selectionPolicy, nameof(replacementPolicy), nameof(selectionPolicy));
            return PushBackWithManagedPolicies(algorithm, problem, populationSize, seed, replacementPolicy, selectionPolicy, evaluator, islandType);
        }

        /// <summary>
        /// Adds an island with explicit evaluator and built-in policies.
        /// </summary>
        public ulong push_back_island(algorithm algorithm, IProblem problem, bfe evaluator, ulong populationSize, fair_replace replacementPolicy, select_best selectionPolicy, uint seed = 0u, thread_island islandType = null)
        {
            ValidatePolicyPair(replacementPolicy, selectionPolicy, nameof(replacementPolicy), nameof(selectionPolicy));
            return PushBackCore(algorithm, problem, populationSize, seed, evaluator, islandType, replacementPolicy, selectionPolicy);
        }

        /// <summary>
        /// Adds an island from a managed algorithm and managed problem.
        /// </summary>
        public ulong push_back_island(IAlgorithm algorithm, IProblem problem, ulong populationSize, uint seed = 0u, thread_island islandType = null)
        {
            return PushBackNormalizedAlgorithm(algorithm, problem, populationSize, seed, islandType: islandType);
        }

        /// <summary>
        /// Adds an island with managed policies.
        /// </summary>
        public ulong push_back_island(IAlgorithm algorithm, IProblem problem, ulong populationSize, RPolicyCallback replacementPolicy, SPolicyCallback selectionPolicy, uint seed = 0u, thread_island islandType = null)
        {
            ValidatePolicyPair(replacementPolicy, selectionPolicy, nameof(replacementPolicy), nameof(selectionPolicy));
            using var normalized = AlgorithmInterop.NormalizeToTypeErased(algorithm);
            return PushBackWithManagedPolicies(normalized, problem, populationSize, seed, replacementPolicy, selectionPolicy, islandType: islandType);
        }

        /// <summary>
        /// Adds an island with built-in policies.
        /// </summary>
        public ulong push_back_island(IAlgorithm algorithm, IProblem problem, ulong populationSize, fair_replace replacementPolicy, select_best selectionPolicy, uint seed = 0u, thread_island islandType = null)
        {
            ValidatePolicyPair(replacementPolicy, selectionPolicy, nameof(replacementPolicy), nameof(selectionPolicy));
            return PushBackNormalizedAlgorithm(algorithm, problem, populationSize, seed, islandType: islandType, replacementPolicy: replacementPolicy, selectionPolicy: selectionPolicy);
        }

        /// <summary>
        /// Adds an island with explicit evaluator.
        /// </summary>
        public ulong push_back_island(IAlgorithm algorithm, IProblem problem, bfe evaluator, ulong populationSize, uint seed = 0u, thread_island islandType = null)
        {
            return PushBackNormalizedAlgorithm(algorithm, problem, populationSize, seed, evaluator, islandType);
        }

        /// <summary>
        /// Adds an island with explicit evaluator and managed policies.
        /// </summary>
        public ulong push_back_island(IAlgorithm algorithm, IProblem problem, bfe evaluator, ulong populationSize, RPolicyCallback replacementPolicy, SPolicyCallback selectionPolicy, uint seed = 0u, thread_island islandType = null)
        {
            ValidatePolicyPair(replacementPolicy, selectionPolicy, nameof(replacementPolicy), nameof(selectionPolicy));
            using var normalized = AlgorithmInterop.NormalizeToTypeErased(algorithm);
            return PushBackWithManagedPolicies(normalized, problem, populationSize, seed, replacementPolicy, selectionPolicy, evaluator, islandType);
        }

        /// <summary>
        /// Adds an island with explicit evaluator and built-in policies.
        /// </summary>
        public ulong push_back_island(IAlgorithm algorithm, IProblem problem, bfe evaluator, ulong populationSize, fair_replace replacementPolicy, select_best selectionPolicy, uint seed = 0u, thread_island islandType = null)
        {
            ValidatePolicyPair(replacementPolicy, selectionPolicy, nameof(replacementPolicy), nameof(selectionPolicy));
            return PushBackNormalizedAlgorithm(algorithm, problem, populationSize, seed, evaluator, islandType, replacementPolicy, selectionPolicy);
        }

        /// <summary>
        /// PascalCase alias for <see cref="GetIslandCopy"/>.
        /// </summary>
        public island GetIsland(ulong index)
        {
            return GetIslandCopy(index);
        }

        /// <summary>
        /// PascalCase alias for <see cref="push_back_island(IAlgorithm,IProblem,ulong,uint,thread_island)"/>.
        /// </summary>
        public ulong PushBackIsland(IAlgorithm algorithm, IProblem problem, ulong populationSize, uint seed = 0u, thread_island islandType = null)
        {
            return push_back_island(algorithm, problem, populationSize, seed, islandType);
        }

        /// <summary>
        /// PascalCase alias for <see cref="push_back_island(IAlgorithm,IProblem,bfe,ulong,uint,thread_island)"/>.
        /// </summary>
        public ulong PushBackIsland(IAlgorithm algorithm, IProblem problem, bfe evaluator, ulong populationSize, uint seed = 0u, thread_island islandType = null)
        {
            return push_back_island(algorithm, problem, evaluator, populationSize, seed, islandType);
        }

        /// <summary>
        /// PascalCase alias for <see cref="push_back_island(IAlgorithm,IProblem,ulong,RPolicyCallback,SPolicyCallback,uint,thread_island)"/>.
        /// </summary>
        public ulong PushBackIsland(IAlgorithm algorithm, IProblem problem, ulong populationSize, RPolicyCallback replacementPolicy, SPolicyCallback selectionPolicy, uint seed = 0u, thread_island islandType = null)
        {
            return push_back_island(algorithm, problem, populationSize, replacementPolicy, selectionPolicy, seed, islandType);
        }

        /// <summary>
        /// PascalCase alias for <see cref="push_back_island(IAlgorithm,IProblem,bfe,ulong,RPolicyCallback,SPolicyCallback,uint,thread_island)"/>.
        /// </summary>
        public ulong PushBackIsland(IAlgorithm algorithm, IProblem problem, bfe evaluator, ulong populationSize, RPolicyCallback replacementPolicy, SPolicyCallback selectionPolicy, uint seed = 0u, thread_island islandType = null)
        {
            return push_back_island(algorithm, problem, evaluator, populationSize, replacementPolicy, selectionPolicy, seed, islandType);
        }
    }
}
