using System.IO;
using Flagrum.Core.Utilities.Extensions;

namespace Flagrum.Core.Archive.DataSource;

public class OpenFileDataSource : IEbonyArchiveFileDataSource
{
    private readonly ulong _offset;
    private readonly Stream _stream;

    public OpenFileDataSource(Stream stream, ulong offset, uint size)
    {
        _stream = stream;
        _offset = offset;
        Size = size;
    }

    public uint Size { get; }

    public byte[] GetData()
    {
        var buffer = new byte[Size];

        lock (_stream)
        {
            _stream.Seek((long)_offset, SeekOrigin.Begin);
            _ = _stream.Read(buffer);
        }

        return buffer;
    }
}