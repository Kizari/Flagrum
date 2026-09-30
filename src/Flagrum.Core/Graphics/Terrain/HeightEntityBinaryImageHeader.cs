using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Flagrum.Core.Graphics.Textures;
using Flagrum.Core.Graphics.Textures.DirectX;
using Flagrum.Core.Graphics.Textures.Luminous;
using Flagrum.Core.Graphics.Textures.Shared;

namespace Flagrum.Core.Graphics.Terrain;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct HeightEntityBinaryImageHeader
{
    public static int Size
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Unsafe.SizeOf<HeightEntityBinaryImageHeader>();
    }

    public byte Flags;
    public HeightEntityBinaryImageType Type;
    public byte TypeIndex;
    public byte MipCount;
    public uint TextureDataOffset;
    public float AverageHeight;
    public BlackTexturePixelFormat Format;
    public HeightEntityBinaryImageTileMode TileMode;
    public ushort Reserved1;
    public ushort MinValue;
    public ushort MaxValue;
    public uint TextureSizeBytes;
    public uint Width;
    public uint Height;

    /// <summary>
    /// Writes a suitable DDS header for the image represented by this struct.
    /// </summary>
    /// <param name="destination">Buffer to write the header to.</param>
    public void WriteDdsHeader(Span<byte> destination)
    {
        ref var start = ref MemoryMarshal.GetReference(destination);
        ref var magic = ref Unsafe.As<byte, uint>(ref start);
        magic = DirectDrawSurfaceHeader.MagicValue;

        ref var ddsHeader = ref Unsafe.As<byte, DirectDrawSurfaceHeader>(ref Unsafe.Add(ref start, sizeof(uint)));
        ddsHeader.Size = (uint)Unsafe.SizeOf<DirectDrawSurfaceHeader>();
        ddsHeader.Width = Width;
        ddsHeader.Height = Height;
        ddsHeader.Pitch = TextureSizeBytes;
        ddsHeader.Depth = 1;
        ddsHeader.MipMapCount = MipCount > 0 ? MipCount : 1u;
        ddsHeader.Flags = DirectDrawSurfaceFlags.Texture
                          | DirectDrawSurfaceFlags.Pitch
                          | DirectDrawSurfaceFlags.Depth
                          | DirectDrawSurfaceFlags.MipMapCount;
        ddsHeader.PixelFormat = DirectDrawSurfacePixelFormat.Default;
        ddsHeader.Caps = DirectDrawSurfaceCaps.Texture;

        ref var dx10Header = ref Unsafe.As<byte, DirectX10Header>(ref Unsafe.Add(ref start,
            sizeof(uint) + Unsafe.SizeOf<DirectDrawSurfaceHeader>()));
        dx10Header.ArraySize = 1;
        dx10Header.Format = PixelFormatMap.Get(Format); // TODO: sRGB?
        dx10Header.ResourceDimension = DirectX10ResourceDimension.Texture2D;
        dx10Header.MiscFlags = 0;
        dx10Header.MiscFlags2 = DirectX10MiscFlags2.Unknown;
    }
}