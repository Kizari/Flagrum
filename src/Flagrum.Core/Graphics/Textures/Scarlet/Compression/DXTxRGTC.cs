using System;
using Scarlet.IO;

namespace Scarlet.Drawing.Compression;
/* DXT1 (BC1), DXT3 (BC2), DXT5 (BC3) */
/* https://www.khronos.org/registry/OpenGL/extensions/EXT/EXT_texture_compression_s3tc.txt */

/* RGTC1 (BC4), RGTC2 (BC5) */
/* https://www.khronos.org/registry/OpenGL/extensions/EXT/EXT_texture_compression_rgtc.txt */

internal enum DXTxRGTCBlockLayout
{
    Normal,
    PSP
}

internal enum DXTxRGTCSignedness
{
    Unsigned,
    Signed
}

internal delegate byte[] DXTxRGTCBlockDecoderDelegate(EndianBinaryReader reader, PixelDataFormat inputFormat,
    DXTxRGTCBlockLayout blockLayout, DXTxRGTCSignedness signedness);

internal static class DXTxRGTC
{
    public static byte[] Decompress(EndianBinaryReader reader, int width, int height, PixelDataFormat inputFormat,
        long readLength)
    {
        var outPixels = new byte[readLength * 8];

        var pixelOrderingFunc = ImageBinary.GetPixelOrderingFunction(inputFormat & PixelDataFormat.MaskPixelOrdering);

        DXTxRGTCBlockDecoderDelegate blockDecoder;
        DXTxRGTCBlockLayout blockLayout;
        DXTxRGTCSignedness signedness;

        var specialFormat = inputFormat & PixelDataFormat.MaskSpecial;
        switch (specialFormat)
        {
            case PixelDataFormat.SpecialFormatDXT1:
                blockDecoder = DecodeDXT1Block;
                blockLayout = DXTxRGTCBlockLayout.Normal;
                signedness = DXTxRGTCSignedness.Unsigned;
                break;
            case PixelDataFormat.SpecialFormatDXT3:
                blockDecoder = DecodeDXT3Block;
                blockLayout = DXTxRGTCBlockLayout.Normal;
                signedness = DXTxRGTCSignedness.Unsigned;
                break;
            case PixelDataFormat.SpecialFormatDXT5:
                blockDecoder = DecodeDXT5Block;
                blockLayout = DXTxRGTCBlockLayout.Normal;
                signedness = DXTxRGTCSignedness.Unsigned;
                break;
            case PixelDataFormat.SpecialFormatRGTC1:
                blockDecoder = DecodeRGTC1Block;
                blockLayout = DXTxRGTCBlockLayout.Normal;
                signedness = DXTxRGTCSignedness.Unsigned;
                break;
            case PixelDataFormat.SpecialFormatRGTC1_Signed:
                blockDecoder = DecodeRGTC1Block;
                blockLayout = DXTxRGTCBlockLayout.PSP;
                signedness = DXTxRGTCSignedness.Signed;
                break;
            case PixelDataFormat.SpecialFormatRGTC2:
                blockDecoder = DecodeRGTC2Block;
                blockLayout = DXTxRGTCBlockLayout.Normal;
                signedness = DXTxRGTCSignedness.Unsigned;
                break;
            case PixelDataFormat.SpecialFormatRGTC2_Signed:
                blockDecoder = DecodeRGTC2Block;
                blockLayout = DXTxRGTCBlockLayout.PSP;
                signedness = DXTxRGTCSignedness.Signed;
                break;

            case PixelDataFormat.SpecialFormatDXT1_PSP:
                blockDecoder = DecodeDXT1Block;
                blockLayout = DXTxRGTCBlockLayout.PSP;
                signedness = DXTxRGTCSignedness.Unsigned;
                break;
            case PixelDataFormat.SpecialFormatDXT3_PSP:
                blockDecoder = DecodeDXT3Block;
                blockLayout = DXTxRGTCBlockLayout.PSP;
                signedness = DXTxRGTCSignedness.Unsigned;
                break;
            case PixelDataFormat.SpecialFormatDXT5_PSP:
                blockDecoder = DecodeDXT5Block;
                blockLayout = DXTxRGTCBlockLayout.PSP;
                signedness = DXTxRGTCSignedness.Unsigned;
                break;

            default:
                throw new Exception("Trying to decode DXT/RGTC with format set to non-DXT/RGTC");
        }

        for (var y = 0; y < height; y += 4)
        {
            for (var x = 0; x < width; x += 4)
            {
                var decompressedBlock = blockDecoder(reader, inputFormat, blockLayout, signedness);

                int rx, ry;
                pixelOrderingFunc(x / 4, y / 4, width / 4, height / 4, inputFormat, out rx, out ry);
                rx *= 4;
                ry *= 4;

                for (var py = 0; py < 4; py++)
                {
                    for (var px = 0; px < 4; px++)
                    {
                        var ix = rx + px;
                        var iy = ry + py;

                        if (ix >= width || iy >= height)
                        {
                            continue;
                        }

                        for (var c = 0; c < 4; c++)
                        {
                            outPixels[(iy * width + ix) * 4 + c] = decompressedBlock[(py * 4 + px) * 4 + c];
                        }
                    }
                }
            }
        }

        return outPixels;
    }

    /// <summary>
    /// Decodes the given block
    /// </summary>
    /// <param name="inBlock">Block to decode</param>
    /// <param name="has1bitAlpha">Set if block contains DXT1 1-bit alpha (COMPRESSED_RGBA_S3TC_DXT1_EXT)</param>
    /// <param name="isDXT1">Set if block should be decoded as DXT1, else it will be decoded as DXT3/5</param>
    /// <returns></returns>
    private static byte[] DecodeColorBlock(DXT1Block inBlock, bool has1bitAlpha, bool isDXT1)
    {
        var outData = new byte[4 * 4 * 4];
        var colors = new byte[4, 4];

        UnpackRgb565(inBlock.Color0, out colors[0, 2], out colors[0, 1], out colors[0, 0]);
        UnpackRgb565(inBlock.Color1, out colors[1, 2], out colors[1, 1], out colors[1, 0]);
        colors[0, 3] = 255;
        colors[1, 3] = 255;

        if (isDXT1 && inBlock.Color0 <= inBlock.Color1)
        {
            colors[2, 0] = (byte)((colors[0, 0] + colors[1, 0]) / 2);
            colors[2, 1] = (byte)((colors[0, 1] + colors[1, 1]) / 2);
            colors[2, 2] = (byte)((colors[0, 2] + colors[1, 2]) / 2);
            colors[2, 3] = 255;

            colors[3, 0] = 0;
            colors[3, 1] = 0;
            colors[3, 2] = 0;
            colors[3, 3] = (byte)(has1bitAlpha && inBlock.Color0 <= inBlock.Color1 ? 0 : 0xFF);
        }
        else
        {
            colors[2, 0] = (byte)((2 * colors[0, 0] + colors[1, 0]) / 3);
            colors[2, 1] = (byte)((2 * colors[0, 1] + colors[1, 1]) / 3);
            colors[2, 2] = (byte)((2 * colors[0, 2] + colors[1, 2]) / 3);
            colors[2, 3] = 255;

            colors[3, 0] = (byte)((colors[0, 0] + 2 * colors[1, 0]) / 3);
            colors[3, 1] = (byte)((colors[0, 1] + 2 * colors[1, 1]) / 3);
            colors[3, 2] = (byte)((colors[0, 2] + 2 * colors[1, 2]) / 3);
            colors[3, 3] = 255;
        }

        for (var by = 0; by < 4; by++)
        {
            for (var bx = 0; bx < 4; bx++)
            {
                var code = inBlock.Bits[by * 4 + bx];
                for (var c = 0; c < 4; c++)
                {
                    outData[(by * 4 + bx) * 4 + c] = colors[code, c];
                }
            }
        }

        return outData;
    }

    private static byte[] DecodeDXT1Block(EndianBinaryReader reader, PixelDataFormat inputFormat,
        DXTxRGTCBlockLayout blockLayout, DXTxRGTCSignedness signedness)
    {
        var inBlock = new DXT1Block(reader, blockLayout);
        return DecodeColorBlock(inBlock, (inputFormat & PixelDataFormat.MaskChannels) != PixelDataFormat.ChannelsRgb,
            true);
    }

    private static byte[] DecodeDXT3Block(EndianBinaryReader reader, PixelDataFormat inputFormat,
        DXTxRGTCBlockLayout blockLayout, DXTxRGTCSignedness signedness)
    {
        var inBlock = new DXT3Block(reader, blockLayout);
        var outData = DecodeColorBlock(inBlock.Color, false, false);

        var alpha = inBlock.Alpha;
        for (var i = 0; i < outData.Length; i += 4)
        {
            outData[i + 3] = (byte)(((alpha & 0xF) << 4) | (alpha & 0xF));
            alpha >>= 4;
        }

        return outData;
    }

    private static byte[] DecodeDXT5Block(EndianBinaryReader reader, PixelDataFormat inputFormat,
        DXTxRGTCBlockLayout blockLayout, DXTxRGTCSignedness signedness)
    {
        var inBlock = new DXT5Block(reader, blockLayout);
        var outData = DecodeColorBlock(inBlock.Color, false, false);

        for (var y = 0; y < 4; y++)
        {
            for (var x = 0; x < 4; x++)
            {
                var code = inBlock.Bits[y * 4 + x];
                var destOffset = (y * 4 + x) * 4 + 3;

                if (inBlock.Alpha0 > inBlock.Alpha1)
                {
                    switch (code)
                    {
                        case 0x00:
                            outData[destOffset] = inBlock.Alpha0;
                            break;
                        case 0x01:
                            outData[destOffset] = inBlock.Alpha1;
                            break;
                        case 0x02:
                            outData[destOffset] = (byte)((6 * inBlock.Alpha0 + 1 * inBlock.Alpha1) / 7);
                            break;
                        case 0x03:
                            outData[destOffset] = (byte)((5 * inBlock.Alpha0 + 2 * inBlock.Alpha1) / 7);
                            break;
                        case 0x04:
                            outData[destOffset] = (byte)((4 * inBlock.Alpha0 + 3 * inBlock.Alpha1) / 7);
                            break;
                        case 0x05:
                            outData[destOffset] = (byte)((3 * inBlock.Alpha0 + 4 * inBlock.Alpha1) / 7);
                            break;
                        case 0x06:
                            outData[destOffset] = (byte)((2 * inBlock.Alpha0 + 5 * inBlock.Alpha1) / 7);
                            break;
                        case 0x07:
                            outData[destOffset] = (byte)((1 * inBlock.Alpha0 + 6 * inBlock.Alpha1) / 7);
                            break;
                    }
                }
                else
                {
                    switch (code)
                    {
                        case 0x00:
                            outData[destOffset] = inBlock.Alpha0;
                            break;
                        case 0x01:
                            outData[destOffset] = inBlock.Alpha1;
                            break;
                        case 0x02:
                            outData[destOffset] = (byte)((4 * inBlock.Alpha0 + 1 * inBlock.Alpha1) / 5);
                            break;
                        case 0x03:
                            outData[destOffset] = (byte)((3 * inBlock.Alpha0 + 2 * inBlock.Alpha1) / 5);
                            break;
                        case 0x04:
                            outData[destOffset] = (byte)((2 * inBlock.Alpha0 + 3 * inBlock.Alpha1) / 5);
                            break;
                        case 0x05:
                            outData[destOffset] = (byte)((1 * inBlock.Alpha0 + 4 * inBlock.Alpha1) / 5);
                            break;
                        case 0x06:
                            outData[destOffset] = 0x00;
                            break;
                        case 0x07:
                            outData[destOffset] = 0xFF;
                            break;
                    }
                }
            }
        }

        return outData;
    }

    // TODO: verify channel order! ImageBinary works on ARGB (BGRA b/c endianness), so this *should* be correct now

    private static byte[] DecodeRGTC1Block(EndianBinaryReader reader, PixelDataFormat inputFormat,
        DXTxRGTCBlockLayout blockLayout, DXTxRGTCSignedness signedness)
    {
        var inBlock = new RGTCBlock(reader, blockLayout);
        var outData = new byte[4 * 4 * 4];

        for (var y = 0; y < 4; y++)
        {
            for (var x = 0; x < 4; x++)
            {
                var destOffset = (y * 4 + x) * 4;
                outData[destOffset + 0] = 0x00;
                outData[destOffset + 1] = 0x00;
                outData[destOffset + 2] = DecodeRGTCValue(inBlock, x, y, signedness);
                outData[destOffset + 3] = 0xFF;
            }
        }

        return outData;
    }

    private static byte[] DecodeRGTC2Block(EndianBinaryReader reader, PixelDataFormat inputFormat,
        DXTxRGTCBlockLayout blockLayout, DXTxRGTCSignedness signedness)
    {
        var inBlockRed = new RGTCBlock(reader, blockLayout);
        var inBlockGreen = new RGTCBlock(reader, blockLayout);
        var outData = new byte[4 * 4 * 4];

        for (var y = 0; y < 4; y++)
        {
            for (var x = 0; x < 4; x++)
            {
                var destOffset = (y * 4 + x) * 4;
                outData[destOffset + 0] = 0x00;
                outData[destOffset + 1] = DecodeRGTCValue(inBlockGreen, x, y, signedness);
                outData[destOffset + 2] = DecodeRGTCValue(inBlockRed, x, y, signedness);
                outData[destOffset + 3] = 0xFF;
            }
        }

        return outData;
    }

    private static byte DecodeRGTCValue(RGTCBlock inBlock, int x, int y, DXTxRGTCSignedness signedness)
    {
        int code = inBlock.Bits[y * 4 + x];

        byte data0, data1;
        if (signedness == DXTxRGTCSignedness.Unsigned)
        {
            data0 = inBlock.Data0;
            data1 = inBlock.Data1;
        }
        else
        {
            data0 = (byte)((sbyte)inBlock.Data0 + 128);
            data1 = (byte)((sbyte)inBlock.Data1 + 128);
        }

        if (data0 > data1)
        {
            switch (code)
            {
                case 0x00: return data0;
                case 0x01: return data1;
                case 0x02: return (byte)((6 * data0 + 1 * data1) / 7);
                case 0x03: return (byte)((5 * data0 + 2 * data1) / 7);
                case 0x04: return (byte)((4 * data0 + 3 * data1) / 7);
                case 0x05: return (byte)((3 * data0 + 4 * data1) / 7);
                case 0x06: return (byte)((2 * data0 + 5 * data1) / 7);
                case 0x07: return (byte)((1 * data0 + 6 * data1) / 7);
            }
        }
        else
        {
            switch (code)
            {
                case 0x00: return data0;
                case 0x01: return data1;
                case 0x02: return (byte)((4 * data0 + 1 * data1) / 5);
                case 0x03: return (byte)((3 * data0 + 2 * data1) / 5);
                case 0x04: return (byte)((2 * data0 + 3 * data1) / 5);
                case 0x05: return (byte)((1 * data0 + 4 * data1) / 5);
                case 0x06: return 0x00;
                case 0x07: return 0xFF;
            }
        }

        throw new Exception("RGTC value decode exception; this shouldn't happen!");
    }

    public static byte[] ExtractBits(ulong bits, int numBits)
    {
        var bitsExt = new byte[16];
        for (var i = 0; i < bitsExt.Length; i++)
        {
            bitsExt[i] = (byte)((bits >> (i * numBits)) & (byte)((1 << numBits) - 1));
        }

        return bitsExt;
    }

    private static void UnpackRgb565(ushort rgb565, out byte r, out byte g, out byte b)
    {
        r = (byte)((rgb565 & 0xF800) >> 11);
        r = (byte)((r << 3) | (r >> 2));
        g = (byte)((rgb565 & 0x07E0) >> 5);
        g = (byte)((g << 2) | (g >> 4));
        b = (byte)(rgb565 & 0x1F);
        b = (byte)((b << 3) | (b >> 2));
    }
}

internal class DXT1Block
{
    public DXT1Block(EndianBinaryReader reader, DXTxRGTCBlockLayout blockLayout)
    {
        byte color0_hi, color0_lo, color1_hi, color1_lo, bits_3, bits_2, bits_1, bits_0;

        switch (blockLayout)
        {
            case DXTxRGTCBlockLayout.Normal:
                color0_hi = reader.ReadByte();
                color0_lo = reader.ReadByte();
                color1_hi = reader.ReadByte();
                color1_lo = reader.ReadByte();
                bits_3 = reader.ReadByte();
                bits_2 = reader.ReadByte();
                bits_1 = reader.ReadByte();
                bits_0 = reader.ReadByte();
                break;

            case DXTxRGTCBlockLayout.PSP:
                bits_3 = reader.ReadByte();
                bits_2 = reader.ReadByte();
                bits_1 = reader.ReadByte();
                bits_0 = reader.ReadByte();
                color0_hi = reader.ReadByte();
                color0_lo = reader.ReadByte();
                color1_hi = reader.ReadByte();
                color1_lo = reader.ReadByte();
                break;

            default:
                throw new Exception("Unknown block layout");
        }

        Bits = DXTxRGTC.ExtractBits(((uint)bits_0 << 24) | ((uint)bits_1 << 16) | ((uint)bits_2 << 8) | bits_3, 2);
        Color0 = (ushort)((color0_lo << 8) | color0_hi);
        Color1 = (ushort)((color1_lo << 8) | color1_hi);
    }

    public ushort Color0 { get; }
    public ushort Color1 { get; }
    public byte[] Bits { get; }
}

internal class DXT3Block
{
    public DXT3Block(EndianBinaryReader reader, DXTxRGTCBlockLayout blockLayout)
    {
        switch (blockLayout)
        {
            case DXTxRGTCBlockLayout.Normal:
                Alpha = reader.ReadUInt64();
                Color = new DXT1Block(reader, blockLayout);
                break;

            case DXTxRGTCBlockLayout.PSP:
                Color = new DXT1Block(reader, blockLayout);
                Alpha = reader.ReadUInt64();
                break;

            default:
                throw new Exception("Unknown block layout");
        }
    }

    public ulong Alpha { get; }
    public DXT1Block Color { get; }
}

internal class DXT5Block
{
    public DXT5Block(EndianBinaryReader reader, DXTxRGTCBlockLayout blockLayout)
    {
        byte bits_5, bits_4, bits_3, bits_2, bits_1, bits_0;

        switch (blockLayout)
        {
            case DXTxRGTCBlockLayout.Normal:
                Alpha0 = reader.ReadByte();
                Alpha1 = reader.ReadByte();

                bits_5 = reader.ReadByte();
                bits_4 = reader.ReadByte();
                bits_3 = reader.ReadByte();
                bits_2 = reader.ReadByte();
                bits_1 = reader.ReadByte();
                bits_0 = reader.ReadByte();
                Bits = DXTxRGTC.ExtractBits(
                    ((ulong)bits_0 << 40) | ((ulong)bits_1 << 32) | ((ulong)bits_2 << 24) | ((ulong)bits_3 << 16) |
                    ((ulong)bits_4 << 8) | bits_5, 3);

                Color = new DXT1Block(reader, blockLayout);
                break;

            case DXTxRGTCBlockLayout.PSP:
                Color = new DXT1Block(reader, blockLayout);
                Alpha0 = reader.ReadByte();
                Alpha1 = reader.ReadByte();

                bits_5 = reader.ReadByte();
                bits_4 = reader.ReadByte();
                bits_3 = reader.ReadByte();
                bits_2 = reader.ReadByte();
                bits_1 = reader.ReadByte();
                bits_0 = reader.ReadByte();
                Bits = DXTxRGTC.ExtractBits(
                    ((ulong)bits_0 << 40) | ((ulong)bits_1 << 32) | ((ulong)bits_2 << 24) | ((ulong)bits_3 << 16) |
                    ((ulong)bits_4 << 8) | bits_5, 3);
                break;

            default:
                throw new Exception("Unknown block layout");
        }
    }

    public byte Alpha0 { get; }
    public byte Alpha1 { get; }
    public byte[] Bits { get; }
    public DXT1Block Color { get; }
}

internal class RGTCBlock
{
    public RGTCBlock(EndianBinaryReader reader, DXTxRGTCBlockLayout blockLayout)
    {
        byte bits_5, bits_4, bits_3, bits_2, bits_1, bits_0;

        switch (blockLayout)
        {
            case DXTxRGTCBlockLayout.Normal:
                Data0 = reader.ReadByte();
                Data1 = reader.ReadByte();

                bits_5 = reader.ReadByte();
                bits_4 = reader.ReadByte();
                bits_3 = reader.ReadByte();
                bits_2 = reader.ReadByte();
                bits_1 = reader.ReadByte();
                bits_0 = reader.ReadByte();
                Bits = DXTxRGTC.ExtractBits(
                    ((ulong)bits_0 << 40) | ((ulong)bits_1 << 32) | ((ulong)bits_2 << 24) | ((ulong)bits_3 << 16) |
                    ((ulong)bits_4 << 8) | bits_5, 3);
                break;

            case DXTxRGTCBlockLayout.PSP:
                Data0 = reader.ReadByte();
                Data1 = reader.ReadByte();

                bits_5 = reader.ReadByte();
                bits_4 = reader.ReadByte();
                bits_3 = reader.ReadByte();
                bits_2 = reader.ReadByte();
                bits_1 = reader.ReadByte();
                bits_0 = reader.ReadByte();
                Bits = DXTxRGTC.ExtractBits(
                    ((ulong)bits_0 << 40) | ((ulong)bits_1 << 32) | ((ulong)bits_2 << 24) | ((ulong)bits_3 << 16) |
                    ((ulong)bits_4 << 8) | bits_5, 3);
                break;

            default:
                throw new Exception("Unknown block layout");
        }
    }

    public byte Data0 { get; }
    public byte Data1 { get; }
    public byte[] Bits { get; }
}