using System;
using System.IO;
using System.Linq;
using Flagrum.Core.Utilities;

namespace Flagrum.Archiver.Data
{
    [Flags]
    public enum ArchiveFileFlag : uint
    {
        None = 0,
        Autoload = 1,
        Compressed = 2,
        Reference = 4,
        Patched = 16,
        PatchedDeleted = 32,
        Encrypted = 64,
        MaskProtected = 128
    }

    public class ArchiveFile
    {
        public const uint HeaderSize = 40;
        public const ulong HeaderHash = 14695981039346656037;
        private ushort _key;

        public ArchiveFile() { }

        public ArchiveFile(string uri)
        {
            RelativePath = uri.Replace("data://", "");

            if (RelativePath.EndsWith(".tga") || RelativePath.EndsWith(".tif"))
            {
                RelativePath = RelativePath.Replace(".tga", ".btex").Replace(".tif", ".btex");
            }

            var newUri = uri
                .Replace(".gmdl.gfxbin", ".fbx")
                .Replace(".gmtl.gfxbin", ".gmtl")
                .Replace(".exml", ".ebex");

            if (!uri.StartsWith("data://mod"))
            {
                if (uri.StartsWith("data://shader/defaulttextures/wetness"))
                {
                    newUri = newUri.Replace(".btex", ".tga");
                }
                else
                {
                    newUri = newUri.Replace(".btex", ".tif");
                }
            }
            else
            {
                //newUri = newUri.Replace(".btex", ".png");
            }

            Uri = newUri;

            var tokens = uri.Split('\\', '/');
            var fileName = tokens.Last();
            var index = fileName.IndexOf('.');
            var type = index < 0 ? "" : fileName.Substring(index + 1);

            UriHash = Cryptography.Hash(Uri);
            TypeHash = Cryptography.Hash(type);

            UriAndTypeHash = (ulong)(((long)UriHash & 17592186044415L) | (((long)TypeHash << 44) & -17592186044416L));

            Flags = GetDefaultBinmodFlags();
        }

        public ulong TypeHash { get; }
        public ulong UriAndTypeHash { get; set; }
        public string Uri { get; set; }
        public ulong UriHash { get; }
        public uint UriOffset { get; set; }
        public string RelativePath { get; }
        public uint RelativePathOffset { get; set; }
        public ulong DataOffset { get; set; }
        public uint Size { get; set; }
        public uint ProcessedSize { get; set; }
        public ArchiveFileFlag Flags { get; set; }
        public byte LocalizationType { get; set; }
        public byte Locale { get; set; }

        public ushort Key
        {
            get
            {
                if (Uri != null && (Uri.EndsWith(".modmeta") || Uri.EndsWith(".bin")))
                {
                    var hash = Uri.GetHashCode();
                    var key = (hash >> 16) ^ hash;
                    return (ushort)(key == 0 ? 57005 : key);
                }

                return _key;
            }

            set => _key = value;
        }

        protected virtual byte[] Data => Array.Empty<byte>();

        public byte[] GetData()
        {
            var data = Data;
            Size = (uint)data.Length;

            if (Flags.HasFlag(ArchiveFileFlag.Encrypted))
            {
                var encryptedData = Cryptography.Encrypt(data);
                ProcessedSize = (uint)encryptedData.Length;
                return encryptedData;
            }

            ProcessedSize = Size;
            return data;
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
}