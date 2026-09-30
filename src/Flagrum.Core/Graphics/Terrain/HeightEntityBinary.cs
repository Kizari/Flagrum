using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Flagrum.Core.Graphics.Textures;
using Flagrum.Core.Graphics.Textures.NvidiaTextureTools;
using Flagrum.Core.Graphics.Textures.Shared;
using Flagrum.Core.Utilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

namespace Flagrum.Core.Graphics.Terrain;

// TODO: Assuming PS4 HEBs are also swizzled, they need to be deswizzled like in BTEX

public ref struct HeightEntityBinary
{
    private readonly ReadOnlySpan<byte> _buffer;

    public HeightEntityBinary(ReadOnlySpan<byte> buffer)
    {
        _buffer = buffer;

        ref var start = ref MemoryMarshal.GetReference(buffer);
        Header = ref Unsafe.As<byte, HeightEntityBinaryHeader>(ref start);
        Images = MemoryMarshal.Cast<byte, HeightEntityBinaryImageHeader>(buffer.Slice(
            (int)Header.ImageHeadersOffset,
            HeightEntityBinaryImageHeader.Size * Header.ImageCount));
        var pData = HeightEntityBinaryHeader.Size + HeightEntityBinaryImageHeader.Size * Header.ImageCount;
        Data = buffer[pData..];
    }

    public ref HeightEntityBinaryHeader Header;
    public ReadOnlySpan<HeightEntityBinaryImageHeader> Images { get; }
    public ReadOnlySpan<byte> Data { get; }

    /// <summary>
    /// Gets the pixel data for an image in this binary.
    /// </summary>
    /// <param name="index">Index of the image in <see cref="Images" />.</param>
    public ReadOnlySpan<byte> GetPixelData(int index)
    {
        var header = Images[index];
        var dataOffset = HeightEntityBinaryHeader.Size + HeightEntityBinaryImageHeader.Size * index
                                                       + 4 + (int)header.TextureDataOffset;
        return Data.Slice(dataOffset, (int)header.TextureSizeBytes);
    }

    public bool TrySave(string path, ImageFileFormat format)
    {
        var pathNoExtension = path[..path.LastIndexOf('.')];

        // ReSharper disable twice PossibleUnintendedReferenceComparison
        if (format == ImageFileFormat.Heb)
        {
            var finalPath = $"{pathNoExtension}.{format}";
            IOHelper.EnsureDirectoriesExistForFilePath(finalPath);
            using var stream = new FileStream(finalPath, FileMode.Create, FileAccess.Write, FileShare.None);
            stream.Write(_buffer);
        }
        else
        {
            // TODO: Implement conversion for DDS, PNG, TGA
            //       See BlackTexture.Save for similar method to replicate
            throw new NotImplementedException();

            if (Header.ImageCount > 1)
            {
                // Export texture array
            }
            else if (Header.ImageCount > 0)
            {
                // Export single
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    public unsafe byte[] Convert(int imageIndex, IImageEncoder encoder)
    {
        var imageHeader = Images[imageIndex];
        var surfaceSpan = GetPixelData(imageIndex);

        using var surface = new NvttSurface(surfaceSpan, imageHeader.Format,
            (int)imageHeader.Width, (int)imageHeader.Height);
        using var image = Image.WrapMemory<RgbaVector>(
            surface.Data.ToPointer(),
            (int)(imageHeader.Width * imageHeader.Height * 4 * sizeof(float)),
            (int)imageHeader.Width,
            (int)imageHeader.Height);

        using var stream = new MemoryStream();
        image.Save(stream, encoder);
        return stream.ToArray();
    }
}