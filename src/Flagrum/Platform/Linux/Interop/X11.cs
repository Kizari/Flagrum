using System;
using System.Runtime.InteropServices;

namespace Flagrum.Platform.Linux.Interop;

internal static class X11
{
    private const string LibraryName = "libX11.so.6";
    
    [DllImport(LibraryName)]
    internal static extern int XReparentWindow(IntPtr display, IntPtr window, IntPtr parent, int x, int y);

    [DllImport(LibraryName)]
    internal static extern int XResizeWindow(IntPtr display, IntPtr window, int width, int height);
}