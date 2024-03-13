namespace BetterTasks;

public class TaskExecutionError : Exception
{

    public TaskExecutionError() : base("An exception occurred while executing a task.")
    {
    }

    public TaskExecutionError(string message) : base(message)
    {
    }

    public TaskExecutionError(string message, Exception innerException) : base(
        message, innerException)
    {
    }

}