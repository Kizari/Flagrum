using System;

namespace Flagrum.Core.Graphics.Textures.DirectX;

[Flags]
public enum DirectDrawSurfacePixelFormatFlags : uint
{
    AlphaPixels = 0x01,
    Alpha = 0x02,
    FourCC = 0x04,
    RGB = 0x40,
    YUV = 0x200,
    Luminance = 0x20000
}