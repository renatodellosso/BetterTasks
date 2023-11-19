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
