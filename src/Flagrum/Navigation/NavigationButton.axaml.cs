using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Material.Icons;

namespace Flagrum.Navigation;

/// <summary>
/// Represents the type of target that a <see cref="NavigationButton" /> can navigate to.
/// </summary>
public enum NavigationTargetType
{
    /// <summary>
    /// Target is a view model that will resolve to an Avalonia view.
    /// </summary>
    ViewModel,

    /// <summary>
    /// Target is a Razor component that will resolve to a web view URL.
    /// </summary>
    RazorComponent,

    /// <summary>
    /// Target is a URL that should open in the system's browser.
    /// </summary>
    ExternalLink
}

/// <summary>
/// Menu button for the left-hand navigation.
/// </summary>
[PseudoClasses(":selected")]
public class NavigationButton : TemplatedControl
{
    /// <inheritdoc cref="Icon" />
    public static readonly StyledProperty<MaterialIconKind> IconProperty =
        AvaloniaProperty.Register<NavigationButton, MaterialIconKind>(nameof(Icon));

    /// <inheritdoc cref="Caption" />
    public static readonly StyledProperty<string?> CaptionProperty =
        AvaloniaProperty.Register<NavigationButton, string?>(nameof(Caption));

    /// <inheritdoc cref="IsSelected" />
    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<NavigationButton, bool>(nameof(IsSelected));

    /// <inheritdoc cref="TargetType" />
    public static readonly StyledProperty<NavigationTargetType> TargetTypeProperty =
        AvaloniaProperty.Register<NavigationButton, NavigationTargetType>(nameof(TargetType));

    /// <inheritdoc cref="Target" />
    public static readonly StyledProperty<object?> TargetProperty =
        AvaloniaProperty.Register<NavigationButton, object?>(nameof(Target));

    static NavigationButton()
    {
        // Apply/remove pseudoclass when the button is (de)selected
        IsSelectedProperty.Changed.AddClassHandler<NavigationButton>((control, args) =>
            control.PseudoClasses.Set(":selected", args.GetNewValue<bool>()));
    }

    /// <summary>
    /// Icon to display on the button.
    /// </summary>
    public MaterialIconKind Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Text to display on the button.
    /// </summary>
    public string? Caption
    {
        get => GetValue(CaptionProperty);
        set => SetValue(CaptionProperty, value);
    }

    /// <summary>
    /// Whether the button is currently selected.
    /// </summary>
    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>
    /// Type of the navigation <see cref="Target" />.
    /// </summary>
    public NavigationTargetType TargetType
    {
        get => GetValue(TargetTypeProperty);
        set => SetValue(TargetTypeProperty, value);
    }

    /// <summary>
    /// Type of the component or view model that this button navigates to.
    /// </summary>
    public object? Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    /// <summary>
    /// Optional actions to invoke when the button is clicked.
    /// </summary>
    public event Action<NavigationButton>? OnClick;

    /// <inheritdoc />
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        OnClick?.Invoke(this);
        base.OnPointerReleased(e);
    }
}