using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Black.Entity;
using Black.Sequence.Actor.SceneControl;
using Flagrum.Console.Utilities;
using Flagrum.Core.Animation;
using Flagrum.Core.Archive;
using Flagrum.Core.Ebex.Xmb2;
using Flagrum.Core.Gfxbin.Data;
using Flagrum.Core.Gfxbin.Gmdl;
using Flagrum.Core.Gfxbin.Gmdl.Components;
using Flagrum.Core.Gfxbin.Gmtl;
using Flagrum.Core.Ps4;
using Flagrum.Core.Utilities;
using Flagrum.Core.Vfx;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;
using Flagrum.Web.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SQEX.Ebony.Framework.Entity;
using BinaryReader = Flagrum.Core.Gfxbin.Serialization.BinaryReader;

namespace Flagrum.Console.Ps4.Festivals;

public static class ChocomogPorter
{
    private const string Ps4DatabasePath = @"C:\Users\Kieran\AppData\Local\Flagrum-PS4\flagrum.db";
    private const string GameDirectory = @"C:\Modding\Chocomog\Final Fantasy XV - RAW PS4\dummy.exe";
    private const string OutputDirectory = @"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV\datas";
    private const string StagingDirectory = @"C:\Modding\Chocomog\Testing";

    private static readonly SettingsService _settings = new();
    private static SettingsService _pcSettings;
    
    private static readonly ConcurrentDictionary<string, bool> _checked = new();
    
    
    
    
    
    private static byte[] _materialData;

    

    public static async Task Run()
    {
        _settings.GamePath = GameDirectory;
        _pcSettings = new SettingsService();

        // using var context = new FlagrumDbContext(_pcSettings);
        // var vfx = context.GetFileByUri("data://effects/character/me/me10/100/vfx/me10_atk_rain.vfx");
        // foreach (var uri in Vfx.GetDependencies(vfx))
        // {
        //     System.Console.WriteLine(uri);
        // }
        //
        // return;

        // var names = new ConcurrentDictionary<string, bool>();
        // new FileFinder().FindByQuery(file => file.Uri.EndsWith(".gmtl"),
        //     (earc, file) =>
        //     {
        //         var material = new MaterialReader(file.GetReadableData()).Read();
        //         if (material.Interfaces[0].Name.StartsWith("ENV")
        //             && material.Textures.Any(t =>
        //                 t.ShaderGenName.Contains("emissive", StringComparison.OrdinalIgnoreCase))
        //             
        //             && material.InterfaceInputs.Any(i => i.ShaderGenName == "AlphaThreshold"))
        //         {
        //             System.Console.WriteLine(material.Interfaces[0].Name + " - " + file.Uri);
        //             names.TryAdd(material.Interfaces[0].Name, true);
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

        // new FileFinder().FindByQuery(file => file.Uri.Contains("mog") && file.Uri.EndsWith(".ebex"),
        //     (_, file) =>
        //     {
        //         var data = file.GetReadableData();
        //         if (data[0] == 'X' && data[1] == 'M' && data[2] == 'B' && data[3] == '2')
        //         {
        //             var builder = new StringBuilder();
        //             Xmb2Document.Dump(data, builder);
        //             var text = builder.ToString();
        //             if (text.Contains("isSpecifyParty_", StringComparison.OrdinalIgnoreCase))
        //             {
        //                 System.Console.WriteLine(file.Uri);
        //             }
        //         }
        //     },
        //     true);
        // return;

        //new FileFinder().UnpackEverything();
        //return;

        // using var ps4Context = Ps4Utilities.NewContext();
        // foreach (var ebex in ps4Context.FestivalDependencies)
        // {
        //     var data = Ps4Utilities.GetFileByUri(ps4Context, ebex.Uri);
        //     if (data[0] == 'X' && data[1] == 'M' && data[2] == 'B' && data[3] == '2')
        //     {
        //         var builder = new StringBuilder();
        //         Xmb2Document.Dump(data, builder);
        //         var text = builder.ToString();
        //         if (text.Contains("isSpecifyParty_", StringComparison.OrdinalIgnoreCase))
        //         {
        //             System.Console.WriteLine(ebex.Uri);
        //         }
        //     }
        // }
        //
        // return;
        
        //FileFinder.FindStringInExml("level_v_nox00_mcc_town_skb_motion040_010.lsd");
        //return;
        
        // var choice = "ds36";
        //OutputFileByUriPC("data://shader/shadergen/fwd_lighting_ps/ec/fwd_lighting_ps_ec6d49b7.ps.sb");
        // OutputFileByUri($"data://character/ds/{choice}/model_000/{choice}_000.gmdl");
        //OutputFileByUri("data://level/dlc_ex/mog/worldshare/worldshare_mog.ebex");
        //return;

        //OutputXmlForMatchingDependency("al_pr_candy01_type.gmdl");
        //return;

        //var counter = 0;
        //new Utilities.FileFinder().FindByQuery(file => file.Uri.Equals("data://character/pr/pr81/pr81.amdl", StringComparison.OrdinalIgnoreCase),
        //    (earc, file) =>
        //    {
        //        var i = counter++;
        //        File.WriteAllBytes(@$"C:\Modding\Chocomog\Testing\XML\pr81_000[{i}].amdl", file.GetReadableData());
        //        System.Console.WriteLine($"{i}: {earc}");
        //    },
        //    true);
        //return;

        //await using var ps4Context = Ps4Utilities.NewContext();
        //foreach (var amdl in ps4Context.FestivalSubdependencies.Where(d => d.Uri.EndsWith(".amdl")))
        //{
        //    if (!pcContext.AssetUris.Any(a => EF.Functions.Like(a.Uri, amdl.Uri)))
        //    {
        //        File.WriteAllBytes(@$"C:\Modding\Chocomog\Testing\XML\{amdl.Uri.Split('/').Last()}", Ps4Utilities.GetFileByUri(ps4Context, amdl.Uri));
        //    }
        //}
        //return;
        
        //FixCarbuncle();
        //AddBoys();
        //return;
        
        //PortAudioDependencies();
        //return;

        // Step1BuildUriMapFromPs4Files();
        // Step2BuildDependencyList();
        // Step3BuildSubdependencyList();
        // Step4GetModelDependencies();
        // Step5GetMaterialDependencies();
        // ResetEbexEarcs();
        // Step6CreateEarcs();
        // ResetAssetEarcs();
        // Step7PortAssets();
        // Step9FixWeirdEarcs();
        
        //FixCarbuncle();
    }

    
    
    private static void OutputFileByUriPC(string uri)
    {
        using var context = new FlagrumDbContext(_pcSettings);
        var data = context.GetFileByUri(uri);

        // var relativePath = uri.Replace("data://", "").Replace('/', '\\');
        // var earcRelativePath = relativePath[..relativePath.LastIndexOf('\\')] + "\\autoexternal.earc";
        // var earcLocation = $@"{_pcSettings.GameDataDirectory}\{earcRelativePath}";
        //
        // using var unpacker = new Unpacker(earcLocation);
        // var data = unpacker.UnpackReadableByUri(uri);
        
        if (uri.EndsWith(".ebex"))
        {
            var output = new StringBuilder();
            Xmb2Document.Dump(data, output);
            data = Encoding.UTF8.GetBytes(output.ToString());
        }

        File.WriteAllBytes($@"C:\Modding\Chocomog\Testing\XML\PC_{uri.Split('/').Last()}", data);
    }

    private static void OutputXmlForMatchingDependency(string query)
    {
        foreach (var file in Directory.EnumerateFiles(@"C:\Modding\Chocomog\Testing\XML"))
        {
            File.Delete(file);
        }

        using var ps4Context = new FlagrumDbContext(_settings, Ps4DatabasePath);
        foreach (var ebex in ps4Context.FestivalDependencies)
        {
            var exml = Ps4Utilities.GetFileByUri(ps4Context, ebex.Uri);
            var sb = new StringBuilder();
            Xmb2Document.Dump(exml, sb);
            var xml = sb.ToString();
            if (xml.Contains(query))
            {
                File.WriteAllText($@"C:\Modding\Chocomog\Testing\XML\{ebex.Uri.Split('/').Last()}.xml", xml);
            }
        }
    }

    

    

    private static void FixCarbuncle()
    {
        var path = $@"{_pcSettings.GameDataDirectory}\character\sm\sm05\entry\sm05_000_carbuncle_mogchoco.earc";
        var unpacker = new Unpacker(path);
        var packer = unpacker.ToPacker();
        packer.AddReference("data://character/sm/sm05/entry/sm05_000_carbuncle_platinum.ebex@", true);
        packer.WriteToFile(path);
    }

    private static void AddBoys()
    {
        var path =
            @"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV\datas\level\dlc_ex\mog\area_ravettrice_mog.earc";
        var unpacker = new Unpacker(path);
        var packer = unpacker.ToPacker();
        
        var entries = new[]
        {
            "data://character/nh/nh01/entry/nh01_base.ebex@",
            "data://character/nh/nh02/entry/nh02_base.ebex@",
            "data://character/nh/nh03/entry/nh03_base.ebex@"
        };

        foreach (var entry in entries)
        {
            if (!packer.HasFile(entry))
            {
                packer.AddReference(entry, true);
            }
        }
        
        packer.WriteToFile(path);
    }

    

    

    

    

    

    

    
}