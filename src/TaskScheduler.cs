namespace BetterTasks
{
    internal class TaskScheduler
    {

        private static TaskScheduler? instance;

        private List<Thread> threads;

        private int tasksPerThread = 1;

        private List<BetterTask> waitingTasks;

        public static void CreateTask(BetterTask task)
        {
            instance ??= new();
            instance.CreateThread(task);
        }

        private TaskScheduler()
        {
            threads = new();
            waitingTasks = new();
        }

        private void CreateThread(BetterTask task)
        {
            waitingTasks.Add(task);

            if (waitingTasks.Count >= tasksPerThread * threads.Count)
            {
                Thread thread = new(new ThreadStart(StartThread));
                thread.Start();

                threads.Add(thread);
            }
        }

        private void StartThread()
        {
            while (true)
            {
                BetterTask task;

                // Lock prevents other threads from accessing an object. If another thread is already accessing the object, it will wait until the lock is released.
                lock (waitingTasks)
                {
                    if (waitingTasks.Count == 0)
                        break;

                    task = waitingTasks[0];
                    waitingTasks.RemoveAt(0);
                }

                task.Thread = Thread.CurrentThread;

                task.Action();

                task.Thread = null;
            }
        }

    }
}
