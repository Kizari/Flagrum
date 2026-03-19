using System;
using System.Threading;
using System.Threading.Tasks;

namespace Flagrum.Host;

/// <summary>
/// Ensures that work from the Blazor application executes on the UI thread.
/// </summary>
/// <remarks>
/// Ensures that all work is serialized in submission order, is executed on the UI thread,
/// and executes inline when possible.
/// </remarks>
public class BlazorSynchronizationContext : SynchronizationContext
{
    /// <summary>
    /// Continuation delegate that executes a queued <see cref="WorkItem" /> on the UI thread.
    /// </summary>
    private static readonly Action<Task, object?> BackgroundWorkThunk = (_, state) =>
    {
        var workItem = (WorkItem)state!;
        workItem.SynchronizationContext.ExecuteBackground(workItem);
    };

    /// <summary>
    /// Delegate that executes a <see cref="WorkItem" /> within an <see cref="ExecutionContext" />.
    /// </summary>
    private static readonly ContextCallback ExecutionContextThunk = state =>
    {
        var workItem = (WorkItem)state!;
        workItem.SynchronizationContext.ExecuteSynchronously(null, workItem.Callback, workItem.AttachedState);
    };

    private readonly ApplicationHost _application;
    private readonly State _state;

    public BlazorSynchronizationContext(ApplicationHost application) : this(application, new State()) { }

    private BlazorSynchronizationContext(ApplicationHost application, State state)
    {
        _application = application;
        _state = state;
    }

    /// <summary>
    /// Exception handlers to invoke when an unhandled exception occurs during background work.
    /// </summary>
    public event UnhandledExceptionEventHandler? UnhandledException;

    /// <summary>
    /// Creates a shallow copy of this synchronization context.
    /// </summary>
    /// <remarks>
    /// Multiple copies share the same underlying <see cref="State" /> instance so that
    /// all work is serialized consistently, regardless of which copy receives it.
    /// </remarks>
    public override SynchronizationContext CreateCopy() =>
        // ReSharper disable once InconsistentlySynchronizedField (this is safe for a readonly field used by this class)
        new BlazorSynchronizationContext(_application, _state); // Shallow copy

    /// <summary>
    /// Schedules a callback to run asynchronously on the UI thread.
    /// </summary>
    /// <param name="callback">Delegate to invoke.</param>
    /// <param name="state">Optional state to pass to the callback.</param>
    public override void Post(SendOrPostCallback callback, object? state)
    {
        lock (_state.Lock)
        {
            _state.Task = Enqueue(callback, state, true);
        }
    }

    /// <summary>
    /// Executes a callback synchronously on the UI thread.
    /// </summary>
    /// <param name="callback">Delegate to invoke.</param>
    /// <param name="state">Optional state to pass to the callback.</param>
    /// <remarks>
    /// Blocks the calling thread until the callback has completed.
    /// </remarks>
    public override void Send(SendOrPostCallback callback, object? state)
    {
        // Create a new task and add it to the end of the queue
        Task previous;
        var completion = new TaskCompletionSource();

        lock (_state.Lock)
        {
            previous = _state.Task;
            _state.Task = completion.Task;
        }

        // Wait for everything else in the queue to complete execution
        previous.Wait();

        // Execute the callback
        ExecuteSynchronously(completion, callback, state);
    }

    /// <summary>
    /// Executes the specified callback either synchronously or asynchronously,
    /// depending on whether the synchronization context is currently idle.
    /// </summary>
    /// <param name="callback">Delegate to invoke.</param>
    /// <param name="state">Optional state to pass to the callback.</param>
    /// <remarks>
    /// If no work is currently in progress, the callback executes immediately on the UI thread.
    /// Otherwise, it is enqueued and executed later as part of the serialized task chain.
    /// </remarks>
    public void HandleExecution(SendOrPostCallback callback, object state)
    {
        TaskCompletionSource completion;

        lock (_state.Lock)
        {
            // Enqueue the callback if work is already being done
            if (!_state.Task.IsCompleted)
            {
                _state.Task = Enqueue(callback, state, false);
                return;
            }

            // Execute synchronously as no work is being done currently
            completion = new TaskCompletionSource();
            _state.Task = completion.Task;
        }

        ExecuteSynchronously(completion, callback, state);
    }

    /// <summary>
    /// Executes work synchronously.
    /// </summary>
    /// <param name="completion">Optional completion to set when execution is finished.</param>
    /// <param name="callback">Delegate to invoke.</param>
    /// <param name="state">Optional state to pass to the callback.</param>
    /// <remarks>
    /// Work is always dispatched to the UI thread, as Blazor must be able to interact with the native web view.
    /// This is synchronous in the sense that it will not return until the work is complete.
    /// </remarks>
    private void ExecuteSynchronously(
        TaskCompletionSource? completion,
        SendOrPostCallback callback,
        object? state)
    {
        _application.Invoke(() =>
        {
            var original = Current;

            try
            {
                SetSynchronizationContext(this);
                callback(state);
            }
            finally
            {
                SetSynchronizationContext(original);
                completion?.SetResult();
            }
        });
    }

    /// <summary>
    /// Executes a queued <see cref="WorkItem" /> as part of the serialized task chain.
    /// </summary>
    /// <param name="workItem">Work item to execute.</param>
    /// <remarks>
    /// If the work item has a captured <see cref="ExecutionContext" />, it is restored before invoking the callback.
    /// Exceptions are caught and forwarded to <see cref="UnhandledException" /> because there is no synchronous
    /// caller to observe them.
    /// </remarks>
    private void ExecuteBackground(WorkItem workItem)
    {
        // Execute on the original execution context if applicable
        if (workItem.ExecutionContext != null)
        {
            try
            {
                ExecutionContext.Run(workItem.ExecutionContext, ExecutionContextThunk, workItem);
            }
            catch (Exception exception)
            {
                UnhandledException?.Invoke(this, new UnhandledExceptionEventArgs(exception, false));
            }

            return;
        }

        // Execute in the current execution context
        try
        {
            ExecuteSynchronously(null, workItem.Callback, workItem.AttachedState);
        }
        catch (Exception exception)
        {
            UnhandledException?.Invoke(this, new UnhandledExceptionEventArgs(exception, false));
        }
    }

    /// <summary>
    /// Appends a callback to the serialized task chain.
    /// </summary>
    /// <param name="callback">Delegate to invoke.</param>
    /// <param name="state">Optional state to pass to the callback.</param>
    /// <param name="forceAsync">
    /// Whether to run the continuation asynchronously, even if the antecedent has already completed.
    /// </param>
    /// <returns>
    /// A <see cref="Task" /> that represents the scheduled work.
    /// The task completed when the callback has finished executing on the UI thread.
    /// </returns>
    private Task Enqueue(SendOrPostCallback callback, object? state, bool forceAsync)
    {
        // Attempt to capture the execution context
        ExecutionContext? executionContext = null;
        if (!ExecutionContext.IsFlowSuppressed())
        {
            executionContext = ExecutionContext.Capture();
        }

        // Add callback as a continuation of the current task
        return _state.Task.ContinueWith(BackgroundWorkThunk,
            new WorkItem(this, executionContext, callback, state),
            CancellationToken.None,
            forceAsync ? TaskContinuationOptions.RunContinuationsAsynchronously : TaskContinuationOptions.None,
            TaskScheduler.Current);
    }

    /// <summary>
    /// Holds state for <see cref="BlazorSynchronizationContext" />.
    /// </summary>
    private class State
    {
        /// <summary>
        /// Used to synchronize this class.
        /// </summary>
        public Lock Lock { get; } = new();

        /// <summary>
        /// End of the serialized task chain.
        /// </summary>
        public Task Task { get; set; } = Task.CompletedTask;
    }

    /// <summary>
    /// Represents work that is to be executed on the UI thread.
    /// </summary>
    /// <param name="SynchronizationContext">Instance of the parent class.</param>
    /// <param name="ExecutionContext">Optional execution context in which to run the callback.</param>
    /// <param name="Callback">Delegate to invoke.</param>
    /// <param name="AttachedState">Optional state to pass to the callback.</param>
    private record WorkItem(
        BlazorSynchronizationContext SynchronizationContext,
        ExecutionContext? ExecutionContext,
        SendOrPostCallback Callback,
        object? AttachedState);
}