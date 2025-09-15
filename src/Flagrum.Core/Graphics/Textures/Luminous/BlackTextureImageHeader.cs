using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Flagrum.Core.Graphics.Textures.Luminous;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct BlackTextureImageHeader
{
    /// <summary>
    /// Size of this struct, in bytes.
    /// </summary>
    public static int StructSize
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Unsafe.SizeOf<BlackTextureImageHeader>();
    }

    /// <summary>
    /// Width of the image, in pixels.
    /// </summary>
    public ushort Width;

    /// <summary>
    /// Height of the image, in pixels.
    /// </summary>
    public ushort Height;

    /// <summary>
    /// Unsure how to explain this one.
    /// </summary>
    public ushort Pitch;

    /// <summary>
    /// Layout of the pixel data.
    /// </summary>
    public BlackTexturePixelFormat Format;

    /// <summary>
    /// Number of mipmaps in this image.
    /// </summary>
    public byte MipMapCount;

    /// <summary>
    /// Unsure how to explain this one.
    /// </summary>
    public byte Depth;

    /// <summary>
    /// Number of dimensions in this image. Typically 2D, but may be 3D for cube maps.
    /// </summary>
    public byte DimensionCount;

    /// <summary>
    /// Attributes that apply to this image.
    /// </summary>
    public BlackTextureImageFlags Flags;

    /// <summary>
    /// Total number of surfaces in this image.
    /// </summary>
    public ushort SurfaceCount;

    /// <summary>
    /// Presumably the size of the surface header.
    /// </summary>
    public ushort SurfaceHeaderStride;

    /// <summary>
    /// Not well understood.
    /// </summary>
    public uint PlatformDataOffset;

    /// <summary>
    /// Byte position of the first <see cref="BlackTextureSurfaceHeader" />, relative to the start of this struct.
    /// </summary>
    public uint SurfaceHeaderOffset;

    /// <summary>
    /// Byte position of the name of this image, relative to the start of this struct.
    /// </summary>
    public uint NameOffset;

    /// <summary>
    /// Not well understood.
    /// </summary>
    public uint PlatformDataSize;

    public uint HighResolutionMipCount;
    public uint HighResolutionBtexSize;

    /// <summary>
    /// Unused space, ignore this.
    /// </summary>
    public ulong Reserved;

    public uint TileMode;

    /// <summary>
    /// Number of images in this texture (exluding mipmaps).
    /// </summary>
    public uint ArrayCount;
}