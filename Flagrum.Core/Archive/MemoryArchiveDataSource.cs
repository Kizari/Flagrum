using System.IO;

namespace Flagrum.Core.Archive;

public class MemoryArchiveDataSource : ArchiveDataSource
{
    private readonly byte[] _buffer;

    public MemoryArchiveDataSource(byte[] buffer)
    {
        _buffer = buffer;
    }

    public override Stream GetDataStream()
    {
        return new MemoryStream(_buffer);
    }
}