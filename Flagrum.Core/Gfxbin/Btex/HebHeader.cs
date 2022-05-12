using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Flagrum.Core.Gfxbin.Btex;

public enum HebImageType : byte
{
    TYPE_HEIGHT_MAP,
    TYPE_MASK_MAP,
    TYPE_TEXTURE_MAP,
    TYPE_ATTRIBUTE,
    TYPE_NORMAL_MAP,
    TYPE_EROSION_MAP,
    TYPE_MERGED_MASK_MAP,
    TYPE_VERTEX_DATA,
    TYPE_DIFFUSE_MAP,
    TYPE_OCCLUSION_MAP,
    TYPE_ROUGHNESS_MAP,
    TYPE_SLOPE_MAP,
    TYPE_OCCLUDER_DATA,
    TYPE_PHYSX_COLLISION_DATA,
    TYPE_SLOPE_MASK_MAP,
    TYPE_UMBRA_DATA,
    TYPE_SCULPT_FROTTAGE
}

public enum HebImageTileMode : byte
{
    TILE_MODE_TYPE_NONE = 0x0,
    TILE_MODE_TYPE_COMP_DEPTH_1 = 0x1,
    TILE_MODE_TYPE_COMP_DEPTH_2 = 0x2,
    TILE_MODE_TYPE_COMP_DEPTH_3 = 0x3,
    TILE_MODE_TYPE_COMP_DEPTH_4 = 0x4,
    TILE_MODE_TYPE_UNC_DEPTH_5 = 0x5,
    TILE_MODE_TYPE_UNC_DEPTH_6 = 0x6,
    TILE_MODE_TYPE_UNC_DEPTH_7 = 0x7,
    TILE_MODE_TYPE_LINEAR = 0x8,
    TILE_MODE_TYPE_DISPLAY = 0x9,
    TILE_MODE_TYPE_2D_DISPLAY = 0xA,
    TILE_MODE_TYPE_RESERVED_11 = 0xB,
    TILE_MODE_TYPE_RESERVED_12 = 0xC,
    TILE_MODE_TYPE_1D_THIN = 0xD,
    TILE_MODE_TYPE_2D_THIN = 0xE,
    TILE_MODE_TYPE_RESERVED_15 = 0xF,
    TILE_MODE_TYPE_TILED_1D_THIN = 0x10,
    TILE_MODE_TYPE_TILED_2D_TIN = 0x11,
    TILE_MODE_TYPE_RESERVED_18 = 0x12,
    TILE_MODE_TYPE_RESERVED_19 = 0x13,
    TILE_MODE_TYPE_RESERVED_20 = 0x14,
    TILE_MODE_TYPE_RESERVED_21 = 0x15,
    TILE_MODE_TYPE_RESERVED_22 = 0x16,
    TILE_MODE_TYPE_RESERVED_23 = 0x17,
    TILE_MODE_TYPE_RESERVED_24 = 0x18,
    TILE_MODE_TYPE_RESERVED_25 = 0x19,
    TILE_MODE_TYPE_RESERVED_26 = 0x1A,
    TILE_MODE_TYPE_RESERVED_27 = 0x1B,
    TILE_MODE_TYPE_RESERVED_28 = 0x1C,
    TILE_MODE_TYPE_RESERVED_29 = 0x1D,
    TILE_MODE_TYPE_RESERVED_30 = 0x1E,
    TILE_MODE_TYPE_LINEAR_GENERAL = 0x1F
}

public class HebImageHeader
{
    public byte Flags { get; set; }
    public HebImageType Type { get; set; }
    public byte TypeIndex { get; set; }
    public byte MipCount { get; set; }
    public long TextureDataOffsetOffset { get; set; }
    public uint TextureDataOffset { get; set; }
    public float AverageHeight { get; set; }
    public byte TextureFormat { get; set; }
    public HebImageTileMode TileMode { get; set; }
    public ushort Reserved1 { get; set; }
    public ushort MinValue { get; set; }
    public ushort MaxValue { get; set; }
    public uint TextureSizeBytes { get; set; }
    public uint TextureWidth { get; set; }
    public uint TextureHeight { get; set; }
    public byte[] DdsData { get; set; }
}

public class HebImageDataBase
{
    public int Index { get; set; }
    public string Extension { get; set; }
}

public class HebHeightMapData : HebImageDataBase
{
    public HebHeightMap Data { get; set; }
}

public class HebImageData : HebImageDataBase
{
    public byte[] Data { get; set; }
}

public class HebHeightMap
{
    public uint Width { get; set; }
    public uint Height { get; set; }
    public float[] Altitudes { get; set; }
}

public class HebHeader
{
    public uint ImageHeadersOffset { get; set; }
    public ushort ImageHeaderSize { get; set; }
    public ushort ImageCount { get; set; }
    public List<HebImageHeader> ImageHeaders { get; set; } = new();

    public static HebHeader FromData(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream, Encoding.UTF8);
        var header = new HebHeader();

        // Read HebHeader properties in Big Endian
        var imageHeadersOffsetBigEndian = reader.ReadBytes(4);
        Array.Reverse(imageHeadersOffsetBigEndian);
        header.ImageHeadersOffset = BitConverter.ToUInt32(imageHeadersOffsetBigEndian);

        var imageHeaderSizeBigEndian = reader.ReadBytes(2);
        Array.Reverse(imageHeaderSizeBigEndian);
        header.ImageHeaderSize = BitConverter.ToUInt16(imageHeaderSizeBigEndian);

        var imageCountBigEndian = reader.ReadBytes(2);
        Array.Reverse(imageCountBigEndian);
        header.ImageCount = BitConverter.ToUInt16(imageCountBigEndian);

        // Skip padding
        stream.Seek(24, SeekOrigin.Current);

        // Read each image header
        for (var i = 0; i < header.ImageCount; i++)
        {
            var imageHeader = new HebImageHeader
            {
                Flags = reader.ReadByte(),
                Type = (HebImageType)reader.ReadByte(),
                TypeIndex = reader.ReadByte(),
                MipCount = reader.ReadByte(),
                TextureDataOffsetOffset = stream.Position,
                TextureDataOffset = reader.ReadUInt32(),
                AverageHeight = reader.ReadSingle(),
                TextureFormat = reader.ReadByte(),
                TileMode = (HebImageTileMode)reader.ReadByte(),
                Reserved1 = reader.ReadUInt16(),
                MinValue = reader.ReadUInt16(),
                MaxValue = reader.ReadUInt16(),
                TextureSizeBytes = reader.ReadUInt32(),
                TextureWidth = reader.ReadUInt32(),
                TextureHeight = reader.ReadUInt32()
            };

            // Read image data
            imageHeader.DdsData = new byte[imageHeader.TextureSizeBytes];
            var returnAddress = stream.Position;
            stream.Seek(imageHeader.TextureDataOffsetOffset + imageHeader.TextureDataOffset, SeekOrigin.Begin);
            reader.Read(imageHeader.DdsData);
            stream.Seek(returnAddress, SeekOrigin.Begin);
            header.ImageHeaders.Add(imageHeader);
        }

        return header;
    }

    public static byte[] ToData(HebHeader heb)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new BinaryWriter(memoryStream, Encoding.UTF8);

        var imageHeadersOffsetBigEndian = BitConverter.GetBytes(heb.ImageHeadersOffset);
        Array.Reverse(imageHeadersOffsetBigEndian);
        writer.Write(imageHeadersOffsetBigEndian);

        var imageHeaderSizeBigEndian = BitConverter.GetBytes(heb.ImageHeaderSize);
        Array.Reverse(imageHeaderSizeBigEndian);
        writer.Write(imageHeaderSizeBigEndian);

        var imageCountBigEndian = BitConverter.GetBytes((ushort)heb.ImageHeaders.Count);
        Array.Reverse(imageCountBigEndian);
        writer.Write(imageCountBigEndian);

        // Padding
        writer.Write(new byte[24]);

        var totalDataSize = 256u;

        // Write each image header
        foreach (var image in heb.ImageHeaders)
        {
            writer.Write(image.Flags);
            writer.Write((byte)image.Type);
            writer.Write(image.TypeIndex);
            writer.Write(image.MipCount);
            image.TextureDataOffsetOffset = memoryStream.Position;
            image.TextureDataOffset = totalDataSize - (uint)memoryStream.Position;
            writer.Write(image.TextureDataOffset);
            writer.Write(image.AverageHeight);
            writer.Write(image.TextureFormat);
            writer.Write((byte)image.TileMode);
            writer.Write(image.Reserved1);
            writer.Write(image.MinValue);
            writer.Write(image.MaxValue);
            image.TextureSizeBytes = (uint)image.DdsData.Length;
            writer.Write(image.TextureSizeBytes);
            writer.Write(image.TextureWidth);
            writer.Write(image.TextureHeight);
            totalDataSize += image.TextureSizeBytes;
            var endOfData = 256u + totalDataSize;
            var blockSize = 256u;
            var alignment = Utilities.Serialization.GetAlignment(endOfData, blockSize);
            var paddingSize = alignment - endOfData;
            if (paddingSize != blockSize)
            {
                totalDataSize += paddingSize;
                var newData = new byte[image.DdsData.Length + paddingSize];
                Array.Copy(image.DdsData, 0, newData, 0, image.DdsData.Length);
                image.DdsData = newData;
            }
        }

        // Padding
        var padding = 256 - memoryStream.Position;
        if (padding > 0)
        {
            writer.Write(new byte[padding]);
        }

        // Write each image
        foreach (var image in heb.ImageHeaders)
        {
            writer.Write(image.DdsData);
        }

        return memoryStream.ToArray();
    }
}