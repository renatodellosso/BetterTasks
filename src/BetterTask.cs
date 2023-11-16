namespace BetterTasks
{
    public class BetterTask
    {

        internal Action Action { get; private set; }
        internal Thread? Thread { private get; set; }

        public BetterTask(Action action)
        {
            Action = action;

            TaskScheduler.CreateTask(this);
        }

    }
}