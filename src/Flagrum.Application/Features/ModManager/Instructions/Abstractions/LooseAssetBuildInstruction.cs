using Flagrum.Abstractions.ModManager.Instructions;

namespace Flagrum.Application.Features.ModManager.Instructions.Abstractions;

/// <inheritdoc cref="ILooseAssetBuildInstruction" />
public abstract partial class LooseAssetBuildInstruction : GlobalBuildInstruction, ILooseAssetBuildInstruction
{
    /// <inheritdoc />
    public string RelativePath { get; set; }

    /// <inheritdoc />
    public string FilePath { get; set; }
}