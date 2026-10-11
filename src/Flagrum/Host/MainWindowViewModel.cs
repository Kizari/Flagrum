using CommunityToolkit.Mvvm.ComponentModel;
using Flagrum.Navigation;
using Injectio.Attributes;

namespace Flagrum.Host;

/// <summary>
/// Handles business logic for <see cref="MainWindow" />.
/// </summary>
[RegisterTransient<MainWindowViewModel>]
public class MainWindowViewModel(NavigationViewModel navigationViewModel) : ObservableObject
{
    public NavigationViewModel NavigationViewModel { get; } = navigationViewModel;
}