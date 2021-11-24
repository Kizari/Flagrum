namespace Flagrum.Archiver.Data
{
    public class ArchiveMemoryFile : ArchiveFile
    {
        private byte[] _data;

        public ArchiveMemoryFile() { }

        public ArchiveMemoryFile(byte[] data, string uri)
            : base(uri)
        {
            _data = data;
        }

        protected override byte[] Data => _data;

        public void SetData(byte[] data)
        {
            _data = data;
        }
    }
}