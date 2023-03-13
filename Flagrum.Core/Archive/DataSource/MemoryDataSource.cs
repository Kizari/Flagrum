namespace Flagrum.Core.Archive.DataSource;

public class MemoryDataSource : IArchiveFileDataSource
{
    private readonly byte[] _buffer;
    
    public MemoryDataSource(byte[] buffer)
    {
        _buffer = buffer;
    }

    public uint Size => (uint)_buffer.Length;
    
    public byte[] GetData()
    {
        return _buffer;
    }
}