using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.ComponentModel;
using Flagrum.Host.WebView;
using Flagrum.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace Flagrum.Utilities;

/// <summary>
/// Allows Avalonia to instantiate views from view models.
/// </summary>
public class ViewLocator(IServiceProvider services) : IDataTemplate
{
    /// <inheritdoc />
    public Control Build(object? param) => param switch
    {
        NavigationViewModel => services.GetRequiredService<NavigationView>(),
        BlazorViewModel => services.GetRequiredService<BlazorWebView>(),
        _ => throw new NotSupportedException($"Unmapped view model '{param?.GetType()}'")
    };

    /// <inheritdoc />
    public bool Match(object? data) => data is ObservableObject;
}