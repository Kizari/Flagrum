using System;
using System.Linq;
using Flagrum.Core.Utilities.Types;

namespace Flagrum.Core.Gfxbin.Btex;

public class ImageFormat : Enum<string>
{
    private ImageFormat(string value) : base(value) { }

    public static ImageFormat Png { get; } = new("png");
    public static ImageFormat Targa { get; } = new("tga");
    public static ImageFormat Dds { get; } = new("dds");
    public static ImageFormat Btex { get; } = new("btex");

    public static implicit operator string(ImageFormat format) => format.Value;

    public static explicit operator ImageFormat(string value)
    {
        return GetAll<ImageFormat>().First(f => f.Value.Equals(value, StringComparison.OrdinalIgnoreCase));
    }
}