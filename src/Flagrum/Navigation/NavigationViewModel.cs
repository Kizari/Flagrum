using CommunityToolkit.Mvvm.ComponentModel;
using Flagrum.Abstractions;
using Flagrum.Application.Features.Menu;
using Flagrum.Host.WebView;
using Injectio.Attributes;
using Microsoft.Extensions.Localization;

namespace Flagrum.Navigation;

/// <summary>
/// Handles business logic for <see cref="NavigationView" />.
/// </summary>
[RegisterTransient<NavigationViewModel>]
public partial class NavigationViewModel(
    IConfiguration configuration,
    IApplication application,
    IProfileService profile,
    IStringLocalizer<MainMenu> localizer) : ObservableObject
{
    public IStringLocalizer<MainMenu> Localizer { get; } = localizer;

    /// <summary>
    /// Version string to display at the bottom of the navigation menu.
    /// </summary>
    public string Version
    {
        get
        {
            var v = application.Version;
            return $"{Localizer["Version"]}{v.Major}.{v.Minor}.{v.Build}";
        }
    }

    /// <summary>
    /// Whether the page currently navigated to is a razor component.
    /// </summary>
    [ObservableProperty]
    public partial bool IsActivePageRazorComponent { get; set; } = true;

    /// <summary>
    /// The currently navigated to view model, or null if not navigated to an Avalonia view.
    /// </summary>
    [ObservableProperty]
    public partial ObservableObject? ActiveViewModel { get; set; }

    /// <summary>
    /// Business logic for the web view.
    /// </summary>
    public BlazorViewModel BlazorViewModel { get; } = new();

    /// <summary>
    /// Whether the current profile targets Final Fantasy XV.
    /// </summary>
    public bool IsFfxvProfile => profile.Current.Type == LuminousGame.FFXV;

    /// <summary>
    /// Whether the Lucent tab should be displayed in the navigation menu.
    /// </summary>
    [ObservableProperty]
    public partial bool ShouldShowLucentTab { get; set; } = profile.Current.Type == LuminousGame.FFXV
                                                            && !configuration.Get<bool>(StateKey.HideLucentTab);
}