using System;
using System.IO;
using Flagrum.Core.Gfxbin.Btex;
using Flagrum.Web.Services;

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
        var btex = converter.ToBtex(name, "png", TextureType.Greyscale, File.ReadAllBytes(inputPath));
        File.WriteAllBytes(outputPath, btex);
        System.Console.WriteLine((DateTime.Now - timer).TotalMilliseconds);
    }
}