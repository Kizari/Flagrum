using System;
using System.Collections.Generic;

namespace Scarlet.Drawing;

/// <summary>
/// Specifies components of a pixel data format
/// </summary>
[Flags]
public enum PixelDataFormat : ulong
{
    /*
     * BPP 0000000000000007
     * Cha 00000000000000F8
     * Red 0000000000007F00
     * Grn 00000000003F8000
     * Blu 000000001FC00000
     * Alp 0000000FE0000000
     * Spc 000001F000000000
     * Ord 0001FE0000000000
     * Fil 01FE000000000000
     * FCh 0200000000000000
     * Rsv FC00000000000000
     */

    // TODO: maaaybe just split into multiple enums instead...?

    /// <summary>
    /// Format has 0 bits per pixel (invalid)
    /// </summary>
    Bpp0 = Undefined,

    /// <summary>
    /// Format has 4 bits per pixel
    /// </summary>
    Bpp4 = (ulong)1 << 0,

    /// <summary>
    /// Format has 8 bits per pixel
    /// </summary>
    Bpp8 = (ulong)2 << 0,

    /// <summary>
    /// Format has 16 bits per pixel
    /// </summary>
    Bpp16 = (ulong)3 << 0,

    /// <summary>
    /// Format has 24 bits per pixel
    /// </summary>
    Bpp24 = (ulong)4 << 0,

    /// <summary>
    /// Format has 32 bits per pixel
    /// </summary>
    Bpp32 = (ulong)5 << 0,

    /// <summary>
    /// Format has 64 bits per pixel
    /// </summary>
    Bpp64 = (ulong)6 << 0,

    /// <summary>
    /// Mask for extracting BPP value
    /// </summary>
    MaskBpp = (((ulong)1 << 3) - 1) << 0, /* 0000000000000007 */

    /// <summary>
    /// Format has no color channels (invalid)
    /// </summary>
    ChannelsNone = Undefined,

    /// <summary>
    /// Format has channels in RGB order
    /// </summary>
    ChannelsRgb = (ulong)1 << 3,

    /// <summary>
    /// Format has channels in BGR order
    /// </summary>
    ChannelsBgr = (ulong)2 << 3,

    /// <summary>
    /// Format has channels in RGBA order
    /// </summary>
    ChannelsRgba = (ulong)3 << 3,

    /// <summary>
    /// Format has channels in BGRA order
    /// </summary>
    ChannelsBgra = (ulong)4 << 3,

    /// <summary>
    /// Format has channels in ARGB order
    /// </summary>
    ChannelsArgb = (ulong)5 << 3,

    /// <summary>
    /// Format has channels in ABGR order
    /// </summary>
    ChannelsAbgr = (ulong)6 << 3,

    /// <summary>
    /// Format has channels in RGB order, with trailing dummy A channel
    /// </summary>
    ChannelsRgbx = (ulong)7 << 3,

    /// <summary>
    /// Format has channels in BGR order, with trailing dummy A channel
    /// </summary>
    ChannelsBgrx = (ulong)8 << 3,

    /// <summary>
    /// Format has channels in RGB order, with leading dummy A channel
    /// </summary>
    ChannelsXrgb = (ulong)9 << 3,

    /// <summary>
    /// Format has channels in BGR order, with leading dummy A channel
    /// </summary>
    ChannelsXbgr = (ulong)10 << 3,

    /// <summary>
    /// Format has channels in L order
    /// </summary>
    ChannelsLuminance = (ulong)11 << 3,

    /// <summary>
    /// Format has channels in A order
    /// </summary>
    ChannelsAlpha = (ulong)12 << 3,

    /// <summary>
    /// Format has channels in LA order
    /// </summary>
    ChannelsLuminanceAlpha = (ulong)13 << 3,

    /// <summary>
    /// Format has channels in AL order
    /// </summary>
    ChannelsAlphaLuminance = (ulong)14 << 3,

    /// <summary>
    /// Format is indexed color
    /// </summary>
    ChannelsIndexed = (ulong)15 << 3,

    /// <summary>
    /// Mask for extracting channels value
    /// </summary>
    MaskChannels = (((ulong)1 << 5) - 1) << 3, /* 00000000000000F8 */

    /// <summary>
    /// Format has 0 bits in red channel
    /// </summary>
    RedBits0 = Undefined,

    /// <summary>
    /// Format has 4 bits in red channel
    /// </summary>
    RedBits4 = (ulong)1 << 8,

    /// <summary>
    /// Format has 5 bits in red channel
    /// </summary>
    RedBits5 = (ulong)2 << 8,

    /// <summary>
    /// Format has 8 bits in red channel
    /// </summary>
    RedBits8 = (ulong)3 << 8,

    /// <summary>
    /// Mask for extracting red bits value
    /// </summary>
    MaskRedBits = (((ulong)1 << 7) - 1) << 8, /* 0000000000007F00 */

    /// <summary>
    /// Format has 0 bits in green channel
    /// </summary>
    GreenBits0 = Undefined,

    /// <summary>
    /// Format has 4 bits in green channel
    /// </summary>
    GreenBits4 = (ulong)1 << 15,

    /// <summary>
    /// Format has 5 bits in green channel
    /// </summary>
    GreenBits5 = (ulong)2 << 15,

    /// <summary>
    /// Format has 6 bits in green channel
    /// </summary>
    GreenBits6 = (ulong)3 << 15,

    /// <summary>
    /// Format has 8 bits in green channel
    /// </summary>
    GreenBits8 = (ulong)4 << 15,

    /// <summary>
    /// Mask for extracting green bits value
    /// </summary>
    MaskGreenBits = (((ulong)1 << 7) - 1) << 15, /* 00000000003F8000 */

    /// <summary>
    /// Format has 0 bits in blue channel
    /// </summary>
    BlueBits0 = Undefined,

    /// <summary>
    /// Format has 4 bits in blue channel
    /// </summary>
    BlueBits4 = (ulong)1 << 22,

    /// <summary>
    /// Format has 5 bits in blue channel
    /// </summary>
    BlueBits5 = (ulong)2 << 22,

    /// <summary>
    /// Format has 8 bits in blue channel
    /// </summary>
    BlueBits8 = (ulong)3 << 22,

    /// <summary>
    /// Mask for extracting blue bits value
    /// </summary>
    MaskBlueBits = (((ulong)1 << 7) - 1) << 22, /* 000000001FC00000 */

    /// <summary>
    /// Format has 0 bits in alpha channel
    /// </summary>
    AlphaBits0 = Undefined,

    /// <summary>
    /// Format has 1 bit in alpha channel
    /// </summary>
    AlphaBits1 = (ulong)1 << 29,

    /// <summary>
    /// Format has 4 bits in alpha channel
    /// </summary>
    AlphaBits4 = (ulong)2 << 29,

    /// <summary>
    /// Format has 8 bits in alpha channel
    /// </summary>
    AlphaBits8 = (ulong)3 << 29,

    /// <summary>
    /// Mask for extracting alpha bits value
    /// </summary>
    MaskAlphaBits = (((ulong)1 << 7) - 1) << 29, /* 0000000FE0000000 */

    /// <summary>
    /// Format has 0 bits in luminance channel (same as red bits enumeration)
    /// </summary>
    LuminanceBits0 = RedBits0,

    /// <summary>
    /// Format has 8 bits in luminance channel (same as red bits enumeration)
    /// </summary>
    LuminanceBits8 = RedBits8,

    /// <summary>
    /// Format has 5 bits in luminance channel (same as red bits enumeration)
    /// </summary>
    LuminanceBits5 = RedBits5,

    /// <summary>
    /// Format has 4 bits in luminance channel (same as red bits enumeration)
    /// </summary>
    LuminanceBits4 = RedBits4,

    /// <summary>
    /// Mask for extracting luminance bits value (same as red bits enumeration)
    /// </summary>
    MaskLuminanceBits = MaskRedBits,

    /// <summary>
    /// Special format with 3DS-style ETC1 data
    /// </summary>
    SpecialFormatETC1_3DS = (ulong)1 << 36,

    /// <summary>
    /// Special format with 3DS-style ETC1A4 data
    /// </summary>
    SpecialFormatETC1A4_3DS = (ulong)2 << 36,

    /// <summary>
    /// Special format with generic DXT1 data
    /// </summary>
    SpecialFormatDXT1 = (ulong)3 << 36,

    /// <summary>
    /// Special format with PSP-style DXT1 data
    /// </summary>
    SpecialFormatDXT1_PSP = (ulong)4 << 36,

    /// <summary>
    /// Special format with generic DXT3 data
    /// </summary>
    SpecialFormatDXT3 = (ulong)5 << 36,

    /// <summary>
    /// Special format with PSP-style DXT3 data
    /// </summary>
    SpecialFormatDXT3_PSP = (ulong)6 << 36,

    /// <summary>
    /// Special format with generic DXT5 data
    /// </summary>
    SpecialFormatDXT5 = (ulong)7 << 36,

    /// <summary>
    /// Special format with PSP-style DXT5 data
    /// </summary>
    SpecialFormatDXT5_PSP = (ulong)8 << 36,

    /// <summary>
    /// Special format with Vita-style PVRT2 data
    /// </summary>
    SpecialFormatPVRT2_Vita = (ulong)9 << 36,

    /// <summary>
    /// Special format with Vita-style PVRT4 data
    /// </summary>
    SpecialFormatPVRT4_Vita = (ulong)10 << 36,

    /// <summary>
    /// Special format with unsigned RGTC1 data
    /// </summary>
    SpecialFormatRGTC1 = (ulong)11 << 36,

    /// <summary>
    /// Special format with signed RGTC1 data
    /// </summary>
    SpecialFormatRGTC1_Signed = (ulong)12 << 36,

    /// <summary>
    /// Special format with unsigned RGTC2 data
    /// </summary>
    SpecialFormatRGTC2 = (ulong)13 << 36,

    /// <summary>
    /// Special format with signed RGTC2 data
    /// </summary>
    SpecialFormatRGTC2_Signed = (ulong)14 << 36,

    /// <summary>
    /// Special format with generic BPTC data
    /// </summary>
    SpecialFormatBPTC = (ulong)15 << 36,

    /// <summary>
    /// Special format with generic BPTC Float data
    /// </summary>
    SpecialFormatBPTC_Float = (ulong)16 << 36,

    /// <summary>
    /// Special format with generic BPTC Signed Float data
    /// </summary>
    SpecialFormatBPTC_SignedFloat = (ulong)17 << 36,

    /// <summary>
    /// Mask for extracting special format value
    /// </summary>
    MaskSpecial = (((ulong)1 << 5) - 1) << 36, /* 000001F000000000 */

    /// <summary>
    /// Format has pixels in linear order
    /// </summary>
    PixelOrderingLinear = Undefined,

    /// <summary>
    /// Format has pixels in tiled order
    /// </summary>
    PixelOrderingTiled = (ulong)1 << 41,

    /// <summary>
    /// Format has pixels in tiled order, 3DS-style
    /// </summary>
    PixelOrderingTiled3DS = (ulong)1 << 42,

    /// <summary>
    /// Format has pixels in swizzled order, Vita-style
    /// </summary>
    PixelOrderingSwizzledVita = (ulong)1 << 43,

    /// <summary>
    /// Format has pixels in swizzled order, PSP-style
    /// </summary>
    PixelOrderingSwizzledPSP = (ulong)1 << 44,

    /// <summary>
    /// Format has pixels in swizzled order, Switch-style
    /// </summary>
    PixelOrderingSwizzledSwitch = (ulong)1 << 45,

    /// <summary>
    /// Mask for extracting pixel ordering value
    /// </summary>
    MaskPixelOrdering = (((ulong)1 << 8) - 1) << 41, /* 0001FE0000000000 */

    /// <summary>
    /// Format will not apply filtering
    /// </summary>
    FilterNone = Undefined,

    /// <summary>
    /// Format applies simple, ordered dither filter
    /// </summary>
    FilterOrderedDither = (ulong)1 << 49,

    /// <summary>
    /// Mask for extracting filtering value
    /// </summary>
    MaskFilter = (((ulong)1 << 8) - 1) << 49, /* 01FE000000000000 */

    /// <summary>
    /// Format will force unused color channels to zero
    /// </summary>
    ForceClear = Undefined,

    /// <summary>
    /// Format will force unused color channels to the maximum value
    /// </summary>
    ForceFull = (ulong)1 << 57,

    /// <summary>
    /// Mask for extracting channel forcing value
    /// </summary>
    MaskForceChannel = (((ulong)1 << 1) - 1) << 57, /* 0200000000000000 */

    /// <summary>
    /// Reserved
    /// </summary>
    Reserved = Undefined,

    /// <summary>
    /// Mask for extracting reserved bits
    /// </summary>
    MaskReserved = (((ulong)1 << 6) - 1) << 58, /* FC00000000000000 */

    /// <summary>
    /// Format is 24-bit RGB888
    /// </summary>
    FormatRgb888 = Bpp24 | ChannelsRgb | RedBits8 | GreenBits8 | BlueBits8 | AlphaBits0 | ForceFull,

    /// <summary>
    /// Format is 16-bit RGB565
    /// </summary>
    FormatRgb565 = Bpp16 | ChannelsRgb | RedBits5 | GreenBits6 | BlueBits5 | AlphaBits0 | ForceFull,

    /// <summary>
    /// Format is 24-bit BGR888
    /// </summary>
    FormatBgr888 = Bpp24 | ChannelsBgr | RedBits8 | GreenBits8 | BlueBits8 | AlphaBits0 | ForceFull,

    /// <summary>
    /// Format is 16-bit BGR565
    /// </summary>
    FormatBgr565 = Bpp16 | ChannelsBgr | RedBits5 | GreenBits6 | BlueBits5 | AlphaBits0 | ForceFull,

    /// <summary>
    /// Format is 32-bit RGBA8888
    /// </summary>
    FormatRgba8888 = Bpp32 | ChannelsRgba | RedBits8 | GreenBits8 | BlueBits8 | AlphaBits8,

    /// <summary>
    /// Format is 16-bit RGBA5551
    /// </summary>
    FormatRgba5551 = Bpp16 | ChannelsRgba | RedBits5 | GreenBits5 | BlueBits5 | AlphaBits1,

    /// <summary>
    /// Format is 16-bit RGBA4444
    /// </summary>
    FormatRgba4444 = Bpp16 | ChannelsRgba | RedBits4 | GreenBits4 | BlueBits4 | AlphaBits4,

    /// <summary>
    /// Format is 32-bit RGBA8888
    /// </summary>
    FormatBgra8888 = Bpp32 | ChannelsBgra | RedBits8 | GreenBits8 | BlueBits8 | AlphaBits8,

    /// <summary>
    /// Format is 16-bit BGRA5551
    /// </summary>
    FormatBgra5551 = Bpp16 | ChannelsBgra | RedBits5 | GreenBits5 | BlueBits5 | AlphaBits1,

    /// <summary>
    /// Format is 16-bit BGRA4444
    /// </summary>
    FormatBgra4444 = Bpp16 | ChannelsBgra | RedBits4 | GreenBits4 | BlueBits4 | AlphaBits4,

    /// <summary>
    /// Format is 32-bit ARGB8888
    /// </summary>
    FormatArgb8888 = Bpp32 | ChannelsArgb | RedBits8 | GreenBits8 | BlueBits8 | AlphaBits8,

    /// <summary>
    /// Format is 16-bit ARGB1555
    /// </summary>
    FormatArgb1555 = Bpp16 | ChannelsArgb | RedBits5 | GreenBits5 | BlueBits5 | AlphaBits1,

    /// <summary>
    /// Format is 16-bit ARGB4444
    /// </summary>
    FormatArgb4444 = Bpp16 | ChannelsArgb | RedBits4 | GreenBits4 | BlueBits4 | AlphaBits4,

    /// <summary>
    /// Format is 32-bit ABGR8888
    /// </summary>
    FormatAbgr8888 = Bpp32 | ChannelsAbgr | RedBits8 | GreenBits8 | BlueBits8 | AlphaBits8,

    /// <summary>
    /// Format is 16-bit ABGR1555
    /// </summary>
    FormatAbgr1555 = Bpp16 | ChannelsAbgr | RedBits5 | GreenBits5 | BlueBits5 | AlphaBits1,

    /// <summary>
    /// Format is 16-bit ABGR4444
    /// </summary>
    FormatAbgr4444 = Bpp16 | ChannelsAbgr | RedBits4 | GreenBits4 | BlueBits4 | AlphaBits4,

    /// <summary>
    /// Format is 32-bit RGB888 with dummy trailing 8-bit alpha channel
    /// </summary>
    FormatRgbx8888 = Bpp32 | ChannelsRgbx | RedBits8 | GreenBits8 | BlueBits8 | AlphaBits8 | ForceFull,

    /// <summary>
    /// Format is 32-bit BGR888 with dummy trailing 8-bit alpha channel
    /// </summary>
    FormatBgrx8888 = Bpp32 | ChannelsBgrx | RedBits8 | GreenBits8 | BlueBits8 | AlphaBits8 | ForceFull,

    /// <summary>
    /// Format is 32-bit RGB888 with dummy leading 8-bit alpha channel
    /// </summary>
    FormatXrgb8888 = Bpp32 | ChannelsXrgb | RedBits8 | GreenBits8 | BlueBits8 | AlphaBits8 | ForceFull,

    /// <summary>
    /// Format is 32-bit BGR888 with dummy leading 8-bit alpha channel
    /// </summary>
    FormatXbgr8888 = Bpp32 | ChannelsXbgr | RedBits8 | GreenBits8 | BlueBits8 | AlphaBits8 | ForceFull,

    /// <summary>
    /// Format is 16-bit LA88
    /// </summary>
    FormatLuminanceAlpha88 = Bpp16 | ChannelsLuminanceAlpha | RedBits8 | GreenBits0 | BlueBits0 | AlphaBits8,

    /// <summary>
    /// Format is 8-bit LA44
    /// </summary>
    FormatLuminanceAlpha44 = Bpp8 | ChannelsLuminanceAlpha | RedBits4 | GreenBits0 | BlueBits0 | AlphaBits4,

    /// <summary>
    /// Format is 16-bit AL88
    /// </summary>
    FormatAlphaLuminance88 = Bpp16 | ChannelsAlphaLuminance | RedBits8 | GreenBits0 | BlueBits0 | AlphaBits8,

    /// <summary>
    /// Format is 8-bit AL44
    /// </summary>
    FormatAlphaLuminance44 = Bpp8 | ChannelsAlphaLuminance | RedBits4 | GreenBits0 | BlueBits0 | AlphaBits4,

    /// <summary>
    /// Format is 8-bit L8
    /// </summary>
    FormatLuminance8 = Bpp8 | ChannelsLuminance | RedBits8 | GreenBits0 | BlueBits0 | AlphaBits0 | ForceFull,

    /// <summary>
    /// Format is 4-bit L4
    /// </summary>
    FormatLuminance4 = Bpp4 | ChannelsLuminance | RedBits4 | GreenBits0 | BlueBits0 | AlphaBits0 | ForceFull,

    /// <summary>
    /// Format is 8-bit A8
    /// </summary>
    FormatAlpha8 = Bpp8 | ChannelsAlpha | RedBits0 | GreenBits0 | BlueBits0 | AlphaBits8 | ForceClear,

    /// <summary>
    /// Format is 4-bit A4
    /// </summary>
    FormatAlpha4 = Bpp4 | ChannelsAlpha | RedBits0 | GreenBits0 | BlueBits0 | AlphaBits4 | ForceClear,

    /// <summary>
    /// Format is 8-bit indexed
    /// </summary>
    FormatIndexed8 = Bpp8 | ChannelsIndexed | RedBits0 | GreenBits0 | BlueBits0 | AlphaBits0,

    /// <summary>
    /// Format is 4-bit indexed
    /// </summary>
    FormatIndexed4 = Bpp4 | ChannelsIndexed | RedBits0 | GreenBits0 | BlueBits0 | AlphaBits0,

    /// <summary>
    /// Format is 3DS-style ETC1
    /// </summary>
    FormatETC1_3DS = SpecialFormatETC1_3DS,

    /// <summary>
    /// Format is 3DS-style ETC1A4
    /// </summary>
    FormatETC1A4_3DS = SpecialFormatETC1A4_3DS,

    /// <summary>
    /// Format is RGB-mode DXT1
    /// </summary>
    FormatDXT1Rgb = SpecialFormatDXT1 | Bpp4 | ChannelsRgb,

    /// <summary>
    /// Format is RGBA-mode DXT1
    /// </summary>
    FormatDXT1Rgba = SpecialFormatDXT1 | Bpp4 | ChannelsRgba,

    /// <summary>
    /// Format is PSP-style, RGB-mode DXT1
    /// </summary>
    FormatDXT1Rgb_PSP = SpecialFormatDXT1_PSP | Bpp4 | ChannelsRgb,

    /// <summary>
    /// Format is PSP-style, RGBA-mode DXT1
    /// </summary>
    FormatDXT1Rgba_PSP = SpecialFormatDXT1_PSP | Bpp4 | ChannelsRgba,

    /// <summary>
    /// Format is RGBA-mode DXT3
    /// </summary>
    FormatDXT3 = SpecialFormatDXT3 | Bpp8 | ChannelsRgba,

    /// <summary>
    /// Format is PSP-style, RGBA-mode DXT3
    /// </summary>
    FormatDXT3_PSP = SpecialFormatDXT3_PSP | Bpp8 | ChannelsRgba,

    /// <summary>
    /// Format is RGBA-mode DXT5
    /// </summary>
    FormatDXT5 = SpecialFormatDXT5 | Bpp8 | ChannelsRgba,

    /// <summary>
    /// Format is PSP-style, RGBA-mode DXT5
    /// </summary>
    FormatDXT5_PSP = SpecialFormatDXT5_PSP | Bpp8 | ChannelsRgba,

    /// <summary>
    /// Format is Vita-style PVRT2
    /// </summary>
    FormatPVRT2_Vita = SpecialFormatPVRT2_Vita,

    /// <summary>
    /// Format is Vita-style PVRT4
    /// </summary>
    FormatPVRT4_Vita = SpecialFormatPVRT4_Vita,

    /// <summary>
    /// Format is unsigned RGTC1
    /// </summary>
    FormatRGTC1 = SpecialFormatRGTC1 | Bpp4,

    /// <summary>
    /// Format is signed RGTC1
    /// </summary>
    FormatRGTC1_Signed = SpecialFormatRGTC1_Signed | Bpp4,

    /// <summary>
    /// Format is unsigned RGTC2
    /// </summary>
    FormatRGTC2 = SpecialFormatRGTC2 | Bpp8,

    /// <summary>
    /// Format is signed RGTC2
    /// </summary>
    FormatRGTC2_Signed = SpecialFormatRGTC2_Signed | Bpp8,

    /// <summary>
    /// Format is BPTC
    /// </summary>
    FormatBPTC = SpecialFormatBPTC,

    /// <summary>
    /// Format is BPTC Float
    /// </summary>
    FormatBPTC_Float = SpecialFormatBPTC_Float,

    /// <summary>
    /// Format is BPTC Signed Float
    /// </summary>
    FormatBPTC_SignedFloat = SpecialFormatBPTC_SignedFloat,

    /// <summary>
    /// Undefined value
    /// </summary>
    Undefined = 0x00000000
}

internal static class Constants
{
    internal static Dictionary<PixelDataFormat, int> RealBitsPerPixel = new()
    {
        {PixelDataFormat.Bpp4, 4},
        {PixelDataFormat.Bpp8, 8},
        {PixelDataFormat.Bpp16, 16},
        {PixelDataFormat.Bpp24, 24},
        {PixelDataFormat.Bpp32, 32}
    };

    internal static Dictionary<PixelDataFormat, int> InputBitsPerPixel = new()
    {
        {PixelDataFormat.Bpp4, 8},
        {PixelDataFormat.Bpp8, 8},
        {PixelDataFormat.Bpp16, 16},
        {PixelDataFormat.Bpp24, 24},
        {PixelDataFormat.Bpp32, 32}
    };

    internal static Dictionary<PixelDataFormat, int> BitsPerChannel = new()
    {
        {PixelDataFormat.RedBits8, 8},
        {PixelDataFormat.RedBits5, 5},
        {PixelDataFormat.RedBits4, 4},
        {PixelDataFormat.GreenBits8, 8},
        {PixelDataFormat.GreenBits6, 6},
        {PixelDataFormat.GreenBits5, 5},
        {PixelDataFormat.GreenBits4, 4},
        {PixelDataFormat.BlueBits8, 8},
        {PixelDataFormat.BlueBits5, 5},
        {PixelDataFormat.BlueBits4, 4},
        {PixelDataFormat.AlphaBits8, 8},
        {PixelDataFormat.AlphaBits4, 4},
        {PixelDataFormat.AlphaBits1, 1}
    };

    internal static int[,] DitheringBayerMatrix8x8 = new int[8, 8]
    {
        {1, 49, 13, 61, 4, 52, 16, 64},
        {33, 17, 45, 29, 36, 20, 48, 32},
        {9, 57, 5, 53, 12, 60, 8, 56},
        {41, 25, 37, 21, 44, 28, 40, 24},
        {3, 51, 15, 63, 2, 50, 14, 62},
        {35, 19, 47, 31, 34, 18, 46, 30},
        {11, 59, 7, 55, 10, 58, 6, 54},
        {43, 27, 39, 23, 42, 26, 38, 22}
    };
}