namespace BetterTasks
{
    public class BetterTask : BetterTask<object?>
    {
        public BetterTask(Action action, ThreadPriority priority = ThreadPriority.Normal) : base(action, priority) { }
        public BetterTask(Func<object?> action, ThreadPriority priority = ThreadPriority.Normal) : base(action, priority) { }
        public BetterTask(Action<BetterTask<object?>> action, ThreadPriority priority = ThreadPriority.Normal) : base(action, priority) { }

        public static new BetterTask Run(Action action, ThreadPriority priority = ThreadPriority.Normal)
        {
            return (BetterTask)Run(WrapAction(action), priority);
        }

        public static new BetterTask Run(Func<object?> action, ThreadPriority priority = ThreadPriority.Normal)
        {
            return (BetterTask)Run(WrapAction(action), priority);
        }

        public static new BetterTask Run(Action<BetterTask<object?>> action, ThreadPriority priority = ThreadPriority.Normal)
        {
            return (BetterTask)Run(WrapAction(action), priority);
        }
    }

    public class BetterTask<TResult> : IDisposable, IAsyncResult, ITask
    {

        internal Func<BetterTask<TResult>, TResult?>? Action => Actions.Count > 0 ? Actions[0] : null;
        internal List<Func<BetterTask<TResult>, TResult?>> Actions { get; private set; }

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
                    Result = default;
                    return;
                }

                Result = (TResult?)value;
            }
        }

        internal ThreadPriority Priority { get; set; }
        ThreadPriority ITask.Priority => Priority;

        public BetterTask(Func<BetterTask<TResult>, TResult?> action, ThreadPriority priority = ThreadPriority.Normal)
        {
            cancellationTokenSource = new();
            cancellationToken = cancellationTokenSource.Token;

            waitHandle = cancellationToken.WaitHandle;

            Priority = priority;

            Actions = new()
            {
                action
            };
        }

        public BetterTask(Action action, ThreadPriority priority = ThreadPriority.Normal) : this(WrapAction(action), priority) { }
        public BetterTask(Func<TResult?> action, ThreadPriority priority = ThreadPriority.Normal) : this(WrapAction(action), priority) { }
        public BetterTask(Action<BetterTask<TResult>> action, ThreadPriority priority = ThreadPriority.Normal) : this(WrapAction(action), priority) { }

        public void Start()
        {
            TaskScheduler.StartTask(this);
        }

        /// <summary>
        /// Creates a new <see cref="BetterTask{TResult}"/> and starts it.
        /// </summary>
        /// <returns>The <see cref="BetterTask{TResult}"/> that was created</returns>
        public static BetterTask<TResult> Run(Func<BetterTask<TResult>, TResult?> action, ThreadPriority priority = ThreadPriority.Normal)
        {
            BetterTask<TResult> task = new(action, priority);
            task.Start();
            return task;
        }

        /// <summary>
        /// Creates a new <see cref="BetterTask{TResult}"/> and starts it.
        /// </summary>
        /// <returns>The <see cref="BetterTask{TResult}"/> that was created</returns>
        public static BetterTask<TResult> Run(Action action, ThreadPriority priority = ThreadPriority.Normal)
        {
            return Run(WrapAction(action), priority);
        }

        /// <summary>
        /// Creates a new <see cref="BetterTask{TResult}"/> and starts it.
        /// </summary>
        /// <returns>The <see cref="BetterTask{TResult}"/> that was created</returns>
        public static BetterTask<TResult> Run(Func<TResult?> action, ThreadPriority priority = ThreadPriority.Normal)
        {
            return Run(WrapAction(action), priority);
        }

        /// <summary>
        /// Creates a new <see cref="BetterTask{TResult}"/> and starts it.
        /// </summary>
        /// <returns>The <see cref="BetterTask{TResult}"/> that was created</returns>
        public static BetterTask<TResult> Run(Action<BetterTask<TResult>> action, ThreadPriority priority = ThreadPriority.Normal)
        {
            return Run(WrapAction(action), priority);
        }

        internal static Func<BetterTask<TResult>, TResult?> WrapAction(Action action)
        {
            return (task) =>
            {
                action();
                return task.Result;
            };
        }

        internal static Func<BetterTask<TResult>, TResult?> WrapAction(Func<TResult?> action)
        {
            return (task) => action();
        }

        internal static Func<BetterTask<TResult>, TResult?> WrapAction(Action<BetterTask<TResult>> action)
        {
            return (task) =>
            {
                action(task);
                return task.Result;
            };
        }

        object? ITask.Execute()
        {
            Func<BetterTask<TResult>, TResult?>? action = Action;

            if (action == null)
                return null;

            return action.Invoke(this);
        }

        void ITask.OnActionComplete()
        {
            if (Actions.Count == 0)
                return;

            Actions.RemoveAt(0);

            if (Actions.Count == 0)
                return;

            Start();
        }

        public void ContinueWith(Func<BetterTask<TResult>, TResult?> action)
        {
            Actions.Add(action);
        }

        public void ContinueWith(Action action)
        {
            Actions.Add(WrapAction(action));
        }

        public void ContinueWith(Func<TResult?> action)
        {
            Actions.Add(WrapAction(action));
        }

        public void ContinueWith(Action<BetterTask<TResult>> action)
        {
            Actions.Add(WrapAction(action));
        }

        public void Wait()
        {
            while (!IsCompleted)
            {
                Thread.Sleep(1);
            }
        }

        public static void WaitAll(params BetterTask<TResult>[] tasks)
        {
            while (true)
            {
            Cont:
                Thread.Sleep(1);
                foreach (BetterTask<TResult> task in tasks)
                {
                    if (!task.IsCompleted)
                        goto Cont;
                }

                break;
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

        public bool IsCompleted => Actions.Count == 0 && Thread == null;

        // Allows us to use the await keyword
        public Awaiter<TResult> GetAwaiter()
        {
            return new(this);
        }

    }
}