using System;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Interop;
using Flagrum.Utilities;
using HelixToolkit.Wpf.SharpDX;
using Microsoft.AspNetCore.Components.WebView.Wpf;
using Microsoft.Extensions.DependencyInjection;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace Flagrum.Main;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow(string? fmodPath)
    {
        var screen = Screen.FromHandle(new WindowInteropHelper(this).Handle);
        var bounds = screen.Bounds;
        var width = 1680;
        var height = 1024;

        if (width > bounds.Width * 0.95 || height > bounds.Height * 0.9)
        {
            width = (int)(bounds.Width * 0.95);
            height = (int)(bounds.Height * 0.9);

            var dpiXProperty =
                typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            var dpiYProperty =
                typeof(SystemParameters).GetProperty("Dpi", BindingFlags.NonPublic | BindingFlags.Static);

            var dpiX = (int)dpiXProperty!.GetValue(null, null)!;
            var dpiY = (int)dpiYProperty!.GetValue(null, null)!;

            Width = width / (dpiX / 96.0);
            Height = height / (dpiY / 96.0);
        }
        else
        {
            Width = width;
            Height = height;
        }

        InitializeComponent();

        var viewModel = Program.Services.GetRequiredService<MainViewModel>();
        viewModel.FmodPath = fmodPath;
        DataContext = viewModel;

        Closed += (_, _) => { (DataContext as IDisposable)?.Dispose(); };
        Resources.Add("Services", Program.Services);
    }

    private void Viewer_OnInitialized(object? sender, EventArgs e)
    {
        ((MainViewModel)DataContext).ViewportViewModel.Viewer = (Viewport3DX)sender;
    }

    private void AirspacePopup_OnInitialized(object? sender, EventArgs e)
    {
        ((MainViewModel)DataContext).ViewportViewModel.AirspacePopup = (AirspacePopup)sender;
    }

    private void Minimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void Maximize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void MainBlazorWebView_OnInitialized(object? sender, EventArgs e)
    {
        if (sender == null)
        {
            return;
        }

        var webView = (BlazorWebView)sender;
        webView.WebView.DefaultBackgroundColor = ColorTranslator.FromHtml("#181512");
    }

    private CustomPopupPlacement[] AirspacePopup_OnPopupPlaced(Size popupSize, Size targetSize, Point point)
    {
        return new CustomPopupPlacement[] {new(new Point(0, 0), PopupPrimaryAxis.Horizontal)};
    }
}