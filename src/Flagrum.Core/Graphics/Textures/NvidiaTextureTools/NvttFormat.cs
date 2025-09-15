namespace Flagrum.Core.Graphics.Textures.NvidiaTextureTools;

/// <summary>
/// Pixel formats supported by NVIDIA Texture Tools.
/// </summary>
public enum NvttFormat
{
    // No compression
    Format_RGB,
    Format_RGBA = Format_RGB,

    // DX9 formats
    Format_DXT1,
    Format_DXT1a,
    Format_DXT3,
    Format_DXT5,
    Format_DXT5n,

    // DX10 formats
    Format_BC1 = Format_DXT1,
    Format_BC1a = Format_DXT1a,
    Format_BC2 = Format_DXT3,
    Format_BC3 = Format_DXT5,
    Format_BC3n = Format_DXT5n,
    Format_BC4,
    Format_BC4S,
    Format_ATI2,
    Format_BC5,
    Format_BC5S,

    Format_DXT1n,
    Format_CTX1,

    Format_BC6U,
    Format_BC6S,

    Format_BC7,

    Format_BC3_RGBM,

    Format_ASTC_LDR_4x4,
    Format_ASTC_LDR_5x4,
    Format_ASTC_LDR_5x5,
    Format_ASTC_LDR_6x5,
    Format_ASTC_LDR_6x6,
    Format_ASTC_LDR_8x5,
    Format_ASTC_LDR_8x6,
    Format_ASTC_LDR_8x8,
    Format_ASTC_LDR_10x5,
    Format_ASTC_LDR_10x6,
    Format_ASTC_LDR_10x8,
    Format_ASTC_LDR_10x10,
    Format_ASTC_LDR_12x10,
    Format_ASTC_LDR_12x12,

    Format_Count
}