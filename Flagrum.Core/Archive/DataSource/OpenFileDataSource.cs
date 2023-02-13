using System.IO;

namespace Flagrum.Core.Archive.DataSource;

public class OpenFileDataSource : IArchiveFileDataSource
{
    private readonly Stream _stream;
    private readonly ulong _offset;
    private readonly uint _size;
    
    public OpenFileDataSource(Stream stream, ulong offset, uint size)
    {
        _stream = stream;
        _offset = offset;
        _size = size;
    }

    public uint Size => _size;
    
    public byte[] GetData()
    {
        var buffer = new byte[_size];
        
        lock (_stream)
        {
            _stream.Seek((long)_offset, SeekOrigin.Begin);
            _ = _stream.Read(buffer);
        }

        return buffer;
    }
}