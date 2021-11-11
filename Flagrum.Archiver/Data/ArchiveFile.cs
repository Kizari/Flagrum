using Flagrum.Archiver.Utilities;
using System;
using System.IO;
using System.Linq;

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
        public const byte LocalizationType = 0;
        public const byte Locale = 0;

        private uint _size;
        private uint _processedSize;
        private string _path;

        public ulong NameHash { get; }
        public ulong TypeHash { get; }
        public ulong UriAndTypeHash { get; }
        public string Uri { get; }
        public ulong UriHash { get; }
        public uint UriOffset { get; set; }
        public string RelativePath { get; }
        public uint RelativePathOffset { get; set; }
        public ulong DataOffset { get; set; }

        public ArchiveFile(string archiveRoot, string path)
        {
            if (path.StartsWith("data://shader"))
            {
                _path = path.Replace("data://", "C:/Users/Kieran/AppData/Local/SquareEnix/FFXVMODTool/LuminousStudio/bin/rt2/pc/").Replace(".tif", ".btex").Replace(".tga", ".btex");
                RelativePath = path.Replace("data://", "").Replace(".tif", ".btex").Replace(".tga", ".btex");
                Uri = path;
            }
            else if (path.EndsWith(".ebex"))
            {
                _path = "C:\\Users\\Kieran\\AppData\\Local\\SquareEnix\\FFXVMODTool\\LuminousStudio\\bin\\rt2\\pc\\$mod\\temp.exml";
                RelativePath = path.Replace("data://", "").Replace(".ebex", ".exml");
                Uri = path;
            }
            else
            {
                _path = path;
                var directory = "mod/" + archiveRoot.Split('\\', '/').Last() + "/";
                RelativePath = directory + path.Substring(archiveRoot.Length).Trim('\\', '/').Replace('\\', '/');
                Uri = InferUri();
            }

            var tokens = _path.Split('\\', '/');
            var fileName = tokens.Last();
            var index = fileName.IndexOf('.');
            var type = index < 0 ? "" : fileName.Substring(index + 1);

            UriHash = Cryptography.Hash(Uri);
            TypeHash = Cryptography.Hash(type);

            UriAndTypeHash = (ulong)((long)UriHash & 17592186044415L | (long)TypeHash << 44 & -17592186044416L);
        }

        public uint Size
        {
            get
            {
                if (_size == 0)
                {
                    throw new InvalidOperationException($"Size cannot be calculated before reading the file. Call {nameof(GetData)} first.");
                }

                return _size;
            }
        }

        public uint ProcessedSize
        {
            get
            {
                if (_processedSize == 0)
                {
                    throw new InvalidOperationException($"ProcessedSize cannot be calculated before reading the file. Call {nameof(GetData)} first.");
                }

                return _processedSize;
            }
        }

        public ArchiveFileFlag Flags
        {
            get
            {
                if (_path == null)
                {
                    throw new InvalidOperationException($"Flags must be calculated after setting {nameof(_path)}.");
                }

                var flags = ArchiveFileFlag.Autoload;

                if (_path.EndsWith(".modmeta") || _path.EndsWith(".bin"))
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

        public ushort Key
        {
            get
            {
                if (_path.EndsWith(".modmeta") || _path.EndsWith(".bin"))
                {
                    var hash = Uri.GetHashCode();
                    var key = hash >> 16 ^ hash;
                    return (ushort)(key == 0 ? 57005 : key);
                }
                else
                {
                    return 0;
                }
            }
        }

        public byte[] GetData()
        {
            var data = File.ReadAllBytes(_path);
            _size = (uint)data.Length;

            if (Flags.HasFlag(ArchiveFileFlag.Encrypted))
            {
                var encryptedData = Cryptography.Encrypt(data);
                _processedSize = (uint)encryptedData.Length;
                return encryptedData;
            }
            else
            {
                _processedSize = _size;
                return data;
            }
        }

        private string InferUri()
        {
            var uri = Path.Combine("data://", RelativePath).Replace('\\', '/').ToLower();
            
            if (uri.EndsWith(".gmtl.gfxbin"))
            {
                return uri.Replace(".gmtl.gfxbin", ".gmtl");
            }
            
            if (uri.EndsWith(".gmdl.gfxbin"))
            {
                return uri.Replace(".gmdl.gfxbin", ".fbx");
            }

            if (uri.EndsWith(".btex"))
            {
                return uri.Replace(".btex", ".png");
            }

            return uri;
        }
    }
}
