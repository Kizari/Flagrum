using Flagrum.Abstractions;
using Microsoft.AspNetCore.Components;

namespace Flagrum.Application.Services;

/// <summary>
/// Wrapper for <see cref="NavigationManager" /> to enable injection as <see cref="INavigationManager" />.
/// This is so that other dependencies that do not reference Blazor can navigate the application.
/// </summary>
public class NavigationManagerWrapper(NavigationManager manager) : INavigationManager
{
    /// <inheritdoc />
    public void NavigateTo(string path)
    {
        manager.NavigateTo(path);
    }
}