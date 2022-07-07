using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Flagrum.Core.Animation;
using Flagrum.Core.Archive;
using Flagrum.Core.Ebex.Xmb2;
using Flagrum.Core.Utilities;
using Flagrum.Web.Services;
using Newtonsoft.Json;

namespace Flagrum.Console.Ps4.Porting;

public class Ps4Porter
{
    private readonly Dictionary<string, bool> _extensions = new();
    private readonly SettingsService _settings = new();
    private SettingsService _pcSettings;

    public void Run()
    {
        _settings.GamePath = Ps4PorterConfiguration.GamePath;
        _pcSettings = new SettingsService();

        //Copy4KPackFromReleaseBuild();
        //return;

        //FileFinder.FindStringInAllFiles("data://sound/resources/05112SE_dungeon_crestholm/jp/dg_pr_door01_A_open.sax");
        //return;
        //FileFinder.FindStringInExml("ul_tb_g_basea.ebex");
        //return;

        //new Ps4MaterialGenerator().BuildMaterialMap();
        //return;

        // var material = new MaterialReader(File.ReadAllBytes(@"C:\Modding\Chocomog\Testing\ex_pr_illumi_mt.gmtl.gfxbin")).Read();
        // foreach (var shaderUri in material.Header.Dependencies.Where(d => d.Path.EndsWith(".sb")))
        // {
        //     System.Console.WriteLine(shaderUri.Path);
        // }
        //
        // return;

        // using var context = Ps4Utilities.NewContext();
        // var data = Ps4Utilities.GetFileByUri(context,
        //     "data://character/pr/pr56/model_002/materials/al_pr_mog01_flag_b.gmtl");
        // var material = new MaterialReader(data).Read();
        // return;

        // var names = new ConcurrentDictionary<string, bool>();
        // using var context = Ps4Utilities.NewContext();
        // var skinning6Bones = context.Ps4VertexLayoutTypeMaps.Where(m =>
        //     m.VertexLayoutType.HasFlag(VertexLayoutType.Skinning_6Bones)).Select(m => m.Uri).ToList();
        // new FileFinder().FindByQuery(file => file.Uri.EndsWith(".gmtl") && file.Size > 10,
        //     (earc, file) =>
        //     {
        //         var material = new MaterialReader(file.GetReadableData()).Read();
        //         if (material.Textures.Any(t =>
        //                 t.ShaderGenName.Contains("basecolor0", StringComparison.OrdinalIgnoreCase))
        //             && material.Textures.Any(t =>
        //                 t.ShaderGenName.Contains("mro_mix0", StringComparison.OrdinalIgnoreCase))
        //             && material.Textures.Any(t =>
        //                 t.ShaderGenName.Contains("normal0", StringComparison.OrdinalIgnoreCase))
        //             )
        //         {
        //             if (skinning6Bones.Contains(file.Uri))
        //             {
        //                 System.Console.WriteLine(material.Interfaces[0].Name + " - " + file.Uri);
        //                 names.TryAdd(material.Interfaces[0].Name, true);
        //             }
        //         }
        //     },
        //     true);
        //
        // foreach (var (name, _) in names)
        // {
        //     System.Console.WriteLine(name);
        // }
        //
        // return;

        // var json = File.ReadAllText(@"C:\Modding\Chocomog\Testing\assets.json");
        // var assets = JsonConvert.DeserializeObject<List<string>>(json)!;
        // foreach (var materialUri in assets.Where(a => a.EndsWith(".gmtl")))
        // {
        //     var materialData = Ps4Utilities.GetFileByUri(context, materialUri);
        //     var material = new MaterialReader(materialData).Read();
        //     var emissive = material.Textures.FirstOrDefault(t =>
        //         t.ShaderGenName.Contains("emissive", StringComparison.OrdinalIgnoreCase));
        //     if (emissive != null)
        //     {
        //         System.Console.WriteLine(emissive.Path);
        //     }
        // }
        //
        // return;

        //FileFinder.FindUriByString("al_pr_lightobj01_code01_anim");
        //return;

        // var gmdl = Ps4Utilities.GetFileByUri(context,
        //     "data://environment/altissia/props/al_pr_mogcho1/models/al_pr_mogcho1_lampA.gmdl");
        // var gpubin = Ps4Utilities.GetFileByUri(context,
        //     "data://environment/altissia/props/al_pr_mogcho1/models/al_pr_mogcho1_lampA.gpubin");
        // var model = new ModelReader(gmdl, gpubin).Read();
        // return;

        // var btexData = Ps4Utilities.GetFileByUri(context,
        //     "data://environment/altissia/props/al_pr_mogcho1/sourceimages/al_pr_mogcho_gamegatea_e.tif");
        // var btex = Btex.FromData(btexData);
        // btex.Bitmap.Save(@"C:\Modding\Chocomog\Testing\XML\test.png");
        //
        // var materialData =
        //     Ps4Utilities.GetFileByUri(context,
        //         "data://environment/altissia/props/al_pr_lightobj01/materials/al_pr_lightobj01_code01_anim.gmtl");
        // var material = new MaterialReader(materialData).Read();
        // return;

        // var unpacker =
        //     new Unpacker(
        //         @"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV\datas\level\dlc_ex\mog\area_ravettrice_mog.earc");
        //
        // var packer = unpacker.ToPacker();
        // packer.RemoveFile("data://sound/resources/00007se_mogchoco/jp/walla_loop.sax");
        // var data = File.ReadAllBytes(
        //     @"C:\Code\AudioMog\AudioMogTerminal\bin\Debug\walla_loop.orb_Project\walla_loop.orb.sab");
        // packer.AddCompressedFile("data://sound/resources/00007se_mogchoco/jp/walla_loop.sax", data, true);
        // packer.WriteToFile(@"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV\datas\level\dlc_ex\mog\area_ravettrice_mog.earc");
        // return;

        //FileFinder.FindStringInExml("balloon02.gmdl");
        //return;

        //new Ps4MaterialGenerator().FindStubbornMatches();
        //return;

        OutputFileByUri("data://character/um/common/um.amdl");
        return;

        // var data = File.ReadAllBytes(@"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV\ffxv_s.exe");
        // var results = searcher.Search(data);
        // foreach (var result in results)
        // {
        //     System.Console.WriteLine(result);
        // }
        // return;

        // new FileFinder().FindByQuery(file => file.Flags.HasFlag(ArchiveFileFlag.Reference) && file.Uri.Contains("data://sound/resources/00001se_ui/"),
        //     (earc, file) => System.Console.WriteLine($"{file.Uri} found in {earc}"),
        //     false);
        // return;

        // new FileFinder().FindByQuery(file => file.Uri.EndsWith("pr59_DLCmogle_app.vlink", StringComparison.OrdinalIgnoreCase) && !file.Flags.HasFlag(ArchiveFileFlag.Reference),
        //     (earc, file) => System.Console.WriteLine("Found!"),
        //     false);
        // return;

        // RunWithTimer("dependency tree build", new Ps4DependencyTreeBuilder().Run);
        //RunWithTimer("subdependency tree build", new Ps4SubdependencyTreeBuilder().Run);
        //RunWithTimer("model dependency tree build", new Ps4ModelDependencyTreeBuilder().Run);
        //RunWithTimer("material dependency tree build", new Ps4MaterialDependencyTreeBuilder().Run);
        // return;

        // var context = Ps4Utilities.NewContext();
        // var recursiveDependencies = context.FestivalDependencyFestivalDependency
        //     .Where(l => l.ChildId == l.ParentId)
        //     .ToList();
        //
        // foreach (var link in recursiveDependencies)
        // {
        //     context.Remove(link);
        // }
        //
        // context.SaveChanges();
        // return;

        // FindExtensionsRecursively(Ps4PorterConfiguration.OutputDirectory);
        // foreach (var (extension, _) in _extensions)
        // {
        //     System.Console.WriteLine(extension);
        // }
        //
        // return;

        //new Ps4PostRunAudioPorter().Run();
        //return;

        //RunWithTimer("vertex layout mapper", new Ps4MaterialGenerator().BuildVertexLayoutMapForFinalAssets);
        //RunWithTimer("material mapper", new Ps4MaterialGenerator().BuildMaterialMap);
        //return;

        // using var context = Ps4Utilities.NewContext();
        // var usAnis = context.FestivalSubdependencies
        //     .Where(s => s.Uri.EndsWith(".ani") && s.Uri.Contains("/jp/"))
        //     .Select(s => new FestivalSubdependency
        //     {
        //         Uri = s.Uri.Replace("/jp/", "/us/"),
        //         Parents = s.Parents.Select(p => new FestivalDependencyFestivalSubdependency
        //         {
        //             DependencyId = p.DependencyId
        //         }).ToList()
        //     })
        //     .ToList();
        //
        // context.FestivalSubdependencies.AddRange(usAnis);
        // context.SaveChanges();
        // return;

        //RunWithTimer("ebex earc cleanup", ResetEbexEarcs);
        RunWithTimer("asset earc cleanup", ResetAssetEarcs);

        //RunWithTimer("earc porter", new Ps4EarcPorter().Run);
        RunWithTimer("asset porter", new Ps4AssetPorter().Run);

        RunWithTimer("weird earc fixer", FixWeirdEarcs);
        //RunWithTimer("audio inserter", new Ps4PostRunAudioPorter().AddConvertedSoundsToMainEarc);
    }

    private static void OutputFileByUri(string uri)
    {
        using var context = Ps4Utilities.NewContext();
        var data = Ps4Utilities.GetFileByUri(context, uri);
        
        if (uri.EndsWith(".ebex") || uri.EndsWith(".prefab"))
        {
            var output = new StringBuilder();
            Xmb2Document.Dump(data, output);
            data = Encoding.UTF8.GetBytes(output.ToString());
        }
        else if (uri.EndsWith(".amdl"))
        {
            data = AnimationModel.ToPC(data);
        }

        File.WriteAllBytes($@"C:\Modding\Chocomog\Testing\XML\{uri.Split('/').Last()}", data);
    }

    private void FindExtensionsRecursively(string directory)
    {
        foreach (var file in Directory.EnumerateFiles(directory))
        {
            var extension = file[file.LastIndexOf('.')..];
            _extensions.TryAdd(extension, true);
        }

        foreach (var subdirectory in Directory.EnumerateDirectories(directory))
        {
            FindExtensionsRecursively(subdirectory);
        }
    }

    private void FixWeirdEarcs()
    {
        var context = Ps4Utilities.NewContext();
        var data = Ps4Utilities.GetFileByUri(context, "data://celltool_dlc_mog.mid");
        var path = $@"{_pcSettings.GameDataDirectory}\autoexternal.earc";
        var unpacker = new Unpacker(path);

        if (!unpacker.HasFile("data://celltool_dlc_mog.mid"))
        {
            var packer = unpacker.ToPacker();
            packer.AddCompressedFile("data://celltool_dlc_mog.mid", data);
            packer.WriteToFile(path);

            var cells = context.Ps4AssetUris.Where(a =>
                    a.Uri.StartsWith("data://navimesh/level/dlc_ex/mog/worldshare/worldshare_mog/"))
                .Select(a => a.Uri)
                .ToList();

            data = Ps4Utilities.GetFileByUri(context,
                "data://navimesh/level/dlc_ex/mog/worldshare/worldshare_mog/data.nav_world");
            path = $@"{_pcSettings.GameDataDirectory}\level\dlc_ex\mog\worldshare\worldshare_mog.earc";
            unpacker = new Unpacker(path);
            packer = unpacker.ToPacker();
            packer.AddCompressedFile("data://navimesh/level/dlc_ex/mog/worldshare/worldshare_mog/data.nav_world", data);

            foreach (var cell in cells)
            {
                var cellData = Ps4Utilities.GetFileByUri(context, cell);
                packer.AddCompressedFile(cell, cellData);
            }

            packer.WriteToFile(path);
        }
    }

    private void ResetEbexEarcs()
    {
        var ebexEarcsJson = File.ReadAllText($@"{Ps4PorterConfiguration.StagingDirectory}\modified_ebex_earcs.json");
        var earcs = JsonConvert.DeserializeObject<List<string>>(ebexEarcsJson)!;
        earcs.Add($@"{_pcSettings.GameDataDirectory}\autoexternal.earc");
        earcs.Add($@"{_pcSettings.GameDataDirectory}\level\dlc_ex\mog\worldshare\worldshare_mog.earc");

        foreach (var earc in earcs)
        {
            if (File.Exists(earc))
            {
                File.Delete(earc);
            }

            var backupPath = earc!.Replace("\\FINAL FANTASY XV\\", "\\");
            if (File.Exists(backupPath))
            {
                File.Copy(backupPath, earc);
            }
        }
    }

    private void ResetAssetEarcs()
    {
        var assetEarcsJson = File.ReadAllText($@"{Ps4PorterConfiguration.StagingDirectory}\modified_asset_earcs.json");
        var earcs = JsonConvert.DeserializeObject<List<string>>(assetEarcsJson)!;
        earcs.Add($@"{_pcSettings.GameDataDirectory}\autoexternal.earc");

        foreach (var earc in earcs)
        {
            if (File.Exists(earc))
            {
                File.Delete(earc);
            }

            var backupPath = earc!.Replace("\\FINAL FANTASY XV\\", "\\");
            if (File.Exists(backupPath))
            {
                File.Copy(backupPath, earc);
            }
        }
    }

    private void RunWithTimer(string processName, Action action)
    {
        var start = DateTime.Now;
        System.Console.WriteLine($"Starting {processName}...");
        action();
        System.Console.WriteLine(
            $"Finished {processName} after {(DateTime.Now - start).TotalMilliseconds} milliseconds.");
    }

    private void OutputDuplicateRecords()
    {
        using var context = Ps4Utilities.NewContext();
        foreach (var item in context.FestivalDependencies)
        {
            if (context.FestivalDependencies.Count(i => i.Uri == item.Uri) > 1)
            {
                System.Console.WriteLine(item.Uri);
            }
        }
    }

    private void Copy4KPackFromReleaseBuild()
    {
        var releaseDirectory = Path.GetDirectoryName(Ps4PorterConfiguration.ReleaseGamePath) + @"\datas";
        foreach (var file in Directory.GetFiles(releaseDirectory, "*.earc", SearchOption.AllDirectories))
        {
            if (file.EndsWith("_$h2.earc"))
            {
                var destination = file.Replace(@"\FINAL FANTASY XV (Release)\", @"\FINAL FANTASY XV\");
                if (!File.Exists(destination))
                {
                    IOHelper.EnsureDirectoriesExistForFilePath(destination);
                    File.Copy(file, destination);
                }
            }
        }
    }
}