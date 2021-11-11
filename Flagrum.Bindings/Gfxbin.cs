using System;
using System.IO;
using System.Runtime.InteropServices;
using DNNE;

namespace Flagrum.Bindings
{
    public static unsafe class Gfxbin
    {
        [UnmanagedCallersOnly]
        public static IntPtr Export(IntPtr pathPointer)
        {
            var path = Marshal.PtrToStringUni(pathPointer);
            File.WriteAllText(path, "Export Successful.");
            return Marshal.StringToHGlobalAnsi("Export Successful.");
        }
    }
}
