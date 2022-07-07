using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Flagrum.Core.Archive;
using Flagrum.Core.Utilities;
using Flagrum.Web.Persistence;
using Flagrum.Web.Services;

namespace Flagrum.Console.Ps4.Porting;

public class Ps4PostRunAudioPorter
{
    public void Run()
    {
        using var ps4Context = Ps4Utilities.NewContext();
        using var pcContext = new FlagrumDbContext(new SettingsService());

        var sounds = ps4Context.Ps4AssetUris.Where(a => a.Uri.EndsWith(".sax"))
            .Select(a => a.Uri)
            .ToList();

        var missingSounds = sounds
            .Where(sound => !pcContext.AssetUris.Any(a => a.Uri == sound))
            .ToList();

        const string audioDirectory = $@"{Ps4PorterConfiguration.StagingDirectory}\Audio2";
        const string outputDirectory = $@"{audioDirectory}\Output";

        // Clear audio files from previous runs
        // foreach (var file in Directory.EnumerateFiles(audioDirectory)
        //              .Where(f => !f.EndsWith(".exe") && !f.EndsWith(".json") && !f.EndsWith(".pdb")))
        // {
        //     File.Delete(file);
        // }
        //
        // foreach (var directory in Directory.EnumerateDirectories(audioDirectory))
        // {
        //     foreach (var file in Directory.EnumerateFiles(directory))
        //     {
        //         File.Delete(file);
        //     }
        //     
        //     Directory.Delete(directory);
        // }

        IOHelper.EnsureDirectoryExists(outputDirectory);

        var files = new List<(string hash, string extension)>();

        using var context = Ps4Utilities.NewContext();
        foreach (var asset in missingSounds)
        {
            var data = Ps4Utilities.GetFileByUri(context, asset);
            if (data.Length == 0)
            {
                continue;
            }

            var hash = Cryptography.HashFileUri64(asset).ToString();
            var extension = asset.Split('.').Last() == "sax" ? "orb.sab" : "orb.mab";
            // var fileName = $@"{audioDirectory}\{hash}.{extension}";
            // File.WriteAllBytes(fileName, data);
            files.Add((hash, extension));
        }

        // This part requires that AudioMog is present on the system in the staging directory
        // See https://github.com/Yoraiz0r/AudioMog
        // var failed = new List<string>();
        // foreach (var (hash, extension) in files)
        // {
        //     try
        //     {
        //         var arguments = $"/C \"{audioDirectory}\\AudioMog.exe\" {audioDirectory}\\{hash}.{extension}";
        //         var start = new ProcessStartInfo
        //         {
        //             FileName = "cmd",
        //             WorkingDirectory = audioDirectory,
        //             Arguments = arguments,
        //             WindowStyle = ProcessWindowStyle.Hidden
        //         };
        //
        //         var process = new Process {StartInfo = start};
        //         process.Start();
        //         process.WaitForExit();
        //     }
        //     catch
        //     {
        //         // File was most likely dummy codec and doesn't need porting, just copy to output
        //         File.Copy($@"{audioDirectory}\{hash}.{extension}", $@"{audioDirectory}\Output\{hash}.{extension}");
        //         failed.Add(hash);
        //     }
        // }

        // This part requires that AudioMog is present on the system in the staging directory
        // See https://github.com/Yoraiz0r/AudioMog
        foreach (var (hash, extension) in files)
        {
            try
            {
                // if (failed.Contains(hash))
                // {
                //     continue;
                // }

                var arguments =
                    $"/C \"{audioDirectory}\\AudioMog.exe\" {audioDirectory}\\{hash}.orb_Project\\RebuildSettings.json";
                System.Console.WriteLine(arguments);
                var start = new ProcessStartInfo
                {
                    FileName = "cmd",
                    WorkingDirectory = audioDirectory,
                    Arguments = arguments,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                var process = new Process {StartInfo = start};
                process.Start();
                process.WaitForExit();

                File.Move($@"{audioDirectory}\{hash}.orb_Project\{hash}.{extension}",
                    $@"{audioDirectory}\Output\{hash}.{extension}");
            }
            catch
            {
                System.Console.WriteLine($"[E] {hash} failed to convert back!");
            }
        }

        var unpacker =
            new Unpacker(
                @"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV\datas\level\dlc_ex\mog\area_ravettrice_mog.backup");
        var packer = unpacker.ToPacker();

        foreach (var uri in missingSounds)
        {
            var hash = Cryptography.HashFileUri64(uri).ToString();
            var extension = uri.Split('.').Last() == "sax" ? "orb.sab" : "orb.mab";
            var fileName = $@"{outputDirectory}\{hash}.{extension}";

            if (!File.Exists(fileName))
            {
                System.Console.WriteLine($"[W] {uri} had no data!");
            }
            else
            {
                var data = File.ReadAllBytes(fileName);
                packer.AddCompressedFile(uri, data, true);
            }

            var lsd = uri.Insert(uri.LastIndexOf('/'), "/lsd").Replace(".sax", ".lsd").Replace(".max", ".lsd");
            var lsdData = Ps4Utilities.GetFileByUri(ps4Context, lsd);
            if (lsdData.Length > 0)
            {
                packer.AddCompressedFile(lsd, lsdData, true);
            }
        }

        packer.WriteToFile(
            @"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV\datas\level\dlc_ex\mog\area_ravettrice_mog.earc");
    }

    public void AddConvertedSoundsToMainEarc()
    {
        using var ps4Context = Ps4Utilities.NewContext();
        using var pcContext = new FlagrumDbContext(new SettingsService());

        var sounds = ps4Context.Ps4AssetUris.Where(a => a.Uri.EndsWith(".sax"))
            .Select(a => a.Uri)
            .ToList();

        var missingSounds = sounds
            .Where(sound => !pcContext.AssetUris.Any(a => a.Uri == sound))
            .ToList();

        const string audioDirectory = $@"{Ps4PorterConfiguration.StagingDirectory}\Audio2";
        const string outputDirectory = $@"{audioDirectory}\Output";

        var unpacker =
            new Unpacker(
                @"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV\datas\level\dlc_ex\mog\area_ravettrice_mog.backup");
        var packer = unpacker.ToPacker();

        foreach (var uri in missingSounds)
        {
            var hash = Cryptography.HashFileUri64(uri).ToString();
            var extension = uri.Split('.').Last() == "sax" ? "orb.sab" : "orb.mab";
            var fileName = $@"{outputDirectory}\{hash}.{extension}";

            if (!File.Exists(fileName))
            {
                System.Console.WriteLine($"[W] {uri} had no data!");
            }
            else
            {
                var data = File.ReadAllBytes(fileName);
                packer.AddCompressedFile(uri, data, true);
            }

            var lsd = uri.Insert(uri.LastIndexOf('/'), "/lsd").Replace(".sax", ".lsd").Replace(".max", ".lsd");
            var lsdData = Ps4Utilities.GetFileByUri(ps4Context, lsd);
            if (lsdData.Length > 0)
            {
                packer.AddCompressedFile(lsd, lsdData, true);
            }
        }

        packer.WriteToFile(
            @"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV\datas\level\dlc_ex\mog\area_ravettrice_mog.earc");
    }
}