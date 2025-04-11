using System.IO;
using Flagrum.Abstractions.ModManager.Instructions;
using Flagrum.Application.Features.ModManager.Editor;
using Flagrum.Application.Features.ModManager.Instructions.Abstractions;
using MemoryPack;

namespace Flagrum.Application.Features.ModManager.Instructions;

[MemoryPackable]
public partial class ReplaceLooseFileBuildInstruction : LooseAssetBuildInstruction, IReplaceLooseFileBuildInstruction
{
    public override string InstructionGroupName => ModEditorInstructionGroup.LooseFiles;
    public override bool ShouldShowInBuildList => Premium.IsClientWhitelisted;
    public override string FilterableName => RelativePath;

    public override void Apply()
    {
        var destination = Path.Combine(Profile.GameDataDirectory, RelativePath);
        var backupPath = $"{destination}.bak";
        File.Move(destination, backupPath, true);
        File.Copy(FilePath, destination, true);
    }

    public override void Revert()
    {
        var destination = Path.Combine(Profile.GameDataDirectory, RelativePath);
        var backupPath = $"{destination}.bak";
        if (File.Exists(backupPath))
        {
            File.Delete(destination);
            File.Move(backupPath, destination);
        }
    }
}