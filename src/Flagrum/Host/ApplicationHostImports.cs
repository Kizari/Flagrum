using System;
using System.Runtime.InteropServices;

namespace Flagrum.Host;

/// <summary>
/// Allowed icon types for native message boxes.
/// </summary>
public enum MessageBoxType
{
    Information,
    Warning,
    Critical
}

public partial class ApplicationHost
{
    /// <summary>
    /// Path to the native Flagrum library.
    /// </summary>
    private const string LibraryPath =
#if DEBUG
        $"{Build.SolutionDirectory}cmake-build-debug/src/Flagrum.Native/libFlagrum.Native.so";
#else
        $"{AppContext.BaseDirectory}/libFlagrum.Native.so";
#endif

    [LibraryImport(LibraryPath)]
    private static partial IntPtr ApplicationHost_Create();

    [LibraryImport(LibraryPath)]
    private static partial void ApplicationHost_Destroy(IntPtr instance);

    [LibraryImport(LibraryPath)]
    private static partial int ApplicationHost_Run(IntPtr instance);

    [LibraryImport(LibraryPath)]
    private static partial void ApplicationHost_Exit(IntPtr instance, int exitCode);

    [LibraryImport(LibraryPath)]
    private static partial void ApplicationHost_Invoke(IntPtr instance, Action action);

    [LibraryImport(LibraryPath)]
    private static partial void ApplicationHost_OpenSplash(IntPtr instance);

    [LibraryImport(LibraryPath)]
    private static partial void ApplicationHost_CloseSplash(IntPtr instance);

    [LibraryImport(LibraryPath)]
    private static partial void ApplicationHost_SetSplashText(
        IntPtr instance,
        [MarshalAs(UnmanagedType.LPStr)] string text);

    [LibraryImport(LibraryPath)]
    private static partial void ApplicationHost_OpenMainWindow(IntPtr instance);

    [LibraryImport(LibraryPath)]
    private static partial void ApplicationHost_CloseMainWindow(IntPtr instance);

    [LibraryImport(LibraryPath)]
    private static partial void ApplicationHost_SetWebMessageHandler(
        IntPtr instance,
        WebMessageReceivedCallback handler);

    [LibraryImport(LibraryPath)]
    private static partial void ApplicationHost_NavigateWebView(
        IntPtr instance,
        [MarshalAs(UnmanagedType.LPStr)] string url);

    [LibraryImport(LibraryPath)]
    private static partial void ApplicationHost_RunJavaScript(
        IntPtr instance,
        [MarshalAs(UnmanagedType.LPStr)] string script);

    [LibraryImport(LibraryPath)]
    private static partial void ApplicationHost_SetClipboardText(
        IntPtr instance,
        [MarshalAs(UnmanagedType.LPStr)] string text);

    [LibraryImport(LibraryPath)]
    private static partial void ApplicationHost_ShowMessageBox(
        IntPtr instance,
        [MarshalAs(UnmanagedType.LPStr)] string title,
        [MarshalAs(UnmanagedType.LPStr)] string message,
        MessageBoxType type);

    [LibraryImport(LibraryPath)]
    private static partial void ApplicationHost_OpenFile(
        IntPtr instance,
        [MarshalAs(UnmanagedType.LPStr)] string caption,
        [MarshalAs(UnmanagedType.LPStr)] string directory,
        [MarshalAs(UnmanagedType.LPStr)] string filter,
        IntPtr result);

    [LibraryImport(LibraryPath)]
    private static partial void ApplicationHost_SaveFile(
        IntPtr instance,
        [MarshalAs(UnmanagedType.LPStr)] string caption,
        [MarshalAs(UnmanagedType.LPStr)] string directory,
        [MarshalAs(UnmanagedType.LPStr)] string filter,
        IntPtr result);

    [LibraryImport(LibraryPath)]
    private static partial void ApplicationHost_OpenDirectory(
        IntPtr instance,
        [MarshalAs(UnmanagedType.LPStr)] string caption,
        [MarshalAs(UnmanagedType.LPStr)] string directory,
        IntPtr result);
}