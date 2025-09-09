using System;
using System.Runtime.InteropServices;

namespace Flagrum.Platform.Linux.Interop;

internal static class JavaScriptCoreGtk
{
    private const string LibraryName = "libjavascriptcoregtk-4.0.so";
    
    [DllImport(LibraryName, EntryPoint = "jsc_value_to_string")]
    internal static extern IntPtr JscValueToString(IntPtr jsValue);
    
    [DllImport(LibraryName, EntryPoint = "jsc_value_is_string")]
    internal static extern bool JscValueIsString(IntPtr jsValue);
}