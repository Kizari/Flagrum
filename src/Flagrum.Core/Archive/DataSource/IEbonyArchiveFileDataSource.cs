namespace Flagrum.Core.Archive.DataSource;

public interface IEbonyArchiveFileDataSource
{
    public uint Size { get; }
    public byte[] GetData();
}