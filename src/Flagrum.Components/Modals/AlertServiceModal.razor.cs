using Microsoft.AspNetCore.Components;

namespace Flagrum.Components.Modals;

/// <summary>
/// Wrapper for <see cref="AlertModal" /> that links it to the <see cref="IAlertService" /> singleton.
/// </summary>
public partial class AlertServiceModal
{
    [Inject] private IAlertService AlertService { get; set; } = null!;

    /// <summary>
    /// Reference to the global <see cref="AlertModal" />.
    /// </summary>
    private AlertModal Alert { get; set; } = null!;

    /// <summary>
    /// Sets the global <see cref="AlertModal" /> reference in <see cref="IAlertService" /> when
    /// the component finishes initializing.
    /// </summary>
    protected override void OnInitialized()
    {
        ((AlertService)AlertService).SetModalComponent(Alert);
    }
}