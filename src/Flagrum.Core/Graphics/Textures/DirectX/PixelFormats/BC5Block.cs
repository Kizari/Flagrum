using System.Runtime.InteropServices;

namespace Flagrum.Core.Graphics.Textures.DirectX.PixelFormats;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct BC5Block
{
    public byte Red0;
    public byte Red1;
    public UInt48 RedIndices;

    public byte Green0;
    public byte Green1;
    public UInt48 GreenIndices;
}