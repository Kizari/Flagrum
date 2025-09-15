using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Flagrum.Core.Graphics.Terrain;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct HeightEntityBinaryHeader
{
    public static int Size
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Unsafe.SizeOf<HeightEntityBinaryHeader>();
    }

    public uint _imageHeadersOffset;
    public ushort _imageHeaderSize;
    public ushort _imageCount;
    public fixed ulong Reserved[3];

    public uint ImageHeadersOffset => BinaryPrimitives.ReverseEndianness(_imageHeadersOffset);
    public ushort ImageHeaderSize => BinaryPrimitives.ReverseEndianness(_imageHeaderSize);
    public ushort ImageCount => BinaryPrimitives.ReverseEndianness(_imageCount);
}