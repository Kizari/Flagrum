using System;
using System.Linq;
using Flagrum.Core.Utilities.Types;

namespace Flagrum.Core.Graphics.Textures;

public class ImageFormat : Enum<string>
{
    private ImageFormat(string value) : base(value) { }

    public static ImageFormat Png { get; } = new("png");
    public static ImageFormat Targa { get; } = new("tga");
    public static ImageFormat Dds { get; } = new("dds");
    public static ImageFormat Btex { get; } = new("btex");
    public static ImageFormat Heb { get; } = new("heb");

    public static implicit operator string(ImageFormat format)
    {
        return format.Value;
    }

    public static explicit operator ImageFormat(string value)
    {
        return GetAll<ImageFormat>().First(f => f.Value.Equals(value, StringComparison.OrdinalIgnoreCase));
    }
}