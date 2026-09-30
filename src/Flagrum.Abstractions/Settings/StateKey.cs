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
    [Obsolete("Viewport controls are now handled by other keys")] ViewportRotateModifierKey,
    [Obsolete("Viewport controls are now handled by other keys")] ViewportRotateMouseAction,
    [Obsolete("Viewport controls are now handled by other keys")] ViewportPanModifierKey,
    [Obsolete("Viewport controls are now handled by other keys")] ViewportPanMouseAction,
    CurrentEarcCategory,
    HasMigratedBackups,
    CurrentAssetExplorerView,
    CurrentAssetExplorerLayout,
    ViewportTextureFidelity,
    AssetExplorerAddressBarSelect,
    ForspokenPatch,
    HasMigratedAwayFromSqlite,
    HidePatreonButton,
    HideLucentTab,
    ViewportLeftClickAction,
    ViewportMiddleClickAction,
    ViewportRightClickAction,
    SteamRootPath,
    ProtonPrefixPath,
    SteamLaunchWrapperPath,
    SteamReaperPath,
    SteamRuntimePath,
    ProtonPath,
    LaunchCommand
}