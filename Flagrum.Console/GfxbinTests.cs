// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.IO.Compression;
// using System.Linq;
// using System.Text;
// using Flagrum.Core.Archive;
// using Flagrum.Core.Gfxbin.Btex;
// using Flagrum.Core.Gfxbin.Data;
// using Flagrum.Core.Gfxbin.Gmdl;
// using Flagrum.Core.Gfxbin.Gmdl.Components;
// using Flagrum.Core.Gfxbin.Gmdl.Constructs;
// using Flagrum.Core.Gfxbin.Gmdl.Templates;
// using Flagrum.Core.Gfxbin.Gmtl;
// using Flagrum.Core.Utilities;
// using Microsoft.Extensions.Logging;
// using Newtonsoft.Json;
//

using System.IO;
using System.Linq;
using System.Text;
using Flagrum.Core.Archive;
using Flagrum.Core.Gfxbin.Gmdl;
using Flagrum.Core.Gfxbin.Gmtl;

namespace Flagrum.Console;

//
// public class UselessLogger : ILogger
// {
//     public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
//         Func<TState, Exception, string> formatter) { }
//
//     public bool IsEnabled(LogLevel logLevel)
//     {
//         return false;
//     }
//
//     public IDisposable BeginScope<TState>(TState state)
//     {
//         return default!;
//     }
// }
//
public static class GfxbinTests
{
    // public static void BuildGameAssetMod()
    //  {
    //      var previewImage = File.ReadAllBytes($"{IOHelper.GetExecutingDirectory()}\\Resources\\preview.png");
    //      var mod = new Binmod
    //      {
    //          Type = (int)BinmodType.StyleEdit,
    //          Target = (int)OutfitMultiTarget.Gloves,
    //          GameMenuTitle = "Default Outfit",
    //          WorkshopTitle = "Default Outfit",
    //          Description = "So default!",
    //          Uuid = "2fd5d8d5-0e1a-4ac5-8d80-a7a8ccb2715e",
    //          Index = 17120955,
    //          ModDirectoryName = "2fd5d8d5-0e1a-4ac5-8d80-a7a8ccb2715e",
    //          Model1Name = "slim",
    //          Model2Name = "chubby"
    //      };
    //
    //      var packer = new Packer();
    //
    //      packer.AddFile(Modmeta.Build(mod), $"data://mod/{mod.ModDirectoryName}/index.modmeta");
    //      var exml = EntityPackageBuilder.BuildExml(mod);
    //      packer.AddFile(exml, "data://$mod/temp.ebex");
    //
    //      var tempFile = Path.GetTempFileName();
    //      var tempFile2 = Path.GetTempFileName();
    //      File.WriteAllBytes(tempFile, previewImage);
    //      BtexConverter.Convert(btexConverterPath, tempFile, tempFile2, BtexConverter.TextureType.Thumbnail);
    //      var btex = File.ReadAllBytes(tempFile2);
    //
    //      packer.AddFile(previewImage, $"data://mod/{mod.ModDirectoryName}/$preview.png.bin");
    //      packer.AddFile(btex, $"data://mod/{mod.ModDirectoryName}/$preview.png");
    //
    //      foreach (var texturePath in Directory.EnumerateFiles("C:\\Modding\\GameAssetTesting\\nh00_010\\sourceimages"))
    //      {
    //          packer.AddFile(File.ReadAllBytes(texturePath), texturePath
    //              .Replace("C:\\Modding\\GameAssetTesting\\nh00_010", $"data://mod/{mod.ModDirectoryName}")
    //              .Replace('\\', '/')
    //              .Replace(".btex", ".tif"));
    //      }
    //
    //      var gfx = "C:\\Users\\Kieran\\Desktop\\Mods\\Noctis\\character\\nh\\nh00\\model_010\\nh00_010.gmdl.gfxbin";
    //      var gpu = gfx.Replace(".gmdl.gfxbin", ".gpubin");
    //      var reader = new ModelReader(File.ReadAllBytes(gfx), File.ReadAllBytes(gpu));
    //      var model = reader.Read();
    //      var reference = model.Header.Dependencies.FirstOrDefault(d => d.PathHash == "ref");
    //      var assetUri = model.Header.Dependencies.FirstOrDefault(d => d.PathHash == "asset_uri");
    //      reference.Path = $"data://mod/{mod.ModDirectoryName}/{mod.ModelName}.gmdl";
    //      assetUri.Path = $"data://mod/{mod.ModDirectoryName}/";
    //      var gpubin = model.Header.Dependencies.FirstOrDefault(d => d.Path.EndsWith(".gpubin"));
    //      var index = model.Header.Hashes.IndexOf(ulong.Parse(gpubin.PathHash));
    //      gpubin.Path = $"data://mod/{mod.ModDirectoryName}/{mod.ModelName}.gpubin";
    //      var gpubinHash = Cryptography.HashFileUri64(gpubin.Path);
    //      gpubin.PathHash = gpubinHash.ToString();
    //      model.Header.Hashes[index] = gpubinHash;
    //      model.AssetHash = gpubinHash;
    //      model.Header.Dependencies.Clear();
    //
    //      foreach (var materialPath in Directory.EnumerateFiles("C:\\Modding\\GameAssetTesting\\nh00_010\\materials"))
    //      {
    //          var materialUri = materialPath
    //              .Replace("C:\\Modding\\GameAssetTesting\\nh00_010", $"data://mod/{mod.ModDirectoryName}")
    //              .Replace('\\', '/')
    //              .Replace(".gmtl.gfxbin", ".gmtl");
    //
    //          model.Header.Dependencies.Add(new DependencyPath
    //              {Path = materialUri, PathHash = Cryptography.HashFileUri64(materialUri).ToString()});
    //
    //          var materialReader = new MaterialReader(materialPath);
    //          var material = materialReader.Read();
    //          var oldMaterialHash =
    //              Cryptography.HashFileUri64(material.Header.Dependencies.FirstOrDefault(d => d.PathHash == "ref").Path);
    //          var matchingMeshes = model.MeshObjects[0].Meshes.Where(m => m.DefaultMaterialHash == oldMaterialHash);
    //          if (!matchingMeshes.Any())
    //          {
    //              throw new Exception("REEEEEEE");
    //          }
    //
    //          foreach (var matchingMesh in matchingMeshes)
    //          {
    //              matchingMesh.DefaultMaterialHash = Cryptography.HashFileUri64(materialUri);
    //          }
    //
    //          foreach (var texture in material.Textures.Where(t => t.Path.Contains("character/nh/nh00")))
    //          {
    //              texture.Path = texture.Path.Replace("data://character/nh/nh00", "data://mod");
    //              texture.PathHash = Cryptography.Hash32(texture.Path);
    //              texture.ResourceFileHash = Cryptography.HashFileUri64(texture.Path);
    //          }
    //
    //          var dependencies = new List<DependencyPath>();
    //          dependencies.AddRange(material.ShaderBinaries.Where(s => s.ResourceFileHash > 0).Select(s =>
    //              new DependencyPath {Path = s.Path, PathHash = s.ResourceFileHash.ToString()}));
    //          dependencies.AddRange(material.Textures.Where(s => s.ResourceFileHash > 0).Select(s => new DependencyPath
    //              {Path = s.Path, PathHash = s.ResourceFileHash.ToString()}));
    //          dependencies.Add(new DependencyPath
    //              {PathHash = "asset_uri", Path = $"data://mod/{mod.ModDirectoryName}/materials/"});
    //          dependencies.Add(new DependencyPath {PathHash = "ref", Path = materialUri});
    //          material.Header.Dependencies = dependencies
    //              .DistinctBy(d => d.PathHash)
    //              .OrderBy(d => d.PathHash)
    //              .ToList();
    //          material.Header.Hashes = material.Header.Dependencies
    //              .Where(d => ulong.TryParse(d.PathHash, out _))
    //              .Select(d => ulong.Parse(d.PathHash))
    //              .OrderBy(h => h)
    //              .ToList();
    //
    //          var materialWriter = new MaterialWriter(material);
    //          packer.AddFile(materialWriter.Write(), materialUri);
    //      }
    //
    //      var gpubinPath = $"data://mod/{mod.ModDirectoryName}/{mod.ModelName}.gpubin";
    //      model.Header.Dependencies.Add(new DependencyPath
    //          {Path = gpubinPath, PathHash = Cryptography.HashFileUri64(gpubinPath).ToString()});
    //      model.Header.Dependencies.Add(reference);
    //      model.Header.Dependencies.Add(assetUri);
    //      model.Header.Dependencies = model.Header.Dependencies
    //          .OrderBy(d => d.PathHash)
    //          .ToList();
    //      model.Header.Hashes = model.Header.Dependencies
    //          .Where(d => ulong.TryParse(d.PathHash, out _))
    //          .Select(d => ulong.Parse(d.PathHash))
    //          .OrderBy(h => h)
    //          .ToList();
    //
    //      var writer = new ModelWriter(model);
    //      var (gfxData, gpuData) = writer.Write();
    //
    //      packer.AddFile(gfxData, $"data://mod/{mod.ModDirectoryName}/{mod.ModelName}.gmdl.gfxbin");
    //      packer.AddFile(gpuData, $"data://mod/{mod.ModDirectoryName}/{mod.ModelName}.gpubin");
    //      packer.WriteToFile($"C:\\Modding\\{mod.Uuid}.ffxvbinmod");
    //  }
    public static void CheckModel()
    {
        var unpacker =
            new Unpacker(
                @"C:\Users\Kieran\Documents\My Games\FINAL FANTASY XV\Steam\76561198079211203\mod\2fd5d8d5-0e1a-4ac5-8d80-a7a8ccb2715e.ffxvbinmod");
        var gfx = unpacker.UnpackFileByQuery("slim.gmdl", out _);
        var gpu = unpacker.UnpackFileByQuery("slim.gpubin", out _);
        //File.WriteAllBytes(@"C:\Modding\slim.gmdl.gfxbin", gfx);
        //File.WriteAllBytes(@"C:\Modding\slim.gpubin", gpu);
        var reader = new ModelReader(gfx, gpu);
        var model = reader.Read();
        var x = true;
    }

//     public static void Add010ToMod()
//     {
//         var modDirectoryName = "d0120e15-3b15-4821-845c-c7d6b94b1a72";
//         var modelName = "binmod_prep";
//         var unpacker = new Unpacker($"C:\\Modding\\ModelReplacementTesting\\{modDirectoryName}.earc");
//         var packer = unpacker.ToPacker();
//
//         var gfx = "C:\\Users\\Kieran\\Desktop\\Mods\\Noctis\\character\\nh\\nh00\\model_010\\nh00_010.gmdl.gfxbin";
//         var gpu = gfx.Replace(".gmdl.gfxbin", ".gpubin");
//         var reader = new ModelReader(File.ReadAllBytes(gfx), File.ReadAllBytes(gpu));
//         var model = reader.Read();
//
//         var allVertices = model.MeshObjects[0].Meshes
//             .SelectMany(m => m.VertexPositions)
//             .ToList();
//         model.Aabb = new Aabb(
//             new Vector3(
//                 allVertices.Min(v => v.X),
//                 allVertices.Min(v => v.Y),
//                 allVertices.Min(v => v.Z)),
//             new Vector3(
//                 allVertices.Max(v => v.X),
//                 allVertices.Max(v => v.Y),
//                 allVertices.Max(v => v.Z)));
//
//         foreach (var mesh in model.MeshObjects[0].Meshes)
//         {
//             mesh.ColorMaps.Clear();
//
//             mesh.Aabb = new Aabb(
//                 new Vector3(
//                     mesh.VertexPositions.Min(v => v.X),
//                     mesh.VertexPositions.Min(v => v.Y),
//                     mesh.VertexPositions.Min(v => v.Z)),
//                 new Vector3(
//                     mesh.VertexPositions.Max(v => v.X),
//                     mesh.VertexPositions.Max(v => v.Y),
//                     mesh.VertexPositions.Max(v => v.Z)));
//
//             var center = new Vector3(
//                 (mesh.Aabb.Min.X + mesh.Aabb.Max.X) / 2,
//                 (mesh.Aabb.Min.Y + mesh.Aabb.Min.Y) / 2,
//                 (mesh.Aabb.Min.Z + mesh.Aabb.Max.Z) / 2
//             );
//
//             mesh.OrientedBB = new OrientedBB(
//                 center,
//                 new Vector3(mesh.Aabb.Max.X - center.X, 0, 0),
//                 new Vector3(0, mesh.Aabb.Max.Y - center.Y, 0),
//                 new Vector3(0, 0, mesh.Aabb.Max.Z - center.Z)
//             );
//         }
//
//
//         model.Header.Dependencies.Clear();
//
//         var gpubinPath = $"data://mod/{modDirectoryName}/{modelName}.gpubin";
//         var materialUri = $"data://mod/{modDirectoryName}/materials/bodyshape_mat.gmtl";
//         model.AssetHash = Cryptography.HashFileUri64(gpubinPath);
//         foreach (var mesh in model.MeshObjects[0].Meshes)
//         {
//             mesh.DefaultMaterialHash = Cryptography.HashFileUri64(materialUri);
//             mesh.VertexLayoutType = VertexLayoutType.Skinning_4Bones;
//         }
//
//         model.Header.Dependencies.Add(new DependencyPath
//             {Path = materialUri, PathHash = Cryptography.HashFileUri64(materialUri).ToString()});
//         model.Header.Dependencies.Add(new DependencyPath
//             {Path = gpubinPath, PathHash = Cryptography.HashFileUri64(gpubinPath).ToString()});
//         model.Header.Dependencies.Add(new DependencyPath
//             {PathHash = "asset_uri", Path = $"data://mod/{modDirectoryName}/"});
//         model.Header.Dependencies.Add(new DependencyPath
//             {PathHash = "ref", Path = $"data://mod/{modDirectoryName}/{modelName}.gmdl"});
//         model.Header.Dependencies = model.Header.Dependencies
//             .OrderBy(d => d.PathHash)
//             .ToList();
//         model.Header.Hashes = model.Header.Dependencies
//             .Where(d => ulong.TryParse(d.PathHash, out _))
//             .Select(d => ulong.Parse(d.PathHash))
//             .OrderBy(h => h)
//             .ToList();
//
//         var writer = new ModelWriter(model);
//         var (gfxData, gpuData) = writer.Write();
//         packer.UpdateFile($"{modelName}.gmdl", gfxData);
//         packer.UpdateFile($"{modelName}.gpubin", gpuData);
//
//         packer.WriteToFile($"C:\\Modding\\ModelReplacementTesting\\{modDirectoryName}.ffxvbinmod");
//     }
//
//     public static void BuildGameAssetWeaponMod()
//     {
//         var btexConverterPath =
//             $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\squareenix\\ffxvmodtool\\luminousstudio\\luminous\\sdk\\bin\\BTexConverter.exe";
//
//         var previewImage = File.ReadAllBytes($"{IOHelper.GetExecutingDirectory()}\\Resources\\preview.png");
//         var mod = new Binmod
//         {
//             Type = (int)BinmodType.Weapon,
//             Target = (int)WeaponSoloTarget.Sword,
//             GameMenuTitle = "Angery Sword",
//             WorkshopTitle = "Angery Sword",
//             Description = "So default!",
//             Uuid = "33684db3-62c7-4a32-be4b-0deb7c293005",
//             Index = 17197479,
//             ModDirectoryName = "33684db3-62c7-4a32-be4b-0deb7c293005",
//             ModelName = "angery_sword",
//             Attack = 30,
//             Critical = 2,
//             MaxHp = 93,
//             MaxMp = 19,
//             Vitality = 12
//         };
//
//         var packer = new Packer();
//
//         packer.AddFile(Modmeta.Build(mod), $"data://mod/{mod.ModDirectoryName}/index.modmeta");
//         var exml = EntityPackageBuilder.BuildExml(mod);
//         packer.AddFile(exml, "data://$mod/temp.ebex");
//
//         var tempFile = Path.GetTempFileName();
//         var tempFile2 = Path.GetTempFileName();
//         File.WriteAllBytes(tempFile, previewImage);
//         BtexConverter.Convert(btexConverterPath, tempFile, tempFile2, BtexConverter.TextureType.Thumbnail);
//         var btex = File.ReadAllBytes(tempFile2);
//
//         packer.AddFile(previewImage, $"data://mod/{mod.ModDirectoryName}/$preview.png.bin");
//         packer.AddFile(btex, $"data://mod/{mod.ModDirectoryName}/$preview.png");
//
//         foreach (var texturePath in Directory.EnumerateFiles(
//                      "C:\\Modding\\Extractions\\character\\we\\we01\\model_100\\sourceimages"))
//         {
//             packer.AddFile(File.ReadAllBytes(texturePath), texturePath
//                 .Replace("C:\\Modding\\Extractions\\character\\we\\we01\\model_100",
//                     $"data://mod/{mod.ModDirectoryName}")
//                 .Replace('\\', '/')
//                 .Replace(".btex", ".tif"));
//         }
//
//         var gfx = "C:\\Modding\\Extractions\\character\\we\\we01\\model_100\\we01_100.gmdl.gfxbin";
//         var gpu = gfx.Replace(".gmdl.gfxbin", ".gpubin");
//         var reader = new ModelReader(File.ReadAllBytes(gfx), File.ReadAllBytes(gpu));
//         var model = reader.Read();
//         foreach (var mesh in model.MeshObjects[0].Meshes)
//         {
//             mesh.MaterialType = MaterialType.FourWeights;
//         }
//
//         var reference = model.Header.Dependencies.FirstOrDefault(d => d.PathHash == "ref");
//         var assetUri = model.Header.Dependencies.FirstOrDefault(d => d.PathHash == "asset_uri");
//         reference.Path = $"data://mod/{mod.ModDirectoryName}/{mod.ModelName}.gmdl";
//         assetUri.Path = $"data://mod/{mod.ModDirectoryName}/";
//         var gpubin = model.Header.Dependencies.FirstOrDefault(d => d.Path.EndsWith(".gpubin"));
//         var index = model.Header.Hashes.IndexOf(ulong.Parse(gpubin.PathHash));
//         gpubin.Path = $"data://mod/{mod.ModDirectoryName}/{mod.ModelName}.gpubin";
//         var gpubinHash = Cryptography.HashFileUri64(gpubin.Path);
//         gpubin.PathHash = gpubinHash.ToString();
//         model.Header.Hashes[index] = gpubinHash;
//         model.AssetHash = gpubinHash;
//         model.Header.Dependencies.Clear();
//
//         foreach (var materialPath in Directory
//                      .EnumerateFiles("C:\\Modding\\Extractions\\character\\we\\we01\\model_100\\materials")
//                      .Where(f => !f.Contains("backup")))
//         {
//             var materialUri = materialPath
//                 .Replace("C:\\Modding\\Extractions\\character\\we\\we01\\model_100",
//                     $"data://mod/{mod.ModDirectoryName}")
//                 .Replace('\\', '/')
//                 .Replace(".gmtl.gfxbin", ".gmtl");
//
//             model.Header.Dependencies.Add(new DependencyPath
//                 {Path = materialUri, PathHash = Cryptography.HashFileUri64(materialUri).ToString()});
//
//             var materialReader = new MaterialReader(materialPath);
//             var material = materialReader.Read();
//             var oldMaterialHash =
//                 Cryptography.HashFileUri64(material.Header.Dependencies.FirstOrDefault(d => d.PathHash == "ref").Path);
//             var matchingMeshes = model.MeshObjects[0].Meshes; //.Where(m => m.DefaultMaterialHash == oldMaterialHash);
//             if (!matchingMeshes.Any())
//             {
//                 throw new Exception("REEEEEEE");
//             }
//
//             foreach (var matchingMesh in matchingMeshes)
//             {
//                 matchingMesh.DefaultMaterialHash = Cryptography.HashFileUri64(materialUri);
//             }
//
//             foreach (var texture in material.Textures.Where(t => t.Path.Contains("character/we/we01")))
//             {
//                 texture.Path = texture.Path.Replace("data://character/we/we01", "data://mod");
//                 texture.PathHash = Cryptography.Hash32(texture.Path);
//                 texture.ResourceFileHash = Cryptography.HashFileUri64(texture.Path);
//             }
//
//             var dependencies = new List<DependencyPath>();
//             dependencies.AddRange(material.ShaderBinaries.Where(s => s.ResourceFileHash > 0).Select(s =>
//                 new DependencyPath {Path = s.Path, PathHash = s.ResourceFileHash.ToString()}));
//             dependencies.AddRange(material.Textures.Where(s => s.ResourceFileHash > 0).Select(s => new DependencyPath
//                 {Path = s.Path, PathHash = s.ResourceFileHash.ToString()}));
//             dependencies.Add(new DependencyPath
//                 {PathHash = "asset_uri", Path = $"data://mod/{mod.ModDirectoryName}/materials/"});
//             dependencies.Add(new DependencyPath {PathHash = "ref", Path = materialUri});
//             material.Header.Dependencies = dependencies
//                 .DistinctBy(d => d.PathHash)
//                 .OrderBy(d => d.PathHash)
//                 .ToList();
//             material.Header.Hashes = material.Header.Dependencies
//                 .Where(d => ulong.TryParse(d.PathHash, out _))
//                 .Select(d => ulong.Parse(d.PathHash))
//                 .OrderBy(h => h)
//                 .ToList();
//
//             var materialWriter = new MaterialWriter(material);
//             packer.AddFile(materialWriter.Write(), materialUri);
//         }
//
//         var gpubinPath = $"data://mod/{mod.ModDirectoryName}/{mod.ModelName}.gpubin";
//         model.Header.Dependencies.Add(new DependencyPath
//             {Path = gpubinPath, PathHash = Cryptography.HashFileUri64(gpubinPath).ToString()});
//         model.Header.Dependencies.Add(reference);
//         model.Header.Dependencies.Add(assetUri);
//         model.Header.Dependencies = model.Header.Dependencies
//             .OrderBy(d => d.PathHash)
//             .ToList();
//         model.Header.Hashes = model.Header.Dependencies
//             .Where(d => ulong.TryParse(d.PathHash, out _))
//             .Select(d => ulong.Parse(d.PathHash))
//             .OrderBy(h => h)
//             .ToList();
//
//         var writer = new ModelWriter(model);
//         var (gfxData, gpuData) = writer.Write();
//
//         packer.AddFile(gfxData, $"data://mod/{mod.ModDirectoryName}/{mod.ModelName}.gmdl.gfxbin");
//         packer.AddFile(gpuData, $"data://mod/{mod.ModDirectoryName}/{mod.ModelName}.gpubin");
//         packer.WriteToFile($"C:\\Modding\\{mod.Uuid}.ffxvbinmod");
//     }
//
//     public static void BuildGameAssetMod()
//     {
//         var btexConverterPath =
//             $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\squareenix\\ffxvmodtool\\luminousstudio\\luminous\\sdk\\bin\\BTexConverter.exe";
//
//         var previewImage = File.ReadAllBytes($"{IOHelper.GetExecutingDirectory()}\\Resources\\preview.png");
//         var mod = new Binmod
//         {
//             Type = (int)BinmodType.Character,
//             Target = (int)ModelReplacementTarget.Gladiolus,
//             GameMenuTitle = "Default Outfit",
//             WorkshopTitle = "Default Outfit",
//             Description = "So default!",
//             Uuid = "b9c5399e-61d1-4552-89ea-08b4089205b1",
//             Index = 17217034,
//             ModDirectoryName = "model_010",
//             ModelName = "nh00_010",
//             OriginalGmdls =
//                 new List<string>(ModelReplacementPresets.GetOriginalGmdls((int)ModelReplacementTarget.Gladiolus)),
//             OriginalGmdlCount = ModelReplacementPresets.GetOriginalGmdls((int)ModelReplacementTarget.Gladiolus).Length
//         };
//
//         var packer = new Packer();
//
//         packer.AddFile(Modmeta.Build(mod), $"data://mod/{mod.ModDirectoryName}/index.modmeta");
//         var exml = EntityPackageBuilder.BuildExml(mod);
//         packer.AddFile(exml, "data://$mod/temp.ebex");
//
//         var tempFile = Path.GetTempFileName();
//         var tempFile2 = Path.GetTempFileName();
//         File.WriteAllBytes(tempFile, previewImage);
//         BtexConverter.Convert(btexConverterPath, tempFile, tempFile2, BtexConverter.TextureType.Thumbnail);
//         var btex = File.ReadAllBytes(tempFile2);
//
//         packer.AddFile(previewImage, $"data://mod/{mod.ModDirectoryName}/$preview.png.bin");
//         packer.AddFile(btex, $"data://mod/{mod.ModDirectoryName}/$preview.png");
//
//         foreach (var texturePath in Directory.EnumerateFiles("C:\\Modding\\GameAssetTesting\\nh00_010\\sourceimages"))
//         {
//             packer.AddFile(File.ReadAllBytes(texturePath), texturePath
//                 .Replace("C:\\Modding\\GameAssetTesting\\nh00_010", $"data://mod/{mod.ModDirectoryName}")
//                 .Replace('\\', '/')
//                 .Replace(".btex", ".tif"));
//         }
//
//         var gfx = "C:\\Users\\Kieran\\Desktop\\Mods\\Noctis\\character\\nh\\nh00\\model_010\\nh00_010.gmdl.gfxbin";
//         var gpu = gfx.Replace(".gmdl.gfxbin", ".gpubin");
//         var reader = new ModelReader(File.ReadAllBytes(gfx), File.ReadAllBytes(gpu));
//         var model = reader.Read();
//         var reference = model.Header.Dependencies.FirstOrDefault(d => d.PathHash == "ref");
//         var assetUri = model.Header.Dependencies.FirstOrDefault(d => d.PathHash == "asset_uri");
//         reference.Path = $"data://mod/{mod.ModDirectoryName}/{mod.ModelName}.gmdl";
//         assetUri.Path = $"data://mod/{mod.ModDirectoryName}/";
//         var gpubin = model.Header.Dependencies.FirstOrDefault(d => d.Path.EndsWith(".gpubin"));
//         var index = model.Header.Hashes.IndexOf(ulong.Parse(gpubin.PathHash));
//         gpubin.Path = $"data://mod/{mod.ModDirectoryName}/{mod.ModelName}.gpubin";
//         var gpubinHash = Cryptography.HashFileUri64(gpubin.Path);
//         gpubin.PathHash = gpubinHash.ToString();
//         model.Header.Hashes[index] = gpubinHash;
//         model.AssetHash = gpubinHash;
//         model.Header.Dependencies.Clear();
//
//         foreach (var materialPath in Directory.EnumerateFiles("C:\\Modding\\GameAssetTesting\\nh00_010\\materials"))
//         {
//             var materialUri = materialPath
//                 .Replace("C:\\Modding\\GameAssetTesting\\nh00_010", $"data://mod/{mod.ModDirectoryName}")
//                 .Replace('\\', '/')
//                 .Replace(".gmtl.gfxbin", ".gmtl");
//
//             model.Header.Dependencies.Add(new DependencyPath
//                 {Path = materialUri, PathHash = Cryptography.HashFileUri64(materialUri).ToString()});
//
//             var materialReader = new MaterialReader(materialPath);
//             var material = materialReader.Read();
//             var oldMaterialHash =
//                 Cryptography.HashFileUri64(material.Header.Dependencies.FirstOrDefault(d => d.PathHash == "ref").Path);
//             var matchingMeshes = model.MeshObjects[0].Meshes.Where(m => m.DefaultMaterialHash == oldMaterialHash);
//             if (!matchingMeshes.Any())
//             {
//                 throw new Exception("REEEEEEE");
//             }
//
//             foreach (var matchingMesh in matchingMeshes)
//             {
//                 matchingMesh.DefaultMaterialHash = Cryptography.HashFileUri64(materialUri);
//             }
//
//             foreach (var texture in material.Textures.Where(t => t.Path.Contains("character/nh/nh00")))
//             {
//                 texture.Path = texture.Path.Replace("data://character/nh/nh00", "data://mod");
//                 texture.PathHash = Cryptography.Hash32(texture.Path);
//                 texture.ResourceFileHash = Cryptography.HashFileUri64(texture.Path);
//             }
//
//             var dependencies = new List<DependencyPath>();
//             dependencies.AddRange(material.ShaderBinaries.Where(s => s.ResourceFileHash > 0).Select(s =>
//                 new DependencyPath {Path = s.Path, PathHash = s.ResourceFileHash.ToString()}));
//             dependencies.AddRange(material.Textures.Where(s => s.ResourceFileHash > 0).Select(s => new DependencyPath
//                 {Path = s.Path, PathHash = s.ResourceFileHash.ToString()}));
//             dependencies.Add(new DependencyPath
//                 {PathHash = "asset_uri", Path = $"data://mod/{mod.ModDirectoryName}/materials/"});
//             dependencies.Add(new DependencyPath {PathHash = "ref", Path = materialUri});
//             material.Header.Dependencies = dependencies
//                 .DistinctBy(d => d.PathHash)
//                 .OrderBy(d => d.PathHash)
//                 .ToList();
//             material.Header.Hashes = material.Header.Dependencies
//                 .Where(d => ulong.TryParse(d.PathHash, out _))
//                 .Select(d => ulong.Parse(d.PathHash))
//                 .OrderBy(h => h)
//                 .ToList();
//
//             var materialWriter = new MaterialWriter(material);
//             packer.AddFile(materialWriter.Write(), materialUri);
//         }
//
//         var gpubinPath = $"data://mod/{mod.ModDirectoryName}/{mod.ModelName}.gpubin";
//         model.Header.Dependencies.Add(new DependencyPath
//             {Path = gpubinPath, PathHash = Cryptography.HashFileUri64(gpubinPath).ToString()});
//         model.Header.Dependencies.Add(reference);
//         model.Header.Dependencies.Add(assetUri);
//         model.Header.Dependencies = model.Header.Dependencies
//             .OrderBy(d => d.PathHash)
//             .ToList();
//         model.Header.Hashes = model.Header.Dependencies
//             .Where(d => ulong.TryParse(d.PathHash, out _))
//             .Select(d => ulong.Parse(d.PathHash))
//             .OrderBy(h => h)
//             .ToList();
//
//         var writer = new ModelWriter(model);
//         var (gfxData, gpuData) = writer.Write();
//
//         packer.AddFile(gfxData, $"data://mod/{mod.ModDirectoryName}/{mod.ModelName}.gmdl.gfxbin");
//         packer.AddFile(gpuData, $"data://mod/{mod.ModDirectoryName}/{mod.ModelName}.gpubin");
//         packer.WriteToFile($"C:\\Modding\\{mod.Uuid}.ffxvbinmod");
//     }
//
//     public static void CheckMod()
//     {
//         var outPath = "C:\\Testing\\ModelReplacements\\SinglePlayerSword\\";
//         var unpacker =
//             new Unpacker(
//                 "C:\\Testing\\ModelReplacements\\SinglePlayerSword\\33684db3-62c7-4a32-be4b-0deb7c293005.ffxvbinmod");
//         var gfx = unpacker.UnpackFileByQuery("khopesh.fbx", out _);
//         var gpu = unpacker.UnpackFileByQuery("khopesh.gpubin", out _);
//         File.WriteAllBytes($"{outPath}khopesh.gmdl.gfxbin", gfx);
//         var reader = new ModelReader(gfx, gpu);
//         var model = reader.Read();
//         var x = true;
//     }
//
//     public static void FixModelReplacementMod()
//     {
//         var btexConverterPath =
//             $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\squareenix\\ffxvmodtool\\luminousstudio\\luminous\\sdk\\bin\\BTexConverter.exe";
//
//         var modDirectory = "f749648e-54dd-4159-940d-65ff379663d4";
//         var modelName = "ardynmankini3";
//
//         var unpacker = new Unpacker("C:\\Testing\\ModelReplacements\\SinglePlayerSword\\sword_1.ffxvbinmod");
//         var moFiles = unpacker.UnpackFilesByQuery("data://shader");
//
//         var fbxDefault = unpacker.UnpackFileByQuery("material.gmtl", out _);
//         unpacker = new Unpacker($"C:\\Modding\\ModelReplacementTesting\\{modDirectory}.earc");
//         var packer = unpacker.ToPacker();
//
//         var reader = new MaterialReader(fbxDefault);
//         var material = reader.Read();
//         var oldDependency = material.Header.Dependencies.FirstOrDefault(d => d.Path.EndsWith("_d.png"));
//         var oldIndex = material.Header.Hashes.IndexOf(ulong.Parse(oldDependency.PathHash));
//         var oldTexture = material.Textures.FirstOrDefault(t => t.Path == oldDependency.Path);
//         oldTexture.Path = $"data://mod/{modDirectory}/sourceimages/chest_basecolor0_texture_b.btex";
//         oldTexture.PathHash = Cryptography.Hash32(oldTexture.Path);
//         oldTexture.ResourceFileHash = Cryptography.HashFileUri64(oldTexture.Path);
//         material.Header.Hashes[oldIndex] = oldTexture.ResourceFileHash;
//         oldDependency.Path = oldTexture.Path;
//         oldDependency.PathHash = oldTexture.ResourceFileHash.ToString();
//         var assetUri = material.Header.Dependencies.FirstOrDefault(d => d.PathHash == "asset_uri");
//         assetUri.Path = $"data://mod/{modDirectory}/materials/";
//         var reference = material.Header.Dependencies.FirstOrDefault(d => d.PathHash == "ref");
//         reference.Path = $"data://mod/{modDirectory}/materials/chest_mat.gmtl";
//         material.Name = "chest_mat";
//         material.NameHash = Cryptography.Hash32(material.Name);
//         var writer = new MaterialWriter(material);
//
//         packer.RemoveFile("chest_mat.gmtl");
//         packer.AddFile(writer.Write(), $"data://mod/{modDirectory}/materials/chest_mat.gmtl");
//         foreach (var (uri, data) in moFiles)
//         {
//             packer.AddFile(data, uri);
//         }
//
//         packer.WriteToFile($"C:\\Modding\\ModelReplacementTesting\\{modDirectory}.ffxvbinmod");
//     }
//
//     public static void FixWeaponMod()
//     {
//         var btexConverterPath =
//             $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\squareenix\\ffxvmodtool\\luminousstudio\\luminous\\sdk\\bin\\BTexConverter.exe";
//
//         var unpacker = new Unpacker("C:\\Testing\\ModelReplacements\\SinglePlayerSword\\sword_1.ffxvbinmod");
//         var moFiles = unpacker.UnpackFilesByQuery("data://shader");
//
//         var fbxDefault = unpacker.UnpackFileByQuery("material.gmtl", out _);
//         unpacker = new Unpacker("C:\\Modding\\WeaponTesting\\24d5d6ab-e8f4-443f-a5e1-a8830aff7924.earc");
//         var packer = unpacker.ToPacker();
//
//         var reader = new MaterialReader(fbxDefault);
//         var material = reader.Read();
//         var oldDependency = material.Header.Dependencies.FirstOrDefault(d => d.Path.EndsWith("_d.png"));
//         var oldIndex = material.Header.Hashes.IndexOf(ulong.Parse(oldDependency.PathHash));
//         var oldTexture = material.Textures.FirstOrDefault(t => t.Path == oldDependency.Path);
//         oldTexture.Path =
//             "data://mod/24d5d6ab-e8f4-443f-a5e1-a8830aff7924/sourceimages/body_ashape_basecolor0_texture_b.btex";
//         oldTexture.PathHash = Cryptography.Hash32(oldTexture.Path);
//         oldTexture.ResourceFileHash = Cryptography.HashFileUri64(oldTexture.Path);
//         material.Header.Hashes[oldIndex] = oldTexture.ResourceFileHash;
//         oldDependency.Path = oldTexture.Path;
//         oldDependency.PathHash = oldTexture.ResourceFileHash.ToString();
//         var assetUri = material.Header.Dependencies.FirstOrDefault(d => d.PathHash == "asset_uri");
//         assetUri.Path = "data://mod/24d5d6ab-e8f4-443f-a5e1-a8830aff7924/materials/";
//         var reference = material.Header.Dependencies.FirstOrDefault(d => d.PathHash == "ref");
//         reference.Path = "data://mod/24d5d6ab-e8f4-443f-a5e1-a8830aff7924/materials/body_ashape_mat.gmtl";
//         material.Name = "body_ashape_mat";
//         material.NameHash = Cryptography.Hash32(material.Name);
//         var writer = new MaterialWriter(material);
//
//         packer.RemoveFile("body_ashape_mat.gmtl");
//         packer.AddFile(writer.Write(),
//             "data://mod/24d5d6ab-e8f4-443f-a5e1-a8830aff7924/materials/body_ashape_mat.gmtl");
//         foreach (var (uri, data) in moFiles)
//         {
//             packer.AddFile(data, uri);
//         }
//
//         packer.WriteToFile("C:\\Modding\\WeaponTesting\\24d5d6ab-e8f4-443f-a5e1-a8830aff7924.ffxvbinmod");
//     }
//
//     public static void BuildMod()
//     {
//         //CheckMod();
//
//         var btexConverterPath =
//             $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\squareenix\\ffxvmodtool\\luminousstudio\\luminous\\sdk\\bin\\BTexConverter.exe";
//
//         var previewImage = File.ReadAllBytes($"{IOHelper.GetExecutingDirectory()}\\Resources\\preview.png");
//         var mod = new Binmod
//         {
//             Type = (int)BinmodType.Weapon,
//             Target = (int)WeaponSoloTarget.Sword,
//             GameMenuTitle = "Angery Sword",
//             WorkshopTitle = "Angery Sword",
//             Description = "So angry!",
//             Uuid = "33684db3-62c7-4a32-be4b-0deb7c293005",
//             Index = 17197479,
//             ModDirectoryName = "sword_1",
//             ModelName = "khopesh",
//             Attack = 30,
//             MaxHp = 93,
//             MaxMp = 19,
//             Critical = 2,
//             Vitality = 12
//         };
//
//         // var builder = new BinmodBuilder(btexConverterPath, mod, previewImage);
//         //var fmd = File.ReadAllBytes("C:\\Users\\Kieran\\Desktop\\angery_sword.fmd");
//         // builder.AddFmd(btexConverterPath, fmd, new UselessLogger());
//         // builder.WriteToFile("C:\\Testing\\ModelReplacements\\SinglePlayerSword\\33684db3-62c7-4a32-be4b-0deb7c293005.ffxvbinmod");
//
//         var unpacker = new Unpacker("C:\\Testing\\ModelReplacements\\SinglePlayerSword\\sword_1.ffxvbinmod");
//         var material = unpacker.UnpackFileByQuery("material.gmtl", out _);
//         var packer = unpacker.ToPacker();
//
//         packer.UpdateFile("index.modmeta", Modmeta.Build(mod));
//         var exml = EntityPackageBuilder.BuildExml(mod);
//         packer.UpdateFile("temp.ebex", exml);
//
//         var tempFile = Path.GetTempFileName();
//         var tempFile2 = Path.GetTempFileName();
//         File.WriteAllBytes(tempFile, previewImage);
//         BtexConverter.Convert(btexConverterPath, tempFile, tempFile2, BtexConverter.TextureType.Thumbnail);
//         var btex = File.ReadAllBytes(tempFile2);
//
//         packer.UpdateFile("$preview.png.bin", previewImage);
//         packer.UpdateFile("$preview.png", btex);
//
//         var fmd = File.ReadAllBytes("C:\\Users\\Kieran\\Desktop\\angery_sword.fmd");
//         using var memoryStream = new MemoryStream(fmd);
//         using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read);
//
//         var dataEntry = archive.GetEntry("data.json");
//         using var dataStream = new MemoryStream();
//         var stream = dataEntry.Open();
//         stream.CopyTo(dataStream);
//
//         var json = Encoding.UTF8.GetString(dataStream.ToArray());
//         var gpubin = JsonConvert.DeserializeObject<Gpubin>(json);
//
//         var model = OutfitTemplate.Build(mod.ModDirectoryName, mod.ModelName, gpubin);
//         var replacer = new ModelReplacer(model, gpubin, BinmodTypeHelper.GetModmetaTargetName(mod.Type, mod.Target));
//         model = replacer.Replace();
//
//         foreach (var bone in model.BoneHeaders)
//         {
//             bone.Name = "Bone";
//             bone.LodIndex = 4294967295;
//         }
//
//         foreach (var mesh in model.MeshObjects[0].Meshes)
//         {
//             mesh.MaterialType = MaterialType.OneWeight;
//             mesh.BoneIds = new[] {0u};
//             for (var index = 0; index < mesh.WeightIndices[0].Count; index++)
//             {
//                 var weightArray = mesh.WeightIndices[0][index];
//                 for (var i = 0; i < weightArray.Length; i++)
//                 {
//                     weightArray[i] = 0;
//                 }
//             }
//         }
//
//         var writer = new ModelWriter(model);
//         var (gfxData, gpuData) = writer.Write();
//
//         var outPath = "C:\\Testing\\ModelReplacements\\SinglePlayerSword\\khopesh.gmdl.gfxbin";
//         File.WriteAllBytes(outPath, gfxData);
//
//         packer.UpdateFile("khopesh.fbx", gfxData);
//         packer.UpdateFile("khopesh.gpubin", gpuData);
//         packer.RemoveFile("material.gmtl");
//         packer.AddFile(material, "data://mod/sword_1/materials/body_ashape_mat.gmtl");
//
//         var diffusePath =
//             "C:\\Modding\\WeaponTesting\\mod\\24d5d6ab-e8f4-443f-a5e1-a8830aff7924\\sourceimages\\body_ashape_basecolor0_texture_b.btex";
//         packer.UpdateFile("khopesh_d.png", File.ReadAllBytes(diffusePath));
//
//         packer.WriteToFile(
//             "C:\\Testing\\ModelReplacements\\SinglePlayerSword\\33684db3-62c7-4a32-be4b-0deb7c293005.ffxvbinmod");
//     }
//
//     public static void GetBoneTable()
//     {
//         var gfx = "C:\\Testing\\Exineris\\mod\\b376d00b-e6ae-497d-a004-485914158a9b\\ignis.gmdl.gfxbin";
//         var gpu = "C:\\Testing\\Exineris\\mod\\b376d00b-e6ae-497d-a004-485914158a9b\\ignis.gpubin";
//
//         var reader = new ModelReader(File.ReadAllBytes(gfx), File.ReadAllBytes(gpu));
//         var model = reader.Read();
//
//         foreach (var bone in model.BoneHeaders)
//         {
//             System.Console.WriteLine($"{bone.Name}: {bone.LodIndex >> 16}");
//         }
//     }
//
    public static void CheckMaterialDefaults()
    {
        var path = @"C:\Modding\Extractions\character\am\am50\model_010\materials\am50_010_cloth_00_mat.gmtl.gfxbin";
        var reader = new MaterialReader(path);
        var material = reader.Read();

        var builder = new StringBuilder();
        foreach (var input in material.InterfaceInputs.Where(i => i.InterfaceIndex == 0))
        {
            builder.AppendLine($"{input.ShaderGenName}: {string.Join(", ", input.Values)}");
        }

        File.WriteAllText(@"C:\Modding\" + path.Split('\\').Last().Replace(".gmtl.gfxbin", "_input_defaults.txt"),
            builder.ToString());

        builder = new StringBuilder();
        foreach (var texture in material.Textures.Where(t => !t.Path.EndsWith(".sb")))
        {
            builder.AppendLine($"{texture.Name}\n{texture.Path}\n\n");
        }

        File.WriteAllText(@"C:\Modding\" + path.Split('\\').Last().Replace(".gmtl.gfxbin", "_texture_defaults.txt"),
            builder.ToString());
    }
//
//     public static void Compare()
//     {
//         var gfxbin = "C:\\Testing\\extract\\mod\\7594c633-8944-4542-bc12-627b444fdcc4\\prom_shirtless.gmdl.gfxbin";
//         var gpubin = "C:\\Testing\\extract\\mod\\7594c633-8944-4542-bc12-627b444fdcc4\\prom_shirtless.gpubin";
//         //var gfxbin = "C:\\Users\\Kieran\\Desktop\\Mods\\Noctis\\character\\nh\\nh00\\model_010\\nh00_010.gmdl.gfxbin";
//         //var gpubin = "C:\\Users\\Kieran\\Desktop\\Mods\\Noctis\\character\\nh\\nh00\\model_010\\nh00_010.gpubin";
//         //var moGfxbin = "C:\\Testing\\Gfxbin\\mod\\noctis_custom_2\\main.gmdl.gfxbin";
//         //var moGpubin = "C:\\Testing\\Gfxbin\\mod\\noctis_custom_2\\main.gpubin";
//         //var moGfxbin = "C:\\Testing\\Gfxbin\\noctis_custom_2_backup_main.gmdl.gfxbin";
//         //var moGpubin = "C:\\Testing\\Gfxbin\\noctis_custom_2_backup_main.gpubin";
//
//         //var reader = new ModelReader(File.ReadAllBytes(moGfxbin), File.ReadAllBytes(moGpubin));
//         //var mo = reader.Read();
//         var reader = new ModelReader(File.ReadAllBytes(gfxbin), File.ReadAllBytes(gpubin));
//         var model = reader.Read();
//
//         // foreach (var dependency in model.Header.Dependencies)
//         // {
//         //     System.Console.WriteLine($"{dependency.PathHash} - {dependency.Path}");
//         // }
//         //
//         // System.Console.WriteLine($"\n{model.AssetHash}");
//
//         System.Console.WriteLine($"[OG Asset] AABB-min: {model.Aabb.Min.X}, {model.Aabb.Min.Y}, {model.Aabb.Min.Z}");
//         // System.Console.WriteLine($"[MO Asset] AABB-min: {mo.Aabb.Min.X}, {mo.Aabb.Min.Y}, {mo.Aabb.Min.Z}");
//         //
//         System.Console.WriteLine($"[OG Asset] AABB-max: {model.Aabb.Max.X}, {model.Aabb.Max.Y}, {model.Aabb.Max.Z}");
//         // System.Console.WriteLine($"[MO Asset] AABB-max: {mo.Aabb.Max.X}, {mo.Aabb.Max.Y}, {mo.Aabb.Max.Z}\n");
//         //
//         System.Console.WriteLine($"[OG Asset] Name: {model.Name}");
//         // System.Console.WriteLine($"[MO Asset] Name: {mo.Name}\n");
//         //
//         System.Console.WriteLine($"[OG Asset] Unknown1: {model.Unknown1}");
//         //System.Console.WriteLine($"[MO Asset] Unknown1: {mo.Unknown1}\n");
//         //
//         System.Console.WriteLine($"[OG Asset] Asset Hash: {model.AssetHash}");
//         // System.Console.WriteLine($"[MO Asset] Asset Hash: {mo.AssetHash}\n");
//         //
//         System.Console.WriteLine($"[OG Asset] Child Class Format: {model.ChildClassFormat}");
//         // System.Console.WriteLine($"[MO Asset] Child Class Format: {mo.ChildClassFormat}\n");
//         //
//         System.Console.WriteLine($"[OG Asset] Instance Name Format: {model.InstanceNameFormat}");
//         // System.Console.WriteLine($"[MO Asset] Instance Name Format: {mo.InstanceNameFormat}\n");
//         //
//         System.Console.WriteLine($"[OG Asset] Shader Class Format: {model.ShaderClassFormat}");
//         // System.Console.WriteLine($"[MO Asset] Shader Class Format: {mo.ShaderClassFormat}\n");
//         //
//         System.Console.WriteLine($"[OG Asset] Shader Parameter List Format: {model.ShaderParameterListFormat}");
//         // System.Console.WriteLine($"[MO Asset] Shader Parameter List Format: {mo.ShaderParameterListFormat}\n");
//         //
//         System.Console.WriteLine(
//             $"[OG Asset] Shader Sampler Description Format: {model.ShaderSamplerDescriptionFormat}");
//         // System.Console.WriteLine(
//         //     $"[MO Asset] Shader Sampler Description Format: {mo.ShaderSamplerDescriptionFormat}\n");
//         //
//         System.Console.WriteLine($"[OG Asset] Has PSD Path: {model.HasPsdPath}");
//         // System.Console.WriteLine($"[MO Asset] Has PSD Path: {mo.HasPsdPath}\n");
//         //
//         System.Console.WriteLine($"[OG Asset] PSD Path Hash: {model.PsdPathHash}");
//         // System.Console.WriteLine($"[MO Asset] PSD Path Hash: {mo.PsdPathHash}\n");
//
//         foreach (var node in model.NodeTable)
//         {
//             System.Console.WriteLine($"Node name: {node.Name}");
//             System.Console.WriteLine($"{node.Matrix.Rows[0].X}, {node.Matrix.Rows[0].Y}, {node.Matrix.Rows[0].Z}");
//             System.Console.WriteLine($"{node.Matrix.Rows[1].X}, {node.Matrix.Rows[1].Y}, {node.Matrix.Rows[1].Z}");
//             System.Console.WriteLine($"{node.Matrix.Rows[2].X}, {node.Matrix.Rows[2].Y}, {node.Matrix.Rows[2].Z}");
//             System.Console.WriteLine($"{node.Matrix.Rows[3].X}, {node.Matrix.Rows[3].Y}, {node.Matrix.Rows[3].Z}");
//         }
//
//         foreach (var meshObject in model.MeshObjects)
//         {
//             foreach (var mesh in meshObject.Meshes)
//             {
//                 System.Console.WriteLine($"{mesh.Name}");
//
//                 // foreach (var geometry in mesh.SubGeometries)
//                 // {
//                 //     System.Console.WriteLine($"AABB: {geometry.Aabb.Min}, {geometry.Aabb.Max}");
//                 //     System.Console.WriteLine($"Draw order: {geometry.DrawOrder}");
//                 //     System.Console.WriteLine($"Primitive count: {geometry.PrimitiveCount}");
//                 //     System.Console.WriteLine($"Start index: {geometry.StartIndex}");
//                 //     System.Console.WriteLine($"Cluster index bit flag: {geometry.ClusterIndexBitFlag}");
//                 // }
//
//                 System.Console.WriteLine($"AABB Min: {mesh.Aabb.Min.X}, {mesh.Aabb.Min.Y}, {mesh.Aabb.Min.Z}");
//                 System.Console.WriteLine($"AABB Max: {mesh.Aabb.Max.X}, {mesh.Aabb.Max.Y}, {mesh.Aabb.Max.Z}");
//
//                 System.Console.WriteLine($"Flag: {mesh.Flag}");
//                 System.Console.WriteLine($"Flags: {mesh.Flags}");
//                 System.Console.WriteLine($"Instance Number: {mesh.InstanceNumber}");
//                 System.Console.WriteLine($"LodNear: {mesh.LodNear}");
//                 System.Console.WriteLine($"LodFar: {mesh.LodFar}");
//                 System.Console.WriteLine($"LodFade: {mesh.LodFade}");
//                 System.Console.WriteLine($"Breakable bone index: {mesh.BreakableBoneIndex}");
//                 System.Console.WriteLine($"Default Material Hash: {mesh.DefaultMaterialHash}");
//                 System.Console.WriteLine($"Draw priority offset: {mesh.DrawPriorityOffset}");
//                 System.Console.WriteLine($"Vertex layout type: {mesh.VertexLayoutType}");
//                 System.Console.WriteLine($"Low Lod Shadow Cascade No: {mesh.LowLodShadowCascadeNo}");
//                 System.Console.WriteLine($"Is Oriented BB: {mesh.IsOrientedBB}");
//                 System.Console.WriteLine($"Primitive Type: {mesh.PrimitiveType}");
//                 System.Console.WriteLine($"Unknown1: {mesh.Unknown1}");
//                 System.Console.WriteLine($"Unknown2: {mesh.Unknown2}");
//                 System.Console.WriteLine($"Unknown3: {mesh.Unknown3}");
//                 System.Console.WriteLine($"Unknown4: {mesh.Unknown4}");
//                 System.Console.WriteLine($"Unknown5: {mesh.Unknown5}");
//                 System.Console.WriteLine($"Unknown6: {mesh.Unknown6}");
//
//                 System.Console.WriteLine(
//                     $"OBB: {mesh.OrientedBB.XHalfExtent}, {mesh.OrientedBB.YHalfExtent}, {mesh.OrientedBB.ZHalfExtent}, {mesh.OrientedBB.Center}");
//             }
//         }
//         //
//         // foreach (var part in model.Parts)
//         // {
//         //     System.Console.WriteLine($"[OG Asset] Flags: {part.Flags}");
//         //     System.Console.WriteLine($"[OG Asset] Id: {part.Id}");
//         //     System.Console.WriteLine($"[OG Asset] Name: {part.Name}");
//         //     System.Console.WriteLine($"[OG Asset] Unknown: {part.Unknown}\n");
//         // }
//         //
//         // foreach (var part in mo.Parts)
//         // {
//         //     System.Console.WriteLine($"[MO Asset] Flags: {part.Flags}");
//         //     System.Console.WriteLine($"[MO Asset] Id: {part.Id}");
//         //     System.Console.WriteLine($"[MO Asset] Name: {part.Name}");
//         //     System.Console.WriteLine($"[MO Asset] Unknown: {part.Unknown}\n");
//         // }
//
//         System.Console.WriteLine(
//             $"Mesh count: {model.MeshObjects[0].Meshes.Count}, {model.MeshObjects[0].Meshes.Count}");
//         for (var i = 0; i < model.MeshObjects[0].Meshes.Count; i++)
//         {
//             //var moMesh = mo.MeshObjects[0].Meshes[i];
//             // var mesh = model.MeshObjects[0].Meshes[i];
//             //
//             // System.Console.WriteLine($"Mesh name: {mesh.Name}");
//             // foreach (var stream in mesh.VertexStreamDescriptions)
//             // {
//             //     System.Console.WriteLine($"Slot: {stream.Slot}");
//             //     System.Console.WriteLine($"Stride: {stream.Stride}");
//             //     System.Console.WriteLine($"Type: {stream.Type}");
//             //
//             //     foreach (var desc in stream.VertexElementDescriptions)
//             //     {
//             //         System.Console.WriteLine($"\tSemantic: {desc.Semantic}");
//             //         System.Console.WriteLine($"\tFormat: {desc.Format}");
//             //         System.Console.WriteLine($"\tOffset: {desc.Offset}");
//             //     }
//             // }
//
//             // System.Console.WriteLine($"Vertex count: {moMesh.VertexCount}, {mesh.VertexCount}");
//             // System.Console.WriteLine($"Face index count: {moMesh.FaceIndices.Length}, {mesh.FaceIndices.Length}");
//
//             // for (var j = 0; j < Math.Max(moMesh.FaceIndices.Length, mesh.FaceIndices.Length); j++)
//             // {
//             //     var moFaces = new[] {moMesh.FaceIndices[j, 0], moMesh.FaceIndices[j, 1], moMesh.FaceIndices[j, 2]};
//             //     var faces = new[] {mesh.FaceIndices[j, 0], mesh.FaceIndices[j, 1], mesh.FaceIndices[j, 2]};
//             //     System.Console.WriteLine($"MO: [{moFaces[0]}, {moFaces[1]}, {moFaces[2]}] O: [{faces[0]}, {faces[1]}, {faces[2]}]");
//             // }
//
//             // for (var j = 0; j < moMesh.WeightValues[0].Count; j++)
//             // {
//             //     var map1 = moMesh.WeightValues[0][j];
//             //     var map2 = moMesh.WeightValues[1][j];
//             //     var sum = map1.Sum(s => s) + map2.Sum(s => s);
//             //     System.Console.WriteLine(
//             //         $"[{map1[0]}, {map1[1]}, {map1[2]}, {map1[3]}], [{map2[0]}, {map2[1]}, {map2[2]}, {map2[3]}], Sum: {sum}");
//             // }
//
//             // for (var j = 0; j < Math.Max(moMesh.WeightIndices[0].Count, mesh.WeightIndices[0].Count); j++)
//             // {
//             //     var moWeight = moMesh.WeightValues[0][j];
//             //     var moIndex = moMesh.WeightIndices[0][j];
//             //     var weight = mesh.WeightValues[0][j];
//             //     var index = mesh.WeightIndices[0][j];
//             //     
//             //     System.Console.WriteLine($"Indices: [{moIndex[0]}, {moIndex[1]}, {moIndex[2]}, {moIndex[3]}], [{index[0]}, {index[1]}, {index[2]}, {index[3]}]");
//             //     System.Console.WriteLine($"Weights: [{moWeight[0]}, {moWeight[1]}, {moWeight[2]}, {moWeight[3]}], [{weight[0]}, {weight[1]}, {weight[2]}, {weight[3]}]");
//             // }
//         }
//     }
}