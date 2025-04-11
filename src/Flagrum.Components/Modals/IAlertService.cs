namespace Flagrum.Components.Modals;

/// <summary>
/// Represents a service that displays alert modals.
/// </summary>
public interface IAlertService
{
    /// <summary>
    /// Displays the global alert modal instance.
    /// </summary>
    /// <param name="title">Text to display in the title bar of the alert.</param>
    /// <param name="heading">Large text to display first in the alert.</param>
    /// <param name="subtext">Regular text to display in the body of the alert.</param>
    /// <param name="onClose">Action to execute when the alert is dismissed.</param>
    /// <param name="width">Width of the alert modal, in pixels.</param>
    /// <param name="height">Height of the alert modal, in pixels.</param>
    /// <param name="disableMaxWidth">
    /// Whether to disable constraining the maximum width of the alert modal to the
    /// same value as <paramref name="width" />. If <c>false</c> maximum width will be set
    /// to 90% of the width of the application window instead.
    /// </param>
    void Open(
        string title,
        string heading,
        string subtext,
        Action? onClose = null,
        int width = 400,
        int height = 300,
        bool disableMaxWidth = false);

    /// <summary>
    /// Dismisses the global alert modal instance.
    /// </summary>
    void Close();
}