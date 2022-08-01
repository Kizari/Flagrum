using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Flagrum.Console.Ps4;
using Flagrum.Console.Ps4.Porting;
using Flagrum.Console.Utilities;
using Flagrum.Core.Animation;
using Flagrum.Core.Animation.AnimationClip;
using Flagrum.Core.Archive;
using Flagrum.Core.Ebex.Xmb2;
using Flagrum.Core.Gfxbin.Gmdl;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;
using Flagrum.Web.Services;
using Newtonsoft.Json;

// new FileFinder().FindByQuery(file => file.Uri == "data://data/ai/interactions/town/alt_mog/system/autoexternal.ebex@",
//     (earc, file) => System.Console.WriteLine(earc + " - " + file.Uri),
//     false);
// return;

//FileFinder.FindStringInAllFiles("alt_mog_minigame1_receptionist.aiia");
FileFinder.FindBytesInAllFiles(BitConverter.GetBytes((uint)17111186));
return;

// using var context = Ps4Utilities.NewContext();
// var json = File.ReadAllText(@$"{Ps4PorterConfiguration.StagingDirectory}\assets.json");
// var assets = JsonConvert.DeserializeObject<List<string>>(json)!;
//
// foreach (var asset in assets.Where(a => a.EndsWith(".ani")))
// {
//     var animation = Ps4Utilities.GetFileByUri(context, asset);
//     if (animation.Length > 0)
//     {
//         File.WriteAllBytes($@"C:\Modding\Chocomog\Testing\AnimationDump\{asset.Split('/').Last()}", animation);
//     }
// }
//
// return;

//FileFinder.FindStringInExml("luchil");
//return;

// var results = new ConcurrentDictionary<string, List<string>>();
// new FileFinder().FindByQuery(file => file.Uri.EndsWith(".ebex") || file.Uri.EndsWith(".prefab"),
//     (earc, file) =>
//     {
//         var data = file.GetReadableData();
//         var output = new StringBuilder();
//         Xmb2Document.Dump(data, output);
//
//         var xml = output.ToString();
//         var matches = Regex.Matches(xml, "^.+?um20_002.+?$", RegexOptions.Multiline);
//
//         if (matches.Any())
//         {
//             var list = new List<string>();
//             results.TryAdd(file.Uri, list);
//             foreach (Match match in matches)
//             {
//                 list.Add(match.Value);
//             }
//         }
//     },
//     true);
//
// foreach (var (uri, list) in results)
// {
//     Console.WriteLine("\n" + uri);
//     foreach (var match in list)
//     {
//         Console.WriteLine(match);
//     }
// }
// return;

// var results = new Dictionary<string, bool>();
// new FileFinder().FindByQuery(file => file.Uri.Contains("/nh00/") && file.Uri.EndsWith(".gmdl"),
//     (earc, file) =>
//     {
//         Console.WriteLine($"Reading {file.Uri}");
//         try
//         {
//             using var context = new FlagrumDbContext(new SettingsService());
//             var gmdl = file.GetReadableData();
//             var gpubin = context.GetFileByUri(file.Uri.Replace(".gmdl", ".gpubin"));
//             var model = new ModelReader(gmdl, gpubin).Read();
//             foreach (var part in model.Parts)
//             {
//                 results.TryAdd(part.Name, true);
//             }
//         }
//         catch
//         {
//             Console.WriteLine("Model read failed");
//         }
//         
//     }, 
//     true);
//
// Console.WriteLine("");
// foreach (var (name, _) in results)
// {
//     Console.WriteLine(name);
// }
// return;

// new FileFinder().FindByQuery(file => file.Uri.Contains("altc_mog_kenny", StringComparison.OrdinalIgnoreCase),
//     (earc, file) =>
//     {
//         Console.WriteLine($"{file.Uri} - {earc}");
//     }, 
//     false);
// return;

// using var context = Ps4Utilities.NewContext();
// var files = Ps4Utilities.GetFilesByUri(context, "data://character/uw/common/anim/graph/uw_011_facial_mog.anmgph");
// foreach (var (earc, data) in files)
// {
//     var path = $@"C:\Modding\Chocomog\Testing\AnimationDump\{earc.Replace('\\', '-')}";
//     File.WriteAllBytes(path, data);
// }

//return;

new Ps4Porter().Run();
return;

// var clip = AnimationClip.FromData(
//     //File.ReadAllBytes(@"C:\Users\Kieran\Desktop\Models2\um02\um02.amdl"),
//     //File.ReadAllBytes(@"C:\Modding\EarcMods\TalkBasicFacialTest\talk_basic_facial.ani"));
//     File.ReadAllBytes(@"C:\Users\Kieran\Desktop\Models2\nh00\nh00.amdl"),
//     //File.ReadAllBytes(@"C:\Modding\AnimationProject\nh00_basemove\0.ani"));
//     File.ReadAllBytes(@"C:\Modding\AnimationProject\r_cc_c_lp_0000a.ani"));
//     //File.ReadAllBytes(@"C:\Users\Kieran\Desktop\r_cc_a2_lp_0000a_facial.ani"));
// return;

//FileFinder.FindStringInExml("armiger");
//return;

//FileFinder.FindBytesInAllFiles(BitConverter.GetBytes((uint)16817550));
//return;

//
// var directory = @"C:\Users\Kieran\Downloads\mdl";
// foreach (var gmdl in Directory.GetFiles(directory, "*.gmdl.gfxbin", SearchOption.AllDirectories))
// {
//     PlatinumDemoGmdl.Convert(gmdl);
// }
//
// return;

//
// foreach (var file in Directory.GetFiles(directory, "*.btex", SearchOption.AllDirectories))
// {
//     var btex = Btex.FromData(File.ReadAllBytes(file));
//     btex.Bitmap.Save(file.Replace(".btex", ".png"), ImageFormat.Png);
// }
//
// return;

// using var context = new FlagrumDbContext(new SettingsService());
// var gmdl = context.GetFileByUri("data://environment/leide/props/le_ar_gqshop1/models/le_ar_gqshop1_jettya.gmdl");
// var gpubin = context.GetFileByUri("data://environment/leide/props/le_ar_gqshop1/models/le_ar_gqshop1_jettya.gpubin");
// var gmdl = File.ReadAllBytes(@"C:\Users\Kieran\Downloads\TEX_EDIT\WE01_000\restored\we01_000.gmdl.gfxbin");
// var gpubin = File.ReadAllBytes(@"C:\Users\Kieran\Downloads\TEX_EDIT\WE01_000\restored\we01_000.gpubin");
// var model = new ModelReader(gmdl, gpubin).Read();
// var (gmdlOut, gpubinOut) = new ModelWriter(model).Write();
// File.WriteAllBytes(@"C:\Users\Kieran\Downloads\TEX_EDIT\WE01_000\restored\we01_000.gmdl.gfxbin", gmdlOut);
// File.WriteAllBytes(@"C:\Users\Kieran\Downloads\TEX_EDIT\WE01_000\restored\we01_000.gpubin", gpubinOut);
// return;

// using var context = Ps4Utilities.NewContext();
// var gmtl = Ps4Utilities.GetFileByUri(context, "data://character/uw/uw20/model_160/materials/uw20_160_uhcloth_01_mat.gmtl");
// var material = new MaterialReader(gmtl).Read();
// return;

// using var innerContext = Ps4Utilities.NewContext();
// var uri = "data://menu/common/window/vfx/epp_blur_bg.vfx";
// var data = Ps4Utilities.GetFileByUri(innerContext, uri);
// var dataString = Encoding.UTF8.GetString(data);
//
// Console.WriteLine(dataString);
//
// //dataString = "data://effects/Resource/Texture/NonLinear/VC/White_vc.tif  ";
// var matches = Regex.Matches(dataString, @"data:\/\/.+?\..+?" + (char)0x00);
// foreach (Match match in matches)
// {
//     Console.WriteLine(match.Value[..^1]);
// }
//
// return;

// var paths = new Dictionary<string, string>
// {
//     {"character/we/we04/entry/we04_000.exml", "data://character/we/we04/entry/we04_000.ebex"},
//     {"character/we/we04/entry/we04_100.exml", "data://character/we/we04/entry/we04_100.ebex"},
//     {"character/we/we04/entry/we04_200.exml", "data://character/we/we04/entry/we04_200.ebex"}
// };
//
// foreach (var (path, uri) in paths)
// {
//     var tokens = path.Split('\\', '/');
//     var fileName = tokens.Last();
//     var index = fileName.IndexOf('.');
//     var type = index < 0 ? "" : fileName[(index + 1)..];
//
//     var UriHash = Cryptography.Hash64(uri);
//     var TypeHash = Cryptography.Hash64(type);
//
//     var UriAndTypeHash = (ulong)(((long)UriHash & 17592186044415L) | (((long)TypeHash << 44) & -17592186044416L));
//     Console.WriteLine($"{path}\t{uri}\t{UriAndTypeHash:x16}");
// }
//
// return;

// var file = @"C:\Users\Kieran\Downloads\message(3).txt";
// var lines = File.ReadAllLines(file);
// var builder = new StringBuilder();
// var counter = 0;
// foreach (var line in lines)
// {
//     builder.Append($"\"{line}\"");
//     
//     if (line != lines.Last())
//     {
//         builder.Append(", ");
//     }
//
//     if (counter == 7)
//     {
//         builder.Append("\n");
//         counter = 0;
//     }
//
//     counter++;
// }
// Console.WriteLine(builder.ToString());
// return;

var boneMap = new Dictionary<string, string>
{
    {"L_gloveAll", "L_Index1"},
    {"L_gloveB", "L_SleeveA1"},
    {"L_gloveC", "L_Index1"},
    {"L_gloveD", "L_Index1"},
    {"L_gloveE", "L_Index1"},
    {"L_gloveA", "L_Index1"},
    {"L_VRSSleeveRoot", "R_FemorisBKdi"},
    {"L_VRSSleeveH1", "L_PantscordA1"},
    {"L_VRSSleeveE1", "L_Index1"},
    {"L_VRSSleeveD1", "L_Index1"},
    {"L_VRSSleeveC1", "L_Index1"},
    {"L_VRSSleeveB1", "L_Index1"},
    {"L_VRSSleeveA1", "L_Index1"},
    {"L_VRSSleeveF1", "R_CollarRoot"},
    {"L_VRSSleeveG1", "R_SleeveAll"},
    {"R_gloveAll", "L_Middle1"},
    {"R_gloveB", "L_CollarRoot"},
    {"R_gloveC", "L_Middle1"},
    {"R_gloveD", "L_Middle1"},
    {"R_gloveE", "L_Middle1"},
    {"R_gloveA", "L_Middle1"},
    {"R_VRSSleeveRoot", "L_KneeKdi"},
    {"R_VRSSleeveH1", "R_SleeveH1"},
    {"R_VRSSleeveE1", "L_Middle1"},
    {"R_VRSSleeveD1", "L_Middle1"},
    {"R_VRSSleeveC1", "L_Middle1"},
    {"R_VRSSleeveB1", "L_Middle1"},
    {"R_VRSSleeveA1", "L_Middle1"},
    {"R_VRSSleeveF1", "R_CollarRoot"},
    {"R_VRSSleeveG1", "R_SleeveAll"},
    {"C_VRSbelt1", "L_ZipperD"},
    {"C_VRSbelt2", "R_JacketF4"},
    {"C_VRSbelt3", "L_Middle1"},
    {"C_VRSbelt4", "L_Pinky2"},
    {"C_VRSbelt5", "R_JacketA6"},
    {"C_VRSbelt6", "R_Thumb2"},
    {"C_VRSbelt7", "C_JacketR3"},
    {"L_VRSPouchAKdi", "R_ZipperB"},
    {"L_VRSPouchBKdi", "L_ZipperDKdi"},
    {"L_VRSPouchCKdi", "R_CollarRoot"},
    {"b_VRSbelt1", "L_Middle1"},
    {"b_VRSbelt2", "L_JacketE5"},
    {"b_VRSbelt3", "L_Middle1"},
    {"b_VRSbelt4", "L_JacketC2"},
    {"L_VRSBootsRoll", "L_BootsRoll"},
    {"L_VRSShoelaceB", "R_ShoelaceA"},
    {"L_VRSShoelaceA", "R_JacketE1"},
    {"L_PantsSubA", "L_ZipperB"},
    {"L_PantsSubB", "L_ZipperB"},
    {"L_PantsSubC", "L_ZipperB"},
    {"L_PantsSubD", "L_ZipperB"},
    {"L_PantsSubE", "L_ZipperB"},
    {"L_PantsSubF", "L_ZipperBKdi"},
    {"L_PantsSubG", "L_ZipperB"},
    {"L_PantsSubI", "R_SleeveAll"},
    {"R_VRSBootsRoll", "R_BootsRoll"},
    {"R_VRSShoelaceA", "R_ShoelaceA"},
    {"R_VRSShoelaceB", "L_JacketE1"},
    {"R_PantsSubG", "L_ZipperB"},
    {"R_PantsSubF", "L_ZipperBKdi"},
    {"R_PantsSubD", "L_ZipperBKdi"},
    {"R_PantsSubC", "L_ZipperB"},
    {"R_PantsSubB", "L_ZipperB"},
    {"R_PantsSubA", "L_ZipperBKdi"},
    {"R_PantsSubI", "L_ZipperD"},
    {"C_VRSJacketA1", "L_BootsB"},
    {"L_VRSJacketA", "L_JacketC4"},
    {"L_VRSJacketA1", "L_Middle1"},
    {"L_VRSJacketA2", "C_JacketL3"},
    {"L_VRSJacketA3", "L_JacketA6"},
    {"L_VRSJacketA4", "L_Middle2"},
    {"R_VRSJacketA", "L_PantscordB3"},
    {"R_VRSJacketA1", "L_Middle1"},
    {"R_VRSJacketA2", "L_JacketF8"},
    {"R_VRSJacketA3", "L_PantscordB2"},
    {"R_VRSJacketA4", "C_Spine3"},
    {"C_VRSZipper", "R_ZipperBKdi"},
    {"L_VRSJacketD", "L_Index1"},
    {"L_VRSJacketD1", "R_JacketC3"},
    {"L_VRSJacketD2", "L_Index1"},
    {"L_VRSJacketD3", "R_JacketB2"},
    {"L_VRSJacketD4", "C_Neck1"},
    {"L_VRSJacketD5", "L_PantscordB3"},
    {"L_VRSJacketE", "L_BootsB"},
    {"L_VRSJacketE1", "R_Ring2"},
    {"L_VRSJacketE2", "R_JacketC3"},
    {"L_VRSJacketE3", "L_Index1"},
    {"L_VRSJacketE4", "R_JacketD3"},
    {"L_VRSJacketE5", "R_JacketC3"},
    {"L_VRSJacketF", "L_BootsB"},
    {"L_VRSJacketF1", "L_Toe"},
    {"L_VRSJacketF2", "L_Index1"},
    {"L_VRSJacketF3", "R_PantscordA4"},
    {"L_VRSJacketF4", "L_Middle2"},
    {"L_VRSJacketF5", "R_JacketF9"},
    {"L_VRSJacketG", "R_ShoelaceA"},
    {"L_VRSJacketG1", "R_JacketE2"},
    {"L_VRSJacketG2", "L_Index1"},
    {"L_VRSJacketG3", "L_PantscordB3"},
    {"L_VRSJacketG4", "L_JacketA6"},
    {"L_VRSJacketG5", "L_PantscordB2"},
    {"C_VRSJacketE1", "R_ShoelaceA"},
    {"C_VRSJacketE2", "R_JacketF9"},
    {"C_VRSJacketE3", "L_Middle1"},
    {"C_VRSJacketE4", "R_JacketE8"},
    {"C_VRSJacketE5", "L_UpperArm"},
    {"C_VRSJacketE6", "C_Jacket3"},
    {"R_VRSJacketD", "R_SleeveH1"},
    {"R_VRSJacketD1", "R_JacketD5"},
    {"R_VRSJacketD2", "L_Middle1"},
    {"R_VRSJacketD3", "R_JacketC7"},
    {"R_VRSJacketD4", "R_JacketC6"},
    {"R_VRSJacketD5", "R_JacketC6"},
    {"R_VRSJacketG", "R_ShoelaceA"},
    {"R_VRSJacketG1", "R_JacketE7"},
    {"R_VRSJacketG2", "L_Middle1"},
    {"R_VRSJacketG3", "L_JacketA7"},
    {"R_VRSJacketG4", "R_JacketF9"},
    {"R_VRSJacketG5", "R_JacketF9"},
    {"R_VRSJacketF", "R_SleeveH1"},
    {"R_VRSJacketF1", "R_Middle2"},
    {"R_VRSJacketF2", "L_Middle1"},
    {"R_VRSJacketF3", "L_PantscordB3"},
    {"R_VRSJacketF4", "L_JacketD4"},
    {"R_VRSJacketF5", "L_JacketF3"},
    {"R_VRSJacketE", "R_SleeveH1"},
    {"R_VRSJacketE1", "R_JacketD6"},
    {"R_VRSJacketE2", "L_Middle1"},
    {"R_VRSJacketE3", "R_JacketB2"},
    {"R_VRSJacketE4", "R_JacketF9"},
    {"R_VRSJacketE5", "L_JacketC3"},
    {"R_VRSJacketC1", "L_ZipperBKdi"},
    {"R_VRSJacketC2", "L_JacketB3"},
    {"R_VRSJacketC3", "L_Middle1"},
    {"R_VRSJacketC4", "L_JacketC7"},
    {"R_VRSJacketC5", "R_JacketE8"},
    {"R_VRSJacketC6", "R_Ring1"},
    {"L_VRSJacketC1", "L_ZipperDKdi"},
    {"L_VRSJacketC2", "L_JacketB5"},
    {"L_VRSJacketC3", "R_Ring1"},
    {"L_VRSJacketC4", "L_Index1"},
    {"L_VRSJacketC5", "R_Ring3"},
    {"L_VRSJacketC6", "L_JacketC6"},
    {"L_VRSJacketB1", "L_ZipperBKdi"},
    {"L_VRSJacketB2", "L_JacketA2"},
    {"L_VRSJacketB3", "L_JacketA6"},
    {"L_VRSJacketB4", "L_Index1"},
    {"L_VRSJacketB5", "L_JacketF9"},
    {"L_VRSJacketB6", "L_JacketF6"},
    {"R_VRSJacketB1", "L_ZipperB"},
    {"R_VRSJacketB2", "L_JacketC4"},
    {"R_VRSJacketB3", "L_Middle1"},
    {"R_VRSJacketB4", "C_Spine3"},
    {"R_VRSJacketB5", "L_JacketF9"},
    {"R_VRSJacketB6", "R_JacketF9"}
};

// var gmdl = @"C:\Mega\Modding\CutContent\nh00_retailps4\model_050\nh00_050.gmdl.gfxbin";
// var gpubin = @"C:\Mega\Modding\CutContent\nh00_retailps4\model_050\nh00_050.gpubin";
// // var gmdl2 = @"C:\Users\Kieran\Desktop\Models2\nh00\model_010\nh00_010.gmdl.gfxbin";
// // var gpubin2 = @"C:\Users\Kieran\Desktop\Models2\nh00\model_010\nh00_010.gpubin";
var amdl = File.ReadAllBytes(@"C:\Users\Kieran\Desktop\Models2\nh00\nh00.amdl");
var animationModel = AnimationModel.FromData(amdl, false);
// //File.WriteAllBytes(@"C:\Mega\Modding\CutContent\nh00_retailps4\nh00_retailps4.amdl", AnimationModel.ToPC(amdl));
// //return;
// //gmdl = @"C:\Users\Kieran\Downloads\model_000(1)\nh00_000.gmdl.gfxbin";
// //gpubin = @"C:\Users\Kieran\Downloads\model_000(1)\nh00_000.gpubin";
// var model = new ModelReader(File.ReadAllBytes(gmdl), File.ReadAllBytes(gpubin)).Read();
// foreach (var bone in model.BoneHeaders)
// {
//     boneMap.TryGetValue(bone.Name, out var replacement);
//     if (replacement != null)
//     {
//         bone.Name = replacement;
//     }
// }
// // var bone131 = model.BoneHeaders.FirstOrDefault(b => b.UniqueIndex == 131);
// // var model2 = new ModelReader(File.ReadAllBytes(gmdl2), File.ReadAllBytes(gpubin2)).Read();
// foreach (var bone in model.BoneHeaders)
// {
//     var match = animationModel.BoneNames.FirstOrDefault(b => b.Equals(bone.Name, StringComparison.OrdinalIgnoreCase));
//     if (match == null)
//     {
//         System.Console.WriteLine(bone.Name);
//     }
// }
//
// return;

// foreach (var dependency in model.Header.Dependencies)
// {
//     if (ulong.TryParse(dependency.PathHash, out var hash))
//     {
//         var originalHashIndex = model.Header.Hashes.IndexOf(hash);
//         var newHash = Cryptography.HashFileUri64(dependency.Path);
//         dependency.PathHash = newHash.ToString();
//         model.Header.Hashes[originalHashIndex] = newHash;
//         
//         if (dependency.Path.EndsWith(".gmtl"))
//         {
//             var mesh = model.MeshObjects.Select(mo => mo.Meshes.FirstOrDefault(m => m.DefaultMaterialHash == hash)).FirstOrDefault()!;
//             mesh.DefaultMaterialHash = newHash;
//         }
//     }
//
//     dependency.Path = dependency.Path.Replace("model_020", "model_010").Replace("nh00_020", "nh00_010");
// }
//
// var (gmdlOut, gpubinOut) = new ModelWriter(model).Write();
// File.WriteAllBytes(@"C:\Mega\Modding\CutContent\model_010\nh00_010.gmdl.gfxbin", gmdlOut);
// File.WriteAllBytes(@"C:\Mega\Modding\CutContent\model_010\nh00_010.gpubin", gpubinOut);

// foreach (var path in Directory.GetFiles(@"C:\Mega\Modding\CutContent\sourceimages", "*.btex"))
// {
//     var data = File.ReadAllBytes(path);
//     Btex.FromData(data).Bitmap.Save(path.Replace(".btex", ".png"), ImageFormat.Png);
// }
//
// return;

// using Flagrum.Console.Ps4.Festivals;
// using Flagrum.Core.Animation;
//
// var amdlData = File.ReadAllBytes(@"C:\Users\Kieran\Desktop\Models2\nh02\nh02.amdl");
// var amdl = AnimationModel.FromData(amdlData, false);
// File.WriteAllBytes(@"C:\Users\Kieran\Desktop\Models2\nh02\nh02_modified.amdl", AnimationModel.ToData(amdl));
// return;

//
// var info = (LmSkeletalAnimInfo)amdl.CustomUserData.CustomUserDatas[0];
// for (var i = 0; i < info.ChildInfoOffset.Length; i++)
// {
//     if (info.ChildInfoOffset[i] >= 40)
//     {
//         info.ChildInfoOffset[i] -= 40;
//     }
// }
//
// for (var i = 0; i < info.MaybeParentIndexOffsets.Length; i++)
// {
//     if (info.MaybeParentIndexOffsets[i] >= 40)
//     {
//         info.MaybeParentIndexOffsets[i] -= 40;
//     }
// }
//
// var data = AnimationModel.ToData(amdl);
// File.WriteAllBytes(@"C:\Modding\Chocomog\Testing\XML\nh02.amdl", data);
//File.WriteAllBytes(@"C:\Modding\Chocomog\Testing\XML\ds33_000_modified.amdl", AnimationModel.ToPC(amdlData));
//return;
// var x = true;

// var packer = new Ps4EnvironmentPacker(new ConsoleLogger("Logger", () => new ConsoleLoggerConfiguration()),
//     new SettingsService());
// packer.Pack("data://level/dlc_ex/mog/area_ravettrice_mog.ebex",
//     @"C:\Modding\Chocomog\Testing\Environment\area_ravettrice_mog.fed");

//var packer = new EnvironmentPacker(new ConsoleLogger<EnvironmentPacker>("Test", () => new ConsoleLoggerConfiguration()),
//    new SettingsService());
//packer.Pack("data://level/dlc_ex/mog/area_ravettrice_mog.ebex", @"C:\Modding\Chocomog\Testing\Environment\area_ravettrice_mog.fed");

// foreach (var file in Directory.EnumerateFiles(@"C:\Modding\Chocomog\Testing\XML"))
// {
//     var data = File.ReadAllBytes(file);
//     var result = AnimationModel.ToPC(data);
//     File.WriteAllBytes(file.Replace(".amdl", "_modified.amdl"), result);
// }
//
// return;

// FileFinder.FindUriByString("al_pr_mogcho_gameGate.gmdl");
// return;
//
// using var context = new FlagrumDbContext(new SettingsService());
// var location =
//     @"C:\Modding\Chocomog\Final Fantasy XV - RAW PS4\datas\CUSA01633-patch_115\CUSA01633-patch\patch\patch2_initial\common\first_packages_all.earc";
// using var unpacker = new Unpacker(location);
// var item = unpacker.UnpackFileByQuery("menuswfentity_title_special.ebex", out _);
// File.WriteAllBytes(@"C:\Modding\Chocomog\Testing\menuswfentity_title_special.ebex", item);
// return;
//
// var btex = Btex.FromData(item);
// btex.Bitmap.Save(@"C:\Modding\Chocomog\Testing\swf_mogchoco.png", ImageFormat.Png);
// return;
//
// var dds = btex.ToDds();
//
// var pinnedData = GCHandle.Alloc(dds, GCHandleType.Pinned);
// var pointer = pinnedData.AddrOfPinnedObject();
//
// var image = TexHelper.Instance.LoadFromDDSMemory(pointer, dds.Length, DDS_FLAGS.NONE);
//
// pinnedData.Free();
//
// using var stream = new MemoryStream();
// using var ddsStream = image.SaveToDDSMemory(DDS_FLAGS.NONE);
// ddsStream.CopyTo(stream);
// File.WriteAllBytes(
//     @"C:\Modding\Chocomog\Scout\character\nh\nh24\model_000\sourceimages\nh24_000_skin_00_n.dds",
//     stream.ToArray());
//
// var x = true;