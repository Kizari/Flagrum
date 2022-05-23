using System;
using System.IO;
using Flagrum.Core.Gfxbin.Btex;
using Flagrum.Core.Utilities;

namespace Flagrum.Core.Ps4;

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
    public byte[] DdsData { get; set; }

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

        // Skip the GNF header
        stream.Align(256);

        // Read all the DDS data
        var dataSize = btex.SedbHeader.FileSize - stream.Position;
        btex.DdsData = new byte[dataSize];

        var start = stream.Position;
        for (var i = 0; i < dataSize / 16; i += 2)
        {
            reader.Read(btex.DdsData, i * 16, 16);
            stream.Seek(16, SeekOrigin.Current);
        }

        stream.Seek(start, SeekOrigin.Begin);
        for (var i = 1; i < dataSize / 16; i += 2)
        {
            reader.Read(btex.DdsData, i * 16, 16);
            stream.Seek(16, SeekOrigin.Current);
        }

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
}