using System;
using System.Windows;
using Flagrum.Desktop.Services;
using Flagrum.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace Flagrum.Desktop;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
#if DEBUG
    public string HostPageUri { get; } = "wwwroot/index.html";
#else
    public string HostPageUri { get; } = "wwwroot/_content/Flagrum.Web/index.html";
#endif

    public MainWindow()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddConfiguration();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, ConsoleLoggerProvider>());
            LoggerProviderOptions.RegisterProviderOptions<ConsoleLoggerConfiguration, ConsoleLoggerProvider>(
                builder.Services);
            Action<ConsoleLoggerConfiguration> configure = c => { };
            builder.Services.Configure(configure);
        });

        services.AddScoped<IWpfService, WpfService>();
        services.AddBlazorWebView();
        services.AddFlagrum();
        Resources.Add("services", services.BuildServiceProvider());

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