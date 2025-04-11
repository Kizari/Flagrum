namespace Flagrum.Application.Features.ModManager.Mod;

public enum LegacyModBuildInstruction
{
    ReplacePackedFile,
    AddPackedFile,
    RemovePackedFile,
    AddReference,
    AddToPackedTextureArray
}

public enum ModCategory
{
    Other,
    Fix,
    Lighthearted,
    Replacement,
    Content,
    QualityOfLife,
    Gameplay,
    Cheat,
    Rework
}