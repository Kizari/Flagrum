using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flagrum.Abstractions;
using Flagrum.Core.Utilities;
using Flagrum.Services;
using PropertyChanged.SourceGenerator;

namespace Flagrum.Main;

public partial class MainViewModel : ObservableObject
{
    private readonly IConfiguration _configuration;

    [Notify] private bool _hasInitializationStarted;
    [Notify] private bool _hasWebView2Runtime;
    [Notify] private bool _isMigratingFinished;
    [Notify] private bool _showPatreonButton;
    [Notify] private ViewportViewModel _viewportViewModel = null!;

    public MainViewModel(
        IPlatformService platformService,
        ViewportViewModel viewportViewModel,
        IConfiguration configuration)
    {
        _configuration = configuration;
        ((PlatformService)platformService).Main = this;
        ViewportViewModel = viewportViewModel;
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