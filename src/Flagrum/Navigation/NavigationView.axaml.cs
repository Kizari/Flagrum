using System;
using System.Diagnostics;
using System.Linq;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using Flagrum.Components;
using Flagrum.Host.WebView;
using Injectio.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Flagrum.Navigation;

/// <summary>
/// A two-column layout with a left-hand navigation menu, and a main content view.
/// </summary>
[RegisterTransient<NavigationView>]
public partial class NavigationView : UserControl
{
    private readonly ObservedTaskScheduler _scheduler;
    private readonly IServiceProvider _services;

    public NavigationView(
        IServiceProvider services,
        ObservedTaskScheduler scheduler)
    {
        _services = services;
        _scheduler = scheduler;

        InitializeComponent();

        // Register navigation button handlers
        foreach (var button in this.GetLogicalDescendants().OfType<NavigationButton>())
        {
            button.OnClick += OnNavigationButtonClicked;
        }
    }

    /// <summary>
    /// Resolves <see cref="Control.DataContext" /> as a strongly-typed object.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the data context is not yet populated.</exception>
    private NavigationViewModel ViewModel =>
        DataContext as NavigationViewModel
        ?? throw new InvalidOperationException("Cannot resolve ViewModel before it is bound.");

    /// <summary>
    /// Click handler for child <see cref="NavigationButton" /> controls.
    /// </summary>
    private void OnNavigationButtonClicked(NavigationButton target)
    {
        // Get the currently selected navigation button
        var selected = this.GetLogicalDescendants()
            .OfType<NavigationButton>()
            .FirstOrDefault(b => b.IsSelected);

        // Handle navigation
        var shouldUpdateActive = target.TargetType switch
        {
            NavigationTargetType.RazorComponent => HandleRazorComponentNavigation(target.Target),
            NavigationTargetType.ViewModel => HandleViewModelNavigation(target.Target),
            NavigationTargetType.ExternalLink => HandleExternalLinkNavigation(target.Target),
            _ => throw new NotSupportedException($"Unknown navigation target type: '{target.TargetType}'")
        };

        // Update the active navigation button, if applicable
        if (shouldUpdateActive)
        {
            selected?.IsSelected = false;
            target.IsSelected = true;
        }
    }

    private bool HandleRazorComponentNavigation(object? target)
    {
        if (target is not string url)
        {
            throw new InvalidOperationException(
                $"'{nameof(NavigationButton.Target)}' must be 'string' when '{nameof(NavigationButton.TargetType)}' " +
                $"is '{nameof(NavigationTargetType.RazorComponent)}'");
        }

        var webView = this.FindDescendantOfType<BlazorWebView>();
        _scheduler.RunAsyncObserved(async () => { await webView!.NavigateAsync(url); });

        ViewModel.ActiveViewModel = null;
        ViewModel.IsActivePageRazorComponent = true;

        return true;
    }

    private bool HandleViewModelNavigation(object? target)
    {
        if (target is not Type type)
        {
            throw new InvalidOperationException(
                $"'{nameof(NavigationButton.Target)}' must be 'Type' when '{nameof(NavigationButton.TargetType)}' " +
                $"is '{nameof(NavigationTargetType.ViewModel)}'");
        }

        ViewModel.ActiveViewModel = (ObservableObject)_services.GetRequiredService(type);
        ViewModel.IsActivePageRazorComponent = false;

        return true;
    }

    private bool HandleExternalLinkNavigation(object? target)
    {
        if (target is not LocalizedString url)
        {
            throw new InvalidOperationException(
                $"'{nameof(NavigationButton.Target)}' must be 'LocalizedString' " +
                $"when '{nameof(NavigationButton.TargetType)}' is '{nameof(NavigationTargetType.ExternalLink)}'");
        }

        Process.Start(new ProcessStartInfo(url) {UseShellExecute = true});

        return false;
    }
}