using System;

namespace Flagrum.Core.Graphics.Textures.Shared;

/// <summary>
/// Represents one surface in a texture.
/// </summary>
/// <param name="ArrayIndex">Index of the image this surface belongs to in the texture array.</param>
/// <param name="MipLevel">Mip level of the surface.</param>
/// <param name="Width">Width of this mip level.</param>
/// <param name="Height">Height of this mip level.</param>
/// <param name="Offset">
/// Offset of this surface's data in the pixel buffer that contains it.
/// This is relative to the start of the pixel buffer itself, not the start of the texture file.
/// </param>
/// <param name="Size">Size of the surface data, in bytes.</param>
/// <param name="Data">Raw pixel data for the surface.</param>
public record TextureSurface(
    int ArrayIndex,
    int MipLevel,
    int Width,
    int Height,
    int Offset,
    int Size,
    ReadOnlyMemory<byte> Data);