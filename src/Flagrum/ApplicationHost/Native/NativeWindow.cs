using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Flagrum.ApplicationHost.Native;

/// <summary>
/// C# wrapper for the NativeWindow class.
/// </summary>
public sealed partial class NativeWindow : IDisposable
{
    /// <summary>
    /// Pointer to the underlying native class.
    /// </summary>
    public IntPtr Handle { get; } = NativeWindow_Create();

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() => NativeWindow_Destroy(Handle);

    /// <summary>
    /// Sets the window title.
    /// </summary>
    /// <param name="title">The new window title.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetTitle(string title) => NativeWindow_SetTitle(Handle, title);

    /// <summary>
    /// Sets the size of the window.
    /// </summary>
    /// <param name="width">Width of the window, in pixels.</param>
    /// <param name="height">Height of the window, in pixels.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Resize(int width, int height) => NativeWindow_Resize(Handle, width, height);

    /// <summary>
    /// Sets the main content widget of the window to the given web view.
    /// </summary>
    /// <param name="webView">Web view to set as the main content widget.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetWebView(NativeWebView webView) => NativeWindow_SetWebView(Handle, webView.Handle);

    /// <summary>
    /// Shows the window.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Show() => NativeWindow_Show(Handle);
    

    [LibraryImport(NativeHelper.LibraryPath)]
    private static partial IntPtr NativeWindow_Create();

    [LibraryImport(NativeHelper.LibraryPath)]
    private static partial void NativeWindow_Destroy(IntPtr instance);

    [LibraryImport(NativeHelper.LibraryPath)]
    private static partial void NativeWindow_SetTitle(IntPtr instance, [MarshalAs(UnmanagedType.LPStr)] string title);

    [LibraryImport(NativeHelper.LibraryPath)]
    private static partial void NativeWindow_Resize(IntPtr instance, int width, int height);

    [LibraryImport(NativeHelper.LibraryPath)]
    private static partial void NativeWindow_SetWebView(IntPtr instance, IntPtr webView);

    [LibraryImport(NativeHelper.LibraryPath)]
    private static partial void NativeWindow_Show(IntPtr instance);
}