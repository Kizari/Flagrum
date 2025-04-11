using System;

namespace Flagrum.Core.Ps4;

[Flags]
public enum BlackTextureFlags : byte
{
    FLAG_COMPOSITED_IMAGE = 1,
    FLAG_SHARE_TEXTURE = 2,
    FLAG_REFERENCE_TEXTURE = 4
}