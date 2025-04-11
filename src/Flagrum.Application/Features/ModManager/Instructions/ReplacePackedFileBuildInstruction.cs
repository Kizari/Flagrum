using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Flagrum.Abstractions.Archive;
using Flagrum.Abstractions.ModManager.Instructions;
using Flagrum.Abstractions.ModManager.Project;
using Flagrum.Core.Archive;
using Flagrum.Core.Utilities;
using Flagrum.Application.Features.ModManager.Instructions.Abstractions;
using Flagrum.Application.Features.ModManager.Mod;
using MemoryPack;

namespace Flagrum.Application.Features.ModManager.Instructions;

[MemoryPackable]
public partial class ReplacePackedFileBuildInstruction : PackedAssetBuildInstruction, IReplacePackedFileBuildInstruction
{
    public override bool ShouldShowInBuildList => true;

    public override void Apply(IFlagrumProject mod, IEbonyArchive archive, IFlagrumProjectArchive projectArchive)
    {
        var hash = Cryptography.HashFileUri64(Uri);
        var cachePath = $@"{Profile.CacheDirectory}\{mod.Identifier}{hash}.ffg";

        var fragment = new FmodFragment();
        fragment.Read(FilePath.EndsWith(".ffg")
            ? FilePath
            : cachePath);

        var f = (uint)fragment.Flags;

        if ((f & 2) > 0)
        {
            f |= 256;
        }

        archive.AddProcessedFile(Uri, (EbonyArchiveFileFlags)f & ~EbonyArchiveFileFlags.Autoload, fragment.Data,
            fragment.OriginalSize, fragment.Key,
            fragment.RelativePath);
    }

    public override void Revert(IEbonyArchive archive, IFlagrumProjectArchive projectArchive)
    {
        if (archive.HasFile(Uri))
        {
            archive.RemoveFile(Uri);
        }
    }

    public override Task<FmodFragment> Build(EbonyArchive sourceArchive, ConcurrentDictionary<string, byte[]> imageMap)
    {
        if (sourceArchive == null)
        {
            // Skip if 4K pack is missing so Flagrum doesn't crash
            if (Uri.Contains("/highimages/") || Uri.EndsWith("_$h2.autoext"))
            {
                return null;
            }

            throw new Exception($"Could not find earc for {Uri}");
        }

        var original = sourceArchive![Uri];
        var data = imageMap.TryGetValue(Uri, out var value)
            ? value
            : AssetConverter.Convert(this);

        var processedData = EbonyArchiveFile.GetProcessedData(Uri,
            original.Flags,
            data,
            original.Key,
            sourceArchive.IsProtectedArchive,
            out _);

        return Task.FromResult(new FmodFragment
        {
            OriginalSize = (uint)data.Length,
            ProcessedSize = (uint)processedData.Length,
            Flags = original.Flags,
            Key = original.Key,
            RelativePath = original.RelativePath,
            Data = processedData
        });
    }
}