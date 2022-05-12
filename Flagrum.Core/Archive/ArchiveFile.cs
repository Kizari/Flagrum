using System;
using System.IO;
using System.IO.Compression;
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
    NoEarc = 8,             // Not sure what this means, possibly copyguard? (i.e. ffxvbinmod)
    Patched = 16,
    PatchedDeleted = 32,
    Encrypted = 64,
    MaskProtected = 128
}

public class ArchiveFile
{
    public const uint HeaderSize = 40;
    public const ulong HeaderHash = 14695981039346656037;
    public const ulong MasterChunkKeyA = unchecked(0x10E64D70C2A29A69);
    public const ulong MasterChunkKeyB = unchecked(0xC63D3dC167E);
    private byte[] _buffer;

    public ArchiveFile() { }

    public ArchiveFile(string uri)
    {
        RelativePath = uri.Replace("data://", "");

        if (RelativePath.EndsWith(".tga") || RelativePath.EndsWith(".tif") || RelativePath.EndsWith(".dds") ||
            RelativePath.EndsWith(".png"))
        {
            RelativePath = RelativePath
                .Replace(".tga", ".btex")
                .Replace(".tif", ".btex")
                .Replace(".dds", ".btex")
                .Replace(".png", ".btex");
        }

        if (RelativePath.EndsWith(".gmtl"))
        {
            RelativePath = RelativePath.Replace(".gmtl", ".gmtl.gfxbin");
        }
        else if (RelativePath.EndsWith(".gmdl"))
        {
            RelativePath = RelativePath.Replace(".gmdl", ".gmdl.gfxbin");
        }

        var newUri = uri
            .Replace(".gmdl.gfxbin", ".gmdl")
            .Replace(".gmtl.gfxbin", ".gmtl")
            .Replace(".exml", ".ebex");

        Uri = newUri;

        var tokens = RelativePath.Split('\\', '/');
        var fileName = tokens.Last();
        var index = fileName.IndexOf('.');
        var type = index < 0 ? "" : fileName.Substring(index + 1);

        UriHash = Cryptography.Hash64(Uri);
        TypeHash = Cryptography.Hash64(type);

        UriAndTypeHash = (ulong)(((long)UriHash & 17592186044415L) | (((long)TypeHash << 44) & -17592186044416L));

        Flags = GetDefaultBinmodFlags();
    }

    public ulong TypeHash { get; set; }
    public ulong UriAndTypeHash { get; set; }
    public string Uri { get; set; }
    public ulong UriHash { get; set; }
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

    public byte[] GetUnencryptedData()
    {
        return _buffer;
    }

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

    private ArchiveFileFlag GetDefaultBinmodFlags()
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

    /// <summary>
    ///     This is needed if using parameterless constructor to deconstruct the UriAndTypeHash
    /// </summary>
    public void DeconstructUriAndTypeHash()
    {
        var tokens = RelativePath.Split('\\', '/');
        var fileName = tokens.Last();
        var index = fileName.IndexOf('.');
        var type = index < 0 ? "" : fileName.Substring(index + 1);

        UriHash = Cryptography.Hash64(Uri);
        TypeHash = Cryptography.Hash64(type);
    }
}