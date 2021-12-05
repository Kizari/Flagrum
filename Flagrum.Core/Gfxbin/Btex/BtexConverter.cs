using System.Diagnostics;

namespace Flagrum.Core.Gfxbin.Btex;

public static class BtexConverter
{
    public enum TextureType
    {
        Color,
        Greyscale,
        Normal
    }

    public static void Convert(string btexConverterPath, string inPath, string outPath, TextureType type)
    {
        var process = new Process();
        var startInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/C {btexConverterPath} {GetArgsForType(type, inPath, outPath)}",
            CreateNoWindow = true,
            UseShellExecute = false
        };

        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit();
    }

    private static string GetArgsForType(TextureType type, string inPath, string outPath)
    {
        var args = $"-p \"dx11\" --composite --outbtex \"{outPath}\" --adapter 0 --out_format ";

        switch (type)
        {
            case TextureType.Normal:
                args += "BC5 --in_linear ";
                break;
            case TextureType.Greyscale:
                args += "BC4 --in_linear ";
                break;
            case TextureType.Color:
                args += "DXT1 --in_srgb --out_srgb ";
                break;
        }

        args += $"\"{inPath}\"";
        return args;
    }
}