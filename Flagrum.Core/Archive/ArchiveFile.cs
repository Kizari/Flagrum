using System;
using System.Linq;
using Flagrum.Core.Utilities;

namespace Flagrum.Core.Archive;

[Flags]
public enum ArchiveFileFlag : uint
{
    None = 0,
    Autoload = 1,
    Compressed = 2,
    Reference = 4,
    Copyguard = 8,
    Patched = 16,
    PatchedDeleted = 32,
    Encrypted = 64,
    MaskProtected = 128
}

public class ArchiveFile
{
    public const uint HeaderSize = 40;
    public const ulong HeaderHash = 14695981039346656037;
    private byte[] _buffer;

    public ArchiveFile() { }

    public ArchiveFile(string uri)
    {
        RelativePath = uri.Replace("data://", "");

        if (RelativePath.EndsWith(".tga") || RelativePath.EndsWith(".tif") || RelativePath.EndsWith(".dds"))
        {
            RelativePath = RelativePath
                .Replace(".tga", ".btex")
                .Replace(".tif", ".btex")
                .Replace(".dds", ".btex");
        }

        var newUri = uri
            .Replace(".gmdl.gfxbin", ".fbx")
            .Replace(".gmtl.gfxbin", ".gmtl")
            .Replace(".exml", ".ebex");

        Uri = newUri;

        var tokens = uri.Split('\\', '/');
        var fileName = tokens.Last();
        var index = fileName.IndexOf('.');
        var type = index < 0 ? "" : fileName.Substring(index + 1);

        UriHash = Cryptography.Hash64(Uri);
        TypeHash = Cryptography.Hash64(type);

        UriAndTypeHash = (ulong)(((long)UriHash & 17592186044415L) | (((long)TypeHash << 44) & -17592186044416L));

        Flags = GetDefaultBinmodFlags();
    }

    public ulong TypeHash { get; }
    public ulong UriAndTypeHash { get; set; }
    public string Uri { get; set; }
    public ulong UriHash { get; }
    public uint UriOffset { get; set; }
    public string RelativePath { get; set; }
    public uint RelativePathOffset { get; set; }
    public ulong DataOffset { get; set; }
    public uint Size { get; set; }
    public uint ProcessedSize { get; set; }
    public ArchiveFileFlag Flags { get; set; }
    public byte LocalizationType { get; set; }
    public byte Locale { get; set; }

    public ushort Key { get; set; }

    public bool HasData => _buffer?.Length > 0;

    public byte[] GetData()
    {
        Size = (uint)_buffer.Length;

        if (Flags.HasFlag(ArchiveFileFlag.Encrypted))
        {
            var encryptedData = Cryptography.Encrypt(_buffer);
            ProcessedSize = (uint)encryptedData.Length;
            return encryptedData;
        }

        ProcessedSize = Size;
        return _buffer;
    }

    public void SetData(byte[] data)
    {
        _buffer = data;
    }

    public ArchiveFileFlag GetDefaultBinmodFlags()
    {
        var flags = ArchiveFileFlag.Autoload;

        if (Uri.EndsWith(".modmeta") || Uri.EndsWith(".bin"))
        {
            flags |= ArchiveFileFlag.MaskProtected;
        }
        else
        {
            flags |= ArchiveFileFlag.Encrypted;
        }

        return flags;
    }
}