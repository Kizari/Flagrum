using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Scarlet.Drawing.Compression;
using Scarlet.IO;

namespace Scarlet.Drawing;

internal delegate void PixelOrderingDelegate(int origX, int origY, int width, int height, PixelDataFormat pixelFormat,
    out int transformedX, out int transformedY);

/// <summary>
/// Converts from and to various formats of pixel data
/// </summary>
public class ImageBinary
{
    private static readonly int[] pixelOrderingTiledDefault =
    {
        0, 1, 2, 3, 4, 5, 6, 7,
        8, 9, 10, 11, 12, 13, 14, 15,
        16, 17, 18, 19, 20, 21, 22, 23,
        24, 25, 26, 27, 28, 29, 30, 31,
        32, 33, 34, 35, 36, 37, 38, 39,
        40, 41, 42, 43, 44, 45, 46, 47,
        48, 49, 50, 51, 52, 53, 54, 55,
        56, 57, 58, 59, 60, 61, 62, 63
    };

    private static readonly int[] pixelOrderingTiled3DS =
    {
        0, 1, 8, 9, 2, 3, 10, 11,
        16, 17, 24, 25, 18, 19, 26, 27,
        4, 5, 12, 13, 6, 7, 14, 15,
        20, 21, 28, 29, 22, 23, 30, 31,
        32, 33, 40, 41, 34, 35, 42, 43,
        48, 49, 56, 57, 50, 51, 58, 59,
        36, 37, 44, 45, 38, 39, 46, 47,
        52, 53, 60, 61, 54, 55, 62, 63
    };

    private List<byte[]> inputPaletteData;
    private List<byte[]> inputPixelData;

    /// <summary>
    /// Creates a new <see cref="ImageBinary" /> instance, using default values
    /// </summary>
    public ImageBinary()
    {
        InitializeInstance();
    }

    /// <summary>
    /// Creates a new <see cref="ImageBinary" /> instance
    /// </summary>
    /// <param name="width">Width of the image</param>
    /// <param name="height">Height of the image</param>
    /// <param name="inputPixelFormat">Data format of the image's pixel data</param>
    /// <param name="inputPixelData">Byte array with image pixel data</param>
    public ImageBinary(int width, int height, PixelDataFormat inputPixelFormat, byte[] inputPixelData)
    {
        InitializeInstance(width, height, inputPixelFormat, inputPixelData: inputPixelData);
    }

    /// <summary>
    /// Creates a new <see cref="ImageBinary" /> instance
    /// </summary>
    /// <param name="width">Width of the image</param>
    /// <param name="height">Height of the image</param>
    /// <param name="inputPixelFormat">Data format of the image's pixel data</param>
    /// <param name="inputStream">Stream with image pixel data</param>
    public ImageBinary(int width, int height, PixelDataFormat inputPixelFormat, Stream inputStream)
    {
        var inputPixelData = new byte[inputStream.Length];
        inputStream.Seek(0, SeekOrigin.Begin);
        inputStream.Read(inputPixelData, 0, inputPixelData.Length);

        InitializeInstance(width, height, inputPixelFormat, inputPixelData: inputPixelData);
    }

    /// <summary>
    /// Creates a new <see cref="ImageBinary" /> instance
    /// </summary>
    /// <param name="width">Width of the image</param>
    /// <param name="height">Height of the image</param>
    /// <param name="inputPixelFormat">Data format of the image's pixel data</param>
    /// <param name="inputStream">Stream with image pixel data</param>
    /// <param name="inputOffset">Offset in stream to read pixel data from</param>
    /// <param name="inputLength">Length of pixel data in bytes</param>
    public ImageBinary(int width, int height, PixelDataFormat inputPixelFormat, Stream inputStream, int inputOffset,
        int inputLength)
    {
        var inputPixelData = new byte[inputLength];
        inputStream.Seek(inputOffset, SeekOrigin.Begin);
        inputStream.Read(inputPixelData, 0, inputLength);

        InitializeInstance(width, height, inputPixelFormat, inputPixelData: inputPixelData);
    }

    /// <summary>
    /// Creates a new <see cref="ImageBinary" /> instance
    /// </summary>
    /// <param name="width">Width of the image</param>
    /// <param name="height">Height of the image</param>
    /// <param name="inputPixelFormat">Data format of the image's pixel data</param>
    /// <param name="inputEndianness">Endianness of the image's pixel data</param>
    /// <param name="inputPixelData">Byte array with image pixel data</param>
    public ImageBinary(int width, int height, PixelDataFormat inputPixelFormat, Endian inputEndianness,
        byte[] inputPixelData)
    {
        InitializeInstance(width, height, inputPixelFormat, inputEndianness, inputPixelData: inputPixelData);
    }

    /// <summary>
    /// Creates a new <see cref="ImageBinary" /> instance
    /// </summary>
    /// <param name="width">Width of the image</param>
    /// <param name="height">Height of the image</param>
    /// <param name="inputPixelFormat">Data format of the image's pixel data</param>
    /// <param name="inputEndianness">Endianness of the image's pixel data</param>
    /// <param name="inputStream">Stream with image pixel data</param>
    public ImageBinary(int width, int height, PixelDataFormat inputPixelFormat, Endian inputEndianness,
        Stream inputStream)
    {
        var inputPixelData = new byte[inputStream.Length];
        inputStream.Seek(0, SeekOrigin.Begin);
        inputStream.Read(inputPixelData, 0, inputPixelData.Length);

        InitializeInstance(width, height, inputPixelFormat, inputEndianness, inputPixelData: inputPixelData);
    }

    /// <summary>
    /// Creates a new <see cref="ImageBinary" /> instance
    /// </summary>
    /// <param name="width">Width of the image</param>
    /// <param name="height">Height of the image</param>
    /// <param name="inputPixelFormat">Data format of the image's pixel data</param>
    /// <param name="inputEndianness">Endianness of the image's pixel data</param>
    /// <param name="inputStream">Stream with image pixel data</param>
    /// <param name="inputOffset">Offset in stream to read pixel data from</param>
    /// <param name="inputLength">Length of pixel data in bytes</param>
    public ImageBinary(int width, int height, PixelDataFormat inputPixelFormat, Endian inputEndianness,
        Stream inputStream, int inputOffset, int inputLength)
    {
        var inputPixelData = new byte[inputLength];
        inputStream.Seek(inputOffset, SeekOrigin.Begin);
        inputStream.Read(inputPixelData, 0, inputLength);

        InitializeInstance(width, height, inputPixelFormat, inputEndianness, inputPixelData: inputPixelData);
    }

    /// <summary>
    /// Creates a new <see cref="ImageBinary" /> instance
    /// </summary>
    /// <param name="width">Width of the image</param>
    /// <param name="height">Height of the image</param>
    /// <param name="inputPixelFormat">Data format of the image's pixel data</param>
    /// <param name="outputFormat">Data format of the converted pixel data</param>
    /// <param name="inputPixelData">Byte array with image pixel data</param>
    public ImageBinary(int width, int height, PixelDataFormat inputPixelFormat, PixelDataFormat outputFormat,
        byte[] inputPixelData)
    {
        InitializeInstance(width, height, inputPixelFormat, outputFormat: outputFormat, inputPixelData: inputPixelData);
    }

    /// <summary>
    /// Creates a new <see cref="ImageBinary" /> instance
    /// </summary>
    /// <param name="width">Width of the image</param>
    /// <param name="height">Height of the image</param>
    /// <param name="inputPixelFormat">Data format of the image's pixel data</param>
    /// <param name="outputFormat">Data format of the converted pixel data</param>
    /// <param name="inputStream">Stream with image pixel data</param>
    public ImageBinary(int width, int height, PixelDataFormat inputPixelFormat, PixelDataFormat outputFormat,
        Stream inputStream)
    {
        var inputPixelData = new byte[inputStream.Length];
        inputStream.Seek(0, SeekOrigin.Begin);
        inputStream.Read(inputPixelData, 0, inputPixelData.Length);

        InitializeInstance(width, height, inputPixelFormat, outputFormat: outputFormat, inputPixelData: inputPixelData);
    }

    /// <summary>
    /// Creates a new <see cref="ImageBinary" /> instance
    /// </summary>
    /// <param name="width">Width of the image</param>
    /// <param name="height">Height of the image</param>
    /// <param name="inputPixelFormat">Data format of the image's pixel data</param>
    /// <param name="outputFormat">Data format of the converted pixel data</param>
    /// <param name="inputStream">Stream with image pixel data</param>
    /// <param name="inputOffset">Offset in stream to read pixel data from</param>
    /// <param name="inputLength">Length of pixel data in bytes</param>
    public ImageBinary(int width, int height, PixelDataFormat inputPixelFormat, PixelDataFormat outputFormat,
        Stream inputStream, int inputOffset, int inputLength)
    {
        var inputPixelData = new byte[inputLength];
        inputStream.Seek(inputOffset, SeekOrigin.Begin);
        inputStream.Read(inputPixelData, 0, inputLength);

        InitializeInstance(width, height, inputPixelFormat, outputFormat: outputFormat, inputPixelData: inputPixelData);
    }

    /// <summary>
    /// Creates a new <see cref="ImageBinary" /> instance
    /// </summary>
    /// <param name="width">Width of the image</param>
    /// <param name="height">Height of the image</param>
    /// <param name="inputPixelFormat">Data format of the image's pixel data</param>
    /// <param name="inputEndianness">Endianness of the image's pixel data</param>
    /// <param name="outputFormat">Data format of the converted pixel data</param>
    /// <param name="outputEndianness">Endianness of the converted pixel data</param>
    /// <param name="inputPixelData">Byte array with image pixel data</param>
    public ImageBinary(int width, int height, PixelDataFormat inputPixelFormat, Endian inputEndianness,
        PixelDataFormat outputFormat, Endian outputEndianness, byte[] inputPixelData)
    {
        InitializeInstance(width, height, inputPixelFormat, inputEndianness, outputFormat, outputEndianness,
            inputPixelData);
    }

    /// <summary>
    /// Creates a new <see cref="ImageBinary" /> instance
    /// </summary>
    /// <param name="width">Width of the image</param>
    /// <param name="height">Height of the image</param>
    /// <param name="inputPixelFormat">Data format of the image's pixel data</param>
    /// <param name="inputEndianness">Endianness of the image's pixel data</param>
    /// <param name="outputFormat">Data format of the converted pixel data</param>
    /// <param name="outputEndianness">Endianness of the converted pixel data</param>
    /// <param name="inputStream">Stream with image pixel data</param>
    public ImageBinary(int width, int height, PixelDataFormat inputPixelFormat, Endian inputEndianness,
        PixelDataFormat outputFormat, Endian outputEndianness, Stream inputStream)
    {
        var inputPixelData = new byte[inputStream.Length];
        inputStream.Read(inputPixelData, 0, inputPixelData.Length);

        InitializeInstance(width, height, inputPixelFormat, inputEndianness, outputFormat, outputEndianness,
            inputPixelData);
    }

    /// <summary>
    /// Creates a new <see cref="ImageBinary" /> instance
    /// </summary>
    /// <param name="width">Width of the image</param>
    /// <param name="height">Height of the image</param>
    /// <param name="inputPixelFormat">Data format of the image's pixel data</param>
    /// <param name="inputEndianness">Endianness of the image's pixel data</param>
    /// <param name="outputFormat">Data format of the converted pixel data</param>
    /// <param name="outputEndianness">Endianness of the converted pixel data</param>
    /// <param name="inputStream">Stream with image pixel data</param>
    /// <param name="inputOffset">Offset in stream to read pixel data from</param>
    /// <param name="inputLength">Length of pixel data in bytes</param>
    public ImageBinary(int width, int height, PixelDataFormat inputPixelFormat, Endian inputEndianness,
        PixelDataFormat outputFormat, Endian outputEndianness, Stream inputStream, int inputOffset, int inputLength)
    {
        var inputPixelData = new byte[inputLength];
        inputStream.Read(inputPixelData, inputOffset, inputLength);

        InitializeInstance(width, height, inputPixelFormat, inputEndianness, outputFormat, outputEndianness,
            inputPixelData);
    }

    /// <summary>
    /// Get or set width of the input image
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Get or set height of the input image
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Get or set physical width of the input image
    /// </summary>
    public int PhysicalWidth { get; set; }

    /// <summary>
    /// Get or set physical height of the input image
    /// </summary>
    public int PhysicalHeight { get; set; }

    /// <summary>
    /// Get or set format of the input pixel data
    /// </summary>
    public PixelDataFormat InputPixelFormat { get; set; }

    /// <summary>
    /// Get or set format of the input palette data
    /// </summary>
    public PixelDataFormat InputPaletteFormat { get; set; }

    /// <summary>
    /// Get or set endianness of the input pixel data
    /// </summary>
    public Endian InputEndianness { get; set; }

    /// <summary>
    /// Get or set format of the output pixel data
    /// </summary>
    public PixelDataFormat OutputFormat { get; set; }

    /// <summary>
    /// Get or set endianness of the output pixel data
    /// </summary>
    public Endian OutputEndianness { get; set; }

    private void InitializeInstance(int width = 0, int height = 0,
        PixelDataFormat inputPixelFormat = PixelDataFormat.Undefined,
        Endian inputEndianness = EndianBinaryReader.NativeEndianness,
        PixelDataFormat outputFormat = PixelDataFormat.FormatArgb8888,
        Endian outputEndianness = EndianBinaryReader.NativeEndianness, byte[] inputPixelData = null)
    {
        Width = width;
        Height = height;

        PhysicalWidth = width;
        PhysicalHeight = height;

        InputPixelFormat = inputPixelFormat;
        InputEndianness = inputEndianness;

        OutputFormat = outputFormat;
        OutputEndianness = outputEndianness;

        this.inputPixelData = new List<byte[]>();
        if (inputPixelData != null)
        {
            this.inputPixelData.Add(inputPixelData);
        }

        inputPaletteData = new List<byte[]>();
    }

    /// <summary>
    /// Generates a byte array with pixel data, using this instance's image information
    /// </summary>
    /// <returns>Byte array with pixel data</returns>
    public byte[] GetOutputPixelData(int imageIndex)
    {
        ValidateImageProperties();

        byte[] pixelData;

        if ((InputPixelFormat & PixelDataFormat.MaskChannels) != PixelDataFormat.ChannelsIndexed)
        {
            pixelData = ConvertPixelDataToArgb8888(inputPixelData[imageIndex], InputPixelFormat);
            pixelData = ApplyFilterToArgb8888(PhysicalWidth, PhysicalHeight, OutputFormat, pixelData);

            pixelData = ConvertArgb8888ToOutputFormat(pixelData, OutputFormat, OutputEndianness);
        }
        else
        {
            pixelData = ReadPixelDataIndexed(inputPixelData[imageIndex], InputPixelFormat);
        }

        return pixelData;
    }

    /// <summary>
    /// Adds a byte array with color palette data to this instance
    /// </summary>
    /// <param name="paletteData"></param>
    public void AddInputPalette(byte[] paletteData)
    {
        inputPaletteData.Add(paletteData);
    }

    /// <summary>
    /// Adds a byte array with pixel data to this instance
    /// </summary>
    /// <param name="pixelData"></param>
    public void AddInputPixels(byte[] pixelData)
    {
        inputPixelData.Add(pixelData);
    }

    /// <summary>
    /// Gets the byte array with color palette data at the specified index
    /// </summary>
    /// <param name="paletteIndex">Index of palette to get</param>
    /// <returns>Byte array with palette data</returns>
    public byte[] GetInputPalette(int paletteIndex)
    {
        if (paletteIndex < 0 || paletteIndex >= inputPaletteData.Count)
        {
            throw new IndexOutOfRangeException("Invalid palette index");
        }

        return inputPaletteData[paletteIndex];
    }

    /// <summary>
    /// Gets the byte array with pixel data at the specified index
    /// </summary>
    /// <param name="imageIndex">Index of pixel data to get</param>
    /// <returns>Byte array with pixel data</returns>
    public byte[] GetInputPixels(int imageIndex)
    {
        if (imageIndex < 0 || imageIndex >= inputPixelData.Count)
        {
            throw new IndexOutOfRangeException("Invalid image index");
        }

        return inputPixelData[imageIndex];
    }

    /// <summary>
    /// Generates a color array with palette colors, using this instance's image information and the specified palette index
    /// </summary>
    /// <param name="paletteIndex">Index of palette to use</param>
    /// <returns>Color array with palette colors</returns>
    public Color[] GetOutputPaletteData(int paletteIndex)
    {
        ValidateImageProperties();

        Color[] palette;

        if ((InputPixelFormat & PixelDataFormat.MaskChannels) != PixelDataFormat.ChannelsIndexed)
        {
            throw new Exception("Cannot get palette data from non-indexed image");
        }

        palette = ReadPaletteData(GetInputPalette(paletteIndex), InputPixelFormat, InputPaletteFormat);

        return palette;
    }

    /// <summary>
    /// Generates a bitmap (ARGB8888, Indexed 4bpp, or Indexed 8bpp), using this instance's image information
    /// </summary>
    /// <returns>Generated bitmap</returns>
    public Bitmap GetBitmap()
    {
        return GetBitmap(0, 0);
    }

    /// <summary>
    /// Generates a bitmap (ARGB8888, Indexed 4bpp, or Indexed 8bpp), using this instance's image information and the specified
    /// image and palette indices
    /// </summary>
    /// <param name="imageIndex">Index of pixel data to use</param>
    /// <param name="paletteIndex">Index of palette to use</param>
    /// <returns>Generated bitmap</returns>
    public Bitmap GetBitmap(int imageIndex, int paletteIndex)
    {
        ValidateImageProperties();

        PixelFormat imagePixelFormat;

        var isIndexed = (InputPixelFormat & PixelDataFormat.MaskChannels) == PixelDataFormat.ChannelsIndexed;

        var inputPixels = GetInputPixels(imageIndex);

        byte[] pixelData = null;
        Color[] palette = null;

        if (!isIndexed)
        {
            imagePixelFormat = PixelFormat.Format32bppArgb;
            pixelData = ConvertPixelDataToArgb8888(inputPixels, InputPixelFormat);
            pixelData = ApplyFilterToArgb8888(PhysicalWidth, PhysicalHeight, OutputFormat, pixelData);
        }
        else
        {
            imagePixelFormat = (InputPixelFormat & PixelDataFormat.MaskBpp) == PixelDataFormat.Bpp4
                ? PixelFormat.Format4bppIndexed
                : PixelFormat.Format8bppIndexed;
            pixelData = ReadPixelDataIndexed(inputPixels, InputPixelFormat);
            palette = ReadPaletteData(GetInputPalette(paletteIndex), InputPixelFormat, InputPaletteFormat);
        }

        var image = new Bitmap(PhysicalWidth, PhysicalHeight, imagePixelFormat);
        var bmpData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite,
            image.PixelFormat);

        var pixelsForBmp = new byte[bmpData.Height * bmpData.Stride];
        var bitsPerPixel = Image.GetPixelFormatSize(image.PixelFormat);

        // TODO: verify input stride/line size & copy length logic; *seems* to work okay now...?

        int lineSize, copySize;

        if (bmpData.Width % 8 == 0 || (InputPixelFormat & PixelDataFormat.MaskSpecial) != PixelDataFormat.Undefined)
        {
            lineSize = bmpData.Width / (bitsPerPixel < 8 ? 2 : 1) * (bitsPerPixel < 8 ? 1 : bitsPerPixel / 8);
        }
        else
        {
            lineSize = inputPixels.Length / bmpData.Height;
        }

        if (isIndexed && (InputPixelFormat & PixelDataFormat.MaskBpp) == PixelDataFormat.Bpp4)
        {
            copySize = bmpData.Width / 2;
        }
        else
        {
            copySize = bmpData.Width / (bitsPerPixel < 8 ? 2 : 1) * (bitsPerPixel < 8 ? 1 : bitsPerPixel / 8);
        }

        for (var y = 0; y < bmpData.Height; y++)
        {
            var srcOffset = y * lineSize;
            var dstOffset = y * bmpData.Stride;
            if (srcOffset >= pixelData.Length || dstOffset >= pixelsForBmp.Length)
            {
                continue;
            }

            Buffer.BlockCopy(pixelData, srcOffset, pixelsForBmp, dstOffset, copySize);
        }

        if (isIndexed && palette != null)
        {
            var imagePalette = image.Palette;
            Array.Copy(palette, imagePalette.Entries, palette.Length);
            image.Palette = imagePalette;
        }

        Marshal.Copy(pixelsForBmp, 0, bmpData.Scan0, pixelsForBmp.Length);
        image.UnlockBits(bmpData);

        return image.Clone(new Rectangle(0, 0, Width, Height), image.PixelFormat);
    }

    private bool IsValidPixelDataFormat(PixelDataFormat fmt)
    {
        return (fmt & PixelDataFormat.MaskBpp).IsValid()
               && (fmt & PixelDataFormat.MaskChannels).IsValid()
               && (fmt & PixelDataFormat.MaskRedBits).IsValid()
               && (fmt & PixelDataFormat.MaskGreenBits).IsValid()
               && (fmt & PixelDataFormat.MaskBlueBits).IsValid()
               && (fmt & PixelDataFormat.MaskAlphaBits).IsValid()
               && (fmt & PixelDataFormat.MaskLuminanceBits).IsValid()
               && (fmt & PixelDataFormat.MaskSpecial).IsValid()
               && (fmt & PixelDataFormat.MaskPixelOrdering).IsValid()
               && (fmt & PixelDataFormat.MaskFilter).IsValid()
               && (fmt & PixelDataFormat.MaskForceChannel).IsValid()
               && (fmt & PixelDataFormat.MaskReserved).IsValid();
    }

    private void ValidateImageProperties()
    {
        if (Width <= 0 || Width >= 16384)
        {
            throw new Exception("Invalid virtual width");
        }

        if (Height <= 0 || Height >= 16384)
        {
            throw new Exception("Invalid virtual height");
        }

        if (PhysicalWidth <= 0 || PhysicalWidth >= 16384)
        {
            PhysicalWidth = Width;
        }

        if (PhysicalHeight <= 0 || PhysicalHeight >= 16384)
        {
            PhysicalHeight = Height;
        }

        if (!IsValidPixelDataFormat(InputPixelFormat))
        {
            throw new Exception("Invalid input format");
        }

        if ((InputPixelFormat & PixelDataFormat.MaskSpecial) == PixelDataFormat.Undefined &&
            !Constants.RealBitsPerPixel.ContainsKey(InputPixelFormat & PixelDataFormat.MaskBpp))
        {
            throw new Exception("Invalid input bits per pixel");
        }

        if (!InputEndianness.IsValid())
        {
            throw new Exception("Invalid input endianness");
        }

        if (!IsValidPixelDataFormat(OutputFormat))
        {
            throw new Exception("Invalid output format");
        }

        if ((OutputFormat & PixelDataFormat.MaskSpecial) == PixelDataFormat.Undefined &&
            !Constants.RealBitsPerPixel.ContainsKey(OutputFormat & PixelDataFormat.MaskBpp))
        {
            throw new Exception("Invalid output bits per pixel");
        }

        if (!OutputEndianness.IsValid())
        {
            throw new Exception("Invalid output endianness");
        }
    }

    private byte[] ReadPixelDataIndexed(byte[] inputData, PixelDataFormat inputPixelFormat)
    {
        using (var reader = new EndianBinaryReader(new MemoryStream(inputData), InputEndianness))
        {
            return ReadPixelDataIndexed(reader, inputPixelFormat);
        }
    }

    private byte[] ReadPixelDataIndexed(EndianBinaryReader reader, PixelDataFormat inputPixelFormat)
    {
        if ((inputPixelFormat & PixelDataFormat.MaskChannels) != PixelDataFormat.ChannelsIndexed)
        {
            throw new Exception("Cannot read non-indexed as indexed");
        }

        var inBpp = inputPixelFormat & PixelDataFormat.MaskBpp;
        if (inBpp != PixelDataFormat.Bpp4 && inBpp != PixelDataFormat.Bpp8)
        {
            throw new Exception("Cannot read indexed data that is not 4bpp or 8bpp");
        }

        var pixelOrderingFunc = GetPixelOrderingFunction(inputPixelFormat);

        var dataIndexed = new byte[reader.BaseStream.Length];
        for (int i = 0, x = 0, y = 0; i < dataIndexed.Length; i++)
        {
            int tx, ty;
            pixelOrderingFunc(x, y, PhysicalWidth, PhysicalHeight, inputPixelFormat, out tx, out ty);

            if (inBpp == PixelDataFormat.Bpp8)
            {
                var index = reader.ReadByte();
                if (tx < PhysicalWidth && ty < PhysicalHeight)
                {
                    dataIndexed[ty * PhysicalWidth + tx] = index;
                }

                x++;
                if (x == PhysicalWidth)
                {
                    x = 0;
                    y++;
                }
            }
            else
            {
                var indices = reader.ReadByte();
                if (tx + 1 < PhysicalWidth && ty + 1 < PhysicalHeight)
                {
                    var pixelOffset = (ty * PhysicalWidth + tx) / 2;

                    /* TODO: verify me! */
                    if (reader.Endianness == Endian.BigEndian)
                    {
                        dataIndexed[pixelOffset] = indices;
                    }
                    else
                    {
                        dataIndexed[pixelOffset] = (byte)(((indices & 0xF) << 4) | (indices >> 4));
                    }
                }

                x += 2;
                if (x == PhysicalWidth)
                {
                    x = 0;
                    y++;
                }
            }
        }

        return dataIndexed;
    }

    private Color[] ReadPaletteData(byte[] inputData, PixelDataFormat inputPixelFormat,
        PixelDataFormat inputPaletteFormat)
    {
        using (var reader = new EndianBinaryReader(new MemoryStream(inputData), InputEndianness))
        {
            return ReadPaletteData(reader, inputPixelFormat, inputPaletteFormat);
        }
    }

    private Color[] ReadPaletteData(EndianBinaryReader reader, PixelDataFormat inputPixelFormat,
        PixelDataFormat inputPaletteFormat)
    {
        var paletteData = ConvertInputDataToArgb8888(reader, inputPaletteFormat);
        var colorCount = 1 << Constants.RealBitsPerPixel[inputPixelFormat & PixelDataFormat.MaskBpp];
        var palette = new Color[colorCount];

        if (paletteData.Length < colorCount * 4)
        {
            throw new Exception("Input palette data too short");
        }

        for (var i = 0; i < palette.Length; i++)
        {
            palette[i] = Color.FromArgb(paletteData[i * 4 + 3], paletteData[i * 4 + 2], paletteData[i * 4 + 1],
                paletteData[i * 4 + 0]);
        }

        return palette;
    }

    private byte[] ConvertPixelDataToArgb8888(byte[] inputData, PixelDataFormat inputPixelFormat)
    {
        using (var reader = new EndianBinaryReader(new MemoryStream(inputData), InputEndianness))
        {
            return ConvertInputDataToArgb8888(reader, inputPixelFormat);
        }
    }

    private byte[] ConvertInputDataToArgb8888(EndianBinaryReader reader, PixelDataFormat inputPixelFormat)
    {
        var specialFormat = inputPixelFormat & PixelDataFormat.MaskSpecial;
        if (specialFormat == PixelDataFormat.Undefined)
        {
            return ConvertNormalInputToArgb8888(reader, inputPixelFormat);
        }

        return ConvertSpecialInputToArgb8888(reader, inputPixelFormat);
    }

    private byte[] ConvertSpecialInputToArgb8888(EndianBinaryReader reader, PixelDataFormat inputPixelFormat)
    {
        byte[] outputData;

        var specialFormat = inputPixelFormat & PixelDataFormat.MaskSpecial;
        switch (specialFormat)
        {
            case PixelDataFormat.SpecialFormatETC1_3DS:
            case PixelDataFormat.SpecialFormatETC1A4_3DS:
                outputData = ETC1.Decompress(reader, PhysicalWidth, PhysicalHeight, specialFormat,
                    reader.BaseStream.Length);
                break;

            case PixelDataFormat.SpecialFormatPVRT2_Vita:
            case PixelDataFormat.SpecialFormatPVRT4_Vita:
                outputData = PVRTC.Decompress(reader, PhysicalWidth, PhysicalHeight, specialFormat,
                    reader.BaseStream.Length);
                break;

            case PixelDataFormat.SpecialFormatDXT1:
            case PixelDataFormat.SpecialFormatDXT1_PSP:
            case PixelDataFormat.SpecialFormatDXT3:
            case PixelDataFormat.SpecialFormatDXT3_PSP:
            case PixelDataFormat.SpecialFormatDXT5:
            case PixelDataFormat.SpecialFormatDXT5_PSP:
            case PixelDataFormat.SpecialFormatRGTC1:
            case PixelDataFormat.SpecialFormatRGTC1_Signed:
            case PixelDataFormat.SpecialFormatRGTC2:
            case PixelDataFormat.SpecialFormatRGTC2_Signed:
                outputData = DXTxRGTC.Decompress(reader, PhysicalWidth, PhysicalHeight, inputPixelFormat,
                    reader.BaseStream.Length);
                break;

            case PixelDataFormat.SpecialFormatBPTC_Float:
            case PixelDataFormat.SpecialFormatBPTC_SignedFloat:
                outputData = BPTCFloat.Decompress(reader, PhysicalWidth, PhysicalHeight, inputPixelFormat,
                    reader.BaseStream.Length);
                break;

            case PixelDataFormat.SpecialFormatBPTC:
                outputData = BPTC.Decompress(reader, PhysicalWidth, PhysicalHeight, inputPixelFormat,
                    reader.BaseStream.Length);
                break;

            default: throw new Exception("Unimplemented special format");
        }

        return outputData;
    }

    private byte[] ConvertNormalInputToArgb8888(EndianBinaryReader reader, PixelDataFormat inputPixelFormat)
    {
        var inBpp = inputPixelFormat & PixelDataFormat.MaskBpp;
        var inChannels = inputPixelFormat & PixelDataFormat.MaskChannels;
        var inRedBits = inputPixelFormat & PixelDataFormat.MaskRedBits;
        var inGreenBits = inputPixelFormat & PixelDataFormat.MaskGreenBits;
        var inBlueBits = inputPixelFormat & PixelDataFormat.MaskBlueBits;
        var inAlphaBits = inputPixelFormat & PixelDataFormat.MaskAlphaBits;

        var inputBpp = Constants.RealBitsPerPixel[inBpp];
        var inputBppRead = Constants.InputBitsPerPixel[inBpp];

        var pixelCount = inputBpp < 8
            ? reader.BaseStream.Length * (8 / inputBpp)
            : reader.BaseStream.Length / (inputBpp / 8);

        var bitsBpp32 = Constants.RealBitsPerPixel[PixelDataFormat.Bpp32];
        var dataArgb8888 = new byte[pixelCount * (bitsBpp32 / 8)];

        switch (inChannels)
        {
            case PixelDataFormat.ChannelsRgb:
                CheckBitsPerChannelValidity(inRedBits, inGreenBits, inBlueBits);
                break;
            case PixelDataFormat.ChannelsBgr:
                CheckBitsPerChannelValidity(inBlueBits, inGreenBits, inRedBits);
                break;
            case PixelDataFormat.ChannelsRgba:
                CheckBitsPerChannelValidity(inRedBits, inGreenBits, inBlueBits, inAlphaBits);
                break;
            case PixelDataFormat.ChannelsBgra:
                CheckBitsPerChannelValidity(inBlueBits, inGreenBits, inRedBits, inAlphaBits);
                break;
            case PixelDataFormat.ChannelsArgb:
                CheckBitsPerChannelValidity(inAlphaBits, inRedBits, inGreenBits, inBlueBits);
                break;
            case PixelDataFormat.ChannelsAbgr:
                CheckBitsPerChannelValidity(inAlphaBits, inBlueBits, inGreenBits, inRedBits);
                break;
            case PixelDataFormat.ChannelsRgbx:
                CheckBitsPerChannelValidity(inRedBits, inGreenBits, inBlueBits, inAlphaBits);
                break;
            case PixelDataFormat.ChannelsBgrx:
                CheckBitsPerChannelValidity(inBlueBits, inGreenBits, inRedBits, inAlphaBits);
                break;
            case PixelDataFormat.ChannelsXrgb:
                CheckBitsPerChannelValidity(inAlphaBits, inRedBits, inGreenBits, inBlueBits);
                break;
            case PixelDataFormat.ChannelsXbgr:
                CheckBitsPerChannelValidity(inAlphaBits, inBlueBits, inGreenBits, inRedBits);
                break;
            case PixelDataFormat.ChannelsLuminance:
                CheckBitsPerChannelValidity(inRedBits);
                break;
            case PixelDataFormat.ChannelsAlpha:
                CheckBitsPerChannelValidity(inAlphaBits);
                break;
            case PixelDataFormat.ChannelsLuminanceAlpha:
                CheckBitsPerChannelValidity(inRedBits, inAlphaBits);
                break;
            case PixelDataFormat.ChannelsAlphaLuminance:
                CheckBitsPerChannelValidity(inAlphaBits, inRedBits);
                break;

            default: throw new Exception("Unhandled channel input layout");
        }

        var channelBitsRed = inRedBits != PixelDataFormat.Undefined ? Constants.BitsPerChannel[inRedBits] : 0;
        var channelBitsGreen = inGreenBits != PixelDataFormat.Undefined ? Constants.BitsPerChannel[inGreenBits] : 0;
        var channelBitsBlue = inBlueBits != PixelDataFormat.Undefined ? Constants.BitsPerChannel[inBlueBits] : 0;
        var channelBitsAlpha = inAlphaBits != PixelDataFormat.Undefined ? Constants.BitsPerChannel[inAlphaBits] : 0;

        var forceChannelValue = (inputPixelFormat & PixelDataFormat.MaskForceChannel) == PixelDataFormat.ForceClear
            ? uint.MinValue
            : uint.MaxValue;

        var pixelOrderingFunc = GetPixelOrderingFunction(inputPixelFormat);

        var isNativeLittleEndian = EndianBinaryReader.NativeEndianness == Endian.LittleEndian;
        var byteReadStep = inputBppRead / 8;

        uint rawData = 0;
        for (int i = 0, x = 0, y = 0;
             i < reader.BaseStream.Length - reader.BaseStream.Length % byteReadStep;
             i += byteReadStep)
        {
            switch (inBpp)
            {
                case PixelDataFormat.Bpp4:
                    rawData = reader.ReadByte();
                    if (reader.IsNativeEndianness)
                    {
                        rawData = ((rawData & 0xF) << 4) | ((rawData >> 4) & 0xF); /* TODO: verify me! */
                    }

                    break;
                case PixelDataFormat.Bpp8:
                    rawData = reader.ReadByte();
                    break;
                case PixelDataFormat.Bpp16:
                    rawData = reader.ReadUInt16();
                    break;
                case PixelDataFormat.Bpp24:
                    /* TODO: verify me! */
                    if (reader.IsNativeEndianness)
                    {
                        rawData = (uint)(reader.ReadByte() | (reader.ReadByte() << 8) | (reader.ReadByte() << 16));
                    }
                    else
                    {
                        rawData = (uint)((reader.ReadByte() << 16) | (reader.ReadByte() << 8) | reader.ReadByte());
                    }

                    break;
                case PixelDataFormat.Bpp32:
                    rawData = reader.ReadUInt32();
                    break;

                default: throw new Exception("Unhandled data read");
            }

            for (var k = inputBppRead / inputBpp - 1; k >= 0; k--)
            {
                var bppTemp = inputBpp;
                uint alpha, red, green, blue;

                switch (inChannels)
                {
                    case PixelDataFormat.ChannelsRgb:
                        red = ExtractChannel(rawData >> (k * inputBpp), channelBitsRed, ref bppTemp);
                        green = ExtractChannel(rawData >> (k * inputBpp), channelBitsGreen, ref bppTemp);
                        blue = ExtractChannel(rawData >> (k * inputBpp), channelBitsBlue, ref bppTemp);

                        alpha = forceChannelValue;
                        break;

                    case PixelDataFormat.ChannelsBgr:
                        blue = ExtractChannel(rawData >> (k * inputBpp), channelBitsBlue, ref bppTemp);
                        green = ExtractChannel(rawData >> (k * inputBpp), channelBitsGreen, ref bppTemp);
                        red = ExtractChannel(rawData >> (k * inputBpp), channelBitsRed, ref bppTemp);

                        alpha = forceChannelValue;
                        break;

                    case PixelDataFormat.ChannelsRgba:
                        red = ExtractChannel(rawData >> (k * inputBpp), channelBitsRed, ref bppTemp);
                        green = ExtractChannel(rawData >> (k * inputBpp), channelBitsGreen, ref bppTemp);
                        blue = ExtractChannel(rawData >> (k * inputBpp), channelBitsBlue, ref bppTemp);
                        alpha = ExtractChannel(rawData >> (k * inputBpp), channelBitsAlpha, ref bppTemp);
                        break;

                    case PixelDataFormat.ChannelsBgra:
                        blue = ExtractChannel(rawData >> (k * inputBpp), channelBitsBlue, ref bppTemp);
                        green = ExtractChannel(rawData >> (k * inputBpp), channelBitsGreen, ref bppTemp);
                        red = ExtractChannel(rawData >> (k * inputBpp), channelBitsRed, ref bppTemp);
                        alpha = ExtractChannel(rawData >> (k * inputBpp), channelBitsAlpha, ref bppTemp);
                        break;

                    case PixelDataFormat.ChannelsArgb:
                        alpha = ExtractChannel(rawData >> (k * inputBpp), channelBitsAlpha, ref bppTemp);
                        red = ExtractChannel(rawData >> (k * inputBpp), channelBitsRed, ref bppTemp);
                        green = ExtractChannel(rawData >> (k * inputBpp), channelBitsGreen, ref bppTemp);
                        blue = ExtractChannel(rawData >> (k * inputBpp), channelBitsBlue, ref bppTemp);
                        break;

                    case PixelDataFormat.ChannelsAbgr:
                        alpha = ExtractChannel(rawData >> (k * inputBpp), channelBitsAlpha, ref bppTemp);
                        blue = ExtractChannel(rawData >> (k * inputBpp), channelBitsBlue, ref bppTemp);
                        green = ExtractChannel(rawData >> (k * inputBpp), channelBitsGreen, ref bppTemp);
                        red = ExtractChannel(rawData >> (k * inputBpp), channelBitsRed, ref bppTemp);
                        break;

                    case PixelDataFormat.ChannelsRgbx:
                        red = ExtractChannel(rawData >> (k * inputBpp), channelBitsRed, ref bppTemp);
                        green = ExtractChannel(rawData >> (k * inputBpp), channelBitsGreen, ref bppTemp);
                        blue = ExtractChannel(rawData >> (k * inputBpp), channelBitsBlue, ref bppTemp);

                        ExtractChannel(rawData >> (k * inputBpp), channelBitsAlpha,
                            ref bppTemp); /* Dummy; throw away */
                        alpha = forceChannelValue;
                        break;

                    case PixelDataFormat.ChannelsBgrx:
                        blue = ExtractChannel(rawData >> (k * inputBpp), channelBitsBlue, ref bppTemp);
                        green = ExtractChannel(rawData >> (k * inputBpp), channelBitsGreen, ref bppTemp);
                        red = ExtractChannel(rawData >> (k * inputBpp), channelBitsRed, ref bppTemp);

                        ExtractChannel(rawData >> (k * inputBpp), channelBitsAlpha,
                            ref bppTemp); /* Dummy; throw away */
                        alpha = forceChannelValue;
                        break;

                    case PixelDataFormat.ChannelsXrgb:
                        ExtractChannel(rawData >> (k * inputBpp), channelBitsAlpha,
                            ref bppTemp); /* Dummy; throw away */
                        alpha = forceChannelValue;

                        red = ExtractChannel(rawData >> (k * inputBpp), channelBitsRed, ref bppTemp);
                        green = ExtractChannel(rawData >> (k * inputBpp), channelBitsGreen, ref bppTemp);
                        blue = ExtractChannel(rawData >> (k * inputBpp), channelBitsBlue, ref bppTemp);
                        break;

                    case PixelDataFormat.ChannelsXbgr:
                        ExtractChannel(rawData >> (k * inputBpp), channelBitsAlpha,
                            ref bppTemp); /* Dummy; throw away */
                        alpha = forceChannelValue;

                        blue = ExtractChannel(rawData >> (k * inputBpp), channelBitsBlue, ref bppTemp);
                        green = ExtractChannel(rawData >> (k * inputBpp), channelBitsGreen, ref bppTemp);
                        red = ExtractChannel(rawData >> (k * inputBpp), channelBitsRed, ref bppTemp);
                        break;

                    case PixelDataFormat.ChannelsLuminance:
                        red = ExtractChannel(rawData >> (k * inputBpp), channelBitsRed, ref bppTemp);

                        green = blue = red;
                        alpha = forceChannelValue;
                        break;

                    case PixelDataFormat.ChannelsAlpha:
                        red = green = blue = forceChannelValue;
                        alpha = ExtractChannel(rawData >> (k * inputBpp), channelBitsAlpha, ref bppTemp);
                        break;

                    case PixelDataFormat.ChannelsLuminanceAlpha:
                        red = ExtractChannel(rawData >> (k * inputBpp), channelBitsRed, ref bppTemp);
                        alpha = ExtractChannel(rawData >> (k * inputBpp), channelBitsAlpha, ref bppTemp);

                        green = blue = red;
                        break;

                    case PixelDataFormat.ChannelsAlphaLuminance:
                        alpha = ExtractChannel(rawData >> (k * inputBpp), channelBitsAlpha, ref bppTemp);
                        red = ExtractChannel(rawData >> (k * inputBpp), channelBitsRed, ref bppTemp);

                        green = blue = red;
                        break;

                    default: throw new Exception("Unhandled channel input layout");
                }

                int tx, ty;
                pixelOrderingFunc(x, y, PhysicalWidth, PhysicalHeight, inputPixelFormat, out tx, out ty);

                if (tx < PhysicalWidth && ty < PhysicalHeight)
                {
                    var pixelOffset = (ty * PhysicalWidth + tx) * (bitsBpp32 / 8);

                    if (isNativeLittleEndian)
                    {
                        dataArgb8888[pixelOffset + 3] = (byte)(alpha & 0xFF);
                        dataArgb8888[pixelOffset + 2] = (byte)(red & 0xFF);
                        dataArgb8888[pixelOffset + 1] = (byte)(green & 0xFF);
                        dataArgb8888[pixelOffset + 0] = (byte)(blue & 0xFF);
                    }
                    else
                    {
                        dataArgb8888[pixelOffset + 0] = (byte)(alpha & 0xFF);
                        dataArgb8888[pixelOffset + 1] = (byte)(red & 0xFF);
                        dataArgb8888[pixelOffset + 2] = (byte)(green & 0xFF);
                        dataArgb8888[pixelOffset + 3] = (byte)(blue & 0xFF);
                    }
                }

                x++;
                if (x == PhysicalWidth)
                {
                    x = 0;
                    y++;
                }
            }
        }

        return dataArgb8888;
    }

    private byte[] ApplyFilterToArgb8888(int width, int height, PixelDataFormat outputFormat, byte[] dataArgb8888)
    {
        var filter = outputFormat & PixelDataFormat.MaskFilter;
        switch (filter)
        {
            case PixelDataFormat.FilterNone:
                /* No filtering to be applied, return as-is */
                return dataArgb8888;

            case PixelDataFormat.FilterOrderedDither:
                return ApplyOrderedDithering(width, height, outputFormat, dataArgb8888,
                    Constants.DitheringBayerMatrix8x8);

            default: throw new Exception("Unimplemented filtering mode");
        }
    }

    private byte[] ApplyOrderedDithering(int width, int height, PixelDataFormat outputFormat, byte[] dataArgb8888,
        int[,] thresholdMatrix)
    {
        var outputData = new byte[dataArgb8888.Length];

        var outBpp = outputFormat & PixelDataFormat.MaskBpp;
        var outRedBits = outputFormat & PixelDataFormat.MaskRedBits;
        var outGreenBits = outputFormat & PixelDataFormat.MaskGreenBits;
        var outBlueBits = outputFormat & PixelDataFormat.MaskBlueBits;

        var inputBpp = Constants.RealBitsPerPixel[PixelDataFormat.Bpp32];
        var channelBitsRed = Constants.BitsPerChannel[outRedBits];
        var channelBitsGreen = Constants.BitsPerChannel[outGreenBits];
        var channelBitsBlue = Constants.BitsPerChannel[outBlueBits];

        int channelBitsRedInverse = 8 - channelBitsRed,
            channelBitsGreenInverse = 8 - channelBitsGreen,
            channelBitsBlueInverse = 8 - channelBitsBlue;

        var matrixMax = thresholdMatrix.Cast<int>().Max();
        var matrixWidth = thresholdMatrix.GetLength(0);
        var matrixHeight = thresholdMatrix.GetLength(1);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var pixelOffset = (y * width + x) * (inputBpp / 8);

                var thresholdR = thresholdMatrix[x % matrixWidth, y % matrixHeight] * (1 << channelBitsRedInverse) /
                                 matrixMax;
                var thresholdG = thresholdMatrix[x % matrixWidth, y % matrixHeight] * (1 << channelBitsGreenInverse) /
                                 matrixMax;
                var thresholdB = thresholdMatrix[x % matrixWidth, y % matrixHeight] * (1 << channelBitsBlueInverse) /
                                 matrixMax;

                var r = (byte)(dataArgb8888[pixelOffset + 2] + thresholdR).Clamp(byte.MinValue, byte.MaxValue);
                var g = (byte)(dataArgb8888[pixelOffset + 1] + thresholdG).Clamp(byte.MinValue, byte.MaxValue);
                var b = (byte)(dataArgb8888[pixelOffset + 0] + thresholdB).Clamp(byte.MinValue, byte.MaxValue);

                outputData[pixelOffset + 2] = (byte)(ResampleChannel(r, 8, channelBitsRed) << channelBitsRedInverse);
                outputData[pixelOffset + 1] =
                    (byte)(ResampleChannel(g, 8, channelBitsGreen) << channelBitsGreenInverse);
                outputData[pixelOffset + 0] = (byte)(ResampleChannel(b, 8, channelBitsBlue) << channelBitsBlueInverse);
                outputData[pixelOffset + 3] = dataArgb8888[pixelOffset + 3];
            }
        }

        return outputData;
    }

    private byte[] ConvertArgb8888ToOutputFormat(byte[] dataArgb8888, PixelDataFormat outputFormat, Endian outputEndian)
    {
        if (outputFormat == PixelDataFormat.FormatArgb8888 && OutputEndianness == EndianBinaryReader.NativeEndianness)
        {
            /* Already ARGB8888 and in native endianness, return as-is */
            return dataArgb8888;
        }

        var outBpp = outputFormat & PixelDataFormat.MaskBpp;
        var outChannels = outputFormat & PixelDataFormat.MaskChannels;
        var outRedBits = outputFormat & PixelDataFormat.MaskRedBits;
        var outGreenBits = outputFormat & PixelDataFormat.MaskGreenBits;
        var outBlueBits = outputFormat & PixelDataFormat.MaskBlueBits;
        var outAlphaBits = outputFormat & PixelDataFormat.MaskAlphaBits;

        var inputBpp = Constants.RealBitsPerPixel[PixelDataFormat.Bpp32];
        long pixelCount = dataArgb8888.Length / (inputBpp / 8);

        var outputBpp = Constants.RealBitsPerPixel[outBpp];
        var outputData = new byte[pixelCount * (outputBpp / 8)];

        switch (outChannels)
        {
            case PixelDataFormat.ChannelsRgb:
                CheckBitsPerChannelValidity(outRedBits, outGreenBits, outBlueBits);
                break;
            case PixelDataFormat.ChannelsBgr:
                CheckBitsPerChannelValidity(outBlueBits, outGreenBits, outRedBits);
                break;
            case PixelDataFormat.ChannelsRgba:
                CheckBitsPerChannelValidity(outRedBits, outGreenBits, outBlueBits, outAlphaBits);
                break;
            case PixelDataFormat.ChannelsBgra:
                CheckBitsPerChannelValidity(outBlueBits, outGreenBits, outRedBits, outAlphaBits);
                break;
            case PixelDataFormat.ChannelsArgb:
                CheckBitsPerChannelValidity(outAlphaBits, outRedBits, outGreenBits, outBlueBits);
                break;
            case PixelDataFormat.ChannelsAbgr:
                CheckBitsPerChannelValidity(outAlphaBits, outBlueBits, outGreenBits, outRedBits);
                break;
            case PixelDataFormat.ChannelsRgbx:
                CheckBitsPerChannelValidity(outRedBits, outGreenBits, outBlueBits, outAlphaBits);
                break;
            case PixelDataFormat.ChannelsBgrx:
                CheckBitsPerChannelValidity(outBlueBits, outGreenBits, outRedBits, outAlphaBits);
                break;
            case PixelDataFormat.ChannelsXrgb:
                CheckBitsPerChannelValidity(outAlphaBits, outRedBits, outGreenBits, outBlueBits);
                break;
            case PixelDataFormat.ChannelsXbgr:
                CheckBitsPerChannelValidity(outAlphaBits, outBlueBits, outGreenBits, outRedBits);
                break;
            case PixelDataFormat.ChannelsLuminance:
                CheckBitsPerChannelValidity(outRedBits);
                break;
            case PixelDataFormat.ChannelsAlpha:
                CheckBitsPerChannelValidity(outAlphaBits);
                break;
            case PixelDataFormat.ChannelsLuminanceAlpha:
                CheckBitsPerChannelValidity(outRedBits, outAlphaBits);
                break;
            case PixelDataFormat.ChannelsAlphaLuminance:
                CheckBitsPerChannelValidity(outAlphaBits, outRedBits);
                break;

            default: throw new Exception("Unhandled channel output layout");
        }

        var channelBitsRed = outRedBits != PixelDataFormat.Undefined ? Constants.BitsPerChannel[outRedBits] : 0;
        var channelBitsGreen = outGreenBits != PixelDataFormat.Undefined ? Constants.BitsPerChannel[outGreenBits] : 0;
        var channelBitsBlue = outBlueBits != PixelDataFormat.Undefined ? Constants.BitsPerChannel[outBlueBits] : 0;
        var channelBitsAlpha = outAlphaBits != PixelDataFormat.Undefined ? Constants.BitsPerChannel[outAlphaBits] : 0;

        var isNativeLittleEndian = EndianBinaryReader.NativeEndianness == Endian.LittleEndian;

        for (int i = 0, j = 0; i < dataArgb8888.Length; i += inputBpp / 8, j += outputBpp / 8)
        {
            uint alpha, red, green, blue;

            if (isNativeLittleEndian)
            {
                alpha = dataArgb8888[i + 3];
                red = dataArgb8888[i + 2];
                green = dataArgb8888[i + 1];
                blue = dataArgb8888[i + 0];
            }
            else
            {
                alpha = dataArgb8888[i + 0];
                red = dataArgb8888[i + 1];
                green = dataArgb8888[i + 2];
                blue = dataArgb8888[i + 3];
            }

            uint outputValue = 0;
            var bppOutCount = 0;

            switch (outChannels)
            {
                case PixelDataFormat.ChannelsRgb:
                    red = ResampleChannel(red, 8, channelBitsRed);
                    green = ResampleChannel(green, 8, channelBitsGreen);
                    blue = ResampleChannel(blue, 8, channelBitsBlue);

                    outputValue = MergeChannel(outputValue, blue, channelBitsBlue, ref bppOutCount);
                    outputValue = MergeChannel(outputValue, green, channelBitsGreen, ref bppOutCount);
                    outputValue = MergeChannel(outputValue, red, channelBitsRed, ref bppOutCount);
                    break;

                case PixelDataFormat.ChannelsBgr:
                    blue = ResampleChannel(blue, 8, channelBitsBlue);
                    green = ResampleChannel(green, 8, channelBitsGreen);
                    red = ResampleChannel(red, 8, channelBitsRed);

                    outputValue = MergeChannel(outputValue, red, channelBitsRed, ref bppOutCount);
                    outputValue = MergeChannel(outputValue, green, channelBitsGreen, ref bppOutCount);
                    outputValue = MergeChannel(outputValue, blue, channelBitsBlue, ref bppOutCount);
                    break;

                case PixelDataFormat.ChannelsRgba:
                    red = ResampleChannel(red, 8, channelBitsRed);
                    green = ResampleChannel(green, 8, channelBitsGreen);
                    blue = ResampleChannel(blue, 8, channelBitsBlue);
                    alpha = ResampleChannel(alpha, 8, channelBitsAlpha);

                    outputValue = MergeChannel(outputValue, alpha, channelBitsAlpha, ref bppOutCount);
                    outputValue = MergeChannel(outputValue, blue, channelBitsBlue, ref bppOutCount);
                    outputValue = MergeChannel(outputValue, green, channelBitsGreen, ref bppOutCount);
                    outputValue = MergeChannel(outputValue, red, channelBitsRed, ref bppOutCount);
                    break;

                case PixelDataFormat.ChannelsBgra:
                    blue = ResampleChannel(blue, 8, channelBitsBlue);
                    green = ResampleChannel(green, 8, channelBitsGreen);
                    red = ResampleChannel(red, 8, channelBitsRed);
                    alpha = ResampleChannel(alpha, 8, channelBitsAlpha);

                    outputValue = MergeChannel(outputValue, alpha, channelBitsAlpha, ref bppOutCount);
                    outputValue = MergeChannel(outputValue, red, channelBitsRed, ref bppOutCount);
                    outputValue = MergeChannel(outputValue, green, channelBitsGreen, ref bppOutCount);
                    outputValue = MergeChannel(outputValue, blue, channelBitsBlue, ref bppOutCount);
                    break;

                case PixelDataFormat.ChannelsArgb:
                    alpha = ResampleChannel(alpha, 8, channelBitsAlpha);
                    red = ResampleChannel(red, 8, channelBitsRed);
                    green = ResampleChannel(green, 8, channelBitsGreen);
                    blue = ResampleChannel(blue, 8, channelBitsBlue);

                    outputValue = MergeChannel(outputValue, blue, channelBitsBlue, ref bppOutCount);
                    outputValue = MergeChannel(outputValue, green, channelBitsGreen, ref bppOutCount);
                    outputValue = MergeChannel(outputValue, red, channelBitsRed, ref bppOutCount);
                    outputValue = MergeChannel(outputValue, alpha, channelBitsAlpha, ref bppOutCount);
                    break;

                case PixelDataFormat.ChannelsAbgr:
                    alpha = ResampleChannel(alpha, 8, channelBitsAlpha);
                    blue = ResampleChannel(blue, 8, channelBitsBlue);
                    green = ResampleChannel(green, 8, channelBitsGreen);
                    red = ResampleChannel(red, 8, channelBitsRed);

                    outputValue = MergeChannel(outputValue, red, channelBitsRed, ref bppOutCount);
                    outputValue = MergeChannel(outputValue, green, channelBitsGreen, ref bppOutCount);
                    outputValue = MergeChannel(outputValue, blue, channelBitsBlue, ref bppOutCount);
                    outputValue = MergeChannel(outputValue, alpha, channelBitsAlpha, ref bppOutCount);
                    break;

                case PixelDataFormat.ChannelsRgbx:
                    red = ResampleChannel(red, 8, channelBitsRed);
                    green = ResampleChannel(green, 8, channelBitsGreen);
                    blue = ResampleChannel(blue, 8, channelBitsBlue);
                    alpha = (uint)((1 << channelBitsAlpha) - 1);

                    outputValue = MergeChannel(outputValue, alpha, channelBitsAlpha, ref bppOutCount);
                    outputValue = MergeChannel(outputValue, blue, channelBitsBlue, ref bppOutCount);
                    outputValue = MergeChannel(outputValue, green, channelBitsGreen, ref bppOutCount);
                    outputValue = MergeChannel(outputValue, red, channelBitsRed, ref bppOutCount);
                    break;

                case PixelDataFormat.ChannelsBgrx:
                    blue = ResampleChannel(blue, 8, channelBitsBlue);
                    green = ResampleChannel(green, 8, channelBitsGreen);
                    red = ResampleChannel(red, 8, channelBitsRed);
                    alpha = (uint)((1 << channelBitsAlpha) - 1);

                    outputValue = MergeChannel(outputValue, alpha, channelBitsAlpha, ref bppOutCount);
                    outputValue = MergeChannel(outputValue, red, channelBitsRed, ref bppOutCount);
                    outputValue = MergeChannel(outputValue, green, channelBitsGreen, ref bppOutCount);
                    outputValue = MergeChannel(outputValue, blue, channelBitsBlue, ref bppOutCount);
                    break;

                case PixelDataFormat.ChannelsXrgb:
                    alpha = (uint)((1 << channelBitsAlpha) - 1);
                    red = ResampleChannel(red, 8, channelBitsRed);
                    green = ResampleChannel(green, 8, channelBitsGreen);
                    blue = ResampleChannel(blue, 8, channelBitsBlue);

                    outputValue = MergeChannel(outputValue, blue, channelBitsBlue, ref bppOutCount);
                    outputValue = MergeChannel(outputValue, green, channelBitsGreen, ref bppOutCount);
                    outputValue = MergeChannel(outputValue, red, channelBitsRed, ref bppOutCount);
                    outputValue = MergeChannel(outputValue, alpha, channelBitsAlpha, ref bppOutCount);
                    break;

                case PixelDataFormat.ChannelsXbgr:
                    alpha = (uint)((1 << channelBitsAlpha) - 1);
                    blue = ResampleChannel(blue, 8, channelBitsBlue);
                    green = ResampleChannel(green, 8, channelBitsGreen);
                    red = ResampleChannel(red, 8, channelBitsRed);

                    outputValue = MergeChannel(outputValue, red, channelBitsRed, ref bppOutCount);
                    outputValue = MergeChannel(outputValue, green, channelBitsGreen, ref bppOutCount);
                    outputValue = MergeChannel(outputValue, blue, channelBitsBlue, ref bppOutCount);
                    outputValue = MergeChannel(outputValue, alpha, channelBitsAlpha, ref bppOutCount);
                    break;

                case PixelDataFormat.ChannelsLuminance:
                    red = ResampleChannel(red, 8, channelBitsRed);

                    outputValue = MergeChannel(outputValue, red, channelBitsRed, ref bppOutCount);
                    break;

                case PixelDataFormat.ChannelsAlpha:
                    alpha = ResampleChannel(alpha, 8, channelBitsAlpha);

                    outputValue = MergeChannel(outputValue, alpha, channelBitsAlpha, ref bppOutCount);
                    break;

                case PixelDataFormat.ChannelsLuminanceAlpha:
                    red = ResampleChannel(red, 8, channelBitsRed);
                    alpha = ResampleChannel(alpha, 8, channelBitsAlpha);

                    outputValue = MergeChannel(outputValue, alpha, channelBitsAlpha, ref bppOutCount);
                    outputValue = MergeChannel(outputValue, red, channelBitsRed, ref bppOutCount);
                    break;

                case PixelDataFormat.ChannelsAlphaLuminance:
                    alpha = ResampleChannel(alpha, 8, channelBitsAlpha);
                    red = ResampleChannel(red, 8, channelBitsRed);

                    outputValue = MergeChannel(outputValue, red, channelBitsRed, ref bppOutCount);
                    outputValue = MergeChannel(outputValue, alpha, channelBitsAlpha, ref bppOutCount);
                    break;

                default: throw new Exception("Unhandled channel output layout");
            }

            byte[] outputValueBytes;

            if (outputEndian == EndianBinaryReader.NativeEndianness)
            {
                outputValueBytes = BitConverter.GetBytes(outputValue);
            }
            else
            {
                outputValueBytes = BitConverter.GetBytes(outputValue << (32 - outputBpp)).Reverse().ToArray();
            }

            Buffer.BlockCopy(outputValueBytes, 0, outputData, j, outputBpp / 8);
        }

        return outputData;
    }

    private static void CheckBitsPerChannelValidity(params PixelDataFormat[] channels)
    {
        foreach (var channel in channels)
        {
            if (!Constants.BitsPerChannel.ContainsKey(channel))
            {
                throw new Exception("Invalid bits per channel setting");
            }
        }
    }

    private static byte ResampleChannel(uint value, int sourceBits, int targetBits)
    {
        var sourceMask = (byte)((1 << sourceBits) - 1);
        var targetMask = (byte)((1 << targetBits) - 1);
        return (byte)(((value & sourceMask) * targetMask + (sourceMask >> 1)) / sourceMask);
    }

    private static uint ExtractChannel(uint rawData, int channelBits, ref int bpp)
    {
        return ResampleChannel(rawData >> (bpp -= channelBits), channelBits, 8);
    }

    private static uint MergeChannel(uint value, uint channelValue, int channelBits, ref int bpp)
    {
        var merged = value | (channelValue << bpp);
        bpp += channelBits;
        return merged;
    }

    internal static PixelOrderingDelegate GetPixelOrderingFunction(PixelDataFormat inputPixelFormat)
    {
        var pixelOrdering = inputPixelFormat & PixelDataFormat.MaskPixelOrdering;

        PixelOrderingDelegate pixelOrderingFunc;
        switch (pixelOrdering)
        {
            case PixelDataFormat.PixelOrderingLinear:
                pixelOrderingFunc = (int x, int y, int w, int h, PixelDataFormat pf, out int tx, out int ty) =>
                {
                    tx = x;
                    ty = y;
                };
                break;
            case PixelDataFormat.PixelOrderingTiled:
                pixelOrderingFunc = GetPixelCoordinatesTiled;
                break;
            case PixelDataFormat.PixelOrderingTiled3DS:
                pixelOrderingFunc = GetPixelCoordinates3DS;
                break;
            case PixelDataFormat.PixelOrderingSwizzledVita:
                pixelOrderingFunc = GetPixelCoordinatesSwizzledVita;
                break;
            case PixelDataFormat.PixelOrderingSwizzledPSP:
                pixelOrderingFunc = GetPixelCoordinatesPSP;
                break;
            case PixelDataFormat.PixelOrderingSwizzledSwitch:
                throw new Exception("Switch swizzle is unimplemented; check Tegra X1 TRM for Block Linear?");

            default: throw new Exception("Unimplemented pixel ordering mode");
        }

        return pixelOrderingFunc;
    }

    private static void GetPixelCoordinatesTiled(int origX, int origY, int width, int height,
        PixelDataFormat inputPixelFormat, out int transformedX, out int transformedY)
    {
        GetPixelCoordinatesTiledEx(origX, origY, width, height, inputPixelFormat, out transformedX, out transformedY, 8,
            8, pixelOrderingTiledDefault);
    }

    private static void GetPixelCoordinates3DS(int origX, int origY, int width, int height,
        PixelDataFormat inputPixelFormat, out int transformedX, out int transformedY)
    {
        GetPixelCoordinatesTiledEx(origX, origY, width, height, inputPixelFormat, out transformedX, out transformedY, 8,
            8, pixelOrderingTiled3DS);
    }

    private static void GetPixelCoordinatesPSP(int origX, int origY, int width, int height,
        PixelDataFormat inputPixelFormat, out int transformedX, out int transformedY)
    {
        // TODO: verify me...?

        var inBpp = inputPixelFormat & PixelDataFormat.MaskBpp;
        var bitsPerPixel = Constants.RealBitsPerPixel[inBpp];

        var tileWidth = bitsPerPixel < 8 ? 32 : 16 / (bitsPerPixel / 8);
        GetPixelCoordinatesTiledEx(origX, origY, width, height, inputPixelFormat, out transformedX, out transformedY,
            tileWidth, 8, null);
    }

    private static void GetPixelCoordinatesTiledEx(int origX, int origY, int width, int height,
        PixelDataFormat inputPixelFormat, out int transformedX, out int transformedY, int tileWidth, int tileHeight,
        int[] pixelOrdering)
    {
        // TODO: sometimes eats the last few blocks(?) in the image (ex. BC7 GNFs)

        // Sanity checks
        if (width == 0)
        {
            width = tileWidth;
        }

        if (height == 0)
        {
            height = tileHeight;
        }

        // Calculate coords in image
        var tileSize = tileWidth * tileHeight;
        var globalPixel = origY * width + origX;
        var globalX = globalPixel / tileSize * tileWidth;
        var globalY = globalX / width * tileHeight;
        globalX %= width;

        // Calculate coords in tile
        var inTileX = globalPixel % tileWidth;
        var inTileY = globalPixel / tileWidth % tileHeight;
        var inTilePixel = inTileY * tileHeight + inTileX;

        // If applicable, transform by ordering table
        if (pixelOrdering != null && tileSize <= pixelOrdering.Length)
        {
            inTileX = pixelOrdering[inTilePixel] % 8;
            inTileY = pixelOrdering[inTilePixel] / 8;
        }

        // Set final image coords
        transformedX = globalX + inTileX;
        transformedY = globalY + inTileY;
    }

    // Unswizzle logic by @FireyFly
    // http://xen.firefly.nu/up/rearrange.c.html

    private static int Compact1By1(int x)
    {
        x &= 0x55555555;                 // x = -f-e -d-c -b-a -9-8 -7-6 -5-4 -3-2 -1-0
        x = (x ^ (x >> 1)) & 0x33333333; // x = --fe --dc --ba --98 --76 --54 --32 --10
        x = (x ^ (x >> 2)) & 0x0f0f0f0f; // x = ---- fedc ---- ba98 ---- 7654 ---- 3210
        x = (x ^ (x >> 4)) & 0x00ff00ff; // x = ---- ---- fedc ba98 ---- ---- 7654 3210
        x = (x ^ (x >> 8)) & 0x0000ffff; // x = ---- ---- ---- ---- fedc ba98 7654 3210
        return x;
    }

    private static int DecodeMorton2X(int code)
    {
        return Compact1By1(code >> 0);
    }

    private static int DecodeMorton2Y(int code)
    {
        return Compact1By1(code >> 1);
    }

    private static void GetPixelCoordinatesSwizzledVita(int origX, int origY, int width, int height,
        PixelDataFormat inputPixelFormat, out int transformedX, out int transformedY)
    {
        // TODO: verify this is even sensible
        if (width == 0)
        {
            width = 16;
        }

        if (height == 0)
        {
            height = 16;
        }

        var i = origY * width + origX;

        var min = width < height ? width : height;
        var k = (int)Math.Log(min, 2);

        if (height < width)
        {
            // XXXyxyxyx → XXXxxxyyy
            var j = ((i >> (2 * k)) << (2 * k))
                    | ((DecodeMorton2Y(i) & (min - 1)) << k)
                    | ((DecodeMorton2X(i) & (min - 1)) << 0);
            transformedX = j / height;
            transformedY = j % height;
        }
        else
        {
            // YYYyxyxyx → YYYyyyxxx
            var j = ((i >> (2 * k)) << (2 * k))
                    | ((DecodeMorton2X(i) & (min - 1)) << k)
                    | ((DecodeMorton2Y(i) & (min - 1)) << 0);
            transformedX = j % width;
            transformedY = j / width;
        }
    }
}