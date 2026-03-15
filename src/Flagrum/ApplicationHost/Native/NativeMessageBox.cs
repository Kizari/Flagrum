using System;
using System.Runtime.InteropServices;

namespace Flagrum.ApplicationHost.Native;

/// <summary>
/// Viable icon types for <see cref="NativeMessageBox" />.
/// </summary>
public enum MessageType
{
    Information,
    Warning,
    Error
}

/// <summary>
/// Wraps the native QMessageBox type.
/// </summary>
public static partial class NativeMessageBox
{
    /// <summary>
    /// Displays a native message box.
    /// </summary>
    /// <param name="title">Title to apply to the message box.</param>
    /// <param name="message">Text to show in the body of the message box.</param>
    /// <param name="type">Icon to show in the message box.</param>
    /// <param name="parent">Parent window to show the message in front of, or null.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if <see cref="MessageType" /> is not a valid enum member.
    /// </exception>
    public static void Show(string title, string message, MessageType type, NativeWindow? parent = null)
    {
        var window = parent?.Handle ?? IntPtr.Zero;

        switch (type)
        {
            case MessageType.Information:
                NativeMessageBox_Information(window, title, message);
                break;
            case MessageType.Warning:
                NativeMessageBox_Warning(window, title, message);
                break;
            case MessageType.Error:
                NativeMessageBox_Critical(window, title, message);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    [LibraryImport(NativeHelper.LibraryPath)]
    private static partial void NativeMessageBox_Information(
        IntPtr window,
        [MarshalAs(UnmanagedType.LPStr)] string title,
        [MarshalAs(UnmanagedType.LPStr)] string message);

    [LibraryImport(NativeHelper.LibraryPath)]
    private static partial void NativeMessageBox_Warning(
        IntPtr window,
        [MarshalAs(UnmanagedType.LPStr)] string title,
        [MarshalAs(UnmanagedType.LPStr)] string message);

    [LibraryImport(NativeHelper.LibraryPath)]
    private static partial void NativeMessageBox_Critical(
        IntPtr window,
        [MarshalAs(UnmanagedType.LPStr)] string title,
        [MarshalAs(UnmanagedType.LPStr)] string message);
}