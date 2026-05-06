using System;

namespace pagmo
{
    /// <summary>
    /// Managed convenience overloads for default batch fitness evaluation.
    /// </summary>
    public partial class default_bfe
    {
        /// <summary>
        /// Evaluates a flattened batch of decision vectors against the provided problem.
        /// </summary>
        public DoubleVector Operator(IProblem problem, DoubleVector batchX)
        {
            return BfeBridge.BatchEvaluate(problem, batchX, NativeInterop.default_bfe_operator, getCPtr(this).Handle, requiresParallelSafety: false);
        }
    }

    /// <summary>
    /// Managed convenience overloads for threaded batch fitness evaluation.
    /// </summary>
    public partial class thread_bfe
    {
        /// <summary>
        /// Evaluates a flattened batch of decision vectors against the provided problem using parallel execution.
        /// </summary>
        public DoubleVector Operator(IProblem problem, DoubleVector batchX)
        {
            return BfeBridge.BatchEvaluate(problem, batchX, NativeInterop.thread_bfe_operator, getCPtr(this).Handle, requiresParallelSafety: true);
        }
    }

    /// <summary>
    /// Managed convenience overloads for member batch fitness evaluation.
    /// </summary>
    public partial class member_bfe
    {
        /// <summary>
        /// Evaluates a flattened batch of decision vectors against the provided problem.
        /// </summary>
        public DoubleVector Operator(IProblem problem, DoubleVector batchX)
        {
            return BfeBridge.BatchEvaluate(problem, batchX, NativeInterop.member_bfe_operator, getCPtr(this).Handle, requiresParallelSafety: false);
        }
    }

    internal delegate IntPtr BfeOperator(IntPtr bfePtr, IntPtr problemPtr, IntPtr batchXPtr);

    internal static class BfeBridge
    {
        internal static DoubleVector BatchEvaluate(IProblem problem, DoubleVector batchX, BfeOperator op, IntPtr bfePtr, bool requiresParallelSafety)
        {
            if (problem == null)
            {
                throw new ArgumentNullException(nameof(problem));
            }

            if (batchX == null)
            {
                throw new ArgumentNullException(nameof(batchX));
            }

            if (requiresParallelSafety
                && problem.get_thread_safety() == ThreadSafety.None
                && problem is IThreadCloneableProblem cloneable)
            {
                // Probe: if Clone() returns null the problem doesn't support cloning, so
                // fall through to the standard thread-safety rejection with its error message.
                var probe = cloneable.Clone();
                probe?.Dispose();
                if (probe != null)
                {
                    using var managedBfe = new managed_thread_bfe();
                    return managedBfe.Operator(cloneable, batchX);
                }
            }

            if (requiresParallelSafety)
            {
                problem.ThrowIfNotThreadSafe();
            }

            using var problemHandle = ProblemInterop.CreateProblemHandle(problem, out var callbackAdapter);
            var resultPtr = op(bfePtr, problemHandle.DangerousGetHandle(), DoubleVector.getCPtr(batchX).Handle);
            return NativeInterop.GetVectorOrThrow(resultPtr, "Native batch evaluator returned null.", callbackAdapter);
        }
    }

}
