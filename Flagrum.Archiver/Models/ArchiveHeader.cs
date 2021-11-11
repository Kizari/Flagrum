using Flagrum.Archiver.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagrum.Archiver.Models
{
    public class ArchiveHeader
    {
        public const uint ArchiveVersion = 196628;
        public const uint ProtectedArchiveVersion = 2147483648;
        public const uint Size = 64;
        public const uint ListingItemSize = 40;

        public byte[] Tag { get; }         // Will always be 4 bytes
        public uint Version { get; }
        public uint UriListOffset { get; set; }
        public uint PathListOffset { get; set; }
        public uint DataOffset { get; set; }
        public uint ChunkSize { get; }
        public ulong Hash { get; set; }

        public ArchiveHeader()
        {
            Tag = System.Text.Encoding.UTF8.GetBytes("CRAF");
            Version = ProtectedArchiveVersion | ArchiveVersion;
            ChunkSize = 128;
        }
    }
}
