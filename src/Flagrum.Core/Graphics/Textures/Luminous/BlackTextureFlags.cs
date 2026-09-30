using System;

namespace Flagrum.Core.Graphics.Textures.Luminous;

[Flags]
public enum BlackTextureFlags : byte
{
    FLAG_COMPOSITED_IMAGE = 1,
    FLAG_SHARE_TEXTURE = 2,
    FLAG_REFERENCE_TEXTURE = 4
}