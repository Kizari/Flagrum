using System.IO;
using Flagrum.Abstractions.ModManager.Instructions;
using Flagrum.Core.Utilities;
using Flagrum.Application.Features.ModManager.Editor;
using Flagrum.Application.Features.ModManager.Instructions.Abstractions;
using MemoryPack;

namespace Flagrum.Application.Features.ModManager.Instructions;

[MemoryPackable]
public partial class AddLooseFileBuildInstruction : LooseAssetBuildInstruction, IAddLooseFileBuildInstruction
{
    public override string InstructionGroupName => ModEditorInstructionGroup.LooseFiles;
    public override bool ShouldShowInBuildList => Authentication.IsAuthenticated;
    public override string FilterableName => RelativePath;

    public override void Apply()
    {
        var destination = Path.Combine(Profile.GameDataDirectory, RelativePath);
        IOHelper.EnsureDirectoriesExistForFilePath(destination);
        File.Copy(FilePath, destination, true);
    }

    public override void Revert()
    {
        var path = Path.Combine(Profile.GameDataDirectory, RelativePath);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}