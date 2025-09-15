using System.Runtime.InteropServices;

namespace Flagrum.Core.Graphics.Textures.DirectX.PixelFormats;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct BC6HBlock
{
    public fixed byte Data[16]; // Raw 128-bit block
}