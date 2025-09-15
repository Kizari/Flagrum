using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Flagrum.Core.Graphics.Textures.Luminous;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

namespace Flagrum.Core.Graphics.Textures.NvidiaTextureTools;

/// <summary>
/// Wrapper for NVIDIA Texture Tools' (nvtt) Surface class.
/// </summary>
public sealed class NvttSurface : IDisposable
{
    private readonly bool _doesOwnSelf;
    private readonly MemoryHandle? _pin;

    public readonly IntPtr Handle;

    /// <summary>
    /// Creates a new surface that will be destroyed when this wrapper is disposed.
    /// Designed for surfaces that are <b>not</b> managed by a <see cref="NvttSurfaceSet" />.
    /// </summary>
    public unsafe NvttSurface(
        ReadOnlySpan<byte> pixelBuffer,
        BlackTexturePixelFormat pixelFormat,
        int width,
        int height)
    {
        _doesOwnSelf = true;
        Handle = Nvtt.CreateSurface();
        Nvtt.SurfaceSetImage2D(
            Handle,
            ToNvttFormat(pixelFormat),
            width,
            height,
            new IntPtr(Unsafe.AsPointer(ref MemoryMarshal.GetReference(pixelBuffer))),
            IntPtr.Zero);
    }

    /// <summary>
    /// Creates a new surface that will be destroyed when this wrapper is disposed.
    /// Designed for surfaces that are <b>not</b> managed by a <see cref="NvttSurfaceSet" />.
    /// </summary>
    public unsafe NvttSurface(Image<RgbaVector> image)
    {
        _doesOwnSelf = true;
        Handle = Nvtt.CreateSurface();

        if (!image.DangerousTryGetSinglePixelMemory(out var memory))
        {
            throw new Exception("Unable to get memory from image.");
        }

        _pin = memory.Pin();
        Nvtt.SurfaceSetImage2D(
            Handle,
            NvttFormat.Format_RGBA,
            image.Width,
            image.Height,
            new IntPtr(_pin.Value.Pointer),
            IntPtr.Zero);
    }

    /// <summary>
    /// Wraps a surface that is managed by a <see cref="NvttSurfaceSet" />.
    /// The surface will not be destroyed when this wrapper is disposed,
    /// as the <see cref="NvttSurfaceSet" /> is responsible for destroying it.
    /// </summary>
    /// <param name="handle">Pointer to the NVTT surface to wrap.</param>
    public NvttSurface(IntPtr handle) => Handle = handle;

    /// <summary>
    /// Gets a pointer to the raw pixel data for this surface.
    /// </summary>
    /// <remarks>
    /// Data is in RGBA float32 format.
    /// Each channel is in a contiguous block.
    /// </remarks>
    public IntPtr Data => Nvtt.SurfaceData(Handle);

    /// <summary>
    /// Width of the image represented by this surface, in pixels.
    /// </summary>
    public int Width => Nvtt.SurfaceWidth(Handle);

    /// <summary>
    /// Height of the image represented by this surface, in pixels.
    /// </summary>
    public int Height => Nvtt.SurfaceHeight(Handle);

    public void Dispose()
    {
        if (_doesOwnSelf)
        {
            Nvtt.DestroySurface(Handle);
            _pin?.Dispose();
        }
    }

    public unsafe void Compress(Stream destination, BlackTexturePixelFormat format)
    {
        using var compressionOptions = new NvttCompressionOptions();
        compressionOptions.SetFormat(ToNvttFormat(format));

        using var outputOptions = new NvttOutputOptions();
        outputOptions.SetOutputHandler(
            (_, _, _, _, _, _) => { },
            (pData, size) =>
            {
                destination.Write(new ReadOnlySpan<byte>(pData.ToPointer(), size));
                return true;
            },
            () => { });

        using var context = new NvttContext();
        context.Compress(this, 0, 0, compressionOptions, outputOptions);
    }

    public byte[] SaveDds(BlackTexturePixelFormat format)
    {
        using var stream = new MemoryStream();
        SaveDds(stream, format);
        return stream.ToArray();
    }

    public unsafe void SaveDds(Stream destination, BlackTexturePixelFormat format)
    {
        using var compressionOptions = new NvttCompressionOptions();
        compressionOptions.SetFormat(ToNvttFormat(format));

        using var outputOptions = new NvttOutputOptions();
        outputOptions.SetOutputHandler(
            (_, _, _, _, _, _) => { },
            (pData, size) =>
            {
                destination.Write(new ReadOnlySpan<byte>(pData.ToPointer(), size));
                return true;
            },
            () => { });

        using var context = new NvttContext();
        context.OutputHeader(this, 1, compressionOptions, outputOptions);
        context.Compress(this, 0, 0, compressionOptions, outputOptions);
    }

    /// <summary>
    /// Converts the image represented by this surface into a standard file format.
    /// </summary>
    /// <param name="destination">Stream to write the encoded file to.</param>
    /// <param name="encoder">Encoder to use to determine the file format.</param>
    public unsafe void Save(Stream destination, IImageEncoder encoder)
    {
        // Compute sizes
        var width = Width;
        var height = Height;
        var totalPixels = width * height;

        // Get views of each channel in the pixel buffer
        var pData = Data;
        var source = new Span<float>(pData.ToPointer(), totalPixels * 4);
        var red = source[..totalPixels];
        var green = source.Slice(totalPixels, totalPixels);
        var blue = source.Slice(totalPixels * 2, totalPixels);
        var alpha = source.Slice(totalPixels * 3, totalPixels);

        // Create a new buffer and copy the pixel data into it, interleaved
        var interleavedMemory = new Memory<RgbaVector>(new RgbaVector[totalPixels]);
        var interleaved = interleavedMemory.Span;
        for (var i = 0; i < totalPixels; i++)
        {
            ref var pixel = ref interleaved[i];
            pixel.R = red[i];
            pixel.G = green[i];
            pixel.B = blue[i];
            pixel.A = alpha[i];
        }

        // Wrap the interleaved buffer to avoid copying it
        using var image = Image.WrapMemory(Configuration.Default,
            interleavedMemory,
            width,
            height);

        image.Save(destination, encoder);
    }

    /// <summary>
    /// Gets the corresponding <see cref="NvttFormat" /> for a <see cref="BlackTexturePixelFormat" />.
    /// </summary>
    /// <exception cref="NotSupportedException">
    /// Thrown if <see cref="Nvtt" /> does not support the given <see cref="BlackTexturePixelFormat" />.
    /// </exception>
    private static NvttFormat ToNvttFormat(BlackTexturePixelFormat format) => format switch
    {
        BlackTexturePixelFormat.R8G8B8A8_UNORM => NvttFormat.Format_RGBA,
        BlackTexturePixelFormat.BC1_UNORM => NvttFormat.Format_DXT1,
        BlackTexturePixelFormat.BC2_UNORM => NvttFormat.Format_DXT3,
        BlackTexturePixelFormat.BC3_UNORM => NvttFormat.Format_DXT5,
        BlackTexturePixelFormat.BC4_UNORM => NvttFormat.Format_BC4,
        BlackTexturePixelFormat.BC5_UNORM => NvttFormat.Format_BC5,
        BlackTexturePixelFormat.BC6_UFLOAT => NvttFormat.Format_BC6U,
        BlackTexturePixelFormat.BC7_UNORM => NvttFormat.Format_BC7,
        _ => throw new NotSupportedException($"Unsupported pixel format {format}.")
    };
}