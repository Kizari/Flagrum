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
    private AlertModal Alert
    {
        get;
        set
        {
            field = value;
            ((AlertService)AlertService).SetModalComponent(Alert);
        }
    } = null!;
}