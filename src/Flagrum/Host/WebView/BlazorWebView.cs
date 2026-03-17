using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using Flagrum.Utilities;
using Injectio.Attributes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.FileProviders;

namespace Flagrum.Host.WebView;

/// <summary>
/// A web view control that hosts Blazor applications.
/// </summary>
[RegisterSingleton<BlazorWebView>]
public sealed class BlazorWebView
{
    private readonly BlazorWebViewDispatcher _dispatcher;
    private readonly BlazorWebViewManager _webViewManager;
    private readonly ApplicationHost _application;

    /// <summary>
    /// Creates a new Blazor web view.
    /// </summary>
    public BlazorWebView(
        IServiceProvider serviceProvider,
        IFileProvider fileProvider,
        JSComponentConfigurationStore configStore,
        ApplicationHost application,
        ObservedTaskScheduler scheduler,
        BlazorWebViewDispatcher dispatcher)
    {
        _application = application;
        _dispatcher = dispatcher;
        _webViewManager = new BlazorWebViewManager(
            serviceProvider,
            fileProvider,
            configStore,
            _dispatcher,
            scheduler,
            this);

        _application.SetWebMessageHandler(_webViewManager.OnWebMessageReceived);
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
    public void Navigate(string url) => _application.NavigateWebView(url);

    /// <summary>
    /// Sends a web message to the web view.
    /// </summary>
    /// <param name="message"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SendMessage(string message) =>
        _application.RunJavaScript($"__dispatchMessageCallback({JsonSerializer.Serialize(message)})");
}