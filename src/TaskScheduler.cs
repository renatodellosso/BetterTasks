namespace BetterTasks
{
    internal class TaskScheduler
    {

        private static TaskScheduler? instance;

        private readonly List<Thread> availableThreads;

        /// <summary>
        /// The ratio of waiting tasks to available threads before a new thread is created.
        /// </summary>
        private const int tasksPerThread = 2;

        private readonly List<ITask> waitingTasks;

        public static void StartTask(ITask task)
        {
            instance ??= new();
            instance.CreateThread(task);
        }

        private TaskScheduler()
        {
            availableThreads = new();
            waitingTasks = new();
        }

        private void CreateThread(ITask task)
        {
            waitingTasks.Add(task);

            if (waitingTasks.Count >= tasksPerThread * availableThreads.Count)
            {
                Thread thread = new(new ThreadStart(StartThread));
                thread.Start();

                availableThreads.Add(thread);
            }
        }

        private void StartThread()
        {
            while (true)
            {
                ITask task;

                // Lock prevents other threads from accessing an object. If another thread is already accessing the object, it will wait until the lock is released.
                lock (waitingTasks)
                {
                    if (waitingTasks.Count == 0)
                        break;

                    task = waitingTasks[0];
                    waitingTasks.RemoveAt(0);
                }

                lock (availableThreads)
                    availableThreads.Remove(Thread.CurrentThread);

                task.Thread = Thread.CurrentThread;

                try
                {
                    task.Result = task.Execute();
                }
                catch (ThreadInterruptedException) { }

                task.Thread = null;

                task.OnActionComplete();

                lock (availableThreads)
                    availableThreads.Add(Thread.CurrentThread);
            }
        }

    }
}
