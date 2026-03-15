using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using Flagrum.ApplicationHost.Native;
using Flagrum.Utilities;
using Injectio.Attributes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.FileProviders;

namespace Flagrum.ApplicationHost.WebView;

/// <summary>
/// A web view control that hosts Blazor applications.
/// </summary>
[RegisterSingleton<BlazorWebView>]
public sealed class BlazorWebView : IDisposable
{
    private readonly BlazorWebViewDispatcher _dispatcher;
    private readonly BlazorWebViewManager _webViewManager;

    /// <summary>
    /// Creates a new Blazor web view.
    /// </summary>
    public BlazorWebView(
        IServiceProvider serviceProvider,
        IFileProvider fileProvider,
        JSComponentConfigurationStore configStore,
        NativeWindow window,
        ObservedTaskScheduler scheduler)
    {
        _dispatcher = new BlazorWebViewDispatcher();
        _webViewManager = new BlazorWebViewManager(
            serviceProvider,
            fileProvider,
            configStore,
            _dispatcher,
            scheduler,
            this);

        NativeImpl = new NativeWebView(window, _webViewManager.OnWebMessageReceived);
    }

    /// <summary>
    /// The underlying native web view control.
    /// </summary>
    public NativeWebView NativeImpl { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        NativeImpl.Dispose();
        _dispatcher.Dispose();
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
    public void Navigate(string url) => NativeImpl.Navigate(url);

    /// <summary>
    /// Sends a web message to the web view.
    /// </summary>
    /// <param name="message"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SendMessage(string message) =>
        NativeImpl.RunJavaScript($"__dispatchMessageCallback({JsonSerializer.Serialize(message)})");
}