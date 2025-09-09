using System;
using System.Runtime.InteropServices;

namespace Flagrum.Platform.Linux.Interop;

internal static class GLib
{
    private const string LibraryName = "libglib-2.0.so";
    
    [DllImport(LibraryName, EntryPoint = "g_free")]
    internal static extern void GFree(IntPtr obj);
}