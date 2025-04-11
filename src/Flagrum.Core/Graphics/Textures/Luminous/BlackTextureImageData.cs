using System;
using System.Collections.Generic;
using System.IO;
using Flagrum.Abstractions;
using Flagrum.Core.Graphics.Textures.DirectX;
using Flagrum.Core.Ps4;
using Flagrum.Core.Utilities.Extensions;

namespace Flagrum.Core.Graphics.Textures.Luminous;

public class BlackTextureImageData
{
    private readonly int _alignment;
    private readonly LuminousGame _game;
    private readonly BlackTexturePlatform _platform;

    public BlackTextureImageData(LuminousGame game, BlackTexturePlatform platform)
    {
        _game = game;
        _platform = platform;
        _alignment = platform == BlackTexturePlatform.PLATFORM_PS4 ? 256 : 128;
    }

    public ushort Width { get; set; }
    public ushort Height { get; set; }
    public ushort Pitch { get; set; }
    public BlackTexturePixelFormat Format { get; set; }
    public byte MipCount { get; set; }
    public byte Depth { get; set; }
    public byte DimensionCount { get; set; }
    public BlackTextureImageFlags ImageFlags { get; set; }
    public ushort SurfaceCount { get; set; }
    public ushort SurfaceHeaderStride { get; set; } = 8;
    public uint PlatformDataOffset { get; set; }
    public uint SurfaceHeaderOffset { get; set; } = 56;
    public uint NameOffset { get; set; }
    public uint PlatformDataSize { get; set; }
    public uint HighResolutionMipCount { get; set; }
    public uint HighResolutionBtexSize { get; set; }
    public uint TileMode { get; set; }
    public uint ArrayCount { get; set; }
    public List<List<BlackTextureMipData>> Mips { get; set; } = new();
    public string FileName { get; set; }

    public GnfHeader GnfHeader { get; set; }

    public byte[] PixelData { get; set; }

    public void Read(BinaryReader reader)
    {
        Width = reader.ReadUInt16();
        Height = reader.ReadUInt16();
        Pitch = reader.ReadUInt16();
        Format = (BlackTexturePixelFormat)reader.ReadUInt16();
        MipCount = reader.ReadByte();
        Depth = reader.ReadByte();
        DimensionCount = reader.ReadByte();
        ImageFlags = (BlackTextureImageFlags)reader.ReadByte();
        SurfaceCount = reader.ReadUInt16();
        SurfaceHeaderStride = reader.ReadUInt16();
        PlatformDataOffset = reader.ReadUInt32();
        SurfaceHeaderOffset = reader.ReadUInt32();
        NameOffset = reader.ReadUInt32();
        PlatformDataSize = reader.ReadUInt32();
        HighResolutionMipCount = reader.ReadUInt32();
        HighResolutionBtexSize = reader.ReadUInt32();
        _ = reader.ReadUInt64(); // Padding
        TileMode = reader.ReadUInt32();
        ArrayCount = reader.ReadUInt32();

        // Read mipmap information for PC textures
        if (_platform == BlackTexturePlatform.PLATFORM_WIIU && _game != LuminousGame.Forspoken)
        {
            for (var i = 0; i < ArrayCount; i++)
            {
                var mips = new List<BlackTextureMipData>();
                for (var j = 0; j < MipCount; j++)
                {
                    mips.Add(new BlackTextureMipData
                    {
                        Offset = reader.ReadUInt32(),
                        Size = reader.ReadUInt32()
                    });
                }

                Mips.Add(mips);
            }
        }

        if (_game != LuminousGame.Forspoken)
        {
            FileName = reader.ReadNullTerminatedString();
        }

        reader.Align(_alignment);

        // Handle platform differences
        if (_platform == BlackTexturePlatform.PLATFORM_PS4)
        {
            if (reader.BaseStream.Length <= 512)
            {
                return;
            }

            Format = BlackTexturePixelFormat.B8G8R8A8_UNORM;
            GnfHeader = new GnfHeader();
            PixelData = GnfHeader.Read(reader, ArrayCount);
        }
        else if (_game == LuminousGame.FFXV)
        {
            var imageStart = reader.BaseStream.Position;

            using var stream = new MemoryStream();
            for (var i = 0; i < ArrayCount; i++)
            {
                for (var j = 0; j < MipCount; j++)
                {
                    var mip = Mips[i][j];
                    reader.BaseStream.Seek(imageStart + mip.Offset, SeekOrigin.Begin);
                    reader.BaseStream.CopyTo(stream, mip.Size);
                }
            }

            PixelData = stream.ToArray();
        }
        else if (_game == LuminousGame.Forspoken)
        {
            var imageStart = reader.BaseStream.Position;
            var offset = 0;
            using var stream = new MemoryStream();

            try
            {
                for (var a = 0; a < ArrayCount; a++)
                {
                    var format = TexturePixelFormatMap.Instance[Format];
                    var formatBlockSize = DirectDrawSurface.GetBlockSize(format);
                    uint width = Width;
                    uint height = Height;

                    for (var i = 0; i < MipCount; i++)
                    {
                        var mipSize = (int)((width + 3) / 4) * (int)((height + 3) / 4) * formatBlockSize;
                        var size = (uint)Math.Max(formatBlockSize, mipSize);

                        var blockSize = size == formatBlockSize
                            ? formatBlockSize
                            : Math.Min(256, Math.Max(formatBlockSize, (int)size / (int)(Math.Min(width, height) / 4)));

                        var blockCount = size / blockSize;

                        for (var j = 0; j < blockCount; j++)
                        {
                            reader.BaseStream.Seek(imageStart + offset, SeekOrigin.Begin);
                            reader.BaseStream.CopyTo(stream, (long)blockSize);

                            offset += blockSize;

                            if (size == formatBlockSize && j > blockCount - 4)
                            {
                                offset += 512 - blockSize;
                            }
                            else
                            {
                                offset += 256 - blockSize;
                            }
                        }

                        width /= 2;
                        height /= 2;
                    }
                }
            }
            catch
            {
                // Cringe to avoid dealing with stupid BTEX edge-cases with Forspoken direct storage nonsense
                ArrayCount = 1;
                MipCount = 1;
            }

            PixelData = stream.ToArray();
        }
    }

    public (uint MetadataSize, uint PixelDataSize) Write(BinaryWriter writer)
    {
        var metadataStart = writer.BaseStream.Position;
        writer.Write(Width);
        writer.Write(Height);
        writer.Write(Pitch);
        writer.Write((ushort)Format);
        writer.Write(MipCount);
        writer.Write(Depth);
        writer.Write(DimensionCount);
        writer.Write((byte)ImageFlags);
        writer.Write(SurfaceCount);
        writer.Write(SurfaceHeaderStride);
        writer.Write(PlatformDataOffset);
        writer.Write(SurfaceHeaderOffset);

        var nameOffsetOffset = writer.BaseStream.Position;
        writer.Write(0u); // Will come back to write this

        writer.Write(PlatformDataSize);
        writer.Write(HighResolutionMipCount);
        writer.Write(HighResolutionBtexSize);
        writer.Write(0UL); // Padding
        writer.Write(TileMode);
        writer.Write(ArrayCount);

        if (_game != LuminousGame.Forspoken)
        {
            for (var i = 0; i < ArrayCount; i++)
            {
                for (var j = 0; j < MipCount; j++)
                {
                    writer.Write(Mips[i][j].Offset);
                    writer.Write(Mips[i][j].Size);
                }
            }
        }

        // Go back and write the name offset
        var returnAddress = writer.BaseStream.Position;
        NameOffset = (uint)(returnAddress - metadataStart);
        writer.BaseStream.Seek(nameOffsetOffset, SeekOrigin.Begin);
        writer.Write(NameOffset);
        writer.BaseStream.Seek(returnAddress, SeekOrigin.Begin);

        writer.WriteNullTerminatedString(FileName);
        writer.Align(128, 0x00);
        var metadataSize = writer.BaseStream.Position - metadataStart;

        var pixelDataStart = writer.BaseStream.Position;

        if (_game == LuminousGame.Forspoken)
        {
            // Was already sliced up by the texture converter unlike FFXV
            writer.Write(PixelData);
        }
        else
        {
            var bufferOffset = 0ul;
            for (var i = 0; i < ArrayCount; i++)
            {
                for (var j = 0; j < MipCount; j++)
                {
                    writer.Write(PixelData, (int)bufferOffset, (int)Mips[i][j].Size);
                    bufferOffset += Mips[i][j].Size;
                }

                writer.Align(128, 0x00);
            }
        }

        var pixelDataSize = writer.BaseStream.Position - pixelDataStart;

        return ((uint)metadataSize, (uint)pixelDataSize);
    }
}