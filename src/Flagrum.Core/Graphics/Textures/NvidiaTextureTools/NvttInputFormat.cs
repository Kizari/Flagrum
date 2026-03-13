namespace Flagrum.Core.Graphics.Textures.NvidiaTextureTools;

/// <summary>
/// Pixel formats that NVIDIA Texture Tools (NVTT) accepts as input for a <see cref="NvttSurface" />.
/// </summary>
public enum NvttInputFormat
{
    BGRA_8UB,
    BGRA_8SB,
    RGBA_16F,
    RGBA_32F,
    R_32F
}