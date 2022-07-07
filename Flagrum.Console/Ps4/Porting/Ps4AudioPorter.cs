using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Flagrum.Core.Utilities;
using Newtonsoft.Json;

namespace Flagrum.Console.Ps4.Porting;

public class Ps4AudioPorter
{
    /// <summary>
    /// Dumps all audio files to disk so they can then be converted to PS4.
    /// </summary>
    public void Run()
    {
        const string audioDirectory = $@"{Ps4PorterConfiguration.StagingDirectory}\Audio";
        const string outputDirectory = $@"{audioDirectory}\Output";

        // Clear audio files from previous runs
        foreach (var file in Directory.EnumerateFiles(audioDirectory)
                     .Where(f => !f.EndsWith(".exe") && !f.EndsWith(".json") && !f.EndsWith(".pdb")))
        {
            File.Delete(file);
        }

        foreach (var directory in Directory.EnumerateDirectories(audioDirectory))
        {
            foreach (var file in Directory.EnumerateFiles(directory))
            {
                File.Delete(file);
            }
            
            Directory.Delete(directory);
        }
        
        IOHelper.EnsureDirectoryExists(outputDirectory);
        
        var files = new List<(string hash, string extension)>();
        
        var json = File.ReadAllText(@$"{Ps4PorterConfiguration.StagingDirectory}\assets.json");
        var assets = JsonConvert.DeserializeObject<List<string>>(json)!;
        assets = assets.Where(a => a.EndsWith(".sax") || a.EndsWith(".max"))
            .ToList();

        using var context = Ps4Utilities.NewContext();
        foreach (var asset in assets)
        {
            var data = Ps4Utilities.GetFileByUri(context, asset);
            if (data.Length == 0)
            {
                //System.Console.WriteLine($"{asset} not found in DB, checking file system...");
                var directPath = asset.Replace("data://", "").Replace('/', '\\');
                directPath = directPath.Insert(directPath.LastIndexOf('.'), ".orb");
                directPath = directPath[..^1] + 'b';    // Change sax/max to sab/mab
                directPath = Ps4PorterConfiguration.GameDirectory + @"\CUSA01633-patch_115\CUSA01633-patch\" + directPath;

                if (File.Exists(directPath))
                {
                    data = File.ReadAllBytes(directPath);
                }
                else
                {
                    System.Console.WriteLine($"[E] {directPath} not found!");
                    continue;
                }
            }
            
            var hash = Cryptography.HashFileUri64(asset).ToString();
            var extension = asset.Split('.').Last() == "sax" ? "orb.sab" : "orb.mab";
            var fileName = $@"{audioDirectory}\{hash}.{extension}";
            File.WriteAllBytes(fileName, data);
            files.Add((hash, extension));
        }
        
        // This part requires that AudioMog is present on the system in the staging directory
        // See https://github.com/Yoraiz0r/AudioMog
        foreach (var (hash, extension) in files)
        {
            try
            {
                var arguments = $"/C \"{audioDirectory}\\AudioMog.exe\" {audioDirectory}\\{hash}.{extension}";
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
            }
            catch
            {
                // File was most likely dummy codec and doesn't need porting, just copy to output
                File.Copy($@"{audioDirectory}\{hash}.{extension}", $@"{audioDirectory}\Output\{hash}.{extension}");
            }
        }
        
        // This part requires that AudioMog is present on the system in the staging directory
        // See https://github.com/Yoraiz0r/AudioMog
        foreach (var (hash, extension) in files)
        {
            try
            {
                var arguments = $"/C \"{audioDirectory}\\AudioMog.exe\" {audioDirectory}\\{hash}.orb_Project\\RebuildSettings.json";
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
            
                File.Move($@"{audioDirectory}\{hash}.orb_Project\{hash}.{extension}", $@"{audioDirectory}\Output\{hash}.{extension}");
            }
            catch
            {
                // Failure carried over from previous step, ignore it
            }
        }
    }
}