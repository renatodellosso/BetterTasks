namespace BetterTasks
{
    public class BetterTask : IDisposable, IAsyncResult
    {

        internal Action<BetterTask> Action => Actions[0];
        internal Thread? Thread { private get; set; }

        internal List<Action<BetterTask>> Actions { get; private set; }

        private readonly CancellationToken cancellationToken;
        private readonly CancellationTokenSource cancellationTokenSource;

        private readonly WaitHandle waitHandle;

        public bool IsCanceled => cancellationToken.IsCancellationRequested;

        public BetterTask(Action action)
        {
            cancellationTokenSource = new();
            cancellationToken = cancellationTokenSource.Token;

            waitHandle = cancellationToken.WaitHandle;

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

        /// <summary>
        /// Waits until the <seealso cref="TaskScheduler"/> starts the task."/>
        /// </summary>
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

        /// <summary>
        /// Forces the task to stop. WARNING: This will stop the thread, even if it is in the middle of something. 
        /// NOTE: This can be overridden by catching ThreadInterruptedException.
        /// </summary>
        public void Abort()
        {
            // Interrupt throws a ThreadInterruptedException in the thread.
            Thread?.Interrupt();
        }

        /// <summary>
        /// ForceCancel is a combination of Cancel and Abort.
        /// It will cancel the task and wait for it to finish.
        /// If the task does not finish in the given timeout, it will abort the thread.
        /// </summary>
        /// <param name="timeout">How long to wait for the task to cancel before abort it</param>
        public void ForceCancel(int timeout = 2000)
        {

            Cancel();

            for (int i = 0; i < timeout; i++)
            {
                if (Thread == null)
                    return;
                Thread.Sleep(1);
            }

            Abort();
        }

        public void Dispose()
        {
            // Don't know what this does or why we need it, but I got a suggestion to add it
            GC.SuppressFinalize(this);

            ForceCancel();

            cancellationTokenSource.Dispose();
            waitHandle.Close();
        }

        /// <summary>
        /// Not implemented. I'm not sure what it should return.
        /// </summary>
        public object? AsyncState => throw new NotImplementedException();

        public WaitHandle AsyncWaitHandle => waitHandle;

        /// <summary>
        /// Not implemented. Currently just returns <seealso cref="IsCompleted"/>.
        /// </summary>
        public bool CompletedSynchronously => IsCompleted;

        public bool IsCompleted => Actions.Count == 0;

        // Allows us to use the await keyword
        public Awaiter GetAwaiter()
        {
            return new(this);
        }
    }
}