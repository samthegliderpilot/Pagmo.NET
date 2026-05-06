using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace pagmo
{
    /// <summary>
    /// Managed parallel batch fitness evaluator for cloneable problems.
    /// Each OS thread gets an exclusive clone of the problem, allowing problems
    /// that declare <see cref="ThreadSafety.None"/> but implement
    /// <see cref="IThreadCloneableProblem"/> to be evaluated in parallel.
    /// </summary>
    public sealed class managed_thread_bfe : IDisposable
    {
        /// <summary>
        /// Evaluates a flattened batch of decision vectors in parallel using per-thread
        /// problem clones.
        /// </summary>
        public DoubleVector Operator(IThreadCloneableProblem problem, DoubleVector batchX)
        {
            if (problem == null) throw new ArgumentNullException(nameof(problem));
            if (batchX == null) throw new ArgumentNullException(nameof(batchX));

            using var bounds = problem.get_bounds();
            int dim = bounds.first.Count;
            if (dim <= 0)
                throw new InvalidOperationException($"'{problem.get_name()}' returned non-positive problem dimension.");

            int n = batchX.Count;
            if (n % dim != 0)
                throw new ArgumentException(
                    $"batchX length {n} is not a multiple of problem dimension {dim}.");

            int batchSize = n / dim;
            int fitnessLen = (int)(problem.get_nobj() + problem.get_nec() + problem.get_nic());
            var flatFitness = new double[batchSize * fitnessLen];

            var clones = new ThreadLocal<IProblem>(
                () =>
                {
                    var clone = problem.Clone();
                    if (clone == null)
                        throw new InvalidOperationException(
                            $"'{problem.get_name()}.Clone()' returned null during parallel batch evaluation.");
                    return clone;
                },
                trackAllValues: true);

            try
            {
                Parallel.For(0, batchSize, i =>
                {
                    using var x = SliceVector(batchX, i * dim, dim);
                    using var f = clones.Value.fitness(x);
                    for (int j = 0; j < fitnessLen; j++)
                        flatFitness[i * fitnessLen + j] = f[j];
                });
            }
            catch (AggregateException ae)
            {
                ExceptionDispatchInfo.Capture(ae.InnerExceptions[0]).Throw();
                throw; // unreachable
            }
            finally
            {
                foreach (var clone in clones.Values)
                    clone.Dispose();
                clones.Dispose();
            }

            return new DoubleVector(flatFitness);
        }

        /// <inheritdoc />
        public void Dispose() { }

        private static DoubleVector SliceVector(DoubleVector source, int offset, int length)
        {
            var arr = new double[length];
            for (int i = 0; i < length; i++)
                arr[i] = source[offset + i];
            return new DoubleVector(arr);
        }
    }
}
