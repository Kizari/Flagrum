using System.Diagnostics;

namespace Flagrum.Core.Gfxbin.Btex;

public static class BtexConverter
{
    public enum TextureType
    {
        Color,
        Greyscale,
        Normal,
        Thumbnail
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
        var args = $"-p \"dx11\" --composite --outbtex \"{outPath}\" --adapter 0 ";

        switch (type)
        {
            case TextureType.Normal:
                args += "--out_format BC5 --in_linear ";
                break;
            case TextureType.Greyscale:
                args += "--out_format BC4 --in_linear ";
                break;
            case TextureType.Color:
                args += "--out_format DXT1 --in_srgb --out_srgb ";
                break;
            case TextureType.Thumbnail:
                args += "--in_srgb --out_srgb ";
                break;
        }

        args += $"\"{inPath}\"";
        return args;
    }
}