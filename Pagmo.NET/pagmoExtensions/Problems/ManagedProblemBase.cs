using System;

namespace pagmo
{
    /// <summary>
    /// Base class for managed C# problems.
    /// </summary>
    public abstract partial class ManagedProblemBase : IProblem, IThreadCloneableProblem
    {
        /// <summary>
        /// Evaluates the fitness vector at the given decision vector.
        /// </summary>
        public abstract DoubleVector fitness(DoubleVector x);

        /// <summary>
        /// Returns lower and upper decision bounds.
        /// </summary>
        public abstract PairOfDoubleVectors get_bounds();

        /// <summary>
        /// Returns a default managed problem name (the runtime type name).
        /// </summary>
        public virtual string get_name() => GetType().Name;

        /// <summary>
        /// Returns additional diagnostic information.
        /// </summary>
        public virtual string get_extra_info() => string.Empty;

        /// <summary>
        /// Returns objective-count (<c>f</c>) dimension.
        /// </summary>
        public virtual uint get_nobj() => 1;

        /// <summary>
        /// Returns equality-constraint count.
        /// </summary>
        public virtual uint get_nec() => 0;

        /// <summary>
        /// Returns inequality-constraint count.
        /// </summary>
        public virtual uint get_nic() => 0;

        /// <summary>
        /// Returns number of integer decision variables.
        /// </summary>
        public virtual uint get_nix() => 0;

        /// <summary>
        /// Reports whether <see cref="batch_fitness"/> is implemented.
        /// </summary>
        public virtual bool has_batch_fitness() => false;

        /// <summary>
        /// Returns thread-safety capability for threaded execution paths.
        /// </summary>
        public virtual ThreadSafety get_thread_safety() => ThreadSafety.None;

        /// <summary>
        /// Returns a fully independent copy of this problem for per-thread or per-island use.
        /// Override to opt in to cloning-based parallel execution for problems that declare
        /// <see cref="ThreadSafety.None"/>. Return <c>null</c> (default) to keep the
        /// existing thread-safety guard behaviour.
        /// </summary>
        public virtual IProblem Clone() => null;

        /// <summary>
        /// Evaluates flattened batch inputs when supported.
        /// </summary>
        public virtual DoubleVector batch_fitness(DoubleVector dvs)
        {
            throw new NotSupportedException($"{get_name()} does not provide batch_fitness().");
        }

        /// <summary>
        /// Reports whether <see cref="gradient"/> is implemented.
        /// </summary>
        public virtual bool has_gradient() => false;

        /// <summary>
        /// Returns gradient values when supported.
        /// </summary>
        public virtual DoubleVector gradient(DoubleVector x)
        {
            throw new NotSupportedException($"{get_name()} does not provide gradient().");
        }

        /// <summary>
        /// Reports whether <see cref="gradient_sparsity"/> is implemented.
        /// </summary>
        public virtual bool has_gradient_sparsity() => false;

        /// <summary>
        /// Returns gradient sparsity pattern when supported.
        /// </summary>
        public virtual SparsityPattern gradient_sparsity()
        {
            throw new NotSupportedException($"{get_name()} does not provide gradient_sparsity().");
        }

        /// <summary>
        /// Reports whether <see cref="hessians"/> is implemented.
        /// </summary>
        public virtual bool has_hessians() => false;

        /// <summary>
        /// Returns hessian matrices when supported.
        /// </summary>
        public virtual VectorOfVectorOfDoubles hessians(DoubleVector x)
        {
            throw new NotSupportedException($"{get_name()} does not provide hessians().");
        }

        /// <summary>
        /// Reports whether <see cref="hessians_sparsity"/> is implemented.
        /// </summary>
        public virtual bool has_hessians_sparsity() => false;

        /// <summary>
        /// Returns hessian sparsity patterns when supported.
        /// </summary>
        public virtual VectorOfSparsityPattern hessians_sparsity()
        {
            throw new NotSupportedException($"{get_name()} does not provide hessians_sparsity().");
        }

        /// <summary>
        /// Sets the random seed for stochastic managed problems when supported.
        /// </summary>
        public virtual void set_seed(uint seed)
        {
            throw new NotSupportedException($"{get_name()} does not provide set_seed().");
        }

        /// <summary>
        /// Reports whether <see cref="set_seed"/> is implemented.
        /// </summary>
        public virtual bool has_set_seed() => false;

        /// <summary>
        /// Releases managed resources. Override when your problem holds disposable state.
        /// </summary>
        public virtual void Dispose()
        {
        }

        /// <summary>
        /// Convenience helper for compact vector creation in managed UDPs.
        /// </summary>
        protected static DoubleVector Vec(params double[] values) => new(values);

        /// <summary>
        /// Convenience helper for compact bound creation in managed UDPs.
        /// </summary>
        protected static PairOfDoubleVectors Bounds(DoubleVector lower, DoubleVector upper) => new(lower, upper);

        /// <summary>
        /// Convenience helper for compact bound creation in managed UDPs.
        /// </summary>
        protected static PairOfDoubleVectors Bounds(double[] lower, double[] upper) => new(new DoubleVector(lower), new DoubleVector(upper));

        /// <summary>
        /// Convenience helper for compact gradient sparsity creation in managed UDPs.
        /// </summary>
        protected static SparsityPattern Sparsity(params (uint Row, uint Col)[] entries)
        {
            var result = new SparsityPattern();
            foreach (var (row, col) in entries)
            {
                result.Add(new SizeTPair(row, col));
            }

            return result;
        }

        /// <summary>
        /// Convenience helper for compact hessian sparsity creation in managed UDPs.
        /// </summary>
        protected static VectorOfSparsityPattern HessiansSparsity(params SparsityPattern[] patterns) => new(patterns);
    }
}
