using System;
using System.Linq;
using System.Runtime.InteropServices;
using Injectio.Attributes;

namespace Flagrum.ApplicationHost.Native;

/// <summary>
/// C# wrapper for QFileDialog.
/// </summary>
[RegisterSingleton<NativeFileDialog>]
public partial class NativeFileDialog(NativeWindow window)
{
    /// <summary>
    /// Displays an open file dialog.
    /// </summary>
    /// <param name="caption">Title for the dialog.</param>
    /// <param name="initialDirectory">Directory that the dialog will show when first opened.</param>
    /// <param name="filter">File extension filter for the dialog.</param>
    /// <returns>File path of the file to open, or <c>null</c> if the user canceled.</returns>
    public string? OpenFile(string caption, string initialDirectory, string filter)
    {
        var pResult = Marshal.AllocHGlobal(4096);
        NativeFileDialog_OpenFile(window.Handle, caption, initialDirectory, ConvertFilter(filter), pResult);
        var result = Marshal.PtrToStringUTF8(pResult);
        Marshal.FreeHGlobal(pResult);
        return string.IsNullOrWhiteSpace(result) ? null : result;
    }

    /// <summary>
    /// Displays an open folder dialog.
    /// </summary>
    /// <param name="caption">Title for the dialog.</param>
    /// <param name="initialDirectory">Directory that the dialog will show when first opened.</param>
    /// <returns>File path of the selected directory, or <c>null</c> if the user canceled.</returns>
    public string? OpenDirectory(string caption, string initialDirectory)
    {
        var pResult = Marshal.AllocHGlobal(4096);
        NativeFileDialog_OpenDirectory(window.Handle, caption, initialDirectory, pResult);
        var result = Marshal.PtrToStringUTF8(pResult);
        Marshal.FreeHGlobal(pResult);
        return string.IsNullOrWhiteSpace(result) ? null : result;
    }

    /// <summary>
    /// Displays a save file dialog.
    /// </summary>
    /// <param name="caption">Title for the dialog.</param>
    /// <param name="initialDirectory">Directory that the dialog will show when first opened.</param>
    /// <param name="filter">File extension filter for the dialog.</param>
    /// <returns>File path of the file to save, or <c>null</c> if the user canceled.</returns>
    public string? SaveFile(string caption, string initialDirectory, string filter)
    {
        var pResult = Marshal.AllocHGlobal(4096);
        NativeFileDialog_SaveFile(window.Handle, caption, initialDirectory, ConvertFilter(filter), pResult);
        var result = Marshal.PtrToStringUTF8(pResult);
        Marshal.FreeHGlobal(pResult);
        return string.IsNullOrWhiteSpace(result) ? null : result;
    }

    /// <summary>
    /// Converts a Windows-style filter string into a Qt-style filter string.
    /// </summary>
    private static string ConvertFilter(string filter)
    {
        // Converts "name|extension|name|extension" to "name (extension);;name (extension)"
        var tokens = filter.Split('|');
        return string.Join(";;", Enumerable
            .Range(0, tokens.Length / 2)
            .Select(i => $"{tokens[i * 2]} ({tokens[i * 2 + 1]}"));
    }


    [LibraryImport(NativeHelper.LibraryPath)]
    private static partial void NativeFileDialog_OpenFile(
        IntPtr window,
        [MarshalAs(UnmanagedType.LPStr)] string caption,
        [MarshalAs(UnmanagedType.LPStr)] string initialDirectory,
        [MarshalAs(UnmanagedType.LPStr)] string filter,
        IntPtr result);

    [LibraryImport(NativeHelper.LibraryPath)]
    private static partial void NativeFileDialog_OpenDirectory(
        IntPtr window,
        [MarshalAs(UnmanagedType.LPStr)] string caption,
        [MarshalAs(UnmanagedType.LPStr)] string initialDirectory,
        IntPtr result);

    [LibraryImport(NativeHelper.LibraryPath)]
    private static partial void NativeFileDialog_SaveFile(
        IntPtr window,
        [MarshalAs(UnmanagedType.LPStr)] string caption,
        [MarshalAs(UnmanagedType.LPStr)] string initialDirectory,
        [MarshalAs(UnmanagedType.LPStr)] string filter,
        IntPtr result);
}