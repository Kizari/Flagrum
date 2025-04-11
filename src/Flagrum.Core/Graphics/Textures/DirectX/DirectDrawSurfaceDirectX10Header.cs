using Flagrum.Core.Utilities;

namespace Flagrum.Core.Graphics.Textures.DirectX;

public class DirectDrawSurfaceDirectX10Header
{
    public DxgiFormat Format { get; set; }

    public uint ResourceDimension { get; set; }

    public uint MiscFlags { get; set; }

    public uint ArraySize { get; set; } = 1;

    public DirectDrawSurfaceDirectX10MiscellaneousFlags2 MiscellaneousFlags2 { get; set; }
}