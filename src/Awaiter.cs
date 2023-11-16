using System.Runtime.CompilerServices;

namespace BetterTasks
{
    public class Awaiter : INotifyCompletion
    {

        private readonly BetterTask task;

        public Awaiter(BetterTask task)
        {
            this.task = task;
        }

        public void OnCompleted(Action continuation)
        {
            task.ContinueWith((task) => continuation());
        }

        public bool IsCompleted { get => task.IsCompleted; }

        public void GetResult()
        {

        }

    }
}
