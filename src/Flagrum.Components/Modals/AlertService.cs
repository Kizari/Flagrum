namespace Flagrum.Components.Modals;

/// <inheritdoc />
public class AlertService : IAlertService
{
    private AlertModal? _modal;

    /// <inheritdoc />
    public void Open(
        string title,
        string heading,
        string subtext,
        Action? onClose = null,
        int width = 400,
        int height = 300,
        bool disableMaxWidth = false)
    {
        if (_modal == null)
        {
            throw new InvalidOperationException(
                $"Cannot open a non-existent {nameof(AlertModal)}. Ensure that the " +
                $"<{nameof(AlertServiceModal)} /> tag is present somewhere near the top of the component " +
                $"hierarchy (such as in the layout) and has had time to initialize before calling this method.");
        }

        _modal.Open(title, heading, subtext, onClose, width, height, disableMaxWidth);
    }

    /// <inheritdoc />
    public void Close()
    {
        if (_modal == null)
        {
            throw new InvalidOperationException(
                $"Cannot close a non-existent {nameof(AlertModal)}. Ensure that the " +
                $"<{nameof(AlertServiceModal)} /> tag is present somewhere near the top of the component " +
                $"hierarchy (such as in the layout) and has had time to initialize before calling this method.");
        }

        _modal.Close();
    }

    /// <summary>
    /// Sets the global <see cref="AlertModal" /> for this service.
    /// </summary>
    internal void SetModalComponent(AlertModal modal)
    {
        _modal = modal;
    }
}