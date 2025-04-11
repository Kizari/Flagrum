using Flagrum.Core.Graphics.Textures;
using Flagrum.Core.Graphics.Textures.DirectX;
using Flagrum.Core.Graphics.Textures.Luminous;

namespace Flagrum.Core.Graphics.Terrain;

public class HeightEntityBinaryImage
{
    public byte Flags { get; set; }
    public HeightEntityBinaryImageType Type { get; set; }
    public byte TypeIndex { get; set; }
    public byte MipCount { get; set; }
    public long TextureDataOffsetOffset { get; set; }
    public uint TextureDataOffset { get; set; }
    public float AverageHeight { get; set; }
    public BlackTexturePixelFormat TextureFormat { get; set; }
    public HeightEntityBinaryImageTileMode TileMode { get; set; }
    public ushort Reserved1 { get; set; }
    public ushort MinValue { get; set; }
    public ushort MaxValue { get; set; }
    public uint TextureSizeBytes { get; set; }
    public uint TextureWidth { get; set; }
    public uint TextureHeight { get; set; }
    public byte[] DdsData { get; set; }
    
    public DirectDrawSurface ToDds()
    {
        return new DirectDrawSurface
        {
            Height = TextureHeight,
            Width = TextureWidth,
            Pitch = TextureSizeBytes,
            Depth = 1,
            MipCount = MipCount > 0 ? MipCount : 1u,
            Flags = DirectDrawSurfaceFlags.Texture | DirectDrawSurfaceFlags.Pitch |
                    DirectDrawSurfaceFlags.Depth | DirectDrawSurfaceFlags.MipMapCount,
            Format = new DirectDrawSurfacePixelFormat(),
            DirectX10Header = new DirectDrawSurfaceDirectX10Header
            {
                ArraySize = 1,
                Format = TexturePixelFormatMap.Instance[TextureFormat],
                ResourceDimension = 3
            },
            PixelData = DdsData
        };
    }
}