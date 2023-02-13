// using System;
// using System.Collections.Concurrent;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.IO;
// using System.Linq;
// using System.Threading.Tasks;
// using Flagrum.Core.Archive;
// using Flagrum.Core.Gfxbin.Gmdl;
// using Flagrum.Core.Gfxbin.Gmdl.Components;
// using Flagrum.Core.Gfxbin.Gmtl;
// using Flagrum.Core.Utilities;
// using Newtonsoft.Json;
//
// namespace Flagrum.Console.Utilities;
//
// public class MaterialFinder
// {
//     public const string DataDirectory =
//         @"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV\datas\character";
//
//     private readonly ConcurrentDictionary<string, ulong> _hashes = new();
//     private readonly ConcurrentDictionary<string, VertexLayoutType> _limits = new();
//
//     private ConcurrentBag<(string, string)> _matches;
//
//     public void MakeTemplate()
//     {
//         const string templateName = "NAMED_HUMAN_OUTFIT";
//         const string sourceEarcPath = @$"{DataDirectory}\nh\nh02\model_030\materials\autoexternal.earc";
//         const string sourceMaterialPath = "data://character/nh/nh02/model_030/materials/nh02_030_cloth_00_mat.gmtl";
//
//         using var unpacker = new EbonyArchive(sourceEarcPath);
//         var sourceBytes = unpacker.UnpackFile(sourceMaterialPath, out _);
//         var sourceMaterial = new MaterialReader(sourceBytes).Read();
//
//         var outJson = JsonConvert.SerializeObject(sourceMaterial);
//         File.WriteAllText(@$"C:\Modding\MaterialTesting\{templateName}.json", outJson);
//
//         MaterialToPython.ConvertFromJsonFile(@$"C:\Modding\MaterialTesting\{templateName}.json",
//             @$"C:\Modding\MaterialTesting\{templateName}.py");
//     }
//
//     public void MakeGlassTemplate()
//     {
//         // Get Iggy glasses material from the game files
//         const string iggyPath = $"{DataDirectory}\\nh\\nh03\\model_000\\materials\\autoexternal.earc";
//         using var iggyUnpacker = new EbonyArchive(iggyPath);
//         var iggyBytes = iggyUnpacker.UnpackFile("nh03_000_basic_01_mat.gmtl", out _);
//         var glass = new MaterialReader(iggyBytes).Read();
//
//         const string path =
//             @"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV\datas\character\nh\nh03\model_304\materials\autoexternal.earc";
//
//         // Shiva ribbon
//         //const string path = $"{DataDirectory}\\sm\\sm03\\model_000\\materials\\autoexternal.earc";
//
//         using var unpacker = new EbonyArchive(path);
//         var materialBytes = unpacker.UnpackFile("nh03_304_basic_01_mat.gmtl", out _);
//         var material = new MaterialReader(materialBytes).Read();
//
//         // Overwrite material input values with glass input values
//         foreach (var input in glass.InterfaceInputs)
//         {
//             var match = material.InterfaceInputs.FirstOrDefault(i => i.Name == input.Name);
//             if (match == null)
//             {
//                 System.Console.WriteLine($"No match found for input {input.ShaderGenName}");
//             }
//             else
//             {
//                 match.Values = input.Values;
//             }
//         }
//
//         // Save new template to file
//         var outJson = JsonConvert.SerializeObject(material);
//         File.WriteAllText(@"C:\Modding\MaterialTesting\NAMED_HUMAN_GLASS.json", outJson);
//
//         MaterialToPython.ConvertFromJsonFile(@"C:\Modding\MaterialTesting\NAMED_HUMAN_GLASS.json",
//             @"C:\Modding\MaterialTesting\NAMED_HUMAN_GLASS.py");
//     }
//
//     public void Find()
//     {
//         System.Console.WriteLine("Starting search...");
//         var watch = Stopwatch.StartNew();
//
//         _matches = new ConcurrentBag<(string, string)>();
//         Parallel.ForEach(Directory.EnumerateDirectories(DataDirectory), FindRecursively);
//         //FindRecursively(dataDirectory);
//         File.WriteAllText(@"C:\Modding\MaterialTesting\Cloth.txt", _matches
//             .Distinct()
//             .Aggregate("", (current, next) => current + next.Item1 + " - " + next.Item2 + "\r\n"));
//         watch.Stop();
//         System.Console.WriteLine($"Search finished after {watch.ElapsedMilliseconds} milliseconds.");
//     }
//
//     public void FindWeightLimits()
//     {
//         // var materials = new[]
//         // {
//         //     "CHR_NhBasic_Material",
//         //     "CHR_nhSkin_Material",
//         //     "CHR_NhHairBlendTips_Material",
//         //     "CHR_NhEye_Material",
//         //     "CHR_Transparency_Material",
//         //     "CHR_AhBasic_Material",
//         //     "CHR_AhSkin_Material",
//         //     "CHR_AhCloth_Material",
//         //     "CHR_AhHair_Material",
//         //     "CHR_AhEye_Material",
//         //     "CHR_AhTransparency_Material",
//         //     "CHR_Basic_Material"
//         // };
//         //
//         // Parallel.ForEach(materials, material => { FindMaterialHashRecursively(DataDirectory, material); });
//         //
//         // var builder = new StringBuilder();
//         // builder.AppendLine("var hashes = new Dictionary<string, ulong>");
//         // builder.AppendLine("{");
//         // foreach (var (material, hash) in _hashes)
//         // {
//         //     builder.AppendLine("{\"" + material + "\", " + hash + "}");
//         // }
//         //
//         // builder.AppendLine("}");
//         // System.Console.Write(builder.ToString());
//         // return;
//
//         var hashes = new Dictionary<string, ulong>
//         {
//             {"CHR_Basic_Material", 1364795739100408041},
//             {"CHR_NhBasic_Material", 1364796605585357796},
//             {"CHR_Transparency_Material", 1364801387760584455},
//             {"CHR_AhBasic_Material", 1364792899576541549},
//             {"CHR_AhEye_Material", 1364799758182926714},
//             {"CHR_AhCloth_Material", 1364798494917927011},
//             {"CHR_NhEye_Material", 1364797471743020018},
//             {"CHR_AhHair_Material", 1364796879206193149},
//             {"CHR_AhTransparency_Material", 1364787128362217975},
//             {"CHR_AhSkin_Material", 1364792619619205877},
//             {"CHR_NhHairBlendTips_Material", 1364788630667498024},
//             {"CHR_nhSkin_Material", 1364789999862915357}
//         };
//
//         Parallel.ForEach(hashes, kvp =>
//         {
//             var (name, hash) = kvp;
//             FindWeightLimitRecursively(DataDirectory, name, hash);
//         });
//         // foreach (var (name, hash) in hashes)
//         // {
//         //     FindWeightLimitRecursively(DataDirectory, name, hash);
//         // }
//
//         foreach (var (material, limit) in _limits)
//         {
//             System.Console.WriteLine($"{material}: {limit}");
//         }
//     }
//
//     private void FindRecursively(string directory)
//     {
//         foreach (var file in Directory.EnumerateFiles(directory, "*.earc"))
//         {
//             using var unpacker = new EbonyArchive(file);
//             var materials = unpacker.UnpackFilesByQuery(".gmtl");
//
//             foreach (var (materialUri, materialData) in materials)
//             {
//                 try
//                 {
//                     var reader = new MaterialReader(materialData);
//                     var material = reader.Read();
//                     if (material.Interfaces.Any(i => i.Name == "CHR_NhBasic_Material")
//                         && !material.Textures.Any(t => t.ShaderGenName.Contains("MultiMask"))
//                         && material.InterfaceInputs.Any(i =>
//                             i.ShaderGenName.Contains("BaseColor") &&
//                             i.Values.All(v => v == 1.0)))
//                     {
//                         _matches.Add((materialUri, file));
//                     }
//                 }
//                 catch
//                 {
//                     System.Console.WriteLine($"Failed to read material {materialUri}");
//                 }
//             }
//         }
//
//         foreach (var subdirectory in Directory.EnumerateDirectories(directory))
//         {
//             FindRecursively(subdirectory);
//         }
//     }
//
//     public void FindMaterialHashRecursively(string directory, string materialName)
//     {
//         foreach (var file in Directory.EnumerateFiles(directory, "*.earc"))
//         {
//             using var unpacker = new EbonyArchive(file);
//             var materials = unpacker.UnpackFilesByQuery(".gmtl");
//
//             foreach (var (materialUri, materialData) in materials)
//             {
//                 try
//                 {
//                     var reader = new MaterialReader(materialData);
//                     var material = reader.Read();
//                     if (material.Interfaces.Any(i => i.Name.Equals(materialName, StringComparison.OrdinalIgnoreCase)))
//                     {
//                         var hash = Cryptography
//                             .HashFileUri64(material.Header.Dependencies
//                                 ?.FirstOrDefault(d => d.PathHash == "ref")?.Path);
//                         _hashes.TryAdd(materialName, hash);
//                         return;
//                     }
//                 }
//                 catch (Exception ex)
//                 {
//                     System.Console.WriteLine($"Failed to read material {materialUri}");
//                     System.Console.WriteLine(ex.Message);
//                 }
//             }
//         }
//
//         foreach (var subdirectory in Directory.EnumerateDirectories(directory))
//         {
//             FindMaterialHashRecursively(subdirectory, materialName);
//         }
//     }
//
//     private void FindWeightLimitRecursively(string directory, string materialName, ulong materialHash)
//     {
//         foreach (var file in Directory.EnumerateFiles(directory, "*.earc"))
//         {
//             using var unpacker = new EbonyArchive(file);
//             var models = unpacker.UnpackFilesByQuery(".gmdl");
//             var gpubins = unpacker.UnpackFilesByQuery(".gpubin");
//
//             foreach (var (modelUri, modelData) in models)
//             {
//                 try
//                 {
//                     var (gpuUri, gpuData) = gpubins.FirstOrDefault(g => g.Key == modelUri.Replace(".gmdl", ".gpubin"));
//                     if (gpuData == null)
//                     {
//                         System.Console.WriteLine($"Failed to find gpubin for {modelUri}");
//                     }
//
//                     var reader = new ModelReader(modelData, gpuData);
//                     var model = reader.Read();
//                     foreach (var meshObject in model.MeshObjects)
//                     {
//                         foreach (var mesh in meshObject.Meshes)
//                         {
//                             if (mesh.DefaultMaterialHash == materialHash)
//                             {
//                                 System.Console.WriteLine(
//                                     $"Found VertexLayoutType {mesh.VertexLayoutType} for {materialName}");
//                                 _limits.TryAdd(materialName, mesh.VertexLayoutType);
//                                 return;
//                             }
//                         }
//                     }
//                 }
//                 catch (Exception ex)
//                 {
//                     System.Console.WriteLine($"Failed to read model {modelUri}");
//                     System.Console.WriteLine(ex.StackTrace);
//                     return;
//                 }
//             }
//         }
//
//         foreach (var subdirectory in Directory.EnumerateDirectories(directory))
//         {
//             FindWeightLimitRecursively(subdirectory, materialName, materialHash);
//         }
//     }
// }

