using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using CommunityToolkit.HighPerformance.Buffers;
using Flagrum.Core.Graphics.Textures.Luminous;
using Flagrum.Core.Graphics.Textures.Luminous.Builder;
using Flagrum.Core.Graphics.Textures.Luminous.DataSources;
using Flagrum.Core.Graphics.Textures.Shared.PixelFormats;
using JetBrains.Annotations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Flagrum.Core.Graphics.Textures.OpenExtendedRange;

/// <summary>
/// Convenience methods for utilizing OpenEXR to process EXR image files.
/// </summary>
public static class OpenExr
{
    /// <summary>
    /// Queries the metadata from an EXR file and creates an appropriate
    /// data source from it for <see cref="BlackTextureBuilder" />.
    /// </summary>
    /// <param name="filePath">Path to the <c>.exr</c> file on disk.</param>
    /// <returns>Newly created data source that targets the given EXR file.</returns>
    /// <exception cref="IOException">Thrown if the file fails to read.</exception>
    public static RasterImageDataSource CreateDataSource(string filePath)
    {
        // Open file for reading
        var initializer = OpenExrCore.DefaultInitializer;
        var result = OpenExrCore.StartRead(out var context, filePath, ref initializer);
        if (result != OpenExrCore.EXR_ERR_SUCCESS)
        {
            throw new IOException("Failed to open EXR file.");
        }

        // Query metadata
        var metadata = QueryMetadata(context);

        // Close file and return the data source
        OpenExrCore.Finish(ref context);
        return new RasterImageDataSource(filePath, metadata.Width, metadata.Height, GetPixelFormat(metadata.Channels));
    }

    /// <summary>
    /// Reads non-interleaved pixel data from an EXR file.
    /// </summary>
    /// <param name="stream">Stream for the file on disk.</param>
    /// <returns>Planar pixel data. Caller is responsible for disposing.</returns>
    /// <exception cref="IOException">Thrown if an error occurs while reading the file.</exception>
    [MustDisposeResource]
    public static unsafe Image ReadPlanarPixelData(Stream stream)
    {
        if (stream is not FileStream fileStream)
        {
            throw new InvalidOperationException("OpenEXR only supports reading files from disk.");
        }

        var path = fileStream.Name;
        fileStream.Dispose();

        var init = OpenExrCore.DefaultInitializer;

        // Open the file for reading
        var result = OpenExrCore.StartRead(out var context, path, ref init);
        if (result != OpenExrCore.EXR_ERR_SUCCESS)
        {
            throw new IOException("Failed to open EXR file.");
        }

        // Allocate unmanaged buffer
        var metadata = QueryMetadata(context);
        var totalPixels = metadata.Width * metadata.Height;
        var dataTypeSize = GetPixelTypeSize(metadata.Channels.Values.First());
        var channelSize = totalPixels * dataTypeSize;
        var buffer = MemoryOwner<byte>.Allocate(
            metadata.Channels.Count * channelSize);

        // Read data from each channel
        var channelOrder = new[] {"R", "G", "B", "A"};
        for (var i = 0; i < channelOrder.Length; i++)
        {
            var name = channelOrder[i];

            // Skip channels that don't exist
            if (!metadata.Channels.TryGetValue(name, out var type))
            {
                continue;
            }

            // Read current channel into the buffer
            var slice = buffer.Memory.Slice(channelSize * i, channelSize);
            using var pin = slice.Pin();
            result = OpenExrCore.ReadScanlineChannel(
                context,
                0,
                name,
                type,
                metadata.MinY,
                metadata.MaxY,
                new IntPtr(pin.Pointer),
                metadata.Width * dataTypeSize
            );

            if (result != OpenExrCore.EXR_ERR_SUCCESS)
            {
                OpenExrCore.Finish(ref context);
                throw new IOException($"Failed to read scanline channel '{name}'.");
            }
        }

        // Create an ImageSharp image to hold the interleaved data
        var format = GetPixelFormat(metadata.Channels);
        Image image = format switch
        {
            BlackTexturePixelFormat.R32_FLOAT => Image.WrapMemory<R32F>(buffer, metadata.Width, metadata.Height),
            BlackTexturePixelFormat.R16G16B16A16_FLOAT => InterleaveRgbaHalfVector(metadata, buffer),
            BlackTexturePixelFormat.R16G16B16A16_UNORM => InterleaveRgba64(metadata, buffer),
            BlackTexturePixelFormat.R32G32B32A32_FLOAT => InterleaveRgbaVector(metadata, buffer),
            _ => throw new NotSupportedException($"Unsupported pixel format {format}.")
        };

        // Close the file and hand the image over
        OpenExrCore.Finish(ref context);
        return image;
    }


    private static Image<RgbaHalfVector> InterleaveRgbaHalfVector(Metadata metadata, MemoryOwner<byte> buffer)
    {
        var channelSize = metadata.Width * metadata.Height;
        var image = new Image<RgbaHalfVector>(metadata.Width, metadata.Height);

        image.ProcessPixelRows(accessor =>
        {
            var span = MemoryMarshal.Cast<byte, Half>(buffer.Span);
            var red = span.Slice(0, channelSize);
            var green = span.Slice(channelSize, channelSize);
            var blue = span.Slice(channelSize * 2, channelSize);
            var alpha = span.Slice(channelSize * 3, channelSize);

            for (var y = 0; y < metadata.Height; y++)
            {
                var row = accessor.GetRowSpan(y);

                for (var x = 0; x < metadata.Width; x++)
                {
                    ref var pixel = ref row[x];
                    pixel.R = red[x];
                    pixel.G = green[x];
                    pixel.B = blue[x];
                    pixel.A = alpha[x];
                }
            }
        });

        buffer.Dispose();
        return image;
    }

    private static Image<Rgba64> InterleaveRgba64(Metadata metadata, MemoryOwner<byte> buffer)
    {
        var channelSize = metadata.Width * metadata.Height;
        var image = new Image<Rgba64>(metadata.Width, metadata.Height);

        image.ProcessPixelRows(accessor =>
        {
            var span = MemoryMarshal.Cast<byte, uint>(buffer.Span);
            var red = span.Slice(0, channelSize);
            var green = span.Slice(channelSize, channelSize);
            var blue = span.Slice(channelSize * 2, channelSize);
            var alpha = span.Slice(channelSize * 3, channelSize);

            for (var y = 0; y < metadata.Height; y++)
            {
                var row = accessor.GetRowSpan(y);

                for (var x = 0; x < metadata.Width; x++)
                {
                    ref var pixel = ref row[x];
                    pixel.R = (ushort)red[x];
                    pixel.G = (ushort)green[x];
                    pixel.B = (ushort)blue[x];
                    pixel.A = (ushort)alpha[x];
                }
            }
        });

        buffer.Dispose();
        return image;
    }

    private static Image<RgbaVector> InterleaveRgbaVector(Metadata metadata, MemoryOwner<byte> buffer)
    {
        var channelSize = metadata.Width * metadata.Height;
        var image = new Image<RgbaVector>(metadata.Width, metadata.Height);

        image.ProcessPixelRows(accessor =>
        {
            var span = MemoryMarshal.Cast<byte, float>(buffer.Span);
            var red = span.Slice(0, channelSize);
            var green = span.Slice(channelSize, channelSize);
            var blue = span.Slice(channelSize * 2, channelSize);
            var alpha = span.Slice(channelSize * 3, channelSize);

            for (var y = 0; y < metadata.Height; y++)
            {
                var row = accessor.GetRowSpan(y);

                for (var x = 0; x < metadata.Width; x++)
                {
                    ref var pixel = ref row[x];
                    pixel.R = red[x];
                    pixel.G = green[x];
                    pixel.B = blue[x];
                    pixel.A = alpha[x];
                }
            }
        });

        buffer.Dispose();
        return image;
    }

    /// <summary>
    /// Reads channel information and image dimensions from the EXR metadata.
    /// </summary>
    /// <param name="context">Pointer to the OpenEXR context.</param>
    /// <exception cref="IOException">Thrown if an error occurs while reading the metadata.</exception>
    private static Metadata QueryMetadata(IntPtr context)
    {
        // Query channel list
        var result = OpenExrCore.GetChannelList(context, 0, out var channelList);
        if (result != OpenExrCore.EXR_ERR_SUCCESS)
        {
            OpenExrCore.Finish(ref context);
            throw new IOException("Failed to get channel list.");
        }

        // Read channel types
        var channels = new Dictionary<string, OpenExrCore.PixelType>();
        for (var i = 0; i < channelList.num_channels; i++)
        {
            var entryPtr = IntPtr.Add(channelList.entries, i * Marshal.SizeOf<OpenExrCore.AttributeChannelListEntry>());
            var entry = Marshal.PtrToStructure<OpenExrCore.AttributeChannelListEntry>(entryPtr);
            var name = Marshal.PtrToStringUTF8(entry.name)!;
            channels[name] = entry.pixel_type;
        }

        // Query data window
        result = OpenExrCore.GetDataWindow(context, 0, out var window);
        if (result != OpenExrCore.EXR_ERR_SUCCESS)
        {
            OpenExrCore.Finish(ref context);
            throw new IOException("Failed to get data window");
        }

        var width = window.max.x - window.min.x + 1;
        var height = window.max.y - window.min.y + 1;

        return new Metadata(channels, width, height, window.min.y, window.max.y);
    }

    /// <summary>
    /// Gets the appropriate <see cref="BlackTexturePixelFormat" /> for the image based on its channel layout.
    /// </summary>
    /// <param name="channels">EXR channel layout.</param>
    /// <exception cref="NotSupportedException">
    /// Thrown if image channels are not in an expected configuration.
    /// </exception>
    private static BlackTexturePixelFormat GetPixelFormat(Dictionary<string, OpenExrCore.PixelType> channels)
    {
        // Check if input is R32_FLOAT
        if (channels.Count == 1
            && channels.TryGetValue("R", out var value)
            && value == OpenExrCore.PixelType.Float32)
        {
            return BlackTexturePixelFormat.R32_FLOAT;
        }

        // Check if image has 4 channels
        if (channels.Count == 4)
        {
            var type = channels.Values.First();

            // Make sure RGBA channels are present (order doesn't matter) and that all channels use the same data type
            if (channels.ContainsKey("R")
                && channels.ContainsKey("G")
                && channels.ContainsKey("B")
                && channels.ContainsKey("A")
                && channels.Values.All(c => c == type))
            {
                return type switch
                {
                    // Game only uses uint for 16-bit textures, so EXR uint32 will be used for this
                    OpenExrCore.PixelType.UInt32 => BlackTexturePixelFormat.R16G16B16A16_UINT,
                    OpenExrCore.PixelType.Float16 => BlackTexturePixelFormat.R16G16B16A16_FLOAT,
                    OpenExrCore.PixelType.Float32 => BlackTexturePixelFormat.R32G32B32A32_FLOAT,
                    _ => throw new NotSupportedException($"Unknown EXR pixel type {type}.")
                };
            }
        }

        // If we got here, pixel layout isn't supported, throw an exception
        var builder = new StringBuilder("Unsupported pixel format. Input was:\n");
        foreach (var (channel, type) in channels)
        {
            builder.Append($"  {channel}: {type}");
        }

        throw new NotSupportedException(builder.ToString());
    }

    /// <summary>
    /// Gets the size of one value in one channel for one pixel in an EXR image.
    /// </summary>
    /// <param name="pixelType">Data type to get the size of.</param>
    /// <returns>Size of the data type, in bytes.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if given pixel type is unknown.</exception>
    private static int GetPixelTypeSize(OpenExrCore.PixelType pixelType) => pixelType switch
    {
        OpenExrCore.PixelType.UInt32 => 4,
        OpenExrCore.PixelType.Float16 => 2,
        OpenExrCore.PixelType.Float32 => 4,
        _ => throw new ArgumentOutOfRangeException(nameof(pixelType), pixelType, "Unknown pixel type.")
    };

    /// <summary>
    /// Holds metadata about an EXR file.
    /// </summary>
    /// <param name="Channels">Channel info; key is channel name, value is data type.</param>
    /// <param name="Width">Width of the image, in pixels.</param>
    /// <param name="Height">Height of the image, in pixels.</param>
    /// <param name="MinY">Minimum Y value for the data window.</param>
    /// <param name="MaxY">Maximum Y value for the data window.</param>
    private record Metadata(
        Dictionary<string, OpenExrCore.PixelType> Channels,
        int Width,
        int Height,
        int MinY,
        int MaxY);
}