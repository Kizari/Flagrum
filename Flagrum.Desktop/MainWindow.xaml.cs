using System.IO;
using System.Windows;
using Blazored.LocalStorage;
using Flagrum.Web.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Flagrum.Desktop;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddBlazorWebView();
        serviceCollection.AddBlazoredLocalStorage();
        serviceCollection.AddSingleton<Settings>();
        serviceCollection.AddScoped<JSInterop>();
        Resources.Add("services", serviceCollection.BuildServiceProvider());

        InitializeComponent();
    }

    private void Minimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void Maximize_Click(object sender, RoutedEventArgs e)
    {
        if (WindowState == WindowState.Maximized)
        {
            WindowState = WindowState.Normal;
        }
        else
        {
            WindowState = WindowState.Maximized;
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}