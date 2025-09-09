using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flagrum.Abstractions;
using Flagrum.Core.Utilities;
using Flagrum.Services;

namespace Flagrum.ApplicationHost;

public partial class MainViewModel : ObservableObject
{
    private readonly IConfiguration _configuration;

    [ObservableProperty] private bool _hasInitializationStarted;
    [ObservableProperty] private bool _hasWebView2Runtime;
    [ObservableProperty] private bool _isMigratingFinished;
    [ObservableProperty] private bool _showPatreonButton;

    public MainViewModel(
        IPlatformService platformService,
        IConfiguration configuration)
    {
        _configuration = configuration;
        ((PlatformService)platformService).Main = this;
        RefreshPatreonButton();
    }

    public string HostPage => IOHelper.GetWebRoot() + "/index.html";
    public string? FmodPath { get; set; }

    public ICommand PatreonLink { get; } = new RelayCommand(() =>
    {
        Process.Start(new ProcessStartInfo("https://www.patreon.com/Kizari")
        {
            UseShellExecute = true
        });
    });

    public void RefreshPatreonButton()
    {
        ShowPatreonButton = !_configuration.Get<bool>(StateKey.HidePatreonButton);
    }
}