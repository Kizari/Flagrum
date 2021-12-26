using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using Flagrum.Desktop.Services;
using Flagrum.Web.Services;
using Microsoft.AspNetCore.Components.WebView.Wpf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Web.WebView2.Core;

namespace Flagrum.Desktop;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly string flagrumDirectory;
    private readonly string logFile;

#if DEBUG
    public string HostPageUri { get; } = "wwwroot/index.html";
#else
    public string HostPageUri { get; } = "wwwroot/_content/Flagrum.Web/index.html";
#endif

    private bool _hasWebView2Runtime;

    public bool HasWebView2Runtime
    {
        get => _hasWebView2Runtime;
        set
        {
            _hasWebView2Runtime = value;
            OnPropertyChanged(nameof(HasWebView2Runtime));
        }
    }

    public MainWindow()
    {
        flagrumDirectory = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\Flagrum";
        if (!Directory.Exists(flagrumDirectory))
        {
            Directory.CreateDirectory(flagrumDirectory);
        }

        logFile = $"{flagrumDirectory}\\Log.txt";

        if (File.Exists(logFile))
        {
            File.Delete(logFile);
        }

        File.Create(logFile);

        try
        {
            CoreWebView2Environment.GetAvailableBrowserVersionString();
            HasWebView2Runtime = true;
        }
        catch (WebView2RuntimeNotFoundException e)
        {
            InstallWebView2Runtime();
        }

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

        services.AddScoped<IWpfService, WpfService>(services => new WpfService(this));
        services.AddBlazorWebView();
        services.AddFlagrum();
        Resources.Add("services", services.BuildServiceProvider());

        InitializeComponent();
    }

    private async void InstallWebView2Runtime()
    {
        var start = new ProcessStartInfo
        {
            FileName = "MicrosoftEdgeWebview2Setup.exe",
            Arguments = "/silent /install",
            WindowStyle = ProcessWindowStyle.Hidden
        };

        var process = new Process {StartInfo = start};
        process.Start();
        await process.WaitForExitAsync();
        await Dispatcher.InvokeAsync(() => HasWebView2Runtime = true);
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
        var webView = (BlazorWebView)sender;
        webView.WebView.DefaultBackgroundColor = ColorTranslator.FromHtml("#181512");
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}