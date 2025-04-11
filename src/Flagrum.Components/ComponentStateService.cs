namespace Flagrum.Components;

/// <summary>
/// Simple service for updating component state from remote parts of the application.
/// </summary>
public class ComponentStateService
{
    /// <summary>
    /// Actions to invoke when <see cref="RefreshLayout" /> is called.
    /// </summary>
    public event Action? OnRefreshLayout;

    /// <summary>
    /// Actions to invoke when <see cref="RefreshMenu" /> is called.
    /// </summary>
    public event Action? OnRefreshMenu;

    /// <summary>
    /// Refreshes the application layout component.
    /// This is typically used when premium features are enabled/disabled or file indexing begins/ends.
    /// </summary>
    public void RefreshLayout()
    {
        OnRefreshLayout?.Invoke();
    }

    /// <summary>
    /// Refreshes the application menu component.
    /// This is typically used when settings are adjusted that affects what is displayed in the sidebar.
    /// </summary>
    public void RefreshMenu()
    {
        OnRefreshMenu?.Invoke();
    }
}