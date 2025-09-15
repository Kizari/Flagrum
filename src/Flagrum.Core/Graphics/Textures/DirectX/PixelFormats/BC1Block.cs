using System.Runtime.InteropServices;

namespace Flagrum.Core.Graphics.Textures.DirectX.PixelFormats;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct BC1Block
{
    public ushort Color0;
    public ushort Color1;
    public uint Indices; // 16 2-bit indices
}