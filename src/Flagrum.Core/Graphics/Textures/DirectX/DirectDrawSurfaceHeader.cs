using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Flagrum.Core.Graphics.Textures.DirectX;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct DirectDrawSurfaceHeader
{
    public const uint MagicValue = 0x20534444; // "DDS "

    public static int StructSize
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Unsafe.SizeOf<DirectDrawSurfaceHeader>();
    }

    public uint Size;
    public DirectDrawSurfaceFlags Flags;
    public uint Height;
    public uint Width;
    public uint Pitch;
    public uint Depth;
    public uint MipMapCount;
    public fixed uint Reserved1[11];
    public DirectDrawSurfacePixelFormat PixelFormat;

    /// <summary>
    /// Flags that define texture capabilities.
    /// </summary>
    public DirectDrawSurfaceCaps Caps;

    /// <summary>
    /// Flags relating to cube map and volume texture capabilities.
    /// </summary>
    public DirectDrawSurfaceCaps2 Caps2;

    /// <summary>
    /// Reserved for future use or specific legacy DirectDraw features that are not relevant in modern pipelines.
    /// </summary>
    public uint Caps3;

    /// <summary>
    /// Reserved for future use or specific legacy DirectDraw features that are not relevant in modern pipelines.
    /// </summary>
    public uint Caps4;

    /// <summary>
    /// Reserved for future use, ignore this.
    /// </summary>
    public uint Reserved2;
}