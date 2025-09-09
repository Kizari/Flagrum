using System;
using Flagrum.ApplicationHost.Shell;
using Microsoft.Extensions.DependencyInjection;

namespace Flagrum.ApplicationHost;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : CustomShellWindow
{
    public MainWindow(string? fmodPath)
    {
        var screen = Screens.ScreenFromTopLevel(this)!;
        var bounds = screen.Bounds;
        var width = 1680;
        var height = 1024;
        
        if (width > bounds.Width * 0.95 || height > bounds.Height * 0.9)
        {
            width = (int)(bounds.Width * 0.95);
            height = (int)(bounds.Height * 0.9);
        
            Width = width * screen.Scaling;
            Height = height * screen.Scaling;
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
}