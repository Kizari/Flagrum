using System.Runtime.InteropServices;

namespace Flagrum.Core.Graphics.Textures.DirectX.PixelFormats;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct BC4Block
{
    public byte Red0;
    public byte Red1;
    
    // Represents 16 3-bit indices
    public UInt48 RedIndices;
}