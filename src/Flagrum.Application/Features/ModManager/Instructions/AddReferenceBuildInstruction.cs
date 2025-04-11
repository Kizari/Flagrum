using Flagrum.Abstractions.Archive;
using Flagrum.Abstractions.ModManager.Instructions;
using Flagrum.Abstractions.ModManager.Project;
using Flagrum.Application.Features.ModManager.Instructions.Abstractions;
using MemoryPack;

namespace Flagrum.Application.Features.ModManager.Instructions;

[MemoryPackable]
public partial class AddReferenceBuildInstruction : PackedBuildInstruction, IAddReferenceBuildInstruction
{
    public override bool ShouldShowInBuildList => Authentication.IsAuthenticated;

    public override void Apply(IFlagrumProject mod, IEbonyArchive archive, IFlagrumProjectArchive projectArchive)
    {
        var uri = Uri;

        if (!archive.HasFile(uri))
        {
            archive.AddFile(uri, Flags, []);
        }
    }

    public override void Revert(IEbonyArchive archive, IFlagrumProjectArchive projectArchive)
    {
        var uri = Uri;

        if (archive.HasFile(uri))
        {
            archive.RemoveFile(uri);
        }
    }
}