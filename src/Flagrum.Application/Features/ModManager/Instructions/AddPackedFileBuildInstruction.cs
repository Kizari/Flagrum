using Flagrum.Abstractions.Archive;
using Flagrum.Abstractions.ModManager.Instructions;
using Flagrum.Abstractions.ModManager.Project;
using Flagrum.Core.Utilities;
using Flagrum.Application.Features.ModManager.Instructions.Abstractions;
using Flagrum.Application.Features.ModManager.Mod;
using MemoryPack;

namespace Flagrum.Application.Features.ModManager.Instructions;

[MemoryPackable]
public partial class AddPackedFileBuildInstruction : PackedAssetBuildInstruction, IAddPackedFileBuildInstruction
{
    public override bool ShouldShowInBuildList => Authentication.IsAuthenticated;

    public override void Apply(IFlagrumProject mod, IEbonyArchive archive, IFlagrumProjectArchive projectArchive)
    {
        var hash = Cryptography.HashFileUri64(Uri);
        var cachePath = $@"{Profile.CacheDirectory}\{mod.Identifier}{hash}.ffg";

        var fragment = new FmodFragment();
        fragment.Read(FilePath.EndsWith(".ffg")
            ? FilePath
            : cachePath);

        var flags = projectArchive.Type == ModChangeType.Change
            ? fragment.Flags | EbonyArchiveFileFlags.Patched
            : fragment.Flags;

        var f = (uint)flags;
        if ((f & 2) > 0)
        {
            f |= 256;
        }

        // TODO: Remove this and handle these files properly
        // Or don't, I'm not the police.
        if (Uri.Split('.')[^1].Contains("nav"))
        {
            f &= ~1u;
        }

        archive.AddProcessedFile(Uri, (EbonyArchiveFileFlags)f, fragment.Data,
            fragment.OriginalSize, fragment.Key, fragment.RelativePath);
    }

    public override void Revert(IEbonyArchive archive, IFlagrumProjectArchive projectArchive)
    {
        if (archive.HasFile(Uri))
        {
            archive.RemoveFile(Uri);
        }
    }
}