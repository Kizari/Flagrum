using System;
using System.Runtime.InteropServices;

namespace Flagrum.Platform.Linux.Interop;

internal static class Gdk
{
    private const string LibraryName = "libgdk-3.so.0";
    
    [DllImport(LibraryName, EntryPoint = "gdk_x11_window_get_xid")]
    internal static extern IntPtr GdkX11WindowGetXid(IntPtr window);
    
    [DllImport(LibraryName, EntryPoint = "gdk_x11_display_get_xdisplay")]
    internal static extern IntPtr GdkX11DisplayGetXDisplay(IntPtr display);
}