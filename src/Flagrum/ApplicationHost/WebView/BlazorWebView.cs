using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Flagrum.Platform.Linux.Interop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace Flagrum.ApplicationHost.WebView;

public class BlazorWebView : NativeControlHost
{
    public static readonly StyledProperty<string> SelectorProperty =
        AvaloniaProperty.Register<BlazorWebView, string>(nameof(Selector));
    
    public static readonly StyledProperty<Type> RootComponentProperty =
        AvaloniaProperty.Register<BlazorWebView, Type>(nameof(RootComponent));

    private readonly WebKit.WebContextRegisterUriSchemeCallback _onWebResourceRequested;
    private readonly GObject.ScriptMessageReceivedCallback _onWebMessageReceived;
    private readonly BlazorWebViewManager _webViewManager;

    private IntPtr _window;
    private IntPtr _webView;
    private IntPtr _xWindow;
    private IntPtr _display;
    
    public BlazorWebView()
    {
        _onWebMessageReceived = OnWebMessageReceived;
        _onWebResourceRequested = OnWebResourceRequested;
        _webViewManager = new BlazorWebViewManager(
            Program.Services.GetRequiredService<IServiceProvider>(),
            Program.Services.GetRequiredService<IFileProvider>(),
            Program.Services.GetRequiredService<JSComponentConfigurationStore>(),
            new BlazorWebViewDispatcher(),
            this);
    }

    public string Selector
    {
        get => GetValue(SelectorProperty);
        set => SetValue(SelectorProperty, value);
    }
    
    public Type RootComponent
    {
        get => GetValue(RootComponentProperty);
        set => SetValue(RootComponentProperty, value);
    }
    
    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        _webViewManager.AddRootComponentAsync(RootComponent, Selector, ParameterView.Empty)
            .ConfigureAwait(false).GetAwaiter().GetResult();
        
        return Avalonia.X11.Interop.GtkInteropHelper.RunOnGlibThread(() =>
        {
            _window = Gtk.GtkWindowNew(0); // GTK_WINDOW_TOPLEVEL
            _webView = CreateWebView();
            Gtk.GtkContainerAdd(_window, _webView);
            Gtk.GtkWidgetRealize(_window);
            
            _xWindow = Gdk.GdkX11WindowGetXid(Gtk.GtkWidgetGetWindow(_window));
            _display = Gdk.GdkX11DisplayGetXDisplay(Gtk.GtkWidgetGetDisplay(_window));
            X11.XReparentWindow(_display, _xWindow, parent.Handle, 0, 0);
            Gtk.GtkWidgetShowAll(_window);
            Navigate(BlazorWebViewManager.CreateUri("/"));
            
            return new PlatformHandle(_webView, "GtkWebView");
        }).Result;
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        Gtk.GtkWidgetDestroy(_window);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (_webView != IntPtr.Zero)
        {
            var scaling = VisualRoot?.RenderScaling ?? 1.0;
            var width = (int)(finalSize.Width * scaling);
            var height = (int)(finalSize.Height * scaling);
            X11.XResizeWindow(_display, _xWindow, width, height);
        }
        
        return base.ArrangeOverride(finalSize);
    }

    public void Navigate(string uri)
    {
        WebKit.WebKitWebViewLoadUri(_webView, uri);
    }

    public void SendMessage(string message)
    {
        var script = $"__dispatchMessageCallback({JsonSerializer.Serialize(message)})";
        var completion = new ManualResetEventSlim(false);

        WebKit.WebKitWebViewEvaluateJavaScript(
            _webView,
            script,
            script.Length,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero,
            (_, _, _) => completion.Set(),
            IntPtr.Zero);

        completion.Wait();
    }
    
    private IntPtr CreateWebView()
    {
        // Create the web view
        var contentManager = WebKit.WebKitUserContentManagerNew();
        var webView = WebKit.WebKitWebViewNewWithUserContentManager(contentManager);
        
        // Load embedded interop script
        var uri = new Uri("avares://Flagrum/Assets/native_web_view.js");
        using var stream = AssetLoader.Open(uri);
        using var reader = new StreamReader(stream);
        var javaScript = reader.ReadToEnd();
        var pScript = Marshal.StringToCoTaskMemUTF8(javaScript);
        var script = WebKit.WebKitUserScriptNew(pScript,
            0, // WEBKIT_USER_CONTENT_INJECT_ALL_FRAMES
            0, // WEBKIT_USER_SCRIPT_INJECT_AT_DOCUMENT_START
            IntPtr.Zero,
            IntPtr.Zero);
        Marshal.FreeCoTaskMem(pScript);
        
        // Apply the interop script to the web view
        WebKit.WebKitUserContentManagerAddScript(contentManager, script);
        WebKit.WebKitUserScriptUnref(script);
        
        // Hook the post message function up to the web view manager
        GObject.GSignalConnectData(contentManager, 
            "script-message-received::testudo",
            _onWebMessageReceived,
            IntPtr.Zero, IntPtr.Zero, 0);
        WebKit.WebKitUserContentManagerRegisterScriptMessageHandler(contentManager, "testudo");
        
        // Set up custom scheme handler
        var context = WebKit.WebKitWebContextGetDefault();
        WebKit.WebKitWebContextRegisterUriScheme(context,
            "app",
            _onWebResourceRequested,
            IntPtr.Zero, IntPtr.Zero);
        
#if DEBUG
        // Enable dev tools
        var settings = WebKit.WebKitWebViewGetSettings(webView);
        WebKit.WebKitSettingsSetEnableDeveloperExtras(settings, true);
#endif
        
        return webView;
    }
    
    private void OnWebMessageReceived(IntPtr contentManager, IntPtr jsResult, IntPtr data)
    {
        var jsValue = WebKit.WebKitJavaScriptResultGetJSValue(jsResult);
        
        if (JavaScriptCoreGtk.JscValueIsString(jsValue))
        {
            var value = JavaScriptCoreGtk.JscValueToString(jsValue);
            var result = Marshal.PtrToStringUTF8(value);
            GLib.GFree(value);
            
            if (result != null)
            {
                _webViewManager.OnWebMessageReceived(result);
            }
        }

        WebKit.WebKitJavaScriptResultUnref(jsResult);
    }

    private void OnWebResourceRequested(IntPtr request, IntPtr data)
    {
        var pUri = WebKit.WebKitUriSchemeRequestGetUri(request);
        var uri = Marshal.PtrToStringUTF8(pUri)!;
        var buffer = _webViewManager.OnWebResourceRequested(uri, out var size, out var contentType);
        var stream = GIO.GMemoryInputStreamNewFromData(buffer, size, IntPtr.Zero);
        WebKit.WebKitUriSchemeRequestFinish(request, stream, -1, contentType);
        GObject.GObjectUnref(stream);
        
        // TODO: Buffer isn't freed until app closes, but can't be freed here as WebKitUriSchemeRequestFinish
        //       does not make a copy of the buffer. Need to figure out a way to free it when it's no longer needed.
    }
}