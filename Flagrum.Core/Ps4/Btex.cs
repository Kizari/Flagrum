using System;
using System.Drawing;
using System.IO;
using System.Text;
using Flagrum.Core.Gfxbin.Btex;
using Flagrum.Core.Utilities;
using Scarlet.Drawing;

namespace Flagrum.Core.Ps4;

public enum GnfDataFormat : byte
{
    FormatInvalid = 0x0,
    Format8 = 0x1,
    Format16 = 0x2,
    Format8_8 = 0x3,
    Format32 = 0x4,
    Format16_16 = 0x5,
    Format10_11_11 = 0x6,
    Format11_11_10 = 0x7,
    Format10_10_10_2 = 0x8,
    Format2_10_10_10 = 0x9,
    Format8_8_8_8 = 0xa,
    Format32_32 = 0xb,
    Format16_16_16_16 = 0xc,
    Format32_32_32 = 0xd,
    Format32_32_32_32 = 0xe,
    FormatReserved_15 = 0xf,
    Format5_6_5 = 0x10,
    Format1_5_5_5 = 0x11,
    Format5_5_5_1 = 0x12,
    Format4_4_4_4 = 0x13,
    Format8_24 = 0x14,
    Format24_8 = 0x15,
    FormatX24_8_32 = 0x16,
    FormatReserved_23 = 0x17,
    FormatReserved_24 = 0x18,
    FormatReserved_25 = 0x19,
    FormatReserved_26 = 0x1a,
    FormatReserved_27 = 0x1b,
    FormatReserved_28 = 0x1c,
    FormatReserved_29 = 0x1d,
    FormatReserved_30 = 0x1e,
    FormatReserved_31 = 0x1f,
    FormatGB_GR = 0x20,
    FormatBG_RG = 0x21,
    Format5_9_9_9 = 0x22,
    FormatBC1 = 0x23,
    FormatBC2 = 0x24,
    FormatBC3 = 0x25,
    FormatBC4 = 0x26,
    FormatBC5 = 0x27,
    FormatBC6 = 0x28,
    FormatBC7 = 0x29,
    FormatReserved_42 = 0x2a,
    FormatReserved_43 = 0x2b,
    FormatFMask8_S2_F1 = 0x2c,
    FormatFMask8_S4_F1 = 0x2d,
    FormatFMask8_S8_F1 = 0x2e,
    FormatFMask8_S2_F2 = 0x2f,
    FormatFMask8_S4_F2 = 0x30,
    FormatFMask8_S4_F4 = 0x31,
    FormatFMask16_S16_F1 = 0x32,
    FormatFMask16_S8_F2 = 0x33,
    FormatFMask32_S16_F2 = 0x34,
    FormatFMask32_S8_F4 = 0x35,
    FormatFMask32_S8_F8 = 0x36,
    FormatFMask64_S16_F4 = 0x37,
    FormatFMask64_S16_F8 = 0x38,
    Format4_4 = 0x39,
    Format6_5_5 = 0x3a,
    Format1 = 0x3b,
    Format1_Reversed = 0x3c,
    Format32_AS_8 = 0x3d,
    Format32_AS_8_8 = 0x3e,
    Format32_AS_32_32_32_32 = 0x3f
}

public enum GnfNumFormat
{
    FormatUNorm = 0x0,
    FormatSNorm = 0x1,
    FormatUScaled = 0x2,
    FormatSScaled = 0x3,
    FormatUInt = 0x4,
    FormatSInt = 0x5,
    FormatSNorm_OGL = 0x6,
    FormatFloat = 0x7,
    FormatReserved_8 = 0x8,
    FormatSRGB = 0x9,
    FormatUBNorm = 0xa,
    FormatUBNorm_OGL = 0xb,
    FormatUBInt = 0xc,
    FormatUBScaled = 0xd,
    FormatReserved_14 = 0xe,
    FormatReserved_15 = 0xf
}

public enum GnfSqSel : byte
{
    Sel0 = 0x0,
    Sel1 = 0x1,
    SelReserved_0 = 0x2,
    SelReserved_1 = 0x3,
    SelX = 0x4,
    SelY = 0x5,
    SelZ = 0x6,
    SelW = 0x7
}

[Flags]
public enum BtexFlags : byte
{
    FLAG_COMPOSITED_IMAGE = 1,
    FLAG_SHARE_TEXTURE = 2,
    FLAG_REFERENCE_TEXTURE = 4
}

public enum BtexPlatform : byte
{
    PLATFORM_WIN = 0,
    PLATFORM_X360 = 1,
    PLATFORM_PS3 = 2,
    PLATFORM_WII = 3,
    PLATFORM_PSP2 = 4,
    PLATFORM_DX11 = 5,
    PLATFORM_WIIU = 6,
    PLATFORM_IOS = 7,
    PLATFORM_ANDROID = 8
}

public class SedbHeader
{
    public char[] Magic { get; set; }
    public long Version { get; set; }
    public long FileSize { get; set; }
}

public class BtexHeader
{
    public char[] Magic { get; set; }
    public ulong HeaderSize { get; set; }
    public ulong ImageDataSize { get; set; }
    public ushort Version { get; set; }
    public BtexPlatform Platform { get; set; }
    public BtexFlags Flags { get; set; }
    public ushort ImageCount { get; set; }
    public ushort ImageHeaderStride { get; set; }
    public uint ImageHeaderOffset { get; set; }
}

public class BtexImageData
{
    public ushort Width { get; set; }
    public ushort Height { get; set; }
    public ushort Pitch { get; set; }
    public BtexFormat Format { get; set; }
    public byte MipCount { get; set; }
    public byte Depth { get; set; }
    public byte DimensionsCount { get; set; }
    public byte ImageFlags { get; set; }
    public ushort SurfaceCount { get; set; }
    public ushort SurfaceHeaderStride { get; set; }
    public int PlatformDataOffset { get; set; }
    public int SurfaceHeaderOffset { get; set; }
    public int NameOffset { get; set; }
    public int PlatformDataSize { get; set; }
    public uint HighMipCount { get; set; }
    public uint HighBtexSize { get; set; }
    public uint TileMode { get; set; }
    public uint ArrayCount { get; set; }
    public string Name { get; set; }
}

public class GnfHeader
{
    public string Magic { get; set; }
    public uint Unknown0x04 { get; set; }
    public uint Unknown0x08 { get; set; }
    public uint FileSize { get; set; }
    public uint Unknown0x10 { get; set; }
    public uint ImageInformation1 { get; set; }
    public uint ImageInformation2 { get; set; }
    public uint ImageInformation3 { get; set; }
    public uint ImageInformation4 { get; set; }
    public uint Unknown0x24 { get; set; }
    public uint Unknown0x28 { get; set; }
    public uint DataSize { get; set; }

    public GnfDataFormat DataFormat { get; set; }
    public GnfNumFormat NumFormat { get; set; }
    public uint Width { get; set; }
    public uint Height { get; set; }
    public uint Depth { get; set; }
    public uint Pitch { get; set; }
    public GnfSqSel DestinationX { get; set; }
    public GnfSqSel DestinationY { get; set; }
    public GnfSqSel DestinationZ { get; set; }
    public GnfSqSel DestinationW { get; set; }
}

public class Btex
{
    private Btex()
    {
        FormatMap.Add(DxgiFormat.BC6H_UF16, BtexFormat.BC6H_UF16);
        FormatMap.Add(DxgiFormat.BC1_UNORM, BtexFormat.BC1_UNORM);
        FormatMap.Add(DxgiFormat.BC2_UNORM, BtexFormat.BC2_UNORM);
        FormatMap.Add(DxgiFormat.BC3_UNORM, BtexFormat.BC3_UNORM);
        FormatMap.Add(DxgiFormat.BC4_UNORM, BtexFormat.BC4_UNORM);
        FormatMap.Add(DxgiFormat.BC5_UNORM, BtexFormat.BC5_UNORM);
        FormatMap.Add(DxgiFormat.BC7_UNORM, BtexFormat.BC7_UNORM);
        FormatMap.Add(DxgiFormat.R8G8B8A8_UNORM, BtexFormat.R8G8B8A8_UNORM);
        FormatMap.Add(DxgiFormat.B8G8R8A8_UNORM, BtexFormat.B8G8R8A8_UNORM);
    }

    private FallbackMap<DxgiFormat, BtexFormat> FormatMap { get; } = new(DxgiFormat.BC1_UNORM, BtexFormat.BC1_UNORM);

    public SedbHeader SedbHeader { get; set; } = new();
    public BtexHeader BtexHeader { get; set; } = new();
    public BtexImageData ImageData { get; set; } = new();
    public GnfHeader GnfHeader { get; set; } = new();
    public byte[] DdsData { get; set; }
    public Bitmap Bitmap { get; set; }

    public static Btex FromData(byte[] data)
    {
        var btex = new Btex();
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);

        // Read the SEDB header
        btex.SedbHeader.Magic = reader.ReadChars(8);
        btex.SedbHeader.Version = reader.ReadInt64();
        btex.SedbHeader.FileSize = reader.ReadInt64();
        stream.Align(256);

        // Read the Btex header
        btex.BtexHeader.Magic = reader.ReadChars(4);
        btex.BtexHeader.HeaderSize = reader.ReadUInt32();
        btex.BtexHeader.ImageDataSize = reader.ReadUInt32();
        btex.BtexHeader.Version = reader.ReadUInt16();
        btex.BtexHeader.Platform = (BtexPlatform)reader.ReadByte();
        btex.BtexHeader.Flags = (BtexFlags)reader.ReadByte();
        btex.BtexHeader.ImageCount = reader.ReadUInt16();
        btex.BtexHeader.ImageHeaderStride = reader.ReadUInt16();
        btex.BtexHeader.ImageHeaderOffset = reader.ReadUInt32();
        reader.ReadUInt64(); // Padding

        // Read the image data
        btex.ImageData.Width = reader.ReadUInt16();
        btex.ImageData.Height = reader.ReadUInt16();
        btex.ImageData.Pitch = reader.ReadUInt16();
        btex.ImageData.Format = (BtexFormat)reader.ReadUInt16();
        btex.ImageData.MipCount = reader.ReadByte();
        btex.ImageData.Depth = reader.ReadByte();
        btex.ImageData.DimensionsCount = reader.ReadByte();
        btex.ImageData.ImageFlags = reader.ReadByte();
        btex.ImageData.SurfaceCount = reader.ReadUInt16();
        btex.ImageData.SurfaceHeaderStride = reader.ReadUInt16();
        btex.ImageData.PlatformDataOffset = reader.ReadInt32();
        btex.ImageData.SurfaceHeaderOffset = reader.ReadInt32();
        btex.ImageData.NameOffset = reader.ReadInt32();
        btex.ImageData.PlatformDataSize = reader.ReadInt32();
        btex.ImageData.HighMipCount = reader.ReadUInt32();
        btex.ImageData.HighBtexSize = reader.ReadUInt32();
        reader.ReadUInt64(); // Padding
        btex.ImageData.TileMode = reader.ReadUInt32();
        btex.ImageData.ArrayCount = reader.ReadUInt32();
        btex.ImageData.Name = reader.ReadString();
        stream.Align(256);

        // Read the GNF header
        btex.GnfHeader.Magic = Encoding.UTF8.GetString(reader.ReadBytes(4));
        btex.GnfHeader.Unknown0x04 = reader.ReadUInt32();
        btex.GnfHeader.Unknown0x08 = reader.ReadUInt32();
        btex.GnfHeader.FileSize = reader.ReadUInt32();
        btex.GnfHeader.Unknown0x10 = reader.ReadUInt32();
        btex.GnfHeader.ImageInformation1 = reader.ReadUInt32();
        btex.GnfHeader.ImageInformation2 = reader.ReadUInt32();
        btex.GnfHeader.ImageInformation3 = reader.ReadUInt32();
        btex.GnfHeader.ImageInformation4 = reader.ReadUInt32();
        btex.GnfHeader.Unknown0x24 = reader.ReadUInt32();
        btex.GnfHeader.Unknown0x28 = reader.ReadUInt32();
        btex.GnfHeader.DataSize = reader.ReadUInt32();
        stream.Align(256);

        // Extract the data from the GNF header
        btex.GnfHeader.DataFormat = (GnfDataFormat)ExtractData(btex.GnfHeader.ImageInformation1, 20, 25);
        btex.GnfHeader.NumFormat = (GnfNumFormat)ExtractData(btex.GnfHeader.ImageInformation1, 26, 29);
        btex.GnfHeader.Width = ExtractData(btex.GnfHeader.ImageInformation2, 0, 13) + 1;
        btex.GnfHeader.Height = ExtractData(btex.GnfHeader.ImageInformation2, 14, 27) + 1;
        btex.GnfHeader.Depth = ExtractData(btex.GnfHeader.ImageInformation4, 0, 12);
        btex.GnfHeader.Pitch = ExtractData(btex.GnfHeader.ImageInformation4, 13, 26) + 1;
        btex.GnfHeader.DestinationX = (GnfSqSel)ExtractData(btex.GnfHeader.ImageInformation3, 0, 2);
        btex.GnfHeader.DestinationY = (GnfSqSel)ExtractData(btex.GnfHeader.ImageInformation3, 3, 5);
        btex.GnfHeader.DestinationZ = (GnfSqSel)ExtractData(btex.GnfHeader.ImageInformation3, 6, 8);
        btex.GnfHeader.DestinationW = (GnfSqSel)ExtractData(btex.GnfHeader.ImageInformation3, 9, 11);

        // Read all the pixel data
        btex.DdsData = new byte[btex.GnfHeader.DataSize];
        reader.Read(btex.DdsData);

        var imageBinary = new ImageBinary
        {
            Width = (int)btex.GnfHeader.Width,
            Height = (int)btex.GnfHeader.Height,
            PhysicalWidth = (int)btex.GnfHeader.Pitch,
            PhysicalHeight = (int)btex.GnfHeader.Height
        };

        switch (btex.GnfHeader.DataFormat)
        {
            //case GnfDataFormat.Format8_8_8_8: imageBinary.InputPixelFormat = (PixelDataFormat.Bpp32 | channelOrder | PixelDataFormat.RedBits8 | PixelDataFormat.GreenBits8 | PixelDataFormat.BlueBits8 | PixelDataFormat.AlphaBits8); break;
            case GnfDataFormat.FormatBC1:
                imageBinary.InputPixelFormat = PixelDataFormat.FormatDXT1Rgba;
                break;
            case GnfDataFormat.FormatBC2:
                imageBinary.InputPixelFormat = PixelDataFormat.FormatDXT3;
                break;
            case GnfDataFormat.FormatBC3:
                imageBinary.InputPixelFormat = PixelDataFormat.FormatDXT5;
                break;
            case GnfDataFormat.FormatBC4:
                imageBinary.InputPixelFormat = btex.GnfHeader.NumFormat == GnfNumFormat.FormatSNorm
                    ? PixelDataFormat.FormatRGTC1_Signed
                    : PixelDataFormat.FormatRGTC1;
                break;
            case GnfDataFormat.FormatBC5:
                imageBinary.InputPixelFormat = btex.GnfHeader.NumFormat == GnfNumFormat.FormatSNorm
                    ? PixelDataFormat.FormatRGTC2_Signed
                    : PixelDataFormat.FormatRGTC2;
                break;
            //case GnfDataFormat.FormatBC6: imageBinary.InputPixelFormat = PixelDataFormat.FormatBPTC_Float;/*(numFormat == GnfNumFormat.FormatSNorm ? PixelDataFormat.FormatBPTC_SignedFloat : PixelDataFormat.FormatBPTC_Float);*/ break;   // TODO: fixme!!
            case GnfDataFormat.FormatBC7:
                imageBinary.InputPixelFormat = PixelDataFormat.FormatBPTC;
                break;

            // TODO
            //case GnfDataFormat.Format16_16_16_16: imageBinary.InputPixelFormat = PixelDataFormat.FormatAbgr8888; break;
            //case GnfDataFormat.Format32: imageBinary.InputPixelFormat = PixelDataFormat.FormatLuminance8; break;
            //case GnfDataFormat.Format32_32_32_32: imageBinary.InputPixelFormat = PixelDataFormat.FormatAbgr8888; break;

            // WRONG
            //case GnfDataFormat.Format8: imageBinary.InputPixelFormat = PixelDataFormat.FormatLuminance8; break;
            //case GnfDataFormat.Format8_8: imageBinary.InputPixelFormat = PixelDataFormat.FormatLuminance8; break;

            default: throw new Exception($"Unimplemented GNF data format {btex.GnfHeader.DataFormat}");
        }

        imageBinary.InputPixelFormat |= PixelDataFormat.PixelOrderingTiled3DS;
        imageBinary.AddInputPixels(btex.DdsData);
        btex.Bitmap = imageBinary.GetBitmap();

        return btex;
    }

    public byte[] ToDds()
    {
        var ddsHeader = new DdsHeader
        {
            Height = ImageData.Height,
            Width = ImageData.Width,
            PitchOrLinearSize = ImageData.Pitch,
            Depth = ImageData.Depth,
            MipMapCount = 1,
            Flags = DDSFlags.Texture | DDSFlags.Pitch | DDSFlags.Depth | DDSFlags.MipMapCount,
            PixelFormat = new PixelFormat(),
            DX10Header = new DX10
            {
                ArraySize = ImageData.ArrayCount,
                Format = FormatMap[ImageData.Format],
                ResourceDimension = ImageData.DimensionsCount + 1u
            }
        };

        return BtexConverter.WriteDds(ddsHeader, DdsData);
    }

    private static uint ExtractData(uint val, int first, int last)
    {
        var mask = ((uint)(1 << (last + 1 - first)) - 1) << first;
        return (val & mask) >> first;
    }
}