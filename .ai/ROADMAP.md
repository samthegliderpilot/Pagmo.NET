# Pagmo.NET Roadmap

Last updated: 2026-04-08

## Pagmo.NET Roadmap Reset (v1.0 with Explicit Breadth Sprint)

### Summary
- [x] Preserve reset from Sprint 0 with completed items marked complete.
- [x] Keep Windows-first v1.0 release before Linux/CMake.
- [x] v1.0 extensibility targets: `Problem`, `Algorithm`, `BFE`, `Policy`, `Topology`.
- [x] Defer `Island` and custom threading extensibility.
- [x] Make breadth explicit: **Sprint 3A is the "dozens of algorithms/problems" wrapping sprint**.

### Sprint Structure
1. **Sprint 0 (Completed): Tooling Baseline**
- [x] SWIG workflow, build/test path, hello-world SWIG validation.

2. **Sprint 1 (Completed): Fundamentals**
- [x] `Program` and C# `Problem` baseline, partial types, foundational tests.

3. **Sprint 2 (Completed): Critical Runtime + Patterns**
- [x] Finish runtime-critical orchestration (`island/archipelago`, migration controls, policies/topologies needed for runtime correctness).
- [x] Establish extensibility implementation patterns for `Algorithm`, `BFE`, `Policy`, `Topology`.
- [x] Acceptance: core runtime scenarios and pattern examples are stable.
- Active backlog additions:
  - [x] Serialize SWIG regeneration and native build execution to prevent concurrent wrapper-copy races.
  - [x] Replace `Assert.Ignore()` with `Assert.Pass()` where algorithm-test non-applicability is expected behavior.
  - [x] Prefer descriptive parameter names in handwritten managed/test code (avoid `arg0` / `arg1`).
  - [x] Add topology runtime coverage for `fully_connected`, `ring`, and `free_form` with smoke tests.
  - [x] Add archipelago migration/topology control wrappers and validation tests.
  - [x] Add explicit `thread_island` creation paths for `island` and cover with runtime tests.
  - [x] Strengthen `ring` topology tests to validate constructor-argument effects, not just type construction.
  - [x] Add thread-island managed-policy constructor coverage tests (`CreateWithThreadIslandAndPolicies`).
  - [x] Rework `bfe` interop model for safe concrete-BFE to type-erased-BFE conversion (remove invalid inheritance/upcasts).
  - [x] Re-enable `island + bfe` constructor surfaces using explicit safe conversion paths.
  - [x] Reintroduce explicit `thread_island + bfe` constructor surfaces with parity runtime tests.
  - [x] Strengthen island/thread-island constructor tests to assert configuration wiring (algorithm identity, population size, vector dimensions), not only no-throw.
  - [x] Add first concrete extensibility boundary patterns for `Algorithm` and `BFE` (internal normalization/dispatch, no new public factory surface).
  - [x] Add managed educational algorithm `grid_search` with constrained-feasible selection and runtime tests on 2D unconstrained/constrained problems.
  - [x] Add `archipelago.push_back_island` parity wrappers for thread-island/BFE/policy combinations with typed policy surfaces (`fair_replace`/`select_best`, managed policy wrappers).
  - [x] Add `archipelago` island snapshot access (`GetIslandCopy`) and strengthen archipelago runtime tests to assert configuration wiring.
  - [x] Harden managed `Policy` wrapper ownership semantics (`r_policyPagmoWrapper` / `s_policyPagmoWrapper`) to avoid raw-pointer shallow-copy lifetime risk.

4. **Sprint 3A: Broad Coverage Pass (Breadth)**
- [x] Primary goal: add `.i` + wrapper coverage for targeted v1 algorithm/problem catalog (the "dozens" sprint).
- [x] For each newly wrapped type, complete pragmatic finish work in the same slice (meaningful assertions, API polish, and lifecycle sanity), not smoke-only.
- [x] Output: large usable surface with per-type baseline quality, while deeper cross-cutting hardening remains in 3B.
- Active progress:
  - [x] Fully wrap `null_algorithm` with managed extension polish (`IAlgorithm` compatibility helpers) and dedicated regression tests.
  - [x] Fully wrap `null_problem` with managed extension polish and dedicated regression tests (metadata + fitness behavior assertions).
  - [x] Fully wrap `rosenbrock` with managed extension polish and dedicated regression tests (metadata, bounds, optimum fitness, and evolve-path assertions).
  - [x] Fully wrap `rastrigin` with managed extension polish and dedicated regression tests (metadata, differential-info APIs, optimum fitness, and evolve-path assertions).
  - [x] Fully wrap `griewank` with managed extension polish and dedicated regression tests (metadata, bounds, optimum fitness, and evolve-path assertions).
  - [x] Fully wrap `schwefel` with managed extension polish and dedicated regression tests (metadata, bounds, near-optimum fitness, and evolve-path assertions).
  - [x] Fully wrap `lennard_jones` with managed extension polish and dedicated regression tests (metadata, safe fitness evaluation, and evolve-path assertions).
  - [x] Fully wrap `hock_schittkowski_71` with managed extension polish and dedicated regression tests (constrained outputs, differential-info surfaces, and evolve-path assertions).
  - [x] Fully wrap `luksan_vlcek1` with managed extension polish and dedicated regression tests (constraint-vector outputs, gradient contract checks, and evolve-path assertions).
  - [x] Fully wrap `cec2009` with managed extension polish and dedicated regression tests (multi-objective metadata, bounded evolve-path assertions, and safe known-input fitness validation).
  - [x] Fully wrap `cec2013` with managed extension polish and dedicated regression tests (metadata, bounds, evolve-path assertions, and guarded safe-input fitness validation for unstable midpoint probes).
  - [x] Fully wrap `cec2014` with managed extension polish and dedicated regression tests (metadata, bounds, evolve-path assertions, and regression baseline checks).
  - [x] Fully wrap `dtlz` with managed extension polish and dedicated regression tests (multi-objective metadata, bounded evolve-path assertions, Pareto-distance API checks, and safe known-input fitness validation).
  - [x] Fully wrap `wfg` with managed extension polish and dedicated regression tests (multi-objective metadata, bounded evolve-path assertions, and safe known-input fitness validation).
  - [x] Fully wrap `unconstrain` with managed extension polish and dedicated regression tests (death-penalty semantics, constrained-input adaptation, and safe known-input validation).
  - [x] Reach full parity with current pagmo `problems/*.hpp` catalog in Sprint 3A wrapper coverage (no remaining unwrapped problem headers).
  - [x] Fully wrap `translate` with managed extension polish and dedicated regression tests (translation vector surfaces, shifted-optimum behavior checks, and safe known-input validation).
  - [x] Fully wrap `decompose` with managed extension polish and dedicated regression tests (meta-problem construction from managed `IProblem`, decomposed/original fitness assertions, and safe known-input validation).
  - [x] Extend midpoint-probe hardening to constrained/singular-probe problems by adding explicit safe-input fitness-size checks in per-type tests.
  - [x] Harden shared problem test base with per-problem midpoint-probe opt-out to avoid singular fitness crashes while preserving explicit safe-input contract checks.
  - [x] Harden legacy `cec2006` tests with explicit safe-input fitness-length checks and midpoint-probe opt-out to eliminate host-crash instability during grouped runs.
  - [x] Standardize problem test architecture with shared `TestProblemBase` runtime checks and instance-based regression data providers (no static-only regression plumbing).
  - [x] Fully wrap `ihs` with managed extension polish and dedicated regression tests (name/seed/verbosity/log access, evolve-path assertions, and compatibility gating in shared algorithm tests).
  - [x] Fully wrap `nsga2` with managed extension polish and dedicated regression tests (multi-objective evolve-path assertions and compatibility gating in shared algorithm tests).
  - [x] Fully wrap `moead` with managed extension polish and dedicated regression tests (multi-objective evolve-path assertions and compatibility gating in shared algorithm tests).
  - [x] Fully wrap `moead_gen` with managed extension polish and dedicated regression tests (multi-objective evolve-path assertions and compatibility gating in shared algorithm tests).
  - [x] Fully wrap `maco` with managed extension polish and dedicated regression tests (multi-objective evolve-path assertions and compatibility gating in shared algorithm tests).
  - [x] Fully wrap `cstrs_self_adaptive` with managed extension polish and dedicated regression tests (constrained evolve-path assertions, seed/verbosity coverage, and runtime API checks).
  - [x] Fully wrap `mbh` with managed extension polish and dedicated regression tests (name/seed/verbosity coverage and perturbation configuration assertions).
  - [x] Fully wrap `not_population_based` helper surface with managed extension polish and dedicated regression tests (selection/replacement policy configuration coverage).
  - [x] Strengthen `not_population_based` regression tests to assert invalid selection/replacement inputs throw expected wrapper exceptions (not just happy-path no-throw).
  - [x] Reach full parity with current pagmo `algorithms/*.hpp` catalog in Sprint 3A wrapper coverage (no remaining unwrapped algorithm headers).
  - [x] Add type-erasure bridges (`to_algorithm`) for newly wrapped v1 algorithms (`ihs`, `nsga2`, `moead`, `moead_gen`, `maco`, `mbh`, `cstrs_self_adaptive`) and wire them in `AlgorithmInterop`.
  - [x] Add runtime validation that new bridged algorithms flow through `archipelago`/`island` managed paths without unsupported-type failures (including constrained and unconstrained managed-problem cases).
  - [x] Refactor shared `TestAlgorithmBase` multi-objective coverage to assert correct population/objective-shape behavior (instead of single-objective champion assumptions), and re-enable `MultiObjective=true` on wrapped MO algorithms.
  - [x] Add managed multi-objective `IProblem` test wrapper so multi-objective algorithm bridges can be validated through managed `archipelago` overloads end-to-end.
  - [x] Phase 1: remove SWIG `%include`-level `namespace pagmo` wrapping for touched algorithm surfaces (`cstrs_self_adaptive`, `ihs`, `maco`, `mbh`, `moead`, `moead_gen`, `nsga2`) using fully-qualified `.i` declarations.
  - [x] Phase 2 slice: remove SWIG `%include`-level `namespace pagmo` wrapping for `batch_evaluators` + `r/s_policies` + `topologies` by converting touched `.i` declarations to fully qualified `pagmo::` forms and moving includes outside namespace scope.
  - [x] Phase 2 slice: remove `%include`-level namespace wrapping for `utils/hv_algos/hv_algorithm` and `utils/hypervolume` (fully-qualified `pagmo::` declarations + includes moved outside namespace scope).
  - [x] Phase 2+: continue rolling namespace-wrapper removal across remaining wrapper groups (`problems/*` and residual `utils/*`, especially `utils/multi_objective`) with per-slice regen/build/test gates.
  - [x] Remove topology connection `SWIGTYPE_*` exposure by instantiating typed SWIG templates (`SizeTVector`, `TopologyConnections`), add managed `GetConnectionsData` helpers, and strengthen topology tests to assert neighbor/weight wiring.
  - [x] Remove `de1220` constructor `SWIGTYPE_*` leakage by instantiating typed `UIntVector` and add dedicated constructor regression coverage for `allowed_variants`.

5. **Sprint 3B (Completed): Hardening + Extensibility Completion (Depth)**
- [x] Apply/complete C# extensibility surfaces where in v1 scope.
- [x] Remove/contain `SWIGTYPE_*` leakage on touched public APIs (handwritten extension surface now guarded by audit tests; remaining generated/director `SWIGTYPE_*` surfaces are intentionally internal plumbing).
- [x] Audit and eliminate shallow raw-pointer ownership semantics across wrapper facades (copy/assign/destructor ownership rules), replacing with robust lifetime-safe patterns.
- [x] Centralized managed-callback pagmo::problem lifetime ownership via Interop/ProblemHandle (SafeHandle) and removed direct feature-layer problem_delete calls from population, default_bfe/thread_bfe/member_bfe, and GradientsAndHessians.
- [x] Hardened managed policy ownership transfer paths (`r_policy`/`s_policy`) to be exception-safe after ownership release, with regression coverage for null/disposed inputs and validity checks.
- [x] Added direct managed-policy extensibility entrypoints: `island`/`archipelago` policy overloads now accept `r_policyBase` + `s_policyBase` directly (no manual wrapper ceremony), including `bfe` + non-`bfe` paths with typed `default_bfe`/`thread_bfe`/`member_bfe` parity on `island` and typed `default_bfe`/`thread_bfe`/`member_bfe` parity on `archipelago`, plus PascalCase alias parity on `archipelago.PushBackIsland`, with runtime regression coverage (`Test_island_managed_policies`, `Test_archipelago_managed_policies`).
- [x] Fixed native-vector ownership leak risk in `archipelago` snapshot properties (`MigrationLog`, `MigrantsDb`) by disposing temporary native containers via `using var`.
- [x] Reduced raw-pointer plumbing in `archipelago` managed-problem path by replacing manual `CreateProblemPointer`/`problem_delete` handling with safe `new problem(IProblem)` ownership flow.
- [x] Added ownership guardrail audit: direct `NativeInterop.CreateProblemPointer(...)` usage is constrained to dedicated interop boundary files (`problem`, `population`, `BatchEvaluators/bfe`, `Utils/GradientsAndHessians`).
- [x] Added ownership guardrail audit: swigRelease usage and non-owning raw-pointer wrapper construction are constrained to explicitly approved ownership-boundary files.
- [x] Fixed managed problem callback lifetime in native director flows by rooting `ProblemCallbackAdapter` instances via `ConditionalWeakTable`, preventing GC callback crashes during long-running native calls.
- [x] Normalize naming/signatures and add deeper behavior/regression tests (PascalCase aliases + expanded execute-path and API-surface guard regressions).
- [x] Standardize C++?C# exception bubbling and mapping across constructor/evolve/wait paths, including explicit `wait()` (non-throwing completion barrier) vs `wait_check()` (error-surfacing barrier) semantics in runtime regression tests.
- [x] Replace null-return `catch (...)` patterns in native bridge with explicit error propagation so managed exceptions preserve actionable native context.
- [x] Verified non-generated native bridge surface (`managed_bridge.cpp`) consistently records contextual thread-local errors for both `std::exception` and `catch (...)` paths before null returns, and managed interop consumes that channel.
- [x] Add thread-local native bridge error channel for managed_bridge exports (pagmosharp_get_last_error / pagmosharp_clear_last_error) and consume it in managed interop (problem creation, population creation, gradient/sparsity helpers, BFE operators) so null-return paths surface actionable failure messages.
- [x] Add execute-path SWIG exception context for runtime orchestration methods (`algorithm.evolve`, `island.evolve`, `island.wait`, `island.wait_check`, `archipelago.evolve`, `archipelago.wait`, `archipelago.wait_check`, `thread_island.run_evolve`) and lock behavior with regression tests.
- [x] Correct built-in problem `thread_safety` metadata in wrappers (remove placeholder `none` defaults where inaccurate) and add threaded runtime verification coverage.
- [x] Completed pass: corrected placeholder thread-safety metadata from `none` to `basic` for `ackley`, `cec2006`, `cec2009`, `cec2013`, `cec2014`, `dtlz`, `golomb_ruler`, `griewank`, `hock_schittkowski_71`, `inventory`, `lennard_jones`, `luksan_vlcek1`, `minlp_rastrigin`, `null_problem`, `rastrigin`, `schwefel`, `wfg`, and `zdt`, with explicit assertions in corresponding problem tests.
- [x] Investigate and eliminate full-suite post-run test-host crashes (all tests pass but host process aborts during teardown), with explicit native-lifetime root cause and regression guard (resolved by switching midpoint probe vector construction to array-based initialization in `TestProblemBase`).
- [x] Implemented native-first `default_bfe` for all `IProblem` inputs and centralized callback-boundary exception policy in interop (`NativeInterop` + `ProblemCallbackAdapter`), removing feature-level managed/unmanaged branching from `default_bfe`.
- [x] Closed default_bfe teardown-crash follow-up by handling managed callback exceptions at the interop boundary (defer in adapter, rethrow on managed return path) while preserving SWIG pending-exception flow as the primary channel for regular wrapper calls.
- [x] Extended centralized callback-boundary exception handling to additional managed-callback native paths (`population(IProblem, ...)`, gradient/gradient_h/sparsity `IProblem` helpers), with explicit bubbling regressions and repeated full-suite stability passes.
- [x] Hardened callback-boundary regression assertions: verify inner-exception chaining for managed callback failures, cover managed `batch_fitness` exception bubbling through `default_bfe`, and lock first-failure retention semantics for deferred callback exceptions.
- [x] Deduplicate SWIG director/include declarations in root interface and keep one canonical registration path for `problem`, `r_policy`, and `s_policy` bridges.
- [x] Normalize SWIG fragment hygiene by removing per-file `%module` directives from included `.i` fragments and keeping module definition at the root interface only.
- [x] Complete multi-objective support end-to-end (problem/algorithm flows, champion and population semantics, and static helper functions in `utils/multi_objective`).
- [x] Define and implement a project-wide std::size_t managed boundary strategy for handwritten extension surfaces via centralized `SizeTInterop` conversions (`ulong` input with checked `uint` native mapping + actionable `ArgumentOutOfRangeException` context).
- [x] Documented Sprint 3B size_t compatibility matrix and phased plan in `.ai/SIZE_T_STRATEGY.md`.
- [x] Implement managed projection wrappers for remaining size_t-heavy opaque log/sparsity surfaces so touched public APIs avoid `SWIGTYPE_*size_t*` (residual generated director plumbing remains intentionally internal-facing).
- [x] Add regression tests for size_t projection wrappers and boundary conversions (shape/count/index transfer assertions + overflow guards on population/island/archipelago managed entrypoints).
- [x] First size_t projection slice completed: gaco log projection (get_log_entries + managed GetLogLines()) with behavior assertions in Test_gaco.
- [x] Added shared C# log abstraction (IAlgorithmLogLine + universal IAlgorithm.GetLogLines() default-empty contract, no per-algorithm `IHas...` marker interface requirement) and implemented it for gaco with generic raw-field access assertions.
- [x] Added second size_t projection slice: mbh log projection (get_log_entries + typed/generic managed logs) with field-parity assertions in Test_mbh.
- [x] Added third size_t projection slice: ihs log projection (including ideal-point vector payload) with field-parity assertions in Test_ihs.
- [x] Added fourth size_t projection slice: cstrs_self_adaptive log projection with typed/generic managed logs and field-parity assertions in Test_cstrs_self_adaptive.
- [x] Added fifth size_t projection slice: de log projection (get_log_entries + typed/generic managed logs) with field-parity assertions in Test_de.
- [x] Added sixth size_t projection slice: cmaes log projection (get_log_entries + typed/generic managed logs) with field-parity assertions in Test_cmaes.
- [x] Added sparsity projection slice for size_t-heavy derivative metadata: typed managed `SparsityIndex` projections on `problem` / `managed_problem` / `minlp_rastrigin` (`GetGradientSparsityEntries`, `GetHessiansSparsityEntries`) with shape/index regression assertions in `Test_de_managed_problem_pipeline` and `Test_minlp_Rastrigin`.
- [x] Added GradientsAndHessians size_t projection slice: EstimateSparsityEntries(problem/IProblem, ...) returning typed SparsityIndex[], with dedicated regression assertions in Test_gradients_and_hessians.
- [x] Hardened gradient/sparsity helper contracts with explicit null-argument validation in GradientsAndHessians + sparsity projection paths, and expanded tests for high-order gradient behavior and null-argument coverage.
- [x] Hardened threaded managed-problem contract guard (IProblemThreadingExtensions.ThrowIfNotThreadSafe) with explicit null-argument handling and problem-name context in failure messages; added BFE regressions for null/thread-safety-none behavior.
- [x] Hardened core interop facades (`DoubleVector`, `r_policy`, `s_policy`, `bfe`, `NativeInterop`, `ProblemHandle`) with explicit null/disposed argument contracts, removed DoubleVector params-LINQ overhead path, and added regression coverage for policy ownership disposal, BFE null-input behavior, and DoubleVector constructor semantics.
- [x] Hardened managed core orchestration facades (`problem`, `population`, `island`, `archipelago`) with explicit null-argument contracts across managed entrypoints (including archipelago managed-problem path), plus dedicated null-contract regression tests to lock behavior.
- [x] Completed algorithm/problem support hardening pass: added universal default log surface on type-erased `algorithm`, hardened `ProblemCallbackAdapter` callback-result contracts against null returns for deferred-failure paths (`fitness`, `batch_fitness`), and expanded `IProblem`/minimal-managed-problem default-contract regressions.
- [x] Completed algorithm-log projection sweep for all active wrapped algorithms that expose logs in v1 surface (`bee_colony`, `compass_search`, `cmaes`, `cstrs_self_adaptive`, `de`, `de1220`, `gaco`, `gwo`, `ihs`, `maco`, `mbh`, `moead`, `moead_gen`, `nsga2`, `nspso`, `pso`, `pso_gen`, `sade`, `sea`, `sga`, `simulated_annealing`, `xnes`), with universal `IAlgorithm.GetLogLines()` plus typed `GetTypedLogLines()` surfaces and shared evolve-path log assertions.
- [x] Hardened algorithm support contracts: refactored AlgorithmInterop.NormalizeToTypeErased to exhaustive switch-dispatch with explicit managed-only grid_search guidance, added regression for grid_search log-default + type-erased rejection path, and completed IAlgorithm contract documentation cleanup.
- [x] Removed raw tuple-based algorithm log leakage from generated APIs by ignoring direct `get_log()` on active algorithm wrappers and retaining only typed log projection surfaces (`get_log_entries` / `GetTypedLogLines` / `GetLogLines`).
- [x] Removed legacy bee-colony tuple bridge (`FromBeeColonyLogTuple`) so tuple `SWIGTYPE_*` artifacts no longer leak into generated managed surfaces.
- [x] Added a concrete end-to-end constrained optimization regression using a simple managed two-parabola problem (`objective = parabola1 + parabola2`, one inequality constraint) and asserted real feasible-objective improvement after `cstrs_self_adaptive` evolve.
- [x] Strengthened managed multi-objective runtime regression in `archipelago` (`nsga2` + managed MO problem): assert ideal-point improvement after evolve and verify non-trivial non-dominated front extraction via `non_dominated_front_2d`.
- [x] Expanded type-erasure bridge coverage to all active algorithm UDAs with explicit `to_algorithm()` regressions (`Test_algorithm_type_erasure_bridges`) so `AlgorithmInterop` mappings are locked by tests.
- [x] Added end-to-end managed runtime interop coverage for all active bridged algorithms via `island.Create(IAlgorithm, IProblem, ...)` (single-objective, constrained, and multi-objective cases), including evolve/wait_check and population-shape assertions.
- [x] Added matching `archipelago.push_back_island(IAlgorithm, IProblem, ...)` interop matrix coverage for all active bridged algorithms with evolve/wait_check and objective/champion-shape assertions.
- [x] Add managed wrapper surface for core `utils/multi_objective` static helpers (`pareto_dominance`, `non_dominated_front_2d`, `crowding_distance`, `sort_population_mo`, `select_best_N_mo`, `ideal`, `nadir`, `decompose_objectives`) and assert concrete behavior in `Test_multi_objective`.
- [x] Added C#-friendly multi-objective projection helpers (ParetoDominates, index-array selectors, ideal/nadir/decompose array projections) to reduce container-heavy call sites and improve naming clarity, with parity assertions in Test_multi_objective.
- [x] Assert and lock multi-objective population semantics in shared algorithm tests: champion_x/champion_f must throw on multi-objective populations while get_x/get_f remain the supported data path.
- [x] Added naming/signature polish aliases for `archipelago` managed entrypoints (`PushBackIsland`, `GetIsland`) while preserving existing pagmo-style APIs.
- [x] Consolidated `archipelago` managed overload surface to a smaller canonical API: removed redundant typed-BFE and thread-island-first overload families, centralized dispatch/policy wrapping, and updated tests to use type-erased `bfe` + optional `thread_island` parameter.
- [x] Consolidated island managed overload surface to a smaller canonical API: centralized creation dispatch, collapsed redundant overload families (including typed-BFE variants), and kept compatibility shims for explicit thread-island method names.
- [x] Completed topology extension overload review: retained per-type `GetConnectionsData` overloads (`topology`, `fully_connected`, `ring`, `unconnected`) because SWIG emits sibling wrapper classes rather than C# inheritance, and hardened each overload with explicit null-argument validation plus stronger topology projection tests.
- [x] Added handwritten API surface audit tests to prevent new public `SWIGTYPE_*` leakage and generated placeholder argument names (`arg0`, `arg1`) outside explicit director-plumbing exceptions.
- [x] Reduce wrapper layering for multi-objective helpers by removing intermediate MultiObjectiveUtils C++/C# helper classes and exposing direct pagmo.pagmo.* static bindings via SWIG namespace declarations.
- [x] Remove hypervolume/hv_algorithm shared-pointer SWIGTYPE_* exposure by instantiating typed HvAlgorithmSharedPtr and add runtime selector coverage in Test_hypervolume.
- [x] Remove `std::ostream` SWIGTYPE leakage from managed surface by pruning unused ostream-only declarations in `io.i` (no managed API usage, no runtime behavior change).
- [x] Remove `double*` SWIGTYPE leakage from `hv_algorithm` by suppressing the raw-pointer `volume_between(double*, double*, size_t)` overload and keeping vector-based managed overloads.
- [x] Remove policy/raw-pointer SWIGTYPE leakage from generated surface by suppressing low-level `r_policyPagmoWrapper::replace` / `s_policyPagmoWrapper::select` exports; managed policy APIs remain unchanged.
- [x] Remove managed-problem shared_ptr-constructor SWIGTYPE leakage by suppressing the shared_ptr overload and retaining callback-based managed constructor paths.
- [x] Contained remaining sparsity-pattern `SWIGTYPE_*` leakage by adding first-class typed projection APIs (`SparsityIndex`-based) for public managed usage while preserving low-level generated pointer surfaces only for SWIG/director plumbing.
- [x] Remove MigrationEntry ID SWIGTYPE_* exposure by mapping adapter IDs (migration_id, immigrant_id) to ulong and add dedicated regression coverage in Test_migration_entry.
- [x] Anything from 3A is not considered production-ready until 3B gates pass.
### Sprint 3B Remaining Work Breakdown (Type-by-Type)
- [x] Core facade hardening: `algorithm`
- [x] Core facade hardening: `problem`
- [x] Core facade hardening: `population`
- [x] Core facade hardening: `island`
- [x] Core facade hardening: `archipelago`
- [x] Core facade hardening: `topology`
- [x] Core facade hardening: `r_policy`
- [x] Core facade hardening: `s_policy`
- [x] Core facade hardening: `bfe`
- [x] Core facade hardening: `DoubleVector`
- [x] Core facade hardening: `NativeInterop`
- [x] Core facade hardening: `Interop/ProblemHandle`
- [x] Utility hardening: `GradientsAndHessians`
- [x] Utility hardening: `multi_objective.projections`
- [x] Topology hardening: `unconnected`
- [x] Problem wrapper hardening: `ackley`
- [x] Problem wrapper hardening: `cec2006`
- [x] Problem wrapper hardening: `cec2009`
- [x] Problem wrapper hardening: `cec2013`
- [x] Problem wrapper hardening: `cec2014`
- [x] Problem wrapper hardening: `decompose`
- [x] Problem wrapper hardening: `dtlz`
- [x] Problem wrapper hardening: `golomb_ruler`
- [x] Problem wrapper hardening: `griewank`
- [x] Problem wrapper hardening: `hock_schittkowski_71`
- [x] Problem wrapper hardening: `inventory`
- [x] Problem wrapper hardening: `lennard_jones`
- [x] Problem wrapper hardening: `luksan_vlcek1`
- [x] Problem wrapper hardening: `minlp_rastrigin`
- [x] Problem wrapper hardening: `null_problem`
- [x] Problem wrapper hardening: `rastrigin`
- [x] Problem wrapper hardening: `rosenbrock`
- [x] Problem wrapper hardening: `schwefel`
- [x] Problem wrapper hardening: `translate`
- [x] Problem wrapper hardening: `unconstrain`
- [x] Problem wrapper hardening: `wfg`
- [x] Problem wrapper hardening: `zdt`
- [x] Problem support hardening: `ManagedProblemBase`
- [x] Problem support hardening: `ProblemCallbackAdapter`
- [x] Problem support hardening: `problem.sparsity`
- [x] Problem support hardening: `sparsity.projections`
- [x] Problem support hardening: `IProblem`
- [x] Problem support hardening: `IProblemThreadingExtensions`
- [x] Algorithm wrapper hardening: `bee_colony`
- [x] Algorithm wrapper hardening: `cmaes`
- [x] Algorithm wrapper hardening: `compass_search`
- [x] Algorithm wrapper hardening: `cstrs_self_adaptive`
- [x] Algorithm wrapper hardening: `de`
- [x] Algorithm wrapper hardening: `de1220`
- [x] Algorithm wrapper hardening: `gaco`
- [x] Algorithm wrapper hardening: `grid_search`
- [x] Algorithm wrapper hardening: `gwo`
- [x] Algorithm wrapper hardening: `ihs`
- [x] Algorithm wrapper hardening: `maco`
- [x] Algorithm wrapper hardening: `mbh`
- [x] Algorithm wrapper hardening: `moead`
- [x] Algorithm wrapper hardening: `moead_gen`
- [x] Algorithm wrapper hardening: `not_population_based`
- [x] Algorithm wrapper hardening: `nsga2`
- [x] Algorithm wrapper hardening: `nspso`
- [x] Algorithm wrapper hardening: `null_algorithm`
- [x] Algorithm wrapper hardening: `pso`
- [x] Algorithm wrapper hardening: `pso_gen`
- [x] Algorithm wrapper hardening: `sade`
- [x] Algorithm wrapper hardening: `sea`
- [x] Algorithm wrapper hardening: `sga`
- [x] Algorithm wrapper hardening: `simulated_annealing`
- [x] Algorithm wrapper hardening: `xnes`
- [x] Algorithm support hardening: `AlgorithmInterop`
- [x] Algorithm support hardening: `AlgorithmLogging`
- [x] Algorithm support hardening: `IAlgorithm`
- [x] Added feature-gated optional solver availability tests (`nlopt`) to make build-dependent support explicit and test-verified.
- [x] Optional solver hardening (feature-gated): `nlopt`
- [x] Cross-cutting gate: for each type above enforce lifetime safety, actionable exceptions, naming/signature consistency, no new handwritten public `SWIGTYPE_*` leaks, and meaningful behavior assertions.
- [x] Sprint 3B closure: deferred all remaining IPOPT-specific work to Sprint 4.

6. **Sprint 4: Documentation + Samples**
- [x] C#-first docs with canonical runnable examples (dedicated example files/projects).
- [x] Added runnable non-test teaching project: `Examples/Examples.Pagmo.NET` (single-island baseline, archipelago topology comparison, policy comparison).
- [x] Convert docs to an "executable documentation" model: keep runnable examples as source-of-truth, add concept-first `docs/` walkthrough pages that link directly to example code paths, and add drift guards (smoke execution checks) for documented scenarios.
- [x] Added `docs/` walkthroughs (`getting-started`, `archipelago-topology-policies`) linked to runnable `Examples/Examples.Pagmo.NET` code paths.
- [x] Added docs drift guard script `scripts/docs-smoke.ps1` to execute documented scenarios (`single`, `archipelago`, `policies`).
- [x] Added local (git-ignored) C++ learning playground scaffold mirroring core sample concepts (`scratch/CppPagmoPlayground`) for side-by-side pagmo semantics exploration without shipping it.
- [x] README includes C# quickstart snippet and usage notes for core managed flows.
- [x] Publish a supported-feature matrix by build/environment (for example optional algorithm availability such as IPOPT/NLopt).
- [x] Perform an exception-usage audit across managed/native wrapper layers to verify existing code is surfacing actionable exceptions consistently and not silently swallowing failure context.
- [x] Review high-overload managed API surfaces (`archipelago` + `island`) and remove non-essential overloads while preserving a small canonical core plus compatibility shims where needed.
- [x] Normalize managed API naming conventions across extension surfaces (for example snake_case vs PascalCase) and define a single public-style policy with compatibility aliases/deprecation plan.
- [x] Added naming policy and guardrails (.ai/API_NAMING_POLICY.md + reflection tests) to preserve PascalCase extension helpers with intentional snake_case compatibility entrypoints.
- [x] Complete optional solver availability/hardening for `ipopt` in an IPOPT-enabled environment:
- [x] Prepared compile-guarded `ipopt` wrapper modernization scaffolding (typed SWIG `ipopt.i` shape + `to_algorithm()` bridge + log-entry projection helpers).
- [x] Validate `ipopt` availability tests and runtime construct/evolve/type-erasure/log behavior where IPOPT is present.
- [x] Optional solver hardening (feature-gated): `ipopt`.
- [x] Removed IPOPT `SWIGTYPE_*` leakage from the public managed surface (raw pointer/map APIs ignored in SWIG; replaced with primitive typed helpers).

7. **Sprint 5: Release Readiness**
- [x] Packaging/versioning/changelog/release checklist and ship gates.
- [x] Added .ai/RELEASE_CHECKLIST.md with explicit versioning, build/test, artifact, documentation, changelog, and publish gates.
- [x] Remove machine-local hardcoded tool/include paths from build scripts/projects (`swig.exe`, include/lib paths) and replace with configurable/CI-friendly inputs.
- [x] Removed hardcoded SWIG fallback path from `createSwigWrappersAndPlaceThem.bat`; SWIG now resolves via `SWIG_EXE`, `SWIG_HOME`, or `PATH`.
- [x] Purge dead/orphan native files and stale placeholders from the solution (unused headers/cpps and abandoned stubs) before release freeze.
- [x] Removed clearly dead native placeholders/stubs (`AndH.cpp`, `multi_objective.cpp`, `r_policy.cpp`, `r_policyBase.cpp`, `s_policy.cpp`, `vec_of_vec.h`, `archipelago_bindings_access.h`, `r_policyBase.h`, `gradientsAndHessiansCallback.h`) and verified native rebuild success.
- [x] Add managed algorithm callback bridge so C# algorithms can participate in native pagmo orchestration (`pagmo::algorithm` type-erasure + `island`/`archipelago` execution paths) with centralized interop-owned exception boundary handling and teardown-stability regression coverage.
- [x] Implemented native `managed_algorithm` callback bridge (`pagmosharp_algorithm_from_callback`) + managed adapter path, enabling managed `IAlgorithm` (for example `grid_search`) in island/archipelago type-erased runtime flows.
- [x] Added regression coverage for managed algorithm type-erased execution and callback-failure bubbling through `wait_check()` in island/archipelago paths.
- [x] Added consolidated release-gate automation script (`scripts/release-gates.ps1`) to run SWIG regen reproducibility checks, native rebuilds (`Debug`/`Release`), full managed tests, and optional solver availability tests from one command.
- [x] Hardened local CI/test scripts to use repo-local package cache (`NUGET_PACKAGES=./.nuget/packages`) for more deterministic restore/build behavior in constrained environments.
- [x] Updated managed target frameworks to current LTS baseline (`net10.0`) across library/tests/examples and validated managed build pipeline on .NET SDK 10.x.
- [x] Updated native language standard baseline from C++17 to C++20 in `pagmoWrapper.vcxproj` while keeping stable VS toolset (`v143`).
- [x] Rename default branch from `master` to `main`.
- [x] Investigate and harden `problem(IProblem)` normalization for wrapped-native `IProblem` inputs (avoid callback-wrapping native problem wrappers that can trigger native access-violation paths under some constructor/evaluation flows).

8. **Sprint 6: v1.0 Release**
 - [x] Add API documentation comments (`///`) for all public/protected types and members (no internal/private pass in v1.0).
 - [x] Documentation strategy for API comments:
  - Do not bulk-copy pagmo docs verbatim.
  - Prefer concise C#-first summaries focused on wrapper behavior and usage contracts.
  - Where semantics mirror upstream pagmo closely, include a short "See pagmo reference" pointer in remarks and keep this project's notes focused on wrapper-specific differences.
 - [x] Add doc quality gates:
  - build with XML docs enabled and no unresolved XML-doc warnings on handwritten public/protected API surface (`pagmoExtensions/*`, excluding generated `pygmoWrappers/*`),
  - generate and publish full public-symbol API reference from compiled assembly for complete consumer-visible coverage (including generated surfaces),
  - spot-check docs against runnable examples to avoid drift.
- Active progress:
  - [x] Reordered Sprint 6 so API documentation work is explicitly pre-release and first in sequence.
  - [x] Enabled XML documentation file generation in `Pagmo.NET.csproj` and stabilized build output (generated-wrapper CS1591 suppressed; handwritten surfaces enforced via dedicated gate script).
  - [x] Completed handwritten API-doc pass across `pagmoExtensions/*` public/protected members.
  - [x] Added generated API reference pipeline (`scripts/generate-api-reference.ps1`) to document complete consumer-visible surface without editing autogenerated sources.
  - [x] Added handwritten-doc enforcement gate (`scripts/check-handwritten-api-docs.ps1`) and linked it in docs.
  - [x] Added full-surface generated-doc enforcement gate (`scripts/check-generated-api-docs.ps1`) to verify all public types and all public/protected members are documented in generated API reference output.
  - [x] Locked beta versioning baseline to `1.0.0-beta.1` in packaging metadata.
  - [x] Added release artifact production script (`scripts/build-release-artifacts.ps1`) to emit NuGet packages, release-only native runtime bundle, source archive, and `SHA256SUMS.txt` under `artifacts/release/<version>/`.
- [x] Define and lock v1.0 artifact set: NuGet `.nupkg` + `.snupkg`, native runtime bundles, source archive, `SHA256SUMS.txt`.
- [x] Define and lock distribution channels: GitHub Releases for tagged binaries, NuGet.org for managed package, README as install instructions source.
- [x] Add release notes content: RELEASE_NOTES.md with highlights, breaking/behavior notes, supported environment matrix.
- [x] Publish artifacts and release notes — v1.0.0-beta.1 and v1.0.0-beta.2 published to NuGet.org.

9. **Sprint 7 (v1.x): Linux/CMake** ✅ Completed before v1.0 release.
- [x] Cross-platform build — Linux x64 supported. Static vcpkg build (`x64-linux-static-pic` triplet): pagmo2 + Boost + TBB + NLopt + IPOPT statically linked into `libPagmoWrapper.so`. No system runtime dependencies. 593/593 tests pass on Ubuntu 24.04.
- [x] `IThreadCloneableProblem` implemented: non-thread-safe managed problems can participate in threaded paths (`archipelago`, `thread_bfe`) by providing `Clone()`. Per-island cloning via `ExclusiveCloneAdapter`; per-thread cloning via `managed_thread_bfe` (`Parallel.For` + `ThreadLocal<IProblem>`). Stress test included.

10. **Sprint 8 (v2.0): macOS Support** ✅ Completed.
- [x] **arm64 (Apple Silicon):** `triplets/arm64-osx-static-pic.cmake` — static linkage, `CMAKE_SYSTEM_NAME=Darwin`, deployment target 11.0.
- [x] **x86_64 (Intel Mac):** `triplets/x64-osx-static-pic.cmake` — deployment target 10.15.
- [x] **Universal binary:** arm64 + x86_64 slices combined with `lipo -create` in CI `merge-macos` job; shipped as `runtimes/osx/native/libPagmoWrapper.dylib`.
- [x] **CMakeLists.txt:** `@loader_path` RPATH for macOS; SONAME restricted to Linux only.
- [x] **build-native.ps1:** macOS branch detecting architecture via `uname -m`, selecting appropriate triplet, building with NLopt.
- [x] **IPOPT on macOS:** deferred — NLopt ships; IPOPT revisit pending vcpkg macOS port improvements.
- [x] **Code signing:** ad-hoc `codesign --sign -` in `build-release-artifacts.ps1`.
- [x] **NuGet packaging:** `runtimes/osx/native/` ItemGroup in `Pagmo.NET.csproj`; `build-release-artifacts.ps1` updated with macOS artifact collection.
- [x] **CI:** `build-macos-arm64` (`macos-latest`) + `build-macos-x64` (`macos-13`) + `merge-macos` (lipo) jobs in release workflow.

10. **Sprint 9 (v2.x): Additional Language Bindings**

    **Java / Kotlin**
    - SWIG has a first-class Java backend (`-java`). The existing `.i` files (includes, directors, module structure) carry over almost entirely — roughly 70% reuse.
    - What needs replacing: C#-specific typemaps (`%typemap(cstype)`, `%typemap(cscode)`, etc.) with Java equivalents; module declarations; JNI library loading instead of P/Invoke.
    - Memory management: SWIG's Java director pattern handles GC-interop similarly to the C# path; finalizer/dispose patterns map to Java's `AutoCloseable`.
    - Kotlin requires no separate backend — SWIG-generated Java bindings are directly consumable from Kotlin with no extra work.
    - Estimated new work: ~30% on top of what exists.

    **Rust**
    - SWIG has no Rust backend; the `.i` files are not reusable for Rust.
    - **Recommended approach: explicit C ABI wrapper + bindgen** (the `*-sys` crate pattern).
      - Write a deliberate C header (`pagmo_c.h`) and implementation that exposes the pagmo surface as plain C functions. This is the universal stable ABI — every language has mature, battle-tested FFI against C.
      - Run `bindgen` to generate a `pagmo-sys` crate (raw unsafe bindings). Publish a safe wrapper crate on top.
      - Follows the Rust ecosystem convention for native bindings — consumers know what to expect.
    - **Why not `autocxx` or binding directly against SWIG-exported symbols:**
      - `autocxx` is impressive but still chasing a moving target (C++ template instantiation, STL types, exceptions across the FFI boundary). The ecosystem has not settled on it; higher risk of being replaced.
      - Binding directly against the SWIG runtime exports couples Rust to internal ABI details that were never designed to be stable.
    - **Why the C wrapper ages well:** a hand-designed C header forces deliberate API choices upfront. Bindings that leak C++ internals or rely on auto-generated surfaces are what causes people to want to replace FFI crates later. A clean C layer is the thing that doesn't get replaced.

### API and Quality Rules
### Exception Policy (Project Preference)
- Keep explicit pre-check exceptions to a high bar: add them only when they materially improve safety or provide clearly better diagnostic value than natural downstream exceptions.
- For private helper methods, prefer centralized/shared boundary checks over local specialized exception branches unless there is a concrete safety or diagnostics gain.
- Avoid exception-check noise: if a natural exception already communicates failure sufficiently, prefer the simpler path.
- During 3A breadth, prioritize coverage velocity with smoke tests.
- During 3A execution, do not add new `SWIGTYPE_*` leakage on touched public surfaces; either map to usable managed types now or keep the API out of scope until a later sprint.
- During 3B depth, enforce public API quality:
  - no unresolved `SWIGTYPE_*` exposure for in-scope v1 surfaces
  - consistent API shape and lifecycle behavior
  - deeper tests for callbacks/thread-safety/lifetimes where relevant
- Deferred items must be explicitly assigned to a later sprint; no silent carryover.

### Test Plan
- **Sprint 3A:** catalog smoke suite for each added algorithm/problem wrapper.
- **Sprint 3B:** focused deep tests by subsystem (`Algorithm`, `BFE`, `Policy`, `Topology`) plus regressions.
- Pre-release requires stable acceptance subset across fundamentals + runtime + selected breadth set.

### Assumptions
- Breadth-first then depth-hardening is intentional for large catalog onboarding.
- `Problem` remains core and already mature enough to build on.
- v1.0 ships Windows x64 and Linux x64. macOS is explicitly post-v1.













































