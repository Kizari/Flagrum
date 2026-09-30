using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace Flagrum.Host.Shell;

/// <summary>
/// Converts a <see cref="WindowState"/> enumeration member to
/// the respective <see cref="SystemIcon"/> that corresponds to that state.
/// </summary>
public class WindowStateToSystemIconConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is WindowState windowState)
        {
            return windowState == WindowState.Normal ? SystemIcon.WindowMaximize : SystemIcon.WindowRestore;
        }

        return null;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}