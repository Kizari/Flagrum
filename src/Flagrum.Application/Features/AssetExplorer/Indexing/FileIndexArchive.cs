using System.Collections.Generic;
using MemoryPack;

namespace Flagrum.Application.Features.AssetExplorer.Indexing;

[MemoryPackable(GenerateType.CircularReference, SerializeLayout.Sequential)]
public partial class FileIndexArchive
{
    public string RelativePath { get; set; }
    public List<FileIndexFile> Files { get; set; }
}