using System;
using System.Runtime.InteropServices;

namespace Flagrum.ApplicationHost.Native;

/// <summary>
/// C# wrapper for NativeSplashWindow.
/// </summary>
public sealed partial class NativeSplashWindow : IDisposable
{
    private readonly IntPtr _instance = NativeSplashWindow_Create();

    public void Dispose()
    {
        NativeSplashWindow_Destroy(_instance);
    }

    /// <summary>
    /// Shows the splash window.
    /// </summary>
    public void Show() => NativeSplashWindow_Show(_instance);

    /// <summary>
    /// Closes the splash window.
    /// </summary>
    public void Close() => NativeSplashWindow_Close(_instance);

    /// <summary>
    /// Updates the text above the loading bar.
    /// </summary>
    /// <param name="text">Text to display.</param>
    public void SetLoadingText(string text) => NativeSplashWindow_SetLoadingText(_instance, text);


    [LibraryImport(NativeHelper.LibraryPath)]
    private static partial IntPtr NativeSplashWindow_Create();

    [LibraryImport(NativeHelper.LibraryPath)]
    private static partial void NativeSplashWindow_Destroy(IntPtr instance);

    [LibraryImport(NativeHelper.LibraryPath)]
    private static partial void NativeSplashWindow_Show(IntPtr instance);

    [LibraryImport(NativeHelper.LibraryPath)]
    private static partial void NativeSplashWindow_Close(IntPtr instance);

    [LibraryImport(NativeHelper.LibraryPath)]
    private static partial void NativeSplashWindow_SetLoadingText(
        IntPtr instance,
        [MarshalAs(UnmanagedType.LPStr)] string text);
}