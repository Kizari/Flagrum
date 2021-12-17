using System;
using System.IO;
using Flagrum.Web.Services;
using TextureType = Flagrum.Core.Gfxbin.Btex.TextureType;

namespace Flagrum.Console;

public static class BtexTests
{
    public static void Convert()
    {
        var name = "body_o";
        var timer = DateTime.Now;
        var inputPath = $"C:\\Modding\\BTex\\{name}.png";
        var outputPath = $"C:\\Modding\\BTex\\{name}.btex";

        var converter = new TextureConverter();
        var btex = converter.Convert(name, "png", TextureType.Greyscale, File.ReadAllBytes(inputPath));
        File.WriteAllBytes(outputPath, btex);
        System.Console.WriteLine((DateTime.Now - timer).TotalMilliseconds);
    }
}