using System;

namespace Flagrum.Core.Graphics.Textures.DirectX;

[Flags]
public enum DirectDrawSurfaceCaps : uint
{
    None = 0x00,
    Complex = 0x08,
    MipMap = 0x400000,
    Texture = 0x1000
}

[Flags]
public enum DirectDrawSurfaceCaps2 : uint
{
    None = 0,
    Cubemap = 0x00000200,          // DDSCAPS2_CUBEMAP
    CubemapPositiveX = 0x00000400, // DDSCAPS2_CUBEMAP_POSITIVEX
    CubemapNegativeX = 0x00000800, // DDSCAPS2_CUBEMAP_NEGATIVEX
    CubemapPositiveY = 0x00001000, // DDSCAPS2_CUBEMAP_POSITIVEY
    CubemapNegativeY = 0x00002000, // DDSCAPS2_CUBEMAP_NEGATIVEY
    CubemapPositiveZ = 0x00004000, // DDSCAPS2_CUBEMAP_POSITIVEZ
    CubemapNegativeZ = 0x00008000, // DDSCAPS2_CUBEMAP_NEGATIVEZ
    Volume = 0x00200000            // DDSCAPS2_VOLUME
}