using System;
using System.Runtime.InteropServices;

namespace Flagrum.Platform.Linux.Interop;

internal static class GIO
{
    private const string LibraryName = "libgio-2.0.so";
    
    [DllImport(LibraryName, EntryPoint = "g_memory_input_stream_new_from_data")]
    internal static extern IntPtr GMemoryInputStreamNewFromData(IntPtr buffer, int size, IntPtr destroy);
}