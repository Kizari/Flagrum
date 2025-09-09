using System;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace Flagrum.Main;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(string? fmodPath)
    {
        // var screen = Screen.FromHandle(new WindowInteropHelper(this).Handle);
        // var bounds = screen.Bounds;
        // var width = 1680;
        // var height = 1024;
        //
        // if (width > bounds.Width * 0.95 || height > bounds.Height * 0.9)
        // {
        //     width = (int)(bounds.Width * 0.95);
        //     height = (int)(bounds.Height * 0.9);
        //
        //     var dpiXProperty =
        //         typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
        //     var dpiYProperty =
        //         typeof(SystemParameters).GetProperty("Dpi", BindingFlags.NonPublic | BindingFlags.Static);
        //
        //     var dpiX = (int)dpiXProperty!.GetValue(null, null)!;
        //     var dpiY = (int)dpiYProperty!.GetValue(null, null)!;
        //
        //     Width = width / (dpiX / 96.0);
        //     Height = height / (dpiY / 96.0);
        // }
        // else
        // {
        //     Width = width;
        //     Height = height;
        // }

        InitializeComponent();

        var viewModel = Program.Services.GetRequiredService<MainViewModel>();
        viewModel.FmodPath = fmodPath;
        DataContext = viewModel;

        Closed += (_, _) => { (DataContext as IDisposable)?.Dispose(); };
        Resources.Add("Services", Program.Services);
    }
}