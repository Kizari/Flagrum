using System;
using SkiaSharp;

namespace Flagrum.Core.Graphics.Textures;

/// <summary>
/// Wrapper for NVIDIA Texture Tools' (nvtt) Surface class.
/// </summary>
public class Surface(IntPtr pSurface)
{
    /// <summary>
    /// Gets a pointer to the raw pixel data for this surface.
    /// </summary>
    /// <remarks>
    /// Data is in RGBA float32 format.
    /// Each channel is in a contiguous block.
    /// </remarks>
    public IntPtr Data => Nvtt.SurfaceData(pSurface);

    /// <summary>
    /// Width of the image represented by this surface, in pixels.
    /// </summary>
    public int Width => Nvtt.SurfaceWidth(pSurface);
    
    /// <summary>
    /// Height of the image represented by this surface, in pixels.
    /// </summary>
    public int Height => Nvtt.SurfaceHeight(pSurface);
    
    /// <summary>
    /// Converts the image represented by this surface into a PNG file.
    /// </summary>
    public byte[] SavePng()
    {
        using var image = ToSKImage();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    /// <summary>
    /// Converts the image data to an interleaved RGBA 32bpp <see cref="SKImage"/>.
    /// </summary>
    /// <returns><b>Caller must dispose of this!</b></returns>
    private unsafe SKImage ToSKImage()
    {
        var pData = (float*)Data.ToPointer();
        var width = Width;
        var height = Height;
        var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
        var bitmap = new SKBitmap(info);
        var pInterleaved = (byte*)bitmap.GetPixels().ToPointer();
        
        for (var i = 0; i < width * height; i++)
        {
            pInterleaved[i * 4] = (byte)(pData[i] * 255);
            pInterleaved[i * 4 + 1] = (byte)(pData[width * height + i] * 255);
            pInterleaved[i * 4 + 2] = (byte)(pData[width * height * 2 + i] * 255);
            pInterleaved[i * 4 + 3] = (byte)(pData[width * height * 3 + i] * 255);
        }

        return SKImage.FromBitmap(bitmap);
    }
}