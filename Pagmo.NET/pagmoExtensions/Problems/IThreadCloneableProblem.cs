namespace pagmo
{
    /// <summary>
    /// Opt-in contract for managed problems that cannot be used concurrently
    /// (<see cref="ThreadSafety.None"/>) but can produce independent copies of themselves
    /// for per-thread or per-island use.
    /// </summary>
    /// <remarks>
    /// Implement this interface and override <see cref="ManagedProblemBase.Clone"/> to return a
    /// fully independent copy. The system will create one clone per island
    /// (<see cref="archipelago"/>) or one clone per OS thread
    /// (<see cref="thread_bfe"/>) so each uses its own exclusive instance.
    /// </remarks>
    public interface IThreadCloneableProblem : IProblem
    {
        /// <summary>
        /// Returns a fully independent copy of this problem for exclusive use on a single
        /// thread or island. Must not return <c>null</c> or the same instance.
        /// </summary>
        IProblem Clone();
    }
}
