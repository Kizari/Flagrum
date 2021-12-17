using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Flagrum.Core.Gfxbin.Btex;

public enum TextureType
{
    Color,
    Greyscale,
    Normal,
    Thumbnail
}

public static class BtexConverter
{
    public static byte[] DdsToBtex(TextureType type, string fileName, byte[] dds)
    {
        var ddsHeader = ReadDdsHeader(dds, out var ddsContent);
        var btexHeader = new BtexHeader
        {
            Height = (ushort)ddsHeader.Height,
            Width = (ushort)ddsHeader.Width,
            Pitch = CalculatePitch(ddsHeader.Width, type),
            Depth = (byte)ddsHeader.Depth,
            MipMapCount = (byte)ddsHeader.MipMapCount,
            ArraySize = (ushort)ddsHeader.DX10Header.ArraySize,
            Format = DX10FormatToBtexFormat(ddsHeader.DX10Header.Format),
            Dimension = (byte)(ddsHeader.DX10Header.ResourceDimension - 1u),
            Data = ddsContent,
            p_ImageFileSize = (uint)ddsContent.Length,
            p_ImageFlags = ImageFlagsForType(type),
            p_Name = fileName,
            p_SurfaceCount = (ushort)ddsHeader.MipMapCount
        };

        int width = btexHeader.Width;
        int height = btexHeader.Height;
        
        for (var i = 0; i < btexHeader.MipMapCount; i++)
        {
            btexHeader.MipMaps.Add(new BtexMipMap
            {
                Offset = i == 0 ? 0 : btexHeader.MipMaps[i - 1].Offset + btexHeader.MipMaps[i - 1].Size,
                Size = (uint)Math.Max(GetBlockSize(type) * 4, CalculatePitch((uint)width, type) * height)
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
        writer.Write(header.Format);
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

        memoryStream.Seek(3, SeekOrigin.Current);   // Padding

        writer.Write(header.p_HighTextureDataSizeByte);

        memoryStream.Seek(8, SeekOrigin.Current);   // Padding

        writer.Write(header.p_TileMode);
        writer.Write(header.ArraySize);

        memoryStream.Seek(header.p_ImageHeaderOffset + header.p_NameOffset, SeekOrigin.Begin);

        writer.Write(Encoding.UTF8.GetBytes(header.p_Name));
        writer.Write(0x00);    // Null-terminated string

        memoryStream.Seek(header.HeaderSize, SeekOrigin.Begin);
        
        writer.Write(header.Data);
        return memoryStream.ToArray();
    }

    private static ushort DX10FormatToBtexFormat(uint dx10Format) => dx10Format switch
    {
        28 => 0x0B,
        74 => 0x19,
        77 => 0x1A,
        80 => 0x21,
        83 => 0x22,
        95 => 0x23,
        98 => 0x24,
        71 => 0x18,
        _ => 0x18
    };

    private static byte ImageFlagsForType(TextureType type) => type switch
    {
        TextureType.Color => 49,
        _ => 17
    };

    private static ushort CalculatePitch(uint width, TextureType type)
    {
        return (ushort)(Math.Max(1, ((width + 3) / 4) * GetBlockSize(type)));
    }
    
    private static int GetBlockSize(TextureType type) => type switch
    {
        TextureType.Color => 2,
        TextureType.Greyscale => 2,
        _ => 4
    };

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
        for (int i = 0; i < 11; i++)
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
            Format = reader.ReadUInt32(),
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

    public static void Convert(string btexConverterPath, string inPath, string outPath, TextureType type)
    {
        var process = new Process();
        var startInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/C {btexConverterPath} {GetArgsForType(type, inPath, outPath)}",
            CreateNoWindow = true,
            UseShellExecute = false
        };

        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit();
    }

    private static string GetArgsForType(TextureType type, string inPath, string outPath)
    {
        var args = $"-p \"dx11\" --composite --outbtex \"{outPath}\" --adapter 0 ";

        switch (type)
        {
            case TextureType.Normal:
                args += "--out_format BC5 --in_linear ";
                break;
            case TextureType.Greyscale:
                args += "--out_format BC4 --in_linear ";
                break;
            case TextureType.Color:
                args += "--out_format DXT1 --in_srgb --out_srgb ";
                break;
            case TextureType.Thumbnail:
                args += "--in_srgb --out_srgb ";
                break;
        }

        args += $"\"{inPath}\"";
        return args;
    }
}