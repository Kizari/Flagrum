using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Flagrum.Core.Graphics.Textures.DirectX;
using Flagrum.Core.Graphics.Textures.Luminous;
using Flagrum.Core.Serialization;

namespace Flagrum.Core.Graphics.Terrain;

public class HeightEntityBinary
{
    public uint ImageHeadersOffset { get; set; }
    public ushort ImageHeaderSize { get; set; }
    public ushort ImageCount { get; set; }
    public List<HeightEntityBinaryImage> ImageHeaders { get; set; } = new();

    public static HeightEntityBinary FromData(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream, Encoding.UTF8);
        var header = new HeightEntityBinary();

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
            var imageHeader = new HeightEntityBinaryImage
            {
                Flags = reader.ReadByte(),
                Type = (HeightEntityBinaryImageType)reader.ReadByte(),
                TypeIndex = reader.ReadByte(),
                MipCount = reader.ReadByte(),
                TextureDataOffsetOffset = stream.Position,
                TextureDataOffset = reader.ReadUInt32(),
                AverageHeight = reader.ReadSingle(),
                TextureFormat = (BlackTexturePixelFormat)reader.ReadByte(),
                TileMode = (HeightEntityBinaryImageTileMode)reader.ReadByte(),
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

    public static byte[] ToData(HeightEntityBinary heb)
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
            writer.Write((byte)image.TextureFormat);
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
            var alignment = SerializationHelper.GetAlignment(endOfData, blockSize);
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