using Flagrum.Core.Graphics.Textures.DirectX;
using Flagrum.Core.Graphics.Textures.Luminous;
using Flagrum.Core.Utilities;

namespace Flagrum.Core.Graphics.Textures;

public class TexturePixelFormatMap
{
    static TexturePixelFormatMap()
    {
        Instance = new FallbackMap<DxgiFormat, BlackTexturePixelFormat>(DxgiFormat.BC1_UNORM,
            BlackTexturePixelFormat.BC1_UNORM);
        Instance.Add(DxgiFormat.BC6H_UF16, BlackTexturePixelFormat.BC6_UFLOAT);
        Instance.Add(DxgiFormat.BC1_UNORM, BlackTexturePixelFormat.BC1_UNORM);
        Instance.Add(DxgiFormat.BC2_UNORM, BlackTexturePixelFormat.BC2_UNORM);
        Instance.Add(DxgiFormat.BC3_UNORM, BlackTexturePixelFormat.BC3_UNORM);
        Instance.Add(DxgiFormat.BC4_UNORM, BlackTexturePixelFormat.BC4_UNORM);
        Instance.Add(DxgiFormat.BC5_UNORM, BlackTexturePixelFormat.BC5_UNORM);
        Instance.Add(DxgiFormat.BC7_UNORM, BlackTexturePixelFormat.BC7_UNORM);
        Instance.Add(DxgiFormat.R8G8B8A8_UNORM, BlackTexturePixelFormat.R8G8B8A8_UNORM);
        Instance.Add(DxgiFormat.B8G8R8A8_UNORM, BlackTexturePixelFormat.B8G8R8A8_UNORM);
        Instance.Add(DxgiFormat.R8_UNORM, BlackTexturePixelFormat.B8);
        Instance.Add(DxgiFormat.R16G16B16A16_FLOAT, BlackTexturePixelFormat.R16G16B16A16_FLOAT);
        Instance.Add(DxgiFormat.R32G32B32A32_FLOAT, BlackTexturePixelFormat.A32B32G32R32F);
        Instance.Add(DxgiFormat.R32_FLOAT, BlackTexturePixelFormat.R32_FLOAT);
        Instance.Add(DxgiFormat.R8G8_UNORM, BlackTexturePixelFormat.R8G8_UNORM);
        Instance.Add(DxgiFormat.R16G16B16A16_UNORM, BlackTexturePixelFormat.R16G16B16A16_UNORM);
        Instance.Add(DxgiFormat.R16_UNORM, BlackTexturePixelFormat.R16_UNORM);
    }

    public static FallbackMap<DxgiFormat, BlackTexturePixelFormat> Instance { get; }
}