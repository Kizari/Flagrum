using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Flagrum.ApplicationHost.Native;

/// <summary>
/// C# wrapper for the NativeApplication class.
/// </summary>
public sealed partial class NativeApplication : IDisposable
{
    private readonly IntPtr _instance = NativeApplication_Create();
    
    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() => NativeApplication_Destroy(_instance);

    /// <summary>
    /// Runs the application. This method blocks indefinitely until the user quits the application.
    /// </summary>
    /// <returns>Exit code.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Run() => NativeApplication_Run(_instance);

    
    [LibraryImport(NativeHelper.LibraryPath)]
    private static partial IntPtr NativeApplication_Create();

    [LibraryImport(NativeHelper.LibraryPath)]
    private static partial void NativeApplication_Destroy(IntPtr instance);

    [LibraryImport(NativeHelper.LibraryPath)]
    private static partial int NativeApplication_Run(IntPtr instance);
}