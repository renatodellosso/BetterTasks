namespace BetterTasks
{
    /// <summary>
    /// This interface allows us to keep lists of <see cref="BetterTask{TResult}"/> with different TResult types.
    /// </summary>
    internal interface ITask
    {
        internal Thread? Thread { get; set; }

        internal object? Result { get; set; }

        internal object? Execute();
        internal void OnActionComplete();
    }
}
