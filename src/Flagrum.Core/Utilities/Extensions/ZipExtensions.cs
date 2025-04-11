using System.IO.Compression;

namespace Flagrum.Core.Utilities.Extensions;

public static class ZipExtensions
{
    public static byte[] Extract(this ZipArchive zip, string fileName)
    {
        var entry = zip.GetEntry(fileName);
        using var stream = entry!.Open();
        var buffer = new byte[entry.Length];
        _ = stream.Read(buffer);
        return buffer;
    }
}