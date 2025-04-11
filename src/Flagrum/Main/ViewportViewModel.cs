using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using CommunityToolkit.Mvvm.ComponentModel;
using Flagrum.Application.Features.Settings.Data;
using Flagrum.Application.Features.Shared;
using Flagrum.Application.Services;
using Flagrum.Abstractions;
using Flagrum.Services;
using Flagrum.Utilities;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Animations;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Controls;
using PropertyChanged.SourceGenerator;
using Camera = HelixToolkit.Wpf.SharpDX.Camera;
using Geometry3D = HelixToolkit.SharpDX.Core.Geometry3D;
using Material = HelixToolkit.Wpf.SharpDX.Material;
using PerspectiveCamera = HelixToolkit.Wpf.SharpDX.PerspectiveCamera;

namespace Flagrum.Main;

public partial class ViewportViewModel : ObservableObject, IDisposable
{
    private readonly List<BoneSkinMeshNode> _armatureNodes = new();
    private readonly CompositionTargetEx _compositeHelper = new();

    private NodeAnimationUpdater? _animationUpdater;
    [Notify] private Camera? _camera;
    [Notify] private IEffectsManager? _effectsManager;

    [Notify] private bool _isViewportVisible;
    private Material? _material;
    private Geometry3D? _mesh;
    [Notify] private SceneNodeGroupModel3D? _modelGroup;

    private int _viewportHeight;
    private int _viewportLeft = 514;
    [Notify] private InputGesture? _viewportPanGesture;
    [Notify] private InputGesture? _viewportRotateGesture;
    private int _viewportTop;
    private int _viewportWidth;

    public ViewportViewModel(
        AppStateService appState,
        IProfileService profile,
        IPlatformService platformService,
        IConfiguration configuration,
        TextureConverter textureConverter)
    {
        ((PlatformService)platformService).Viewport = this;

        // Set default key bindings for 3D viewer
        if (!configuration.ContainsKey(StateKey.ViewportRotateModifierKey))
        {
            configuration.Set(StateKey.ViewportRotateModifierKey, ModifierKeys.None);
            configuration.Set(StateKey.ViewportRotateMouseAction, MouseAction.MiddleClick);
        }

        if (!configuration.ContainsKey(StateKey.ViewportPanModifierKey))
        {
            configuration.Set(StateKey.ViewportPanModifierKey, ModifierKeys.Shift);
            configuration.Set(StateKey.ViewportPanMouseAction, MouseAction.MiddleClick);
        }

        if (!configuration.ContainsKey(StateKey.CurrentAssetNode))
        {
            configuration.Set(StateKey.CurrentAssetNode, (string?)null);
        }

        // Apply persisted keybindings
        var viewportRotateModifierKey = configuration.Get<ModifierKeys>(StateKey.ViewportRotateModifierKey);
        var viewportRotateMouseAction = configuration.Get<MouseAction>(StateKey.ViewportRotateMouseAction);
        _viewportRotateGesture = new MouseGesture(viewportRotateMouseAction, viewportRotateModifierKey);

        var viewportPanModifierKey = configuration.Get<ModifierKeys>(StateKey.ViewportPanModifierKey);
        var viewportPanMouseAction = configuration.Get<MouseAction>(StateKey.ViewportPanMouseAction);
        _viewportPanGesture = new MouseGesture(viewportPanMouseAction, viewportPanModifierKey);

        ViewportHelper = new ViewportHelper(appState, configuration, textureConverter, this);
        EffectsManager = new DefaultEffectsManager();
        Camera = new PerspectiveCamera
        {
            Position = new Point3D(50, 30, 100),
            LookDirection = new Vector3D(-50, -30, -100),
            UpDirection = new Vector3D(0, 1, 0),
            NearPlaneDistance = 0,
            FarPlaneDistance = 10000
        };
    }

    public ViewportHelper? ViewportHelper { get; private set; }
    public Viewport3DX? Viewer { get; set; }
    public AirspacePopup? AirspacePopup { get; set; }

    public int ViewportLeft
    {
        get => _viewportLeft;
        set
        {
            SetProperty(ref _viewportLeft, value);
            AirspacePopup!.HorizontalOffset = value;
        }
    }

    public int ViewportTop
    {
        get => _viewportTop;
        set
        {
            SetProperty(ref _viewportTop, value);
            AirspacePopup!.VerticalOffset = value;
        }
    }

    public int ViewportWidth
    {
        get => _viewportWidth;
        set
        {
            SetProperty(ref _viewportWidth, value);
            AirspacePopup!.Width = value;
        }
    }

    public int ViewportHeight
    {
        get => _viewportHeight;
        set
        {
            SetProperty(ref _viewportHeight, value);
            AirspacePopup!.Height = value;
        }
    }

    public void Dispose()
    {
        if (EffectsManager != null)
        {
            var effectsManager = EffectsManager as IDisposable;
            Disposer.RemoveAndDispose(ref effectsManager);
        }

        GC.SuppressFinalize(this);
    }

    private void CompositeHelper_Rendering(object? sender, RenderingEventArgs e)
    {
        _animationUpdater?.Update(Stopwatch.GetTimestamp(), Stopwatch.Frequency);
    }
}