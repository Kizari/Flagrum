using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Avalonia.Data.Converters;

namespace Flagrum.Host.Shell;

/// <summary>
/// Represents icons found in the operating system files.
/// </summary>
public enum SystemIcon
{
    Undefined,
    WindowMinimize,
    WindowMaximize,
    WindowRestore,
    WindowClose
}

/// <summary>
/// Converts a <see cref="SystemIcon" /> enumeration member to a fully-qualified file path to the respective icon.
/// </summary>
public class SystemIconConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is SystemIcon icon)
        {
            // Attempt to find native system icons
            var iconPath = FindIconPath(icon switch
            {
                SystemIcon.WindowMinimize => "window-minimize",
                SystemIcon.WindowMaximize => "window-maximize",
                SystemIcon.WindowRestore => "window-restore",
                SystemIcon.WindowClose => "window-close",
                _ => throw new NotSupportedException($"Unknown system icon type {icon}.")
            });

            // Fallback to embedded default icons if native icons were not found
            return iconPath ?? icon switch
            {
                SystemIcon.WindowMinimize => "avares://Flagrum/Assets/Icons/minimize.svg",
                SystemIcon.WindowMaximize => "avares://Flagrum/Assets/Icons/maximize.svg",
                SystemIcon.WindowRestore => "avares://Flagrum/Assets/Icons/restore.svg",
                SystemIcon.WindowClose => "avares://Flagrum/Assets/Icons/close.svg",
                _ => throw new NotSupportedException($"Unknown system icon type {icon}.")
            };
        }

        throw new InvalidOperationException($"Cannot convert non-{nameof(SystemIcon)} type to system icon.");
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();

    /// <summary>
    /// Finds an icon file path on the system.
    /// </summary>
    /// <param name="iconName">File name of the icon to find.</param>
    /// <returns>Absolute path to the icon on disk, or null if it could not be found.</returns>
    private static string? FindIconPath(string iconName)
    {
        // Discern which directories to look in for the icons based on system setup
        var theme = Environment.GetEnvironmentVariable("GTK_THEME") ?? "breeze";
        var directories = new[]
        {
            "/usr/share/icons/",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".icons")
        };

        var sizes = new[] {"22", "24"};
        var categories = new[] {"actions", "apps", "status"};

        // Iterate the target directories until a match is found
        return directories
            .SelectMany(directory => categories
                .SelectMany(category => sizes
                    .Select(size => Path.Combine(
                        directory, theme, category, size, $"{iconName}.svg"))
                    .Where(File.Exists)))
            .FirstOrDefault();
    }
}