namespace Flagrum.Core.Archive.DataSource;

public interface IArchiveFileDataSource
{
    public uint Size { get; }
    public byte[] GetData();
}