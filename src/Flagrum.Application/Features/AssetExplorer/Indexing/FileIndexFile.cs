using MemoryPack;

namespace Flagrum.Application.Features.AssetExplorer.Indexing;

[MemoryPackable(GenerateType.CircularReference, SerializeLayout.Sequential)]
public partial class FileIndexFile
{
    public string Uri { get; set; }
    public FileIndexArchive Archive { get; set; }
}