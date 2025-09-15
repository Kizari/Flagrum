using System.Runtime.InteropServices;

namespace Flagrum.Core.Graphics.Textures.DirectX.PixelFormats;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct BC3Block
{
    public byte Alpha0;
    public byte Alpha1;
    public UInt48 AlphaIndices;

    public ushort Color0;
    public ushort Color1;
    public uint ColorIndices; // 16 2-bit indices
}