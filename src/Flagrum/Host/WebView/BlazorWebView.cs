using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform;
using Flagrum.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.FileProviders;

namespace Flagrum.Host.WebView;

/// <summary>
/// A web view control that hosts Blazor applications.
/// </summary>
public sealed class BlazorWebView : NativeWebView
{
    private readonly BlazorWebViewManager _webViewManager;

    /// <summary>
    /// Creates a new Blazor web view.
    /// </summary>
    public BlazorWebView(
        IServiceProvider serviceProvider,
        IFileProvider fileProvider,
        JSComponentConfigurationStore configStore,
        ObservedTaskScheduler scheduler,
        BlazorWebViewDispatcher dispatcher)
    {
        // Force Linux to use WebKitGTK over WPE WebKit, as the latter is buggy at time of writing
        EnvironmentRequested += (_, args) =>
        {
            if (args is LinuxWpeWebViewEnvironmentRequestedEventArgs wpeArgs)
            {
                wpeArgs.PreferWebKitGtkInstead = true;
            }
        };

        _webViewManager = new BlazorWebViewManager(
            serviceProvider,
            fileProvider,
            configStore,
            dispatcher,
            scheduler,
            this);
    }

    /// <summary>
    /// Sets the root Blazor component for this web view.
    /// </summary>
    /// <param name="selector">CSS selector for the element that the component is to be rendered in.</param>
    /// <typeparam name="TComponent">Type of the root Blazor component.</typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task SetRootComponentAsync<TComponent>(string selector) where TComponent : ComponentBase =>
        _webViewManager.AddRootComponentAsync(typeof(TComponent), selector, ParameterView.Empty);

    /// <summary>
    /// Navigates the web view to the given URL.
    /// </summary>
    /// <param name="url">URL to navigate the web view to.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Navigate(string url)
    {
        Navigate(new Uri(url));
    }

    /// <summary>
    /// Sends a web message to the web view.
    /// </summary>
    /// <param name="message"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SendMessage(string message)
    {
        InvokeScript($"__dispatchMessageCallback({JsonSerializer.Serialize(message)})");
    }
}