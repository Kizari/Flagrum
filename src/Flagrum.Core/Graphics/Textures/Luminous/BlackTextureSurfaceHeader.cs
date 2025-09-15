using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Flagrum.Core.Graphics.Textures.Luminous;

[StructLayout(LayoutKind.Sequential)]
public struct BlackTextureSurfaceHeader
{
    /// <summary>
    /// Size of this struct, in bytes.
    /// </summary>
    public static int StructSize
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Unsafe.SizeOf<BlackTextureSurfaceHeader>();
    }

    public uint Offset;
    public uint Size;
}