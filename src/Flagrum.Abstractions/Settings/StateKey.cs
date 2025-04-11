namespace Flagrum.Abstractions;

/// <summary>
/// Keys for state-related key-value pairs that are stored in <see cref="IConfiguration" />.
/// </summary>
public enum StateKey
{
    CurrentAssetNode,
    CurrentEarcEnabledState,
    Language,
    HaveThumbnailsBeenResized,
    GamePath,
    BinmodListPath,
    LastSeenVersionNotes,
    CurrentAssetExplorerPath,
    ViewportRotateModifierKey,
    ViewportRotateMouseAction,
    ViewportPanModifierKey,
    ViewportPanMouseAction,
    CurrentEarcCategory,
    HasMigratedBackups,
    CurrentAssetExplorerView,
    CurrentAssetExplorerLayout,
    ViewportTextureFidelity,
    AssetExplorerAddressBarSelect,
    ForspokenPatch,
    HasMigratedAwayFromSqlite,
    HidePatreonButton,
    HideLucentTab
}