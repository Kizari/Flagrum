using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Flagrum.Core.Utilities;
using Flagrum.Desktop.Architecture;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;
using Flagrum.Web.Services;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Animations;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Controls;
using Camera = HelixToolkit.Wpf.SharpDX.Camera;
using Geometry3D = HelixToolkit.SharpDX.Core.Geometry3D;
using Material = HelixToolkit.Wpf.SharpDX.Material;
using PerspectiveCamera = HelixToolkit.Wpf.SharpDX.PerspectiveCamera;

namespace Flagrum.Desktop;

public class MainViewModel : ObservableObject, IDisposable
{
    private readonly List<BoneSkinMeshNode> _armatureNodes = new();
    private readonly CompositionTargetEx _compositeHelper = new();
    private readonly IEffectsManager? _effectsManager;
    private NodeAnimationUpdater? _animationUpdater;
    private Camera? _camera;

    private bool _hasWebView2Runtime;
    private bool _isViewportVisible;
    private Material _material;
    private Geometry3D _mesh;
    private SceneNodeGroupModel3D _modelGroup;
    private bool _showArmature;
    private int _viewportHeight;
    private int _viewportLeft = 514;
    private InputGesture _viewportPanGesture;
    private InputGesture _viewportRotateGesture;
    private int _viewportTop;
    private int _viewportWidth;

    public MainViewModel()
    {
        ViewportHelper = new ViewportHelper(this);
        EffectsManager = new DefaultEffectsManager();
        Camera = new PerspectiveCamera
        {
            Position = new Point3D(50, 30, 100),
            LookDirection = new Vector3D(-50, -30, -100),
            UpDirection = new Vector3D(0, 1, 0),
            NearPlaneDistance = 0,
            FarPlaneDistance = 10000
        };

        using var context = new FlagrumDbContext(new ProfileService());

        var viewportRotateModifierKey = context.GetEnum<ModifierKeys>(StateKey.ViewportRotateModifierKey);
        var viewportRotateMouseAction = context.GetEnum<MouseAction>(StateKey.ViewportRotateMouseAction);
        _viewportRotateGesture = new MouseGesture(viewportRotateMouseAction, viewportRotateModifierKey);

        var viewportPanModifierKey = context.GetEnum<ModifierKeys>(StateKey.ViewportPanModifierKey);
        var viewportPanMouseAction = context.GetEnum<MouseAction>(StateKey.ViewportPanMouseAction);
        _viewportPanGesture = new MouseGesture(viewportPanMouseAction, viewportPanModifierKey);
    }

    public string HostPage => IOHelper.GetWebRoot() + "/index.html";
    public ViewportHelper ViewportHelper { get; }
    public Viewport3DX Viewer { get; set; }
    public AirspacePopup AirspacePopup { get; set; }
    public string? FmodPath { get; set; }

    public bool HasWebView2Runtime
    {
        get => _hasWebView2Runtime;
        set => SetValue(ref _hasWebView2Runtime, value);
    }

    private ICommand? _patreonLink;
    public ICommand PatreonLink => _patreonLink ??= new RelayCommand(() =>
    {
        Process.Start(new ProcessStartInfo("https://www.patreon.com/Flagrum")
        {
            UseShellExecute = true
        });
    });

    public int ViewportLeft
    {
        get => _viewportLeft;
        set
        {
            SetValue(ref _viewportLeft, value);
            AirspacePopup.HorizontalOffset = value;
        }
    }

    public int ViewportTop
    {
        get => _viewportTop;
        set
        {
            SetValue(ref _viewportTop, value);
            AirspacePopup.VerticalOffset = value;
        }
    }

    public int ViewportWidth
    {
        get => _viewportWidth;
        set
        {
            SetValue(ref _viewportWidth, value);
            AirspacePopup.Width = value;
        }
    }

    public int ViewportHeight
    {
        get => _viewportHeight;
        set
        {
            SetValue(ref _viewportHeight, value);
            AirspacePopup.Height = value;
        }
    }

    public InputGesture ViewportRotateGesture
    {
        get => _viewportRotateGesture;
        set => SetValue(ref _viewportRotateGesture, value);
    }

    public InputGesture ViewportPanGesture
    {
        get => _viewportPanGesture;
        set => SetValue(ref _viewportPanGesture, value);
    }

    public bool IsViewportVisible
    {
        get => _isViewportVisible;
        set => SetValue(ref _isViewportVisible, value);
    }

    public Camera? Camera
    {
        get => _camera;
        set => SetValue(ref _camera, value);
    }

    public IEffectsManager? EffectsManager
    {
        get => _effectsManager;
        private init => SetValue(ref _effectsManager, value);
    }

    public SceneNodeGroupModel3D ModelGroup
    {
        get => _modelGroup;
        set => SetValue(ref _modelGroup, value);
    }

    public bool ShowArmature
    {
        get => _showArmature;
        set
        {
            if (SetValue(ref _showArmature, value))
            {
                foreach (var node in _armatureNodes)
                {
                    node.Visible = value;
                }
            }
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