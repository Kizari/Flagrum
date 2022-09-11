using System.IO;

namespace Flagrum.Core.Archive;

/// <summary>
/// This data source only holds the file path in-memory so the data can be quickly read and dumped during output
/// </summary>
public class FileArchiveDataSource : ArchiveDataSource
{
    private readonly string _path;
    
    public FileArchiveDataSource(string path)
    {
        _path = path;
    }
    
    public override Stream GetDataStream()
    {
        return new FileStream(_path, FileMode.Open, FileAccess.Read);
    }
}