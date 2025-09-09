using System;
using System.Runtime.InteropServices;

namespace Flagrum.Platform.Linux.Interop;

internal static class WebKit
{
    private const string LibraryName = "libwebkit2gtk-4.0.so.37";
    
    internal delegate void WebContextRegisterUriSchemeCallback(IntPtr request, IntPtr data);
    internal delegate void WebViewEvaluateJavaScriptCallback(IntPtr sourceObject, IntPtr result, IntPtr data);
    
    [DllImport(LibraryName, EntryPoint = "webkit_web_view_get_settings")]
    internal static extern IntPtr WebKitWebViewGetSettings(IntPtr webView);

    [DllImport(LibraryName, EntryPoint = "webkit_settings_set_enable_developer_extras")]
    internal static extern void WebKitSettingsSetEnableDeveloperExtras(IntPtr settings, bool enabled);
    
    [DllImport(LibraryName, EntryPoint = "webkit_web_view_evaluate_javascript")]
    internal static extern void WebKitWebViewEvaluateJavaScript(
        IntPtr webView,
        string script,
        int scriptLength,
        IntPtr worldName,
        IntPtr sourceUri,
        IntPtr cancellable,
        WebViewEvaluateJavaScriptCallback callback,
        IntPtr data);
    
    [DllImport(LibraryName, EntryPoint = "webkit_uri_scheme_request_finish")]
    internal static extern void WebKitUriSchemeRequestFinish(
        IntPtr request, 
        IntPtr stream, 
        long streamLength,
        string contentType);
    
    [DllImport(LibraryName, EntryPoint = "webkit_uri_scheme_request_get_uri")]
    internal static extern IntPtr WebKitUriSchemeRequestGetUri(IntPtr request);

    [DllImport(LibraryName, EntryPoint = "webkit_web_context_register_uri_scheme")]
    internal static extern void WebKitWebContextRegisterUriScheme(
        IntPtr context, 
        string scheme,
        WebContextRegisterUriSchemeCallback callback,
        IntPtr data,
        IntPtr dataDestroy);

    [DllImport(LibraryName, EntryPoint = "webkit_web_context_get_default")]
    internal static extern IntPtr WebKitWebContextGetDefault();

    [DllImport(LibraryName, EntryPoint = "webkit_user_content_manager_register_script_message_handler")]
    internal static extern void WebKitUserContentManagerRegisterScriptMessageHandler(IntPtr contentManager, string name);
    
    [DllImport(LibraryName, EntryPoint = "webkit_javascript_result_unref")]
    internal static extern IntPtr WebKitJavaScriptResultUnref(IntPtr jsResult);
    
    [DllImport(LibraryName, EntryPoint = "webkit_javascript_result_get_js_value")]
    internal static extern IntPtr WebKitJavaScriptResultGetJSValue(IntPtr jsResult);
    
    [DllImport(LibraryName, EntryPoint = "webkit_user_script_unref")]
    internal static extern void WebKitUserScriptUnref(IntPtr script);

    [DllImport(LibraryName, EntryPoint = "webkit_user_content_manager_add_script")]
    internal static extern void WebKitUserContentManagerAddScript(IntPtr contentManager, IntPtr script);

    [DllImport(LibraryName, EntryPoint = "webkit_user_script_new")]
    internal static extern IntPtr WebKitUserScriptNew(IntPtr script, int injectedFrames, int injectionTime,
        IntPtr allowList, IntPtr blockList); 

    [DllImport(LibraryName, EntryPoint = "webkit_user_content_manager_new")]
    internal static extern IntPtr WebKitUserContentManagerNew();
    
    [DllImport(LibraryName, EntryPoint = "webkit_web_view_new")]
    internal static extern IntPtr WebKitWebViewNew();

    [DllImport(LibraryName, EntryPoint = "webkit_web_view_new_with_user_content_manager")]
    internal static extern IntPtr WebKitWebViewNewWithUserContentManager(IntPtr pContentManager);

    [DllImport(LibraryName, EntryPoint = "webkit_web_view_load_uri")]
    internal static extern void WebKitWebViewLoadUri(IntPtr webView, string uri);
}