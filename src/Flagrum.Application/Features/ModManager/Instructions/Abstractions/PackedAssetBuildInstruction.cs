using System.Collections.Concurrent;
using System.Threading.Tasks;
using Flagrum.Abstractions.ModManager.Instructions;
using Flagrum.Core.Archive;
using Flagrum.Application.Features.ModManager.Mod;
using Flagrum.Application.Features.ModManager.Services;
using Flagrum.Application.Utilities;
using MemoryPack;

namespace Flagrum.Application.Features.ModManager.Instructions.Abstractions;

public abstract class PackedAssetBuildInstruction : PackedBuildInstruction, IPackedAssetBuildInstruction
{
    [FactoryInject] protected AssetConverter AssetConverter { get; set; }
    public long FileLastModified { get; set; }

    [MemoryPackIgnore] public string DataSource { get; set; }

    public string FilePath { get; set; }

    public virtual Task<FmodFragment> Build(EbonyArchive sourceArchive, ConcurrentDictionary<string, byte[]> imageMap)
    {
        var data = imageMap.TryGetValue(Uri, out var value)
            ? value
            : AssetConverter.Convert(this);

        var processedData =
            EbonyArchiveFile.GetProcessedData(Uri, Flags, data, 0, true,
                out var archiveFile);

        return Task.FromResult(new FmodFragment
        {
            OriginalSize = (uint)data.Length,
            ProcessedSize = (uint)processedData.Length,
            Flags = archiveFile.Flags,
            Key = archiveFile.Key,
            RelativePath = archiveFile.RelativePath,
            Data = processedData
        });
    }
}