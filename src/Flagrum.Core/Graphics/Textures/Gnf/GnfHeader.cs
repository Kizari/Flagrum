using System;
using System.Runtime.InteropServices;

namespace Flagrum.Core.Graphics.Textures.Gnf;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct GnfHeader
{
    public uint Magic;
    public uint Unknown;
    public uint Unknown2;
    public uint FileSize;
    public uint Unknown3;
    public uint ImageInformation1;
    public uint ImageInformation2;
    public uint ImageInformation3;
    public uint ImageInformation4;
    public uint Unknown4;
    public uint Unknown5;
    public uint DataSize;

    public GnfDataFormat DataFormat => (GnfDataFormat)GetValue(ImageInformation1, 20, 25);
    public GnfNumFormat NumFormat => (GnfNumFormat)GetValue(ImageInformation1, 26, 29);
    public uint Width => GetValue(ImageInformation2, 0, 13) + 1;
    public uint Height => GetValue(ImageInformation2, 14, 27) + 1;
    public uint Depth => GetValue(ImageInformation4, 0, 12);
    public uint Pitch => GetValue(ImageInformation4, 13, 26) + 1;
    public GnfSqSel DestinationX => (GnfSqSel)GetValue(ImageInformation3, 0, 2);
    public GnfSqSel DestinationY => (GnfSqSel)GetValue(ImageInformation3, 3, 5);
    public GnfSqSel DestinationZ => (GnfSqSel)GetValue(ImageInformation3, 6, 8);
    public GnfSqSel DestinationW => (GnfSqSel)GetValue(ImageInformation3, 9, 11);

    public PixelDataFormat PixelDataFormat => DataFormat switch
    {
        GnfDataFormat.Format8_8_8_8 => PixelDataFormat.Bpp32
                                       | ChannelOrder
                                       | PixelDataFormat.RedBits8
                                       | PixelDataFormat.GreenBits8
                                       | PixelDataFormat.BlueBits8
                                       | PixelDataFormat.AlphaBits8,
        GnfDataFormat.FormatBC1 => PixelDataFormat.FormatDXT1Rgba,
        GnfDataFormat.FormatBC2 => PixelDataFormat.FormatDXT3,
        GnfDataFormat.FormatBC3 => PixelDataFormat.FormatDXT5,
        GnfDataFormat.FormatBC4 => NumFormat == GnfNumFormat.FormatSNorm
            ? PixelDataFormat.FormatRGTC1_Signed
            : PixelDataFormat.FormatRGTC1,
        GnfDataFormat.FormatBC5 => NumFormat == GnfNumFormat.FormatSNorm
            ? PixelDataFormat.FormatRGTC2_Signed
            : PixelDataFormat.FormatRGTC2,
        GnfDataFormat.FormatBC7 => PixelDataFormat.FormatBPTC,
        _ => throw new NotSupportedException($"Unsupported {nameof(PixelDataFormat)} '{DataFormat}'.")
    };

    private static uint GetValue(uint information, int first, int last)
    {
        var mask = ((uint)(1 << (last + 1 - first)) - 1) << first;
        return (information & mask) >> first;
    }

    private PixelDataFormat ChannelOrder => DestinationX switch
    {
        GnfSqSel.SelX when DestinationY == GnfSqSel.SelY
                           && DestinationZ == GnfSqSel.SelZ
                           && DestinationW == GnfSqSel.SelW => PixelDataFormat.ChannelsAbgr,
        GnfSqSel.SelZ when DestinationY == GnfSqSel.SelY
                           && DestinationZ == GnfSqSel.SelX
                           && DestinationW == GnfSqSel.SelW => PixelDataFormat.ChannelsArgb,
        _ => throw new Exception($"Unhandled GNF channel destinations " +
                                 $"(X={DestinationX}, Y={DestinationY}, Z={DestinationZ}, W={DestinationW})")
    };
}