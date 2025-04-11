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