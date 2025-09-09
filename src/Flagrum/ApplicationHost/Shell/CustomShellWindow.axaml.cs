using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace Flagrum.ApplicationHost.Shell;

/// <summary>
/// Window that replaces the native shell with a custom Avalonia-rendered shell.
/// </summary>
/// <remarks>
/// Implements most native shell functionality, including moving by title bar drag, resizing by edges and corners,
/// maximize/restore by title bar double-click, and the typical shell buttons on the right side on the title bar.
/// </remarks>
public partial class CustomShellWindow : Window
{
    public static readonly StyledProperty<IBrush?> TitleBarBackgroundProperty =
        AvaloniaProperty.Register<CustomShellWindow, IBrush?>(nameof(TitleBarBackground));
    
    private Point? _dragStartPoint;
    private PointerPressedEventArgs? _pendingDragEvent;

    public IBrush? TitleBarBackground
    {
        get => GetValue(TitleBarBackgroundProperty);
        set => SetValue(TitleBarBackgroundProperty, value);
    }
    
    protected override Type StyleKeyOverride => typeof(CustomShellWindow);

    /// <summary>
    /// Hooks up control events when the template is applied.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        var titleBar = e.NameScope.Find<Grid>("TitleBar");
        titleBar!.PointerPressed += TitleBar_OnPointerPressed;
        titleBar.PointerMoved += TitleBar_OnPointerMoved;
        titleBar.PointerReleased += TitleBar_OnPointerReleased;

        var button = e.NameScope.Find<PlasmaShellButton>("MinimizeButton");
        button!.Click += MinimizeButton_OnClick;
        
        button = e.NameScope.Find<PlasmaShellButton>("RestoreButton");
        button!.Click += RestoreButton_OnClick;
        
        button = e.NameScope.Find<PlasmaShellButton>("CloseButton");
        button!.Click += CloseButton_OnClick;

        var edge = e.NameScope.Find<Border>("LeftEdge");
        edge!.PointerPressed += LeftEdge_OnPointerPressed;
        
        edge = e.NameScope.Find<Border>("TopEdge");
        edge!.PointerPressed += TopEdge_OnPointerPressed;
        
        edge = e.NameScope.Find<Border>("RightEdge");
        edge!.PointerPressed += RightEdge_OnPointerPressed;
        
        edge = e.NameScope.Find<Border>("BottomEdge");
        edge!.PointerPressed += BottomEdge_OnPointerPressed;
        
        var corner = e.NameScope.Find<Border>("TopLeftCorner");
        corner!.PointerPressed += TopLeftCorner_OnPointerPressed;
        
        corner = e.NameScope.Find<Border>("TopRightCorner");
        corner!.PointerPressed += TopRightCorner_OnPointerPressed;
        
        corner = e.NameScope.Find<Border>("BottomLeftCorner");
        corner!.PointerPressed += BottomLeftCorner_OnPointerPressed;
        
        corner = e.NameScope.Find<Border>("BottomRightCorner");
        corner!.PointerPressed += BottomRightCorner_OnPointerPressed;
    }
    
    private void TitleBar_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            // Set pending drag event
            _dragStartPoint = e.GetPosition(this);
            _pendingDragEvent = e;

            // Toggle maximize/restore if double-click
            if (e.ClickCount == 2)
            {
                WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
            }
        }
    }
    
    private void TitleBar_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        // Check for pending drag event
        if (_dragStartPoint.HasValue 
            && _pendingDragEvent != null 
            && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            // Get distance from drag start point
            var currentPoint = e.GetPosition(this);
            var delta = currentPoint - _dragStartPoint.Value;

            // Check if distance is outside the margin of error to prevent accidental drag start
            if (Math.Abs(delta.X) > 4 || Math.Abs(delta.Y) > 4)
            {
                // Start the drag and clear the pending event
                BeginMoveDrag(_pendingDragEvent);
                _dragStartPoint = null;
                _pendingDragEvent = null;
            }
        }
    }
    
    private void TitleBar_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        // Reset window drag state
        _dragStartPoint = null;
        _pendingDragEvent = null;
    }

    private void MinimizeButton_OnClick(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }
    
    private void RestoreButton_OnClick(object? sender, RoutedEventArgs e)
    {
        // Toggle maximize/restore
        WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
    }
    
    private void CloseButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
    
    private void LeftEdge_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginResizeDrag(WindowEdge.West, e);
        }
    }
    
    private void TopEdge_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginResizeDrag(WindowEdge.North, e);
        }
    }
    
    private void RightEdge_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginResizeDrag(WindowEdge.East, e);
        }
    }
    
    private void BottomEdge_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginResizeDrag(WindowEdge.South, e);
        }
    }

    private void TopLeftCorner_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginResizeDrag(WindowEdge.NorthWest, e);
        }
    }
    
    private void TopRightCorner_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginResizeDrag(WindowEdge.NorthEast, e);
        }
    }
    
    private void BottomLeftCorner_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginResizeDrag(WindowEdge.SouthWest, e);
        }
    }
    
    private void BottomRightCorner_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginResizeDrag(WindowEdge.SouthEast, e);
        }
    }
}