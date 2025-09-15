using System.Runtime.InteropServices;

namespace Flagrum.Core.Graphics.Textures.DirectX.PixelFormats;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct UInt48
{
    public byte Byte0;
    public byte Byte1;
    public byte Byte2;
    public byte Byte3;
    public byte Byte4;
    public byte Byte5;
}