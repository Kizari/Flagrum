using System.Runtime.InteropServices;

namespace Flagrum.Core.Graphics.Textures.DirectX;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct DirectDrawSurfacePixelFormat
{
    public uint Size;
    public DirectDrawSurfacePixelFormatFlags Flags;
    public uint FourCC;
    public uint RgbBitCount;
    public uint RBitMask;
    public uint GBitMask;
    public uint BBitMask;
    public uint ABitMask;

    public static DirectDrawSurfacePixelFormat Default => new()
    {
        Size = 32,
        Flags = DirectDrawSurfacePixelFormatFlags.FourCC,
        FourCC = 808540228
    };
}