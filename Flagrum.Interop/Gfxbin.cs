// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Runtime.InteropServices;
// using Flagrum.Archiver.Binmod;
// using Flagrum.Gfxbin.Btex;
// using Flagrum.Gfxbin.Gmdl;
// using Flagrum.Gfxbin.Gmdl.Constructs;
// using Flagrum.Gfxbin.Gmdl.Templates;
// using Flagrum.Gfxbin.Gmtl;
// using Newtonsoft.Json;
//
// namespace Flagrum.Interop;
//
// public static class Gfxbin
// {
//     [UnmanagedCallersOnly]
//     public static IntPtr Import(IntPtr pathPointer)
//     {
//         var gfxbinPath = Marshal.PtrToStringUni(pathPointer);
//         var gpubinPath = gfxbinPath.Replace(".gmdl.gfxbin", ".gpubin");
//         var gfxbin = File.ReadAllBytes(gfxbinPath);
//         var gpubin = File.ReadAllBytes(gpubinPath);
//         var reader = new ModelReader(gfxbin, gpubin);
//         var model = reader.Read();
//
//         Dictionary<int, string> boneTable;
//         if (model.BoneHeaders.Count(b => b.UniqueIndex == ushort.MaxValue) > 1)
//         {
//             // Probably a broken MO gfxbin with all IDs set to this value
//             var arbitraryIndex = 0;
//             boneTable = model.BoneHeaders.ToDictionary(b => arbitraryIndex++, b => b.Name);
//         }
//         else
//         {
//             boneTable = model.BoneHeaders.ToDictionary(b => (int)b.UniqueIndex, b => b.Name);
//         }
//
//         var meshData = new Gpubin
//         {
//             BoneTable = boneTable,
//             Meshes = model.MeshObjects.SelectMany(o => o.Meshes
//                 .Where(m => m.LodNear == 0)
//                 .Select(m => new GpubinMesh
//                 {
//                     Name = m.Name,
//                     FaceIndices = m.FaceIndices,
//                     VertexPositions = m.VertexPositions,
//                     Normals = m.Normals,
//                     UVMaps = m.UVMaps.Select(m => new UVMap32
//                     {
//                         UVs = m.UVs.Select(uv => new UV32
//                         {
//                             U = (float)uv.U,
//                             V = (float)uv.V
//                         }).ToList()
//                     }).ToList(),
//                     WeightIndices = m.WeightIndices,
//                     WeightValues = m.WeightValues.Select(n => n.Select(o => o.Select(p => (int)p).ToArray()).ToList())
//                         .ToList()
//                 }))
//         };
//
//         var json = JsonConvert.SerializeObject(meshData);
//         var temp = Path.GetTempFileName();
//         File.WriteAllText(temp, json);
//
//         var pointer = Marshal.StringToHGlobalAnsi(temp);
//         return pointer;
//     }
//
//     [UnmanagedCallersOnly]
//     public static IntPtr Export(IntPtr pathPointer)
//     {
//         var jsonPath = Marshal.PtrToStringUni(pathPointer);
//         var json = File.ReadAllText(jsonPath);
//         var gpubin = JsonConvert.DeserializeObject<Gpubin>(json);
//         //File.Delete(jsonPath);
//
//         var exportPath = jsonPath.Replace(".json", "");
//         var gameAssets = new List<string>();
//         var modName = jsonPath.Split('/', '\\').Last().Split('.')[0];
//         var builder = new BinmodBuilder(modName, exportPath.Replace(".ffxvbinmod", ""));
//
//         foreach (var mesh in gpubin.Meshes)
//         {
//             foreach (var (textureId, filePath) in mesh.Material.Textures)
//             {
//                 var tempPath = Path.GetTempFileName();
//                 BtexConverter.Convert(filePath, tempPath,
//                     textureId.ToLower().Contains("normal")
//                         ? BtexConverter.TextureType.Normal
//                         : BtexConverter.TextureType.Color);
//                 var btexData = File.ReadAllBytes(tempPath);
//                 File.Delete(tempPath);
//                 var fileName = filePath.Split('/', '\\').Last();
//                 var extension = fileName.Split('.').Last();
//                 var btexFileName = fileName.Replace($".{extension}", ".btex");
//                 var uri = $"data://mod/{modName}/{btexFileName}";
//
//                 mesh.Material.TextureData.Add(new TextureData
//                 {
//                     Id = textureId,
//                     Uri = uri,
//                     Data = btexData
//                 });
//             }
//
//             var material = MaterialBuilder.FromTemplate(
//                 mesh.Material.Name,
//                 mesh.Material.Inputs.Select(p => new MaterialInputData
//                 {
//                     Name = p.Key,
//                     Values = p.Value
//                 }).ToList(),
//                 mesh.Material.TextureData.Select(p => new MaterialTextureData
//                 {
//                     Name = p.Id,
//                     Path = p.Uri
//                 }).ToList());
//
//             var materialWriter = new MaterialWriter(material);
//             builder.AddMaterial(material.Name, materialWriter.Write());
//
//             foreach (var texture in mesh.Material.TextureData)
//             {
//                 builder.AddFile(texture.Uri, texture.Data);
//             }
//
//             gameAssets.AddRange(material.ShaderBinaries.Select(s => s.Path));
//             gameAssets.AddRange(material.Textures
//                 .Where(t => t.Path.StartsWith("data://shader"))
//                 .Select(t => t.Path));
//         }
//
//         var model = OutfitTemplate.Build(exportPath, gpubin);
//         var replacer = new ModelReplacer(model, gpubin);
//         model = replacer.Replace();
//         var writer = new ModelWriter(model);
//         var (gfxData, gpuData) = writer.Write();
//
//         builder.AddModel(model.Name, gfxData, gpuData);
//         builder.AddGameAssets(gameAssets.Distinct());
//         builder.WriteToFile(exportPath);
//
//         var pointer = Marshal.StringToHGlobalAnsi("Success!");
//         return pointer;
//     }
// }