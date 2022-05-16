using System;
using Flagrum.Core.Utilities;

namespace Flagrum.Core.Gfxbin.Btex;

[Flags]
public enum DDSFlags : uint
{
    None = 0x00,
    Caps = 0x01,
    Height = 0x02,
    Width = 0x04,
    Pitch = 0x08,
    PixelFormat = 0x1000,
    MipMapCount = 0x20000,
    LinearSize = 0x80000,
    Depth = 0x800000,
    Texture = Caps | Height | Width | PixelFormat
}

[Flags]
public enum DDSCaps : uint
{
    None = 0x00,
    Complex = 0x08,
    MipMap = 0x400000,
    Texture = 0x1000
}

public class DX10
{
    public DxgiFormat Format { get; set; } = 0;

    public uint ResourceDimension { get; set; } = 0;

    public uint MiscFlags { get; set; } = 0;

    public uint ArraySize { get; set; } = 1;

    public uint MiscFlags2 { get; set; } = 0;
}

public enum DX10MiscFlags2 : uint
{
    Unknown = 0x0,
    Straight = 0x01,
    Premultiplied = 0x02,
    Opaque = 0x03,
    Custom = 0x04
}

[Flags]
public enum PixelFormatFlags : uint
{
    AlphaPixels = 0x01,
    Alpha = 0x02,
    FourCC = 0x04,
    RGB = 0x40,
    YUV = 0x200,
    Luminance = 0x20000
}

public class PixelFormat
{
    public uint Size { get; set; } = 32;

    public PixelFormatFlags Flags { get; set; } = PixelFormatFlags.FourCC;

    public uint FourCC { get; set; } = 808540228;

    public uint RGBBitCount { get; set; } = 0;

    public uint RBitMask { get; set; } = 0;

    public uint GBitMask { get; set; } = 0;

    public uint BBitMask { get; set; } = 0;

    public uint ABitMask { get; set; } = 0;
}

public class DdsHeader
{
    public uint[] p_Reserved = new uint[11];

    public uint p_Size = 124,
        p_Caps = (int)DDSCaps.Texture,
        p_Caps2 = 0,
        p_Caps3 = 0,
        p_Caps4 = 0,
        p_Reserved2 = 0;

    public uint Height { get; set; } = 0;

    public uint Width { get; set; } = 0;

    public uint PitchOrLinearSize { get; set; } = 0;

    public uint Depth { get; set; } = 0;

    public uint MipMapCount { get; set; } = 0;

    public DDSFlags Flags { get; set; } = DDSFlags.Texture;

    public PixelFormat PixelFormat { get; set; } = null;

    public DX10 DX10Header { get; set; } = null;
}