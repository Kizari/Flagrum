using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Flagrum.Host.Shell;

/// <summary>
/// Button for the title bar that attempts to use native OS icons, otherwise falls back to sensible defaults.
/// </summary>
public partial class ShellButton : Button
{
    public static readonly StyledProperty<SystemIcon> IconProperty =
        AvaloniaProperty.Register<ShellButton, SystemIcon>(nameof(Icon));

    public static readonly StyledProperty<IBrush> HoverBackgroundProperty =
        AvaloniaProperty.Register<ShellButton, IBrush>(nameof(HoverBackground),
            new SolidColorBrush(Color.Parse("#CACBCF")));

    /// <summary>
    /// Icon to display in the button.
    /// </summary>
    public SystemIcon Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Color to display when the button is hovered. Defaults to an off-white color.
    /// </summary>
    public IBrush HoverBackground
    {
        get => GetValue(HoverBackgroundProperty);
        set => SetValue(HoverBackgroundProperty, value);
    }
}