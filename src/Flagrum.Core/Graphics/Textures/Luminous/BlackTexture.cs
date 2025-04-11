using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flagrum.Abstractions;
using Flagrum.Core.Data.Binary;
using Flagrum.Core.Graphics.Textures.DirectX;
using Flagrum.Core.Ps4;
using Flagrum.Core.Utilities.Extensions;

namespace Flagrum.Core.Graphics.Textures.Luminous;

public class BlackTexture : SectionDataBinary
{
    private readonly LuminousGame _game;

    public BlackTexture(LuminousGame game)
    {
        _game = game;
        Subtype = "btex".ToCharArray();
    }

    public char[] Magic { get; set; } = "BTEX".ToCharArray();
    public uint HeaderSize { get; set; }
    public uint ImageDataSize { get; set; }
    public BlackTextureVersion Version { get; set; } = BlackTextureVersion.VERSION_LATEST;
    public BlackTexturePlatform Platform { get; set; } = BlackTexturePlatform.PLATFORM_WIIU;
    public BlackTextureFlags Flags { get; set; } = BlackTextureFlags.FLAG_COMPOSITED_IMAGE;
    public ushort ImageCount { get; set; } = 1;
    public ushort ImageHeaderStride { get; set; } = 56;
    public uint ImageHeaderOffset { get; set; } = 32;

    public List<BlackTextureImageData> ImageData { get; set; } = new();

    public static BlackTexture Deserialize(byte[] buffer, LuminousGame game = LuminousGame.FFXV)
    {
        var texture = new BlackTexture(game);
        texture.Read(buffer);
        return texture;
    }

    public override void Read(Stream stream)
    {
        if (_game != LuminousGame.Forspoken)
        {
            base.Read(stream);
            stream.Align(Offset);
        }

        using var reader = new BinaryReader(stream);

        Magic = reader.ReadChars(4);
        HeaderSize = reader.ReadUInt32();
        ImageDataSize = reader.ReadUInt32();
        Version = (BlackTextureVersion)reader.ReadUInt16();
        Platform = (BlackTexturePlatform)reader.ReadByte();
        Flags = (BlackTextureFlags)reader.ReadByte();
        ImageCount = reader.ReadUInt16();
        ImageHeaderStride = reader.ReadUInt16();
        ImageHeaderOffset = reader.ReadUInt32();
        _ = reader.ReadUInt64(); // Padding

        for (var i = 0; i < ImageCount; i++)
        {
            var imageData = new BlackTextureImageData(_game, Platform);
            imageData.Read(reader);
            ImageData.Add(imageData);
        }
    }

    public override void Write(Stream stream)
    {
        if (_game != LuminousGame.Forspoken) // Forspoken doesn't have SEDBbtex header
        {
            base.Write(stream);
        }

        using var writer = new BinaryWriter(stream);
        writer.Align(128, 0x00);

        writer.Write(Magic);

        var sizesOffset = writer.BaseStream.Position;
        writer.Write(0u); // Will come back and write these later
        writer.Write(0u);

        writer.Write((ushort)Version);
        writer.Write((byte)Platform);
        writer.Write((byte)Flags);
        writer.Write(ImageCount);
        writer.Write(ImageHeaderStride);
        writer.Write(ImageHeaderOffset);
        writer.Write(0UL); // Padding

        if (ImageCount > 1)
        {
            throw new Exception("BTEX converter doesn't support multiple images currently");
        }

        var (metadataSize, pixelDataSize) = ImageData[0].Write(writer);
        HeaderSize = 32 + metadataSize;
        ImageDataSize = pixelDataSize;

        var returnAddress = writer.BaseStream.Position;
        writer.BaseStream.Seek(sizesOffset, SeekOrigin.Begin);
        writer.Write(HeaderSize);
        writer.Write(ImageDataSize);
        writer.BaseStream.Seek(returnAddress, SeekOrigin.Begin);
        writer.Align(Offset, 0x00);

        if (_game != LuminousGame.Forspoken)
        {
            // Calculate size and write it back in the SEDB header
            var size = stream.Position;
            writer.Seek(16, SeekOrigin.Begin);
            writer.Write(size);
        }
    }

    public DirectDrawSurface ToDds()
    {
        if (ImageCount > 1)
        {
            throw new Exception("BTEX converter doesn't support multiple images currently");
        }

        var data = ImageData[0];
        return new DirectDrawSurface
        {
            Height = data.Height,
            Width = data.Width,
            Pitch = data.Pitch,
            Depth = data.Depth,
            MipCount = data.MipCount,
            Flags = DirectDrawSurfaceFlags.Texture | DirectDrawSurfaceFlags.Pitch | DirectDrawSurfaceFlags.Depth |
                    DirectDrawSurfaceFlags.MipMapCount,
            Format = new DirectDrawSurfacePixelFormat(),
            DirectX10Header = new DirectDrawSurfaceDirectX10Header
            {
                ArraySize = data.ArrayCount,
                Format = TexturePixelFormatMap.Instance[data.Format],
                ResourceDimension = data.DimensionCount + 1u
            },
            PixelData = data.PixelData
        };
    }

    /// WARNING: Only works for FFXV Windows Edition textures
    public void AppendTextureToArray(BlackTexture textureToAppend)
    {
        // Update metadata
        ImageData[0].ArrayCount++;
        ImageData[0].Mips.Add(textureToAppend.ImageData[0].Mips[0].Select(m => new BlackTextureMipData
        {
            Offset = (uint)(m.Offset + ImageData[0].PixelData.Length),
            Size = m.Size
        }).ToList());

        // Append texture data
        var data = new byte[ImageData[0].PixelData.Length + textureToAppend.ImageData[0].PixelData.Length];
        Array.Copy(ImageData[0].PixelData, 0, data, 0, ImageData[0].PixelData.Length);
        Array.Copy(textureToAppend.ImageData[0].PixelData, 0, data, ImageData[0].PixelData.Length,
            textureToAppend.ImageData[0].PixelData.Length);
        ImageData[0].PixelData = data;
    }
}