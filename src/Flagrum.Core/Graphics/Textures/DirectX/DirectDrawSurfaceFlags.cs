using System;

namespace Flagrum.Core.Graphics.Textures.DirectX;

[Flags]
public enum DirectDrawSurfaceFlags : uint
{
    None = 0x00,
    Caps = 0x01,
    Height = 0x02,
    Width = 0x04,
    Pitch = 0x08,
    PixelFormat = 0x1000,
    MipMapCount = 0x20000,
    LinearSize = 0x80000,
    Depth = 0x800000,
    Texture = Caps | Height | Width | PixelFormat
}