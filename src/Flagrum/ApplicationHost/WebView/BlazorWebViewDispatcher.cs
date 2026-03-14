using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Flagrum.ApplicationHost.Native;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebView;

namespace Flagrum.ApplicationHost.WebView;

// TODO: This was taken from another of my project's where queuing was necessary
//       However, it may not be here since Qt uses its own queue internally
//       and all actions are dispatched regardless of which thread they're called from

/// <summary>
/// Dispatches actions to the native library to be invoked on the main thread in the way that is most
/// suitable for the target platform.
/// </summary>
public sealed partial class BlazorWebViewDispatcher : Dispatcher, IDisposable
{
    private readonly CancellationTokenSource _cancellation = new();
    private readonly NativeDispatcher _dispatcher = new();

    /// <summary>
    /// It's very important that this class uses a queue for the actions, even if the action could have been
    /// run synchronously immediately. The reason for this is that when <see cref="WebViewManager" /> dispatches
    /// actions here, they can execute out of order and cause exceptions. For example, if a root component is
    /// being added asynchronously, and then the render cycle starts synchronously, it would throw an exception
    /// due to the root component not being initialized yet. As such, we will use this queue for all actions.
    /// </summary>
    private readonly BlockingCollection<IQueueItem> _queue = [];

    private readonly Thread _thread;

    public BlazorWebViewDispatcher()
    {
        // Run the dispatch loop on a separate thread to avoid blocking the UI thread
        _thread = new Thread(DispatchLoop);
        _thread.Start();
    }

    public void Dispose()
    {
        _cancellation.Cancel();
        _dispatcher.Dispose();
    }

    /// <summary>
    /// Runs a loop to dispatch actions for the lifetime of the application.
    /// </summary>
    private void DispatchLoop()
    {
        while (!_cancellation.IsCancellationRequested)
        {
            IQueueItem next;

            try
            {
                // Block until the next item is available
                next = _queue.Take(_cancellation.Token);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            // Invoke the action on the main thread
            _dispatcher.Invoke(next.Execute);
        }

        _thread.Join();
    }

    /// <inheritdoc />
    public override bool CheckAccess() => true; // Must always dispatch, as Qt creates and owns its own UI thread

    /// <inheritdoc />
    public override Task InvokeAsync(Action workItem)
    {
        var item = new QueueItem(workItem);
        _queue.Add(item);
        return item.Task;
    }

    /// <inheritdoc />
    public override Task InvokeAsync(Func<Task> workItem)
    {
        var item = new QueueItem(workItem);
        _queue.Add(item);
        return item.Task;
    }

    /// <inheritdoc />
    public override Task<TResult> InvokeAsync<TResult>(Func<TResult> workItem)
    {
        var item = new QueueItem<TResult>(workItem);
        _queue.Add(item);
        return item.Task;
    }

    /// <inheritdoc />
    public override Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> workItem)
    {
        var item = new QueueItem<TResult>(workItem);
        _queue.Add(item);
        return item.Task;
    }

    /// <summary>
    /// Represents an action in the dispatcher queue.
    /// </summary>
    private interface IQueueItem
    {
        /// <summary>
        /// Executes the action associated with this queue item.
        /// </summary>
        void Execute();
    }

    /// <summary>
    /// Represents an action in the dispatcher queue.
    /// </summary>
    /// <param name="callback">The action to dispatch.</param>
    /// <typeparam name="TResult">The return type of the action.</typeparam>
    private abstract class QueueItemBase<TResult>(Delegate callback) : IQueueItem
    {
        protected readonly TaskCompletionSource<TResult> _completion = new();

        /// <inheritdoc />
        /// <exception cref="NotSupportedException">Throws if the delegate type is not recognised.</exception>
        public void Execute()
        {
            var result = callback.DynamicInvoke();
            switch (result)
            {
                case null:
                    break;
                case TResult value:
                    _completion.SetResult(value);
                    break;
                case Task<TResult> resultTask:
                    resultTask.ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                        {
                            throw t.Exception;
                        }

                        _completion.SetResult(t.Result);
                    });
                    break;
                case Task task:
                    task.ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                        {
                            throw t.Exception;
                        }

                        _completion.SetResult(default!);
                    });
                    break;
                default:
                    throw new NotSupportedException($"Unknown delegate type {callback.GetType()}");
            }
        }
    }

    /// <summary>
    /// A queued action with no return value.
    /// </summary>
    /// <param name="callback">The action to dispatch.</param>
    private class QueueItem(Delegate callback) : QueueItemBase<object>(callback)
    {
        /// <summary>
        /// The task representing the completion of the callback execution.
        /// </summary>
        public Task Task => _completion.Task;
    }

    /// <summary>
    /// A queued action with a return value.
    /// </summary>
    /// <param name="callback">The action to dispatch.</param>
    /// <typeparam name="TResult">The return type of the action.</typeparam>
    private class QueueItem<TResult>(Delegate callback) : QueueItemBase<TResult>(callback)
    {
        /// <summary>
        /// The task representing the completion of the callback execution.
        /// </summary>
        public Task<TResult> Task => _completion.Task;
    }
}