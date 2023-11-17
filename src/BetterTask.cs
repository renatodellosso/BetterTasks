namespace BetterTasks
{
    public class BetterTask<TResult> : IDisposable, IAsyncResult, ITask
    {

        internal Func<BetterTask<TResult>, TResult>? Action => Actions.Count > 0 ? Actions[0] : null;
        internal List<Func<BetterTask<TResult>, TResult>> Actions { get; private set; }

        internal Thread? Thread { private get; set; }
        Thread? ITask.Thread { get => Thread; set => Thread = value; }

        private readonly CancellationToken cancellationToken;
        private readonly CancellationTokenSource cancellationTokenSource;

        private readonly WaitHandle waitHandle;

        public bool IsCanceled => cancellationToken.IsCancellationRequested;

        public TResult? Result { get; internal set; }
        object? ITask.Result
        {
            get => Result;
            set
            {
                if (value == null)
                {
                    Result = default!;
                    return;
                }

                Result = (TResult?)value;
            }
        }

        public BetterTask(Func<BetterTask<TResult>, TResult> action)
        {
            cancellationTokenSource = new();
            cancellationToken = cancellationTokenSource.Token;

            waitHandle = cancellationToken.WaitHandle;

            Actions = new()
            {
                action
            };
        }

        public BetterTask(Action action) : this(WrapAction(action)) { }
        public BetterTask(Func<TResult> action) : this(WrapAction(action)) { }
        public BetterTask(Action<BetterTask<TResult>> action) : this(WrapAction(action)) { }

        public void Start()
        {
            TaskScheduler.StartTask(this);
        }

        internal static Func<BetterTask<TResult>, TResult> WrapAction(Action action)
        {
            return (task) =>
            {
                action();
                return default!;
            };
        }

        internal static Func<BetterTask<TResult>, TResult> WrapAction(Func<TResult> action)
        {
            return (task) => action();
        }

        internal static Func<BetterTask<TResult>, TResult> WrapAction(Action<BetterTask<TResult>> action)
        {
            return (task) =>
            {
                action(task);
                return default!;
            };
        }

        object? ITask.Execute()
        {
            Func<BetterTask<TResult>, TResult>? action = Action;

            if (action == null)
                return null;

            return action.Invoke(this);
        }

        void ITask.OnActionComplete()
        {
            if (Actions.Count == 0)
                return;

            Actions.RemoveAt(0);
            Start();
        }

        public void ContinueWith(Func<BetterTask<TResult>, TResult> action)
        {
            Actions.Add(action);
        }

        public void ContinueWith(Action action)
        {
            Actions.Add(WrapAction(action));
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
        /// Waits until the <see cref="TaskScheduler"/> starts the task."/>
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
            Actions.Clear();
            Thread?.Interrupt(); // Interrupt throws a ThreadInterruptedException in the thread.
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
        /// Not implemented. Currently just returns <see cref="IsCompleted"/>.
        /// </summary>
        public bool CompletedSynchronously => IsCompleted;

        public bool IsCompleted => Actions.Count == 0;

        // Allows us to use the await keyword
        public Awaiter<TResult> GetAwaiter()
        {
            return new(this);
        }

    }
}