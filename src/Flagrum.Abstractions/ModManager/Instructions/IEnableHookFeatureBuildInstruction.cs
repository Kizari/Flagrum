namespace Flagrum.Abstractions.ModManager.Instructions;

/// <summary>
/// Represents a mod instruction that enables a feature within the hook DLL that is injected into the game.
/// </summary>
public interface IEnableHookFeatureBuildInstruction : IGlobalBuildInstruction
{
    /// <summary>
    /// The feature to enable when this mod is applied.
    /// </summary>
    FlagrumHookFeature Feature { get; set; }
}