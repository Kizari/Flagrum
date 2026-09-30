using Avalonia.Controls;
using Flagrum.Host.Utilities;

namespace Flagrum.Host;

/// <summary>
/// Splash screen to show while Flagrum is initializing.
/// </summary>
public partial class SplashWindow : Window
{
    public SplashWindow()
    {
        InitializeComponent();
        this.CenterOnPrimaryScreen();
    }

    /// <summary>
    /// Sets the text of the loading label.
    /// </summary>
    public void SetText(string text)
    {
        LoadingLabel.Text = text;
    }
}