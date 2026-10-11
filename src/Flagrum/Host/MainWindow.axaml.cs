using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Flagrum.Abstractions;
using Flagrum.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Flagrum.Host;

/// <summary>
/// Main application window.
/// </summary>
public partial class MainWindow : Window
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Creates the window and adds the Blazor web view to the layout.
    /// </summary>
    /// <param name="services"></param>
    public MainWindow(IServiceProvider services)
    {
        _configuration = services.GetRequiredService<IConfiguration>();

        InitializeComponent();
        this.CenterOnPrimaryScreen();
        RefreshPatreonButton();
    }

    public void RefreshPatreonButton()
    {
        PatreonButton.IsVisible = !_configuration.Get<bool>(StateKey.HidePatreonButton);
    }

    private void OnMinimize(object? sender, RoutedEventArgs args)
    {
        WindowState = WindowState.Minimized;
    }

    private void OnMaximize(object? sender, RoutedEventArgs args)
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void OnClose(object? sender, RoutedEventArgs args)
    {
        Close();
    }

    private void PatreonButton_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://patreon.com/Kizari") {UseShellExecute = true});
    }
}