using Flagrum.Abstractions.ModManager.Instructions;
using Flagrum.Core.Utilities.Extensions;
using Flagrum.Application.Features.ModManager.Editor;
using Flagrum.Application.Features.ModManager.Instructions.Abstractions;
using MemoryPack;

namespace Flagrum.Application.Features.ModManager.Instructions;

/// <summary>
/// Enables a feature within the hook DLL that Flagrum injects upon launching the game.
/// </summary>
[MemoryPackable]
public partial class EnableHookFeatureBuildInstruction : GlobalBuildInstruction, IEnableHookFeatureBuildInstruction
{
    public override string InstructionGroupName => ModEditorInstructionGroup.HookFeatures;
    public override bool ShouldShowInBuildList => Premium.IsClientWhitelisted;
    public override string FilterableName => Feature.ToString().SpacePascalCase();

    /// <inheritdoc />
    public FlagrumHookFeature Feature { get; set; }

    public override void Apply()
    {
        // Nothing needed here as the hook features are inferred from this instruction at launch time
    }

    public override void Revert()
    {
        // Nothing needed here as the hook features are inferred from this instruction at launch time
    }
}