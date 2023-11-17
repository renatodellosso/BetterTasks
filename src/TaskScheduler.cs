namespace BetterTasks
{
    internal class TaskScheduler
    {

        private static TaskScheduler? instance;

        private readonly List<Thread> availableThreads;

        /// <summary>
        /// The ratio of waiting tasks to available threads before a new thread is created.
        /// </summary>
        private const int QUEUED_TASKS_PER_AVAILABLE_THREAD = 2, MIN_THREADS = 1;

        private readonly List<ITask> waitingTasks;

        public static void StartTask(ITask task)
        {
            instance ??= new();
            instance.QueueTask(task);
        }

        private TaskScheduler()
        {
            availableThreads = new();
            waitingTasks = new();
        }

        private void QueueTask(ITask task)
        {
            lock (waitingTasks)
            {
                waitingTasks.Add(task);

                lock (availableThreads)
                {
                    if (waitingTasks.Count >= QUEUED_TASKS_PER_AVAILABLE_THREAD * availableThreads.Count)
                    {
                        Thread thread = new(new ThreadStart(StartThread));
                        thread.Start();

                        availableThreads.Add(thread);
                    }
                }
            }
        }

        private void StartThread()
        {
            try
            {
                while (true)
                {
                    ITask task;

                    // Lock prevents other threads from accessing an object. If another thread is already accessing the object, it will wait until the lock is released.
                    lock (waitingTasks)
                    {
                        if (waitingTasks.Count == 0)
                        {
                            lock (availableThreads)
                            {
                                // Keep one thread running to avoid a delay when starting tasks
                                if (availableThreads.Count > MIN_THREADS)
                                {
                                    availableThreads.Remove(Thread.CurrentThread);
                                    break;
                                }
                                else continue;
                            }
                        }

                        task = waitingTasks[0];
                        waitingTasks.RemoveAt(0);
                    }

                    lock (availableThreads)
                        availableThreads.Remove(Thread.CurrentThread);

                    task.Thread = Thread.CurrentThread;

                    Thread.CurrentThread.Priority = task.Priority;

                    try
                    {
                        //Console.WriteLine("Executing task on thread " + Thread.CurrentThread.ManagedThreadId);
                        task.Result = task.Execute();
                        //Console.WriteLine("Task on thread " + Thread.CurrentThread.ManagedThreadId + " completed");
                    }
                    catch (ThreadInterruptedException) { }

                    Thread.CurrentThread.Priority = ThreadPriority.Normal;

                    task.Thread = null;

                    task.OnActionComplete();

                    lock (availableThreads)
                        availableThreads.Add(Thread.CurrentThread);
                }
            }
            catch (Exception e)
            {
                throw new Exception("An error occurred while executing a task.", e);
            }
        }

    }
}
