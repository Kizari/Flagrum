using Flagrum.Abstractions.Archive;
using Flagrum.Abstractions.ModManager.Instructions;
using Flagrum.Abstractions.ModManager.Project;
using Flagrum.Application.Features.ModManager.Instructions.Abstractions;
using MemoryPack;

namespace Flagrum.Application.Features.ModManager.Instructions;

[MemoryPackable]
public partial class RemovePackedFileBuildInstruction : PackedBuildInstruction, IRemovePackedFileBuildInstruction
{
    public override bool ShouldShowInBuildList => true;

    public override void Apply(IFlagrumProject mod, IEbonyArchive archive, IFlagrumProjectArchive projectArchive)
    {
        var deleted = "deleted"u8.ToArray();
        archive.AddFile(Uri, EbonyArchiveFileFlags.Autoload | EbonyArchiveFileFlags.PatchedDeleted, deleted);
    }

    public override void Revert(IEbonyArchive archive, IFlagrumProjectArchive projectArchive)
    {
        if (archive.HasFile(Uri))
        {
            archive.RemoveFile(Uri);
        }
    }
}