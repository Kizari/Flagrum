using System;

namespace Flagrum.Core.Graphics.Textures.DirectX;

[Flags]
public enum DirectX10MiscFlags : uint
{
    None = 0x0,
    TextureCube = 0x4 // DDS_RESOURCE_MISC_TEXTURECUBE
}

[Flags]
public enum DirectX10MiscFlags2 : uint
{
    Unknown = 0x0,
    Straight = 0x01,
    Premultiplied = 0x02,
    Opaque = 0x03,
    Custom = 0x04
}