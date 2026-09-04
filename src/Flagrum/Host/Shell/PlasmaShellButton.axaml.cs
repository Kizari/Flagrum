using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Flagrum.Host.Shell;

/// <summary>
/// Button for the title bar that mimics the native look and feel of the
/// KDE Plasma shell.
/// </summary>
public partial class PlasmaShellButton : Button
{
    public static readonly StyledProperty<SystemIcon> IconProperty =
        AvaloniaProperty.Register<PlasmaShellButton, SystemIcon>(nameof(Icon));
    
    public static readonly StyledProperty<IBrush> HoverBackgroundProperty =
        AvaloniaProperty.Register<PlasmaShellButton, IBrush>(nameof(HoverBackground), 
            defaultValue: new SolidColorBrush(Color.Parse("#CACBCF")));

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