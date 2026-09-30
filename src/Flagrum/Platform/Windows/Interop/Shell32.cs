using System;
using System.Runtime.InteropServices;

namespace Flagrum.Platform.Windows.Interop;

public static partial class Shell32
{
    private const string LibraryName = "shell32.dll";

    [LibraryImport(LibraryName, SetLastError = true)]
    internal static partial void
        SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string appId);

    [LibraryImport(LibraryName)]
    internal static partial int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);
}