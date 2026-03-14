using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Flagrum.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.FileProviders;

namespace Flagrum.ApplicationHost.WebView;

/// <inheritdoc cref="WebViewManager" />
public class BlazorWebViewManager : WebViewManager
{
    /// <summary>
    /// The path of <c>index.html</c> relative to <c>wwwroot</c>.
    /// </summary>
    private const string HostPageRelativePath = "index.html";

    private readonly CancellationTokenSource _cancellation;
    private readonly HttpListener _listener;

    private readonly BlazorWebView _webView;

    /// <inheritdoc cref="WebViewManager" />
    /// <param name="provider">The service provider associated with this web view's scope.</param>
    /// <param name="fileProvider">A file provider that resolves web resources for this application.</param>
    /// <param name="jsComponents">The JS component configuration store for this application.</param>
    /// <param name="dispatcher">A dispatcher that synchronously dispatches actions to the UI thread.</param>
    /// <param name="webView">Web view that this manager is to manage.</param>
    public BlazorWebViewManager(
        IServiceProvider provider,
        IFileProvider fileProvider,
        JSComponentConfigurationStore jsComponents,
        Dispatcher dispatcher,
        BlazorWebView webView)
        : base(provider, dispatcher, BaseUri, fileProvider, jsComponents, HostPageRelativePath)
    {
        _webView = webView;

        // Start the local HTTP server that serves the application content to the web view
        _listener = new HttpListener();
        _listener.Prefixes.Add(BaseUri.AbsoluteUri);
        _cancellation = new CancellationTokenSource();
        _listener.Start();
        ObservedTaskScheduler.RunLongRunningObserved(() => ListenLoop(_cancellation.Token), _cancellation.Token);
    }

    /// <summary>
    /// The root URI of this web application.
    /// </summary>
    private static Uri BaseUri => field ??= new Uri($"http://localhost:{GetAvailablePort()}/");

    /// <summary>
    /// Creates an absolute URI from a relative path.
    /// </summary>
    /// <param name="relativePath">The resource path, relative to the web root.</param>
    /// <returns>The absolute URI.</returns>
    public static string CreateUri(string relativePath) => $"{BaseUri.ToString().TrimEnd('/')}{relativePath}";

    /// <inheritdoc />
    protected override ValueTask DisposeAsyncCore()
    {
        var task = base.DisposeAsyncCore();
        _cancellation.Cancel();
        _listener.Stop();
        return task;
    }

    /// <inheritdoc />
    protected override void NavigateCore(Uri absoluteUri)
    {
        _webView.Navigate(absoluteUri.ToString());
    }

    /// <inheritdoc />
    protected override void SendMessage(string message)
    {
        _webView.SendMessage(message);
    }

    /// <summary>
    /// Handles web messages received from the web view.
    /// </summary>
    /// <param name="message">The message that was received.</param>
    public void OnWebMessageReceived(string message)
    {
        MessageReceived(BaseUri, message);
    }

    /// <summary>
    /// Listens for HTTP requests from the web view and handles them accordingly.
    /// </summary>
    /// <param name="cancellationToken">Token that stops the loop when canceled.</param>
    private async Task ListenLoop(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            HttpListenerContext context;

            // Wait for the next request
            try
            {
                context = await _listener.GetContextAsync();
            }
            catch
            {
                break;
            }

            // Process the request separately to avoid delaying the next request
            ObservedTaskScheduler.RunAsyncObserved(() => HandleRequest(context));
        }
    }

    /// <summary>
    /// Handles an HTTP request.
    /// </summary>
    /// <param name="context">Context in which the request was received.</param>
    private async Task HandleRequest(HttpListenerContext context)
    {
        var uri = context.Request.Url!.AbsoluteUri;
        var isFile = Path.HasExtension(context.Request.Url.LocalPath);

        // Remove the version query from the URI as it prevents TryGetResponseContent from retrieving the file
        // this is only here to prevent the web view from reusing the cached image anyway
        var index = uri.IndexOf("?v=", StringComparison.OrdinalIgnoreCase);
        if (index > -1)
        {
            uri = uri[..index];
        }

        // Get the response content
        if (TryGetResponseContent(uri, !isFile, out var statusCode, out var statusMessage,
                out var content, out var headers))
        {
            headers.TryGetValue("Content-Type", out var streamContentType);
            context.Response.StatusCode = statusCode;
            context.Response.StatusDescription = statusMessage;
            context.Response.ContentType = streamContentType ?? "application/octet-stream";
            context.Response.ContentLength64 = content.Length;
            await content.CopyToAsync(context.Response.OutputStream);
        }
        else
        {
            context.Response.StatusCode = 404;
        }

        context.Response.Close();
    }

    /// <summary>
    /// Gets the next available TCP port on the local machine.
    /// </summary>
    private static int GetAvailablePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}