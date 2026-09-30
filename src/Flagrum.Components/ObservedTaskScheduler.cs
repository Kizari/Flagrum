using Microsoft.Extensions.Logging;

namespace Flagrum.Components;

/// <summary>
/// Handles scheduling tasks in such a way that exceptions are observed when the task is not awaited.
/// </summary>
public class ObservedTaskScheduler(ILogger<ObservedTaskScheduler> logger)
{
    /// <summary>
    /// Fires off a task asynchronously, ensuring that any exceptions
    /// that occur during the operation of the task are observed.
    /// </summary>
    /// <param name="task">Task to execute.</param>
    /// <exception cref="Exception">
    /// Thrown if any exceptions occur during the operation of the task.
    /// </exception>
    public void RunAsyncObserved(Func<Task> task)
    {
        Task.Run(task).ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                var exception = t.Exception.Flatten().InnerException!;
                logger.LogCritical(exception, "Unhandled exception occurred in task");
                Environment.Exit(1);
            }
        });
    }

    /// <summary>
    /// Fires off a task that is meant to run for a long period of time asynchronously,
    /// ensuring that any exceptions that occur during the operation of the task are observed.
    /// </summary>
    /// <param name="task">Task to execute.</param>
    /// <param name="cancellationToken">Token that may cancel the operation.</param>
    /// <exception cref="Exception">
    /// Thrown if any exceptions occur during the operation of the task.
    /// </exception>
    public void RunLongRunningObserved(Func<Task> task, CancellationToken cancellationToken)
    {
        Task.Factory.StartNew(task, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default)
            .Unwrap()
            .ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    var exception = t.Exception.Flatten().InnerException!;
                    logger.LogCritical(exception, "Unhandled exception occurred in long-running task");
                    Environment.Exit(1);
                }
            }, cancellationToken);
    }
}