using System;
using System.Threading;
using System.Threading.Tasks;
using Flagrum.Abstractions;
using Injectio.Attributes;
using Microsoft.AspNetCore.Components;

namespace Flagrum.Host.WebView;

/// <summary>
/// Dispatches Blazor work to the UI thread to ensure the native web view can be updated accordingly.
/// </summary>
[RegisterSingleton<BlazorWebViewDispatcher>]
public sealed partial class BlazorWebViewDispatcher : Dispatcher
{
    private readonly BlazorSynchronizationContext _context;

    public BlazorWebViewDispatcher(IApplication application)
    {
        _context = new BlazorSynchronizationContext(application);
        _context.UnhandledException += (_, args) => OnUnhandledException(args);
    }

    /// <summary>
    /// Whether the caller is in the correct context to execute work.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the caller can execute work in the current context,
    /// otherwise <c>false</c> if the work needs to be dispatched.
    /// </returns>
    public override bool CheckAccess() => SynchronizationContext.Current == _context;

    /// <inheritdoc />
    public override Task InvokeAsync(Action workItem)
    {
        var taskCompletion = new TaskCompletionSource();

        _context.HandleExecution(state =>
        {
            var (completion, callback) = ((TaskCompletionSource, Action))state!;

            try
            {
                callback();
                completion.SetResult();
            }
            catch (OperationCanceledException)
            {
                completion.SetCanceled();
            }
            catch (Exception exception)
            {
                completion.SetException(exception);
            }
        }, (taskCompletion, workItem));

        return taskCompletion.Task;
    }

    /// <inheritdoc />
    public override Task InvokeAsync(Func<Task> workItem)
    {
        var taskCompletion = new TaskCompletionSource();

        _context.HandleExecution(async state =>
        {
            var (completion, callback) = ((TaskCompletionSource, Func<Task>))state!;

            try
            {
                await callback();
                completion.SetResult();
            }
            catch (OperationCanceledException)
            {
                completion.SetCanceled();
            }
            catch (Exception exception)
            {
                completion.SetException(exception);
            }
        }, (taskCompletion, workItem));

        return taskCompletion.Task;
    }

    /// <inheritdoc />
    public override Task<TResult> InvokeAsync<TResult>(Func<TResult> workItem)
    {
        var taskCompletion = new TaskCompletionSource<TResult>();

        _context.HandleExecution(state =>
        {
            var (completion, callback) = ((TaskCompletionSource<TResult>, Func<TResult>))state!;

            try
            {
                var result = callback();
                completion.SetResult(result);
            }
            catch (OperationCanceledException)
            {
                completion.SetCanceled();
            }
            catch (Exception exception)
            {
                completion.SetException(exception);
            }
        }, (taskCompletion, workItem));

        return taskCompletion.Task;
    }

    /// <inheritdoc />
    public override Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> workItem)
    {
        var taskCompletion = new TaskCompletionSource<TResult>();

        _context.HandleExecution(async state =>
        {
            var (completion, callback) = ((TaskCompletionSource<TResult>, Func<Task<TResult>>))state!;

            try
            {
                var result = await callback();
                completion.SetResult(result);
            }
            catch (OperationCanceledException)
            {
                completion.SetCanceled();
            }
            catch (Exception exception)
            {
                completion.SetException(exception);
            }
        }, (taskCompletion, workItem));

        return taskCompletion.Task;
    }
}