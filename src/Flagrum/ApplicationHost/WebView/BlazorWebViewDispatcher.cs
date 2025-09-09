using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebView;

namespace Flagrum.ApplicationHost.WebView;

/// <summary>
/// Wrapper around Avalonia's dispatcher that allows it to be used with <see cref="WebViewManager"/>.
/// </summary>
public class BlazorWebViewDispatcher : Dispatcher
{
    /// <inheritdoc />
    public override bool CheckAccess() => Environment.CurrentManagedThreadId == Program.MainThreadId;

    /// <inheritdoc />
    public override Task InvokeAsync(Action workItem)
    {
        return Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(workItem).GetTask();
    }

    /// <inheritdoc />
    public override Task InvokeAsync(Func<Task> workItem)
    {
        return Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(workItem);
    }

    /// <inheritdoc />
    public override Task<TResult> InvokeAsync<TResult>(Func<TResult> workItem)
    {
        return Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(workItem).GetTask();
    }

    /// <inheritdoc />
    public override Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> workItem)
    {
        return Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(workItem);
    }
}