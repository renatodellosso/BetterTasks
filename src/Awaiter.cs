using System.Runtime.CompilerServices;

namespace BetterTasks
{
    public class Awaiter<TResult> : INotifyCompletion
    {

        private readonly BetterTask<TResult> task;

        public Awaiter(BetterTask<TResult> task)
        {
            this.task = task;
        }

        public void OnCompleted(Action continuation)
        {
            task.ContinueWith(BetterTask<TResult>.WrapAction(continuation));
        }

        public bool IsCompleted { get => task.IsCompleted; }

        public TResult? GetResult()
        {
            return task.Result;
        }

    }
}
