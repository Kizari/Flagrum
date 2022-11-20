namespace Flagrum.Web.Persistence.Entities.ModManager;

public enum EarcChangeType
{
    Change,
    Create
}

public enum EarcFileChangeType
{
    Replace,
    Add,
    Remove,
    AddReference,
    AddToTextureArray
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

public enum EarcLegacyConversionStatus
{
    Success,
    NoEarcs,
    EarcNotFound,
    NewFiles,
    NeedsDisabling
}