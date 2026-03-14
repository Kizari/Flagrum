namespace Flagrum.ApplicationHost.Native;

/// <summary>
/// Helper methods for native functionality.
/// </summary>
public static class NativeHelper
{
    /// <summary>
    /// Path to the native Flagrum library.
    /// </summary>
    public const string LibraryPath =
#if DEBUG
        $"{Build.SolutionDirectory}cmake-build-debug/src/Flagrum.Native/libFlagrum.Native.so";
#else
        $"{AppContext.BaseDirectory}/libFlagrum.Native.so";
#endif
}