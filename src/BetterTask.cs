namespace BetterTasks
{
    public class BetterTask
    {

        internal Action<BetterTask> Action => Actions[0];
        internal Thread? Thread { private get; set; }

        internal List<Action<BetterTask>> Actions { get; private set; }

        private CancellationToken cancellationToken;
        private CancellationTokenSource cancellationTokenSource;

        public bool IsCanceled => cancellationToken.IsCancellationRequested;

        public BetterTask(Action action)
        {
            cancellationTokenSource = new();
            cancellationToken = cancellationTokenSource.Token;

            Actions = new()
            {
                (task) => action()
            };
        }

        public void Start()
        {
            TaskScheduler.StartTask(this);
        }

        public void OnActionComplete()
        {
            Actions.RemoveAt(0);
            if (Actions.Count == 0)
                return;
            Start();
        }

        public void ContinueWith(Action<BetterTask> action)
        {
            Actions.Add(action);
        }

        public void ContinueWith(Action action)
        {
            Actions.Add((task) => action());
        }

        public void Wait()
        {
            WaitUntilStarted();
            while (Thread != null)
            {
                Thread.Sleep(1);
            }
        }

        public void WaitUntilStarted()
        {
            while (Thread == null)
            {
                Thread.Sleep(1);
            }
        }

        public void Cancel()
        {
            cancellationTokenSource.Cancel();
            Actions.Clear();
        }

        public void Abort()
        {
            Thread?.Abort();
        }

    }
}