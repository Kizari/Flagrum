using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Flagrum.Core.Utilities;

namespace Flagrum.Core.Gfxbin.Btex;

public enum TextureType
{
    Undefined,
    BaseColor,
    AmbientOcclusion,
    Normal,
    Mrs,
    Opacity,
    Preview,
    Thumbnail,
    MenuSprites
}

public static class BtexConverter
{
    public static FallbackMap<DxgiFormat, BtexFormat> FormatMap { get; } = new(DxgiFormat.BC1_UNORM, BtexFormat.BC1_UNORM);

    static BtexConverter()
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

    public static byte[] BtexToDds(byte[] btex)
    {
        // Remove SEDB header
        var withoutSedb = new byte[btex.Length - 128];
        Array.Copy(btex, 128, withoutSedb, 0, withoutSedb.Length);

        var btexHeader = ReadBtexHeader(withoutSedb);
        var ddsHeader = new DdsHeader
        {
            Height = btexHeader.Height,
            Width = btexHeader.Width,
            PitchOrLinearSize = btexHeader.Pitch,
            Depth = btexHeader.Depth,
            MipMapCount = btexHeader.MipMapCount,
            Flags = DDSFlags.Texture | DDSFlags.Pitch | DDSFlags.Depth | DDSFlags.MipMapCount,
            PixelFormat = new PixelFormat(),
            DX10Header = new DX10
            {
                ArraySize = btexHeader.ArraySize,
                Format = FormatMap[btexHeader.Format],
                ResourceDimension = btexHeader.Dimension + 1u
            }
        };

        return WriteDds(ddsHeader, btexHeader.Data);
    }

    public static byte[] DdsToBtex(TextureType type, string fileName, byte[] dds)
    {
        var ddsHeader = ReadDdsHeader(dds, out var ddsContent);
        var btexHeader = new BtexHeader
        {
            Height = (ushort)ddsHeader.Height,
            Width = (ushort)ddsHeader.Width,
            Pitch = (ushort)CalculatePitch(ddsHeader.Width, type),
            Depth = (byte)ddsHeader.Depth,
            MipMapCount = (byte)ddsHeader.MipMapCount,
            ArraySize = (ushort)ddsHeader.DX10Header.ArraySize,
            Format = FormatMap[ddsHeader.DX10Header.Format],
            Dimension = (byte)(ddsHeader.DX10Header.ResourceDimension - 1u),
            Data = ddsContent,
            p_ImageFileSize = (uint)ddsContent.Length,
            p_ImageFlags = ImageFlagsForType(type),
            p_Name = fileName,
            p_SurfaceCount = (ushort)ddsHeader.MipMapCount
        };

        uint width = btexHeader.Width;
        uint height = btexHeader.Height;

        for (var i = 0; i < btexHeader.MipMapCount; i++)
        {
            btexHeader.MipMaps.Add(new BtexMipMap
            {
                Offset = i == 0 ? 0 : btexHeader.MipMaps[i - 1].Offset + btexHeader.MipMaps[i - 1].Size,
                Size = (uint)(type == TextureType.Preview
                    ? Math.Max(GetBlockSize(type), width * height * GetBlockSize(type))
                    : Math.Max(GetBlockSize(type) * 4, CalculatePitch(width, type) * height))
            });

            width /= 2;
            height /= 2;
        }

        var btex = WriteBtex(btexHeader);

        var sedbHeader = new SedbBtexHeader
        {
            FileSize = (ulong)btex.Length + 128 // Size of this header itself
        };

        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8);
        writer.Write(sedbHeader.ToBytes());
        writer.Write(btex);
        return stream.ToArray();
    }
    
    public static byte[] DdsToBtex(Ps4.Btex original, byte[] dds)
    {
        var ddsHeader = ReadDdsHeader(dds, out var ddsContent);
        var btexHeader = new BtexHeader
        {
            Height = original.ImageData.Height,
            Width = original.ImageData.Width,
            Pitch = original.ImageData.Pitch,
            Depth = original.ImageData.Depth,
            MipMapCount = original.ImageData.MipCount,
            ArraySize = (ushort)original.ImageData.ArrayCount,
            Format = original.ImageData.Format,
            Dimension = original.ImageData.DimensionsCount,
            Data = ddsContent,
            p_ImageFileSize = (uint)ddsContent.Length,
            p_ImageFlags = original.ImageData.ImageFlags,
            p_Name = original.ImageData.Name,
            p_SurfaceCount = (ushort)ddsHeader.MipMapCount
        };

        uint width = btexHeader.Width;
        uint height = btexHeader.Height;

        for (var i = 0; i < btexHeader.MipMapCount; i++)
        {
            btexHeader.MipMaps.Add(new BtexMipMap
            {
                Offset = i == 0 ? 0 : btexHeader.MipMaps[i - 1].Offset + btexHeader.MipMaps[i - 1].Size,
                Size = (uint)(Math.Max(GetBlockSize(original.ImageData.Format) * 4, CalculatePitch(width, TextureType.BaseColor) * height))
            });

            width /= 2;
            height /= 2;
        }

        var btex = WriteBtex(btexHeader);

        var sedbHeader = new SedbBtexHeader
        {
            FileSize = (ulong)btex.Length + 128 // Size of this header itself
        };

        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8);
        writer.Write(sedbHeader.ToBytes());
        writer.Write(btex);
        return stream.ToArray();
    }

    public static byte[] WriteDds(DdsHeader header, byte[] data)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new BinaryWriter(memoryStream, Encoding.UTF8);

        writer.Write(542327876);
        writer.Write(header.p_Size);
        writer.Write((uint)header.Flags);
        writer.Write(header.Height);
        writer.Write(header.Width);
        writer.Write(header.PitchOrLinearSize);
        writer.Write(header.Depth);
        writer.Write(header.MipMapCount);
        writer.Write(0);
        writer.Write(0);
        writer.Write(0);
        writer.Write(0);
        writer.Write(0);
        writer.Write(0);
        writer.Write(0);
        writer.Write(0);
        writer.Write(0);
        writer.Write(0);
        writer.Write(0);
        writer.Write(header.PixelFormat.Size);
        writer.Write((uint)header.PixelFormat.Flags);
        writer.Write(header.PixelFormat.FourCC);
        writer.Write(header.PixelFormat.RGBBitCount);
        writer.Write(header.PixelFormat.RBitMask);
        writer.Write(header.PixelFormat.GBitMask);
        writer.Write(header.PixelFormat.BBitMask);
        writer.Write(header.PixelFormat.ABitMask);
        writer.Write(header.p_Caps);
        writer.Write(header.p_Caps2);
        writer.Write(header.p_Caps3);
        writer.Write(header.p_Caps4);
        writer.Write(0);
        writer.Write((uint)header.DX10Header.Format);
        writer.Write(header.DX10Header.ResourceDimension);
        writer.Write(header.DX10Header.MiscFlags);
        writer.Write(header.DX10Header.ArraySize);
        writer.Write(header.DX10Header.MiscFlags2);
        writer.Write(data);

        return memoryStream.ToArray();
    }

    private static byte[] WriteBtex(BtexHeader header)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new BinaryWriter(memoryStream, Encoding.UTF8);

        memoryStream.Seek(header.p_ImageHeaderOffset + header.p_SurfaceHeaderOffset, SeekOrigin.Begin);

        foreach (var mipmap in header.MipMaps)
        {
            writer.Write(mipmap.Offset);
            writer.Write(mipmap.Size);
        }

        header.p_NameOffset = (uint)memoryStream.Position - 32; // Image header offset

        memoryStream.Seek(0, SeekOrigin.Begin);

        writer.Write('B');
        writer.Write('T');
        writer.Write('E');
        writer.Write('X');

        writer.Write(header.HeaderSize);
        writer.Write(header.p_ImageFileSize);
        writer.Write(header.p_Version);
        writer.Write(header.p_Platform);
        writer.Write(header.p_Flags);

        writer.Write(header.p_ImageCount);
        writer.Write(header.p_ImageHeaderStride);
        writer.Write(header.p_ImageHeaderOffset);

        memoryStream.Seek(header.p_ImageHeaderOffset, SeekOrigin.Begin);

        writer.Write(header.Width);
        writer.Write(header.Height);
        writer.Write(header.Pitch);
        writer.Write((ushort)header.Format);
        writer.Write(header.MipMapCount);
        writer.Write(header.Depth);
        writer.Write(header.Dimension);
        writer.Write(header.p_ImageFlags);
        writer.Write(header.p_SurfaceCount);
        writer.Write(header.p_SurfaceHeaderStride);

        writer.Write(header.p_PlatformDataOffset);
        writer.Write(header.p_SurfaceHeaderOffset);
        writer.Write(header.p_NameOffset);
        writer.Write(header.p_PlatformDataSize);

        writer.Write(header.p_HighTextureMipLevels);

        memoryStream.Seek(3, SeekOrigin.Current); // Padding

        writer.Write(header.p_HighTextureDataSizeByte);

        memoryStream.Seek(8, SeekOrigin.Current); // Padding

        writer.Write(header.p_TileMode);
        writer.Write(header.ArraySize);

        memoryStream.Seek(header.p_ImageHeaderOffset + header.p_NameOffset, SeekOrigin.Begin);

        writer.Write(Encoding.UTF8.GetBytes(header.p_Name));
        writer.Write(0x00); // Null-terminated string

        memoryStream.Seek(header.HeaderSize, SeekOrigin.Begin);

        writer.Write(header.Data);
        return memoryStream.ToArray();
    }

    private static byte ImageFlagsForType(TextureType type)
    {
        return type switch
        {
            TextureType.BaseColor => 49,
            TextureType.Preview or TextureType.Thumbnail => 33,
            _ => 17
        };
    }

    private static double CalculatePitch(double width, TextureType type)
    {
        return type switch
        {
            TextureType.Preview or TextureType.Thumbnail => Math.Max(1, (width * (GetBlockSize(type) * 8) + 7) / 8),
            _ => Math.Max(1, (width + 3) / 4 * GetBlockSize(type))
        };
    }

    private static int GetBlockSize(TextureType type)
    {
        return type switch
        {
            TextureType.BaseColor => 2,
            TextureType.AmbientOcclusion => 2,
            TextureType.Mrs => 2,
            TextureType.Undefined => 2,
            _ => 4
        };
    }

    private static int GetBlockSize(BtexFormat format)
    {
        return format switch
        {
            BtexFormat.BC6H_UF16 or BtexFormat.BC1_UNORM or BtexFormat.BC2_UNORM or BtexFormat.BC3_UNORM
                or BtexFormat.BC4_UNORM or BtexFormat.BC5_UNORM or BtexFormat.BC7_UNORM => 2,
            _ => 4
        };
    }

    public static BtexHeader ReadBtexHeader(byte[] btex)
    {
        var header = new BtexHeader();
        using var memoryStream = new MemoryStream(btex);
        using var reader = new BinaryReader(memoryStream, Encoding.UTF8);

        // Skip BTEX tag
        memoryStream.Seek(4, SeekOrigin.Begin);

        header.HeaderSize = reader.ReadUInt32();
        header.p_ImageFileSize = reader.ReadUInt32();
        header.p_Version = reader.ReadUInt16();
        header.p_Platform = reader.ReadByte();
        header.p_Flags = reader.ReadByte();

        header.p_ImageCount = reader.ReadUInt16();
        header.p_ImageHeaderStride = reader.ReadUInt16();
        header.p_ImageHeaderOffset = reader.ReadUInt32();

        memoryStream.Seek(header.p_ImageHeaderOffset, SeekOrigin.Begin);

        header.Width = reader.ReadUInt16();
        header.Height = reader.ReadUInt16();
        header.Pitch = reader.ReadUInt16();
        header.Format = (BtexFormat)reader.ReadUInt16();
        header.MipMapCount = reader.ReadByte();
        header.Depth = reader.ReadByte();
        header.Dimension = reader.ReadByte();
        header.p_ImageFlags = reader.ReadByte();
        header.p_SurfaceCount = reader.ReadUInt16();
        header.p_SurfaceHeaderStride = reader.ReadUInt16();

        header.p_PlatformDataOffset = reader.ReadUInt32();
        header.p_SurfaceHeaderOffset = reader.ReadUInt32();
        header.p_NameOffset = reader.ReadUInt32();
        header.p_PlatformDataSize = reader.ReadUInt32();

        header.p_HighTextureMipLevels = reader.ReadByte();
        memoryStream.Seek(3, SeekOrigin.Current); // Skip padding
        header.p_HighTextureDataSizeByte = reader.ReadUInt32();
        memoryStream.Seek(8, SeekOrigin.Current); // Skip padding

        header.p_TileMode = reader.ReadUInt32();
        header.ArraySize = reader.ReadUInt16();

        memoryStream.Seek(header.p_ImageHeaderOffset + header.p_NameOffset, SeekOrigin.Begin);

        var nameBytes = new List<byte>();
        byte current = 0x00;

        do
        {
            current = reader.ReadByte();

            if (current != 0x00)
            {
                nameBytes.Add(current);
            }
        } while (current != 0x00);

        header.p_Name = Encoding.UTF8.GetString(nameBytes.ToArray());

        memoryStream.Seek(header.HeaderSize, SeekOrigin.Begin);

        header.Data = reader.ReadBytes((int)(memoryStream.Length - memoryStream.Position));
        return header;
    }

    private static DdsHeader ReadDdsHeader(byte[] dds, out byte[] content)
    {
        var header = new DdsHeader();
        using var memoryStream = new MemoryStream(dds);
        using var reader = new BinaryReader(memoryStream, Encoding.UTF8);

        // Skip DDS tag
        reader.ReadByte();
        reader.ReadByte();
        reader.ReadByte();
        reader.ReadByte();

        header.p_Size = reader.ReadUInt32();
        header.Flags = (DDSFlags)reader.ReadUInt32();
        header.Height = reader.ReadUInt32();
        header.Width = reader.ReadUInt32();
        header.PitchOrLinearSize = reader.ReadUInt32();
        header.Depth = reader.ReadUInt32();
        header.MipMapCount = reader.ReadUInt32();

        // Skip reserved DWORDs
        for (var i = 0; i < 11; i++)
        {
            header.p_Reserved[i] = reader.ReadUInt32();
        }

        header.PixelFormat = new PixelFormat
        {
            Size = reader.ReadUInt32(),
            Flags = (PixelFormatFlags)reader.ReadUInt32(),
            FourCC = reader.ReadUInt32(),
            RGBBitCount = reader.ReadUInt32(),
            RBitMask = reader.ReadUInt32(),
            GBitMask = reader.ReadUInt32(),
            BBitMask = reader.ReadUInt32(),
            ABitMask = reader.ReadUInt32()
        };

        header.p_Caps = reader.ReadUInt32();
        header.p_Caps2 = reader.ReadUInt32();
        header.p_Caps3 = reader.ReadUInt32();
        header.p_Caps4 = reader.ReadUInt32();
        header.p_Reserved2 = reader.ReadUInt32();

        header.DX10Header = new DX10
        {
            Format = (DxgiFormat)reader.ReadUInt32(),
            ResourceDimension = reader.ReadUInt32(),
            MiscFlags = reader.ReadUInt32(),
            ArraySize = reader.ReadUInt32(),
            MiscFlags2 = reader.ReadUInt32()
        };

        var contentSize = memoryStream.Length - memoryStream.Position;
        var alignment = Utilities.Serialization.GetAlignment((ulong)contentSize, 128);
        var paddingSize = (int)(alignment - (ulong)contentSize);

        content = new byte[contentSize + paddingSize];
        reader.Read(content, 0, (int)contentSize);

        return header;
    }
}