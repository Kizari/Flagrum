using System;
using System.Linq;
using Flagrum.Core.Utilities.Types;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tga;

namespace Flagrum.Core.Graphics.Textures.Shared;

public class ImageFileFormat : Enum<string>
{
    private ImageFileFormat(string value, IImageEncoder? encoder) : base(value) => Encoder = encoder;

    public IImageEncoder? Encoder { get; }

    public static ImageFileFormat Png { get; } = new("png", new PngEncoder());
    public static ImageFileFormat Targa { get; } = new("tga", new TgaEncoder());
    public static ImageFileFormat Jpeg { get; } = new("jpg", new JpegEncoder());
    public static ImageFileFormat Dds { get; } = new("dds", null);
    public static ImageFileFormat Btex { get; } = new("btex", null);
    public static ImageFileFormat Heb { get; } = new("heb", null);

    public static implicit operator string(ImageFileFormat format) => format.Value;

    public static explicit operator ImageFileFormat(string value)
    {
        return GetAll<ImageFileFormat>().First(f => f.Value.Equals(value, StringComparison.OrdinalIgnoreCase));
    }
}