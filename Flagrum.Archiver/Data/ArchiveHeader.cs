namespace Flagrum.Archiver.Data
{
    public class ArchiveHeader
    {
        public const uint ArchiveVersion = 196628;
        public const uint ProtectedArchiveVersion = 2147483648;
        public const uint Size = 64;
        public const uint ChunkSize = 128;

        public static byte[] Tag { get; } = System.Text.Encoding.UTF8.GetBytes("CRAF");

        public uint Version { get; }
        public uint UriListOffset { get; set; }
        public uint PathListOffset { get; set; }
        public uint DataOffset { get; set; }
        public ulong Hash { get; set; }

        public ArchiveHeader()
        {
            Version = ProtectedArchiveVersion | ArchiveVersion;
        }
    }
}
