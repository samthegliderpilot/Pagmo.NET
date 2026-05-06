using System;

namespace pagmo
{
    internal static class IProblemThreadingExtensions
    {
        internal static void ThrowIfNotThreadSafe(this IProblem problem)
        {
            if (problem == null)
            {
                throw new ArgumentNullException(nameof(problem));
            }

            var declaredThreadSafety = problem.get_thread_safety();
            if (declaredThreadSafety == ThreadSafety.None)
            {
                var cloneHint = problem is IThreadCloneableProblem
                    ? " Alternatively, override Clone() to return a non-null independent copy to enable per-thread cloning."
                    : "";
                throw new InvalidOperationException(
                    $"Managed problem '{problem.get_name()}' must declare ThreadSafety.Basic or ThreadSafety.Constant for this threaded path.{cloneHint}");
            }
        }
    }
}
