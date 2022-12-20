using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Flagrum.Console.Ps4.Mogfest.Utilities;

public class MogfestUtilities
{
    public const string DatasDirectory = @"F:\FFXV\Builds\Festivals_Data\Unpacked";
    public const string ArchetypeMapPath = @"C:\Modding\Chocomog\Staging\archetype.csv";

    public static string UriToFilePath(string uri)
    {
        var relativePath = uri.Replace("data://", "").Replace('/', '\\');
        return $@"{DatasDirectory}\{relativePath}";
    }

    public static byte[] GetFileByUri(string uri)
    {
        var path = UriToFilePath(uri);
        return File.Exists(path) ? File.ReadAllBytes(path) : Array.Empty<byte>();
    }

    public static Dictionary<uint, string> GetArchetypeMap()
    {
        var lines = File.ReadAllLines(ArchetypeMapPath)[1..]; // Skip header row
        var records = lines.Select(l =>
        {
            var columns = l.Split('|');
            return new
            {
                Fixid = uint.Parse(columns[0]),
                Uri = columns[5]
            };
        });

        return records.ToDictionary(r => r.Fixid, r => r.Uri);
    }
}