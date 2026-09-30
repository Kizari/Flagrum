using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Flagrum.Core.Graphics.Textures.DirectX;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct DirectX10Header
{
    public static int StructSize
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Unsafe.SizeOf<DirectX10Header>();
    }

    public DxgiFormat Format;
    public DirectX10ResourceDimension ResourceDimension;
    public DirectX10MiscFlags MiscFlags;
    public uint ArraySize;

    /// <summary>
    /// Flags related to transparency.
    /// </summary>
    public DirectX10MiscFlags2 MiscFlags2;
}