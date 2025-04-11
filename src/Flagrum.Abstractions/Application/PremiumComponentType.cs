using Flagrum.Abstractions.ModManager;

namespace Flagrum.Abstractions;

/// <summary>
/// Represents premium components.
/// </summary>
public enum PremiumComponentType
{
    /// <summary>
    /// Panel that is displayed under the settings tab that is used to set premium-only settings.
    /// </summary>
    PremiumSettingsPanel,

    /// <summary>
    /// Panel that is displayed under the settings tab that is only visible to administrators.
    /// </summary>
    AdministrationPanel,

    /// <summary>
    /// Panel that is displayed at the bottom of the settings tab that allows the user to link their Patreon account.
    /// </summary>
    PatreonAccountPanel,

    /// <summary>
    /// Displayless component that authenticates the user's premium status when initialized.
    /// </summary>
    PremiumAuthenticator,

    /// <summary>
    /// Extra buttons for the mod builder that enable premium actions.
    /// </summary>
    BuildListActions,

    /// <summary>
    /// Extra buttons for the mod card context menu that enable premium actions.
    /// </summary>
    ModCardContextActions,

    /// <summary>
    /// Extra buttons for the header of the mod project editor that enables premium actions.
    /// </summary>
    ProjectActions,

    /// <summary>
    /// Statistics bar that displays at the top of the mod build list to show counts of build instructions.
    /// </summary>
    EditorInstructionCountBar,

    /// <summary>
    /// Extra buttons for an <see cref="IModEditorInstructionRow" /> that enable premium actions.
    /// </summary>
    EditorInstructionPackedAssetActions,

    /// <summary>
    /// Extra buttons for the launch bar at the top of the <see cref="IModLibrary" /> that enable premium actions.
    /// </summary>
    LaunchBarActions,
    
    /// <summary>
    /// Extra buttons for the archive entries in <see cref="IModEditor"/> that enable premium actions.
    /// </summary>
    EditorInstructionArchiveActions,
    
    /// <summary>
    /// Modals for the <see cref="IModEditor"/> that are only used by premium functionality.
    /// </summary>
    ModEditorModals
}