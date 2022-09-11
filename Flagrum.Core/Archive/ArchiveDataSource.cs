using System.IO;

namespace Flagrum.Core.Archive;

public abstract class ArchiveDataSource
{
    public abstract Stream GetDataStream();
}