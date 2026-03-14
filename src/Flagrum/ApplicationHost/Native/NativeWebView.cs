using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Flagrum.ApplicationHost.Native;

/// <summary>
/// C# wrapper for the NativeWebView class.
/// </summary>
public sealed partial class NativeWebView : IDisposable
{
    public delegate void WebMessageReceivedCallback(string message);

    private readonly WebMessageReceivedCallback _onMessageReceived;

    /// <summary>
    /// C# wrapper for the NativeWebView class.
    /// </summary>
    public NativeWebView(NativeWindow parent, WebMessageReceivedCallback onMessageReceived)
    {
        _onMessageReceived = onMessageReceived;
        Handle = NativeWebView_Create(parent.Handle, _onMessageReceived, IntPtr.Zero);
    }

    /// <summary>
    /// Pointer to the underlying native class.
    /// </summary>
    public IntPtr Handle { get; }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() => NativeWebView_Destroy(Handle);

    /// <summary>
    /// Navigates the web view to the given URL.
    /// </summary>
    /// <param name="url">URL to navigate the web view to.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Navigate(string url) => NativeWebView_Navigate(Handle, url);

    /// <summary>
    /// Evaluates the given JavaScript code in the web view.
    /// </summary>
    /// <param name="script">JavaScript to evaluate.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RunJavaScript(string script) => NativeWebView_RunJavaScript(Handle, script);


    [LibraryImport(NativeHelper.LibraryPath)]
    private static partial IntPtr NativeWebView_Create(
        IntPtr window,
        WebMessageReceivedCallback onMessageReceived,
        IntPtr onResourceRequested);

    [LibraryImport(NativeHelper.LibraryPath)]
    private static partial void NativeWebView_Destroy(IntPtr instance);

    [LibraryImport(NativeHelper.LibraryPath)]
    private static partial void NativeWebView_Navigate(IntPtr instance, [MarshalAs(UnmanagedType.LPStr)] string url);

    [LibraryImport(NativeHelper.LibraryPath)]
    private static partial void NativeWebView_RunJavaScript(
        IntPtr instance,
        [MarshalAs(UnmanagedType.LPStr)] string script);
}