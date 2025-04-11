using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using PropertyChanged.SourceGenerator;

namespace Flagrum;

public partial class SplashViewModel : ObservableObject
{
    [Notify] private string _loadingText = "Initialising";

    public SplashViewModel()
    {
        Instance = this;
    }

    public static SplashViewModel Instance { get; private set; } = null!;

    public void SetLoadingText(string text)
    {
        App.Current.Dispatcher.Invoke(() => LoadingText = text);
    }
}