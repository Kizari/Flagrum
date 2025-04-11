namespace Flagrum.Core.Graphics.Textures.DirectX;

public class DirectDrawSurfacePixelFormat
{
    public uint Size { get; set; } = 32;

    public DirectDrawSurfacePixelFormatFlags Flags { get; set; } = DirectDrawSurfacePixelFormatFlags.FourCC;

    public uint FourCC { get; set; } = 808540228;

    public uint RgbBitCount { get; set; }

    public uint RBitMask { get; set; }

    public uint GBitMask { get; set; }

    public uint BBitMask { get; set; }

    public uint ABitMask { get; set; }
}