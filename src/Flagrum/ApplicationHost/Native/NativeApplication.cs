using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Injectio.Attributes;

namespace Flagrum.ApplicationHost.Native;

/// <summary>
/// C# wrapper for the NativeApplication class.
/// </summary>
[RegisterSingleton<NativeApplication>]
public sealed partial class NativeApplication : IDisposable
{
    private readonly IntPtr _instance = NativeApplication_Create();

    /// <summary>
    /// Path to the mod that Flagrum was opened with, if any.
    /// </summary>
    public string? FmodPath { get; set; }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        NativeApplication_Destroy(_instance);
    }

    /// <summary>
    /// Runs the application. This method blocks indefinitely until the user quits the application.
    /// </summary>
    /// <returns>Exit code.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Run() => NativeApplication_Run();

    /// <summary>
    /// Stops the application.
    /// </summary>
    /// <param name="exitCode">Exit code to return from the application.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Exit(int exitCode) => NativeApplication_Exit(exitCode);

    /// <summary>
    /// Copies the given text to the system clipboard.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetClipboardText(string text) => NativeApplication_SetClipboardText(text);


    [LibraryImport(NativeHelper.LibraryPath)]
    private static partial IntPtr NativeApplication_Create();

    [LibraryImport(NativeHelper.LibraryPath)]
    private static partial void NativeApplication_Destroy(IntPtr instance);

    [LibraryImport(NativeHelper.LibraryPath)]
    private static partial int NativeApplication_Run();

    [LibraryImport(NativeHelper.LibraryPath)]
    private static partial void NativeApplication_Exit(int exitCode);

    [LibraryImport(NativeHelper.LibraryPath)]
    private static partial void NativeApplication_SetClipboardText([MarshalAs(UnmanagedType.LPStr)] string text);
}