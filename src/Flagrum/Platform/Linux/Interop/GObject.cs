using System;
using System.Runtime.InteropServices;

namespace Flagrum.Platform.Linux.Interop;

internal static class GObject
{
    private const string LibraryName = "libgobject-2.0.so.0";
    
    internal delegate void ScriptMessageReceivedCallback(IntPtr contentManager, IntPtr jsResult, IntPtr data);
    
    [DllImport(LibraryName, EntryPoint = "g_object_unref")]
    internal static extern void GObjectUnref(IntPtr obj);
    
    [DllImport(LibraryName, EntryPoint = "g_signal_connect_data")]
    internal static extern ulong GSignalConnectData(
        IntPtr instance,
        string detailedSignal,
        ScriptMessageReceivedCallback callback,
        IntPtr data,
        IntPtr destroyData,
        uint connectFlags);
}