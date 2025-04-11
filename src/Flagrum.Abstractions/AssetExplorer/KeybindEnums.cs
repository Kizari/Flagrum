namespace Flagrum.Abstractions.AssetExplorer;

/// <summary>
/// This is a copy of System.Windows.Input.MouseAction that is here to avoid needing a dependency on WPF
/// </summary>
public enum MouseAction : byte
{
    None,
    LeftClick,
    RightClick,
    MiddleClick,
    WheelClick,
    LeftDoubleClick,
    RightDoubleClick,
    MiddleDoubleClick
}

/// <summary>
/// This is a copy of System.Windows.Input.ModifierKeys that is here to avoid needing a dependency on WPF
/// </summary>
public enum ModifierKeys
{
    None = 0,
    Alt = 1,
    Control = 2,
    Shift = 4,
    Windows = 8
}