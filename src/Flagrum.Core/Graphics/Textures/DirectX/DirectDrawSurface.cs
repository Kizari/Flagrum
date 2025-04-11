using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Flagrum.Abstractions;
using Flagrum.Core.Graphics.Textures.Luminous;
using Flagrum.Core.Ps4;
using Flagrum.Core.Serialization;
using Flagrum.Core.Utilities.Extensions;

namespace Flagrum.Core.Graphics.Textures.DirectX;

public class DirectDrawSurface : BinaryReaderWriterBase
{
    public char[] Magic { get; set; } = "DDS ".ToCharArray();
    public uint Height { get; set; }
    public uint Width { get; set; }
    public uint Pitch { get; set; }
    public uint Depth { get; set; }
    public uint MipCount { get; set; }
    public DirectDrawSurfaceFlags Flags { get; set; } = DirectDrawSurfaceFlags.Texture;
    public DirectDrawSurfacePixelFormat Format { get; set; }
    public DirectDrawSurfaceDirectX10Header DirectX10Header { get; set; }

    public byte[] PixelData { get; set; }

    public override void Read(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.UTF8);

        Magic = reader.ReadChars(4);
        _ = reader.ReadUInt32(); // Size
        Flags = (DirectDrawSurfaceFlags)reader.ReadUInt32();
        Height = reader.ReadUInt32();
        Width = reader.ReadUInt32();
        Pitch = reader.ReadUInt32();
        Depth = reader.ReadUInt32();
        MipCount = reader.ReadUInt32();
        _ = reader.ReadBytes(44); // Skip reserved uints

        Format = new DirectDrawSurfacePixelFormat
        {
            Size = reader.ReadUInt32(),
            Flags = (DirectDrawSurfacePixelFormatFlags)reader.ReadUInt32(),
            FourCC = reader.ReadUInt32(),
            RgbBitCount = reader.ReadUInt32(),
            RBitMask = reader.ReadUInt32(),
            GBitMask = reader.ReadUInt32(),
            BBitMask = reader.ReadUInt32(),
            ABitMask = reader.ReadUInt32()
        };

        _ = reader.ReadUInt32(); // Caps
        _ = reader.ReadUInt32(); // Caps2
        _ = reader.ReadUInt32(); // Caps3
        _ = reader.ReadUInt32(); // Caps4
        _ = reader.ReadUInt32(); // Reserved

        DirectX10Header = new DirectDrawSurfaceDirectX10Header
        {
            Format = (DxgiFormat)reader.ReadUInt32(),
            ResourceDimension = reader.ReadUInt32(),
            MiscFlags = reader.ReadUInt32(),
            ArraySize = reader.ReadUInt32(),
            MiscellaneousFlags2 = (DirectDrawSurfaceDirectX10MiscellaneousFlags2)reader.ReadUInt32()
        };

        var contentSize = stream.Length - stream.Position;
        PixelData = new byte[contentSize];
        _ = reader.Read(PixelData);
    }

    public override void Write(Stream stream)
    {
        using var writer = new BinaryWriter(stream, Encoding.UTF8);

        writer.Write(Magic);
        writer.Write(124); // Size
        writer.Write((uint)Flags);
        writer.Write(Height);
        writer.Write(Width);
        writer.Write(Pitch);
        writer.Write(Depth);
        writer.Write(MipCount);
        writer.Write(new byte[44]); // Padding
        writer.Write(Format.Size);
        writer.Write((uint)Format.Flags);
        writer.Write(Format.FourCC);
        writer.Write(Format.RgbBitCount);
        writer.Write(Format.RBitMask);
        writer.Write(Format.GBitMask);
        writer.Write(Format.BBitMask);
        writer.Write(Format.ABitMask);
        writer.Write((int)DirectDrawSurfaceCaps.Texture);
        writer.Write(0); // Caps2
        writer.Write(0); // Caps3
        writer.Write(0); // Caps4
        writer.Write(0); // Reserved
        writer.Write((uint)DirectX10Header.Format);
        writer.Write(DirectX10Header.ResourceDimension);
        writer.Write(DirectX10Header.MiscFlags);
        writer.Write(DirectX10Header.ArraySize);
        writer.Write((uint)DirectX10Header.MiscellaneousFlags2);
        writer.Write(PixelData);
    }

    public BlackTexture ToBtex(LuminousGame game, string name, BlackTextureImageFlags imageFlags, bool isCompressed)
    {
        var btex = new BlackTexture(game)
        {
            ImageData =
            [
                new BlackTextureImageData(game, BlackTexturePlatform.PLATFORM_WIIU)
                {
                    Width = (ushort)Width,
                    Height = (ushort)Height,
                    Pitch = (ushort)CalculatePitch(Width, DirectX10Header.Format, isCompressed),
                    Format = TexturePixelFormatMap.Instance[DirectX10Header.Format],
                    MipCount = (byte)MipCount,
                    Depth = (byte)Depth,
                    DimensionCount = (byte)(DirectX10Header.ResourceDimension - 1u),
                    ImageFlags = imageFlags,
                    SurfaceCount = (ushort)(MipCount * DirectX10Header.ArraySize),
                    ArrayCount = DirectX10Header.ArraySize,
                    FileName = name,
                    PixelData = PixelData
                }
            ]
        };

        // Calculate mipmap data
        if (game == LuminousGame.FFXV)
        {
            var offset = 0u;
            var image = btex.ImageData[0];

            for (var i = 0; i < image.ArrayCount; i++)
            {
                var mips = new List<BlackTextureMipData>();
                var width = Width;
                var height = Height;

                for (var j = 0; j < MipCount; j++)
                {
                    var mip = new BlackTextureMipData
                    {
                        Offset = offset,
                        Size = CalculateMipSize(width, height, DirectX10Header.Format, isCompressed)
                    };

                    mips.Add(mip);

                    offset += mip.Size;
                    width /= 2;
                    height /= 2;
                }

                var alignment = SerializationHelper.Align(offset, 128);
                if (alignment != 128)
                {
                    offset += alignment;
                }

                image.Mips.Add(mips);
            }
        }
        else if (game == LuminousGame.Forspoken)
        {
            var image = btex.ImageData[0];

            var offset = 0u;
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);

            for (var a = 0; a < image.ArrayCount; a++)
            {
                var formatBlockSize = GetBlockSize(TexturePixelFormatMap.Instance[image.Format]);
                uint width = image.Width;
                uint height = image.Height;

                for (var i = 0; i < image.MipCount; i++)
                {
                    var mipSize = (int)((width + 3) / 4) * (int)((height + 3) / 4) * formatBlockSize;
                    var size = (uint)Math.Max(formatBlockSize, mipSize);

                    var blockSize = size == formatBlockSize
                        ? formatBlockSize
                        : Math.Min(256, Math.Max(formatBlockSize, (int)size / (int)(Math.Max(width, height) / 4)));

                    var blockCount = size / blockSize;

                    for (var j = 0; j < blockCount; j++)
                    {
                        writer.Write(image.PixelData, (int)offset, blockSize);
                        offset += (uint)blockSize;

                        if (a == image.ArrayCount - 1 && i == image.MipCount - 1)
                        {
                            image.PlatformDataSize = (uint)stream.Position;
                            writer.Align(256, 0x00);
                            btex.ImageDataSize = (uint)stream.Position;
                        }
                        else if (size == formatBlockSize && j > blockCount - 4)
                        {
                            var paddingSize = 512 - blockSize;
                            if (paddingSize > 0)
                            {
                                writer.Write(new byte[paddingSize]);
                            }
                        }
                        else
                        {
                            var paddingSize = 256 - blockSize;
                            if (paddingSize > 0)
                            {
                                writer.Write(new byte[paddingSize]);
                            }
                        }
                    }

                    width /= 2;
                    height /= 2;
                }
            }

            image.PixelData = stream.ToArray();
        }

        return btex;
    }

    private static uint CalculateMipSize(uint width, uint height, DxgiFormat format, bool isCompressed) =>
        isCompressed
            ? (uint)Math.Max(8, (int)((width + 3) / 4) * (int)((height + 3) / 4) * GetBlockSize(format))
            : CalculatePitch(width, format, false) * height;

    private static uint CalculatePitch(uint width, DxgiFormat format, bool isCompressed) =>
        isCompressed
            ? (uint)Math.Max(1, (width + 3) / 4 * (GetBlockSize(format) / 4))
            : width * GetBytesPerPixel(format);

    public static uint GetBytesPerPixel(DxgiFormat format)
    {
        return format switch
        {
            _ => 4 // This may come back to bite me in the ass :)))))
        };
    }

    public static int GetBlockSize(DxgiFormat format)
    {
        return format switch
        {
            DxgiFormat.BC1_UNORM or DxgiFormat.BC1_UNORM_SRGB or DxgiFormat.BC1_TYPELESS => 8,
            DxgiFormat.BC2_UNORM or DxgiFormat.BC2_UNORM_SRGB or DxgiFormat.BC2_TYPELESS => 16,
            DxgiFormat.BC3_UNORM or DxgiFormat.BC3_UNORM_SRGB or DxgiFormat.BC3_TYPELESS => 16,
            DxgiFormat.BC4_UNORM or DxgiFormat.BC4_SNORM or DxgiFormat.BC4_TYPELESS => 8,
            DxgiFormat.BC5_UNORM or DxgiFormat.BC5_SNORM or DxgiFormat.BC5_TYPELESS => 16,
            DxgiFormat.BC6H_UF16 or DxgiFormat.BC6H_SF16 or DxgiFormat.BC6H_TYPELESS => 16,
            DxgiFormat.BC7_UNORM or DxgiFormat.BC7_UNORM_SRGB or DxgiFormat.BC7_TYPELESS => 16,
            _ => 16 // This may come back to bite me in the ass :)))))
        };
    }
}