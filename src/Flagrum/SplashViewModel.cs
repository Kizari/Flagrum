using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Flagrum;

public partial class SplashViewModel : ObservableObject
{
    [ObservableProperty] private string _loadingText = "Initialising";

    public SplashViewModel()
    {
        Instance = this;
    }

    public static SplashViewModel Instance { get; private set; } = null!;

    public void SetLoadingText(string text)
    {
        Dispatcher.UIThread.Invoke(() => LoadingText = text);
    }
}