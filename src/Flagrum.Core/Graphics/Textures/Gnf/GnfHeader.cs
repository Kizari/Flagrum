using System;
using System.IO;
using System.Text;
using Flagrum.Core.Utilities.Extensions;
using Scarlet.Drawing;

namespace Flagrum.Core.Ps4;

public class GnfHeader
{
    public string Magic { get; set; }
    public uint Unknown0x04 { get; set; }
    public uint Unknown0x08 { get; set; }
    public uint FileSize { get; set; }
    public uint Unknown0x10 { get; set; }
    public uint ImageInformation1 { get; set; }
    public uint ImageInformation2 { get; set; }
    public uint ImageInformation3 { get; set; }
    public uint ImageInformation4 { get; set; }
    public uint Unknown0x24 { get; set; }
    public uint Unknown0x28 { get; set; }
    public uint DataSize { get; set; }

    public GnfDataFormat DataFormat { get; set; }
    public GnfNumFormat NumFormat { get; set; }
    public uint Width { get; set; }
    public uint Height { get; set; }
    public uint Depth { get; set; }
    public uint Pitch { get; set; }
    public GnfSqSel DestinationX { get; set; }
    public GnfSqSel DestinationY { get; set; }
    public GnfSqSel DestinationZ { get; set; }
    public GnfSqSel DestinationW { get; set; }

    public byte[] Read(BinaryReader reader, uint arrayCount)
    {
        // Read the information from the GNF header
        Magic = Encoding.UTF8.GetString(reader.ReadBytes(4));
        Unknown0x04 = reader.ReadUInt32();
        Unknown0x08 = reader.ReadUInt32();
        FileSize = reader.ReadUInt32();
        Unknown0x10 = reader.ReadUInt32();
        ImageInformation1 = reader.ReadUInt32();
        ImageInformation2 = reader.ReadUInt32();
        ImageInformation3 = reader.ReadUInt32();
        ImageInformation4 = reader.ReadUInt32();
        Unknown0x24 = reader.ReadUInt32();
        Unknown0x28 = reader.ReadUInt32();
        DataSize = reader.ReadUInt32();
        reader.Align(256);

        // Extract the data from the GNF header information
        DataFormat = (GnfDataFormat)ExtractData(ImageInformation1, 20, 25);
        NumFormat = (GnfNumFormat)ExtractData(ImageInformation1, 26, 29);
        Width = ExtractData(ImageInformation2, 0, 13) + 1;
        Height = ExtractData(ImageInformation2, 14, 27) + 1;
        Depth = ExtractData(ImageInformation4, 0, 12);
        Pitch = ExtractData(ImageInformation4, 13, 26) + 1;
        DestinationX = (GnfSqSel)ExtractData(ImageInformation3, 0, 2);
        DestinationY = (GnfSqSel)ExtractData(ImageInformation3, 3, 5);
        DestinationZ = (GnfSqSel)ExtractData(ImageInformation3, 6, 8);
        DestinationW = (GnfSqSel)ExtractData(ImageInformation3, 9, 11);

        // Read the pixel data
        var data = new byte[DataSize];
        _ = reader.Read(data);

        var imageBinary = new ImageBinary
        {
            Width = (int)Width,
            Height = (int)Height,
            PhysicalWidth = (int)Pitch,
            PhysicalHeight = (int)Height
        };

        switch (DataFormat)
        {
            case GnfDataFormat.Format8_8_8_8:
                PixelDataFormat channelOrder;
                if (DestinationX == GnfSqSel.SelX && DestinationY == GnfSqSel.SelY &&
                    DestinationZ == GnfSqSel.SelZ && DestinationW == GnfSqSel.SelW)
                {
                    channelOrder = PixelDataFormat.ChannelsAbgr;
                }
                else if (DestinationX == GnfSqSel.SelZ && DestinationY == GnfSqSel.SelY &&
                         DestinationZ == GnfSqSel.SelX && DestinationW == GnfSqSel.SelW)
                {
                    channelOrder = PixelDataFormat.ChannelsArgb;
                }
                else
                {
                    throw new Exception(
                        $"Unhandled GNF channel destinations (X={DestinationX}, Y={DestinationY}, Z={DestinationZ}, W={DestinationW})");
                }

                imageBinary.InputPixelFormat = PixelDataFormat.Bpp32 | channelOrder | PixelDataFormat.RedBits8 |
                                               PixelDataFormat.GreenBits8 | PixelDataFormat.BlueBits8 |
                                               PixelDataFormat.AlphaBits8;
                break;
            case GnfDataFormat.FormatBC1:
                imageBinary.InputPixelFormat = PixelDataFormat.FormatDXT1Rgba;
                break;
            case GnfDataFormat.FormatBC2:
                imageBinary.InputPixelFormat = PixelDataFormat.FormatDXT3;
                break;
            case GnfDataFormat.FormatBC3:
                imageBinary.InputPixelFormat = PixelDataFormat.FormatDXT5;
                break;
            case GnfDataFormat.FormatBC4:
                imageBinary.InputPixelFormat = NumFormat == GnfNumFormat.FormatSNorm
                    ? PixelDataFormat.FormatRGTC1_Signed
                    : PixelDataFormat.FormatRGTC1;
                break;
            case GnfDataFormat.FormatBC5:
                imageBinary.InputPixelFormat = NumFormat == GnfNumFormat.FormatSNorm
                    ? PixelDataFormat.FormatRGTC2_Signed
                    : PixelDataFormat.FormatRGTC2;
                break;
            //case GnfDataFormat.FormatBC6: imageBinary.InputPixelFormat = PixelDataFormat.FormatBPTC_Float;/*(numFormat == GnfNumFormat.FormatSNorm ? PixelDataFormat.FormatBPTC_SignedFloat : PixelDataFormat.FormatBPTC_Float);*/ break;   // TODO: fixme!!
            case GnfDataFormat.FormatBC7:
                imageBinary.InputPixelFormat = PixelDataFormat.FormatBPTC;
                break;

            // TODO
            //case GnfDataFormat.Format16_16_16_16: imageBinary.InputPixelFormat = PixelDataFormat.FormatAbgr8888; break;
            //case GnfDataFormat.Format32: imageBinary.InputPixelFormat = PixelDataFormat.FormatLuminance8; break;
            //case GnfDataFormat.Format32_32_32_32: imageBinary.InputPixelFormat = PixelDataFormat.FormatAbgr8888; break;

            // WRONG
            //case GnfDataFormat.Format8: imageBinary.InputPixelFormat = PixelDataFormat.FormatLuminance8; break;
            //case GnfDataFormat.Format8_8: imageBinary.InputPixelFormat = PixelDataFormat.FormatLuminance8; break;

            default:
                throw new Exception($"Unhandled pixel format {DataFormat}");
        }

        imageBinary.InputPixelFormat |= PixelDataFormat.PixelOrderingTiled3DS;

        if (arrayCount > 1)
        {
            var imageSize = Pitch * imageBinary.Height / 2;
            var start = 0u;
            for (var i = 0; i < arrayCount; i++)
            {
                var currentImage = data[(int)start..(int)(start + imageSize)];
                imageBinary.AddInputPixels(currentImage);
                start += (uint)imageSize;
            }
        }
        else
        {
            imageBinary.AddInputPixels(data);
        }

        return imageBinary.GetOutputPixelData(0);
    }

    private static uint ExtractData(uint val, int first, int last)
    {
        var mask = ((uint)(1 << (last + 1 - first)) - 1) << first;
        return (val & mask) >> first;
    }
}