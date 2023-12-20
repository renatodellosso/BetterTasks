# BetterTasks
A rewrite of the C# Task system. Performs about 10% faster than the built-in Task system.

## Installation
Add the .dll file as a dependency for your project.

## Usage
Import the package:
```csharp
using BetterTasks;
```

There are two ways to create and run a BetterTask:
```csharp
BetterTask task = new BetterTask(() => Console.WriteLine("Hello World!"));
task.Start();
```
or
```csharp
BetterTask task = BetterTask.Run(() => Console.WriteLine("Hello World!"));
```

BetterTask is designed to work as similarly as possible to Task. `.ContinueWith(Action action)` and `.Wait()` both work. However, it has a few extra features:

**Passing the Task as a Parameter**<br>
When passing a delegate to a BetterTask, it can take a parameter that will be the BetterTask.
```csharp
BetterTask.Run((task) => Console.WriteLine(task));
```

**Abort**<br>
BetterTask can be aborted with the `.Abort()` method.

**Cancel**<br>
BetterTask can be cancelled with the `.Cancel()` method. The `IsCanceled` property will be true after this method is called. Actions should check if `IsCanceled` is true and shut down if it is.

**ForceCancel**<br>
The `.ForceCancel(int timeout = 2000)` method calls `.Cancel()`, then after `timeout` milliseconds, if the task has not shut down, calls `.Abort()`.

**Task Priority**<br>
BetterTask's constructors have an optional parameter, `priority`, of type `ThreadPriority`, which defaults to `ThreadPriority.Normal`. Whichever thread executes the task will be set to this priority while executing the task.
