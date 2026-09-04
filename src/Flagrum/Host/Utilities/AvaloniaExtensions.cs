using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Flagrum.Abstractions.Application;

namespace Flagrum.Host.Utilities;

/// <summary>
/// Extension methods related to Avalonia.
/// </summary>
public static class AvaloniaExtensions
{
    /// <summary>
    /// Centers a window on the system's primary display.
    /// </summary>
    /// <param name="window">Window to center.</param>
    /// <remarks>
    /// The window dimensions will also be constrained to the primary display's working area.
    /// </remarks>
    public static void CenterOnPrimaryScreen(this Window window)
    {
        // Get primary screen
        var screen = window.Screens.Primary!;
        var scaling = screen.Scaling;
        
        // Ensure window is not larger than the screen's working area
        var maxWidth = screen.WorkingArea.Width / scaling;
        var maxHeight = screen.WorkingArea.Height / scaling;
        window.Width = Math.Min(window.Width, maxWidth);
        window.Height = Math.Min(window.Height, maxHeight);
        
        // Compute pixel dimensions
        var width = (int)Math.Round(window.Width * scaling);
        var height = (int)Math.Round(window.Height * scaling);
        
        // Center the window
        window.Position = new PixelPoint(
            screen.WorkingArea.X + (screen.WorkingArea.Width - width) / 2,
            screen.WorkingArea.Y + (screen.WorkingArea.Height - height) / 2);
    }

    /// <summary>
    /// Converts an array of <see cref="FilePickerFileType"/> items
    /// to their equivalent <see cref="FileDialogFileType"/> items.
    /// </summary>
    /// <param name="filter">Items to convert.</param>
    /// <returns>Array of converted items.</returns>
    public static FilePickerFileType[] ToAvaloniaFilter(this FileDialogFileType[] filter) => filter
        .Select(f => new FilePickerFileType(f.Name) {Patterns = f.Patterns})
        .ToArray();
}