using System;
using System.IO;
using System.Runtime.InteropServices;
using Flagrum.Core.Graphics.Textures.Luminous;

namespace Flagrum.Core.Graphics.Textures;

/// <summary>
/// Wrapper for NVIDIA Texture Tools' (nvtt) SurfaceSet class.
/// </summary>
/// <remarks>
/// Holds a <see cref="BlackTexture" />'s raw uncompressed pixel data in memory for the life of the object.
/// Offers utilities for converting this pixel data to other formats.
/// </remarks>
public sealed class SurfaceSet : IDisposable
{
    private readonly IntPtr _pSurfaceSet;

    /// <summary>
    /// Loads a texture into memory, decompressing if required.
    /// </summary>
    /// <param name="texture">Texture to load.</param>
    public SurfaceSet(BlackTexture texture)
    {
        ImageCount = texture.ImageCount;
        _pSurfaceSet = Nvtt.CreateSurfaceSet();
        var dds = texture.ToDds();
        var buffer = dds.Write();
        var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        var pBuffer = handle.AddrOfPinnedObject();
        var wasSuccessful = Nvtt.SurfaceSetLoadDDSFromMemory(_pSurfaceSet, pBuffer, (ulong)buffer.Length, false);
        handle.Free();
        
        if (!wasSuccessful)
        {
            throw new ApplicationException("Failed to load surface set from in-memory DDS file.");
        }
    }

    /// <summary>
    /// Retrieves a surface from this set.
    /// </summary>
    /// <param name="faceIndex">Index of the image to get from this set.</param>
    /// <param name="mipIndex">Mip level of the image.</param>
    public Surface this[int faceIndex, int mipIndex] => new(
        Nvtt.SurfaceSetGetSurface(_pSurfaceSet, faceIndex, mipIndex, false));

    /// <summary>
    /// Number of images contained in the texture.
    /// </summary>
    public int ImageCount { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        Nvtt.DestroySurfaceSet(_pSurfaceSet);
    }

    /// <summary>
    /// Saves an image from the texture to a JPG file.
    /// </summary>
    /// <param name="imageIndex">Index of the image to save.</param>
    /// <param name="mipIndex">Index of the mipmap to save.</param>
    /// <param name="filePath">Path to the output file on disk.</param>
    public void SaveJpg(int imageIndex, int mipIndex, string filePath)
    {
        if (!Nvtt.SurfaceSetSaveImage(_pSurfaceSet, filePath, imageIndex, mipIndex))
        {
            throw new ApplicationException("Failed to save surface to JPG file.");
        }
    }
}