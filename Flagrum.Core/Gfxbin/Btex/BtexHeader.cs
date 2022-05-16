using System.Collections.Generic;
using System.IO;
using System.Text;
using Flagrum.Core.Utilities;

namespace Flagrum.Core.Gfxbin.Btex;

public class BtexMipMap
{
    public uint Offset { get; set; } = 0;
    public uint Size { get; set; } = 0;
}

public class SedbBtexHeader
{
    public const string Tag = "SEDBbtex";
    public const ulong Version = 36028797018963968;
    
    public ulong FileSize { get; set; }

    public byte[] ToBytes()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8);
        
        writer.Write(Encoding.UTF8.GetBytes(Tag));
        writer.Write(Version);
        writer.Write(FileSize);

        var padding = new byte[128 - stream.Position];
        writer.Write(padding);
        
        return stream.ToArray();
    }
}

public class BtexHeader
{
    public uint p_ImageFileSize = 0,
        p_ImageHeaderOffset = 32,
        p_PlatformDataOffset = 0,
        p_SurfaceHeaderOffset = 56,
        p_NameOffset = 160,
        p_PlatformDataSize = 0,
        p_HighTextureDataSizeByte = 0,
        p_TileMode = 0,
        p_SurfaceOffset = 0,
        p_SurfaceSize = 0;

    public string p_Name = string.Empty;

    public byte p_Platform = 6,
        p_Flags = 1,
        p_ImageFlags = 0,
        p_HighTextureMipLevels = 0;

    public List<BtexMipMap> MipMaps { get; set; } = new();

    public ushort p_Version = 4,
        p_ImageCount = 1,
        p_ImageHeaderStride = 56,
        p_SurfaceCount = 13,
        p_SurfaceHeaderStride = 8;

    public uint HeaderSize = 256;
    
    public ushort Height { get; set; } = 0;

    public ushort Width { get; set; } = 0;

    public ushort Pitch { get; set; } = 0;

    public ushort ArraySize { get; set; } = 0;

    public BtexFormat Format { get; set; } = 0;

    public byte Depth { get; set; } = 0;

    public byte MipMapCount { get; set; } = 0;

    public byte Dimension { get; set; } = 0;

    public byte[] Data { get; set; } = null;
}