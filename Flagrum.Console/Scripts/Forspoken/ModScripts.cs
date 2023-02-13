using System.Collections.Generic;
using System.IO;
using Flagrum.Core.Archive;
using Flagrum.Core.Utilities;
using Flagrum.Core.Utilities.Types;

namespace Flagrum.Console.Scripts.Forspoken;

public static class ModScripts
{
    private const string ForspokenDirectory = @"C:\Program Files (x86)\Steam\steamapps\common\FORSPOKEN Demo\datas";
    private const string AssetsDirectory = @"C:\Modding\Forspoken\Assets";

    private static EbonyReplace _erep;
    private static EbonyArchive _modArchive;

    private static readonly List<(string originalUri, string replacementUri, string replacementFilePath)>
        _replacements = new()
        {
            ("data://asset/character/hu/hu000/model_000/sourceimages/tshirts/hu000_000_inner_shirts_1001_b.tif",
                "data://mods/hu000_000_inner_shirts_1001_b.tif",
                AssetPath("hu000_000_inner_shirts_1001_b_$h.btex")),

            ("data://asset/character/hu/hu000/model_000/sourceimages/tshirts/hu000_000_inner_shirts_1001_b_$h.tif",
                "data://mods/hu000_000_inner_shirts_1001_b_$h.tif",
                AssetPath("hu000_000_inner_shirts_1001_b_$h.btex")),

            ("data://asset/character/hu/hu000/model_000/sourceimages/tshirts/hu000_000_inner_shirts_1001_b_$m1.tif",
                "data://mods/hu000_000_inner_shirts_1001_b_$m1.tif",
                AssetPath("hu000_000_inner_shirts_1001_b_$h.btex"))
        };

    public static void CreateMod()
    {
        // Load up c000.earc and the erep file
        using var c000 = new EbonyArchive(AbsolutePath("c000.backup"));
        c000.WriteToFile(AbsolutePath("c000.earc"), LuminousGame.Forspoken);
        return;

        var erepFile = c000["data://c000.erep"];
        _erep = new EbonyReplace(erepFile.GetReadableData());

        // Create a new mod earc
        _modArchive = new EbonyArchive(false);
        _modArchive.SetFlags(EbonyArchiveFlags.HasLooseData);

        foreach (var (originalUri, replacementUri, replacementFilePath) in _replacements)
        {
            AddReplacement(originalUri, replacementUri, replacementFilePath);
        }

        _modArchive.WriteToFile(AbsolutePath("mods.earc"), LuminousGame.Forspoken);
        _modArchive.Dispose();

        // Write the updated erep to the archive
        using var stream = new MemoryStream();
        _erep.Write(stream);
        //erepFile.SetRawData(stream.ToArray());

        // Add reference to the new mod archive and write updated c000.earc to disk
        //c000.AddFile("data://mods.ebex@", ArchiveFileFlag.Autoload | ArchiveFileFlag.Reference, Array.Empty<byte>());
        //c000["data://mods.ebex@"].RelativePath = "$archives/mods.earc";
        c000.WriteToFile(AbsolutePath("c000.earc"), LuminousGame.Forspoken);
    }

    private static void AddReplacement(string originalUri, string replacementUri, string replacementFilePath)
    {
        // Add new file to the mod archive
        var data = File.ReadAllBytes(replacementFilePath);
        _modArchive.AddFile(replacementUri, ArchiveFileFlag.Compressed | ArchiveFileFlag.LZ4Compression, data);

        // Add replacement record to the erep
        _erep.Replacements.Add(Cryptography.HashFileUri64(originalUri), Cryptography.HashFileUri64(replacementUri));
    }

    private static string AbsolutePath(string relativePath)
    {
        return $@"{ForspokenDirectory}\{relativePath}";
    }

    private static string AssetPath(string relativePath)
    {
        return $@"{AssetsDirectory}\{relativePath}";
    }
}