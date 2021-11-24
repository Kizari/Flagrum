using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Flagrum.Archiver.Binmod;
using Flagrum.Gfxbin.Btex;
using Flagrum.Gfxbin.Gmdl;
using Flagrum.Gfxbin.Gmdl.Constructs;
using Flagrum.Gfxbin.Gmdl.Templates;
using Flagrum.Gfxbin.Gmtl;
using Newtonsoft.Json;

namespace Flagrum.Console;

public static class InteropTests
{
    public static void Test()
    {
        var baseDir = "C:\\Testing\\Gfxbin";
        var exml = $"{baseDir}\\$mod\\noctis_custom_2\\temp.exml";
        var modDir = $"{baseDir}\\mod\\noctis_custom_2";
        
        var jsonPath = "C:\\Users\\Kieran\\Desktop\\Mods\\Noctis3\\noctis_custom_2.ffxvbinmod.json";
        var json = File.ReadAllText(jsonPath);
        var gpubin = JsonConvert.DeserializeObject<Gpubin>(json);

        var builder = new BinmodBuilder("noctis_custom_2", "de81d8a4-53d8-4ca9-bcf0-f9397e82db81");
        //builder.AddFile("data://$mod/temp.exml", File.ReadAllBytes(exml));
        var modName = "noctis_custom_2";
        var gameAssets = new List<string>();
        
        foreach (var mesh in gpubin.Meshes)
        {
            foreach (var (textureId, filePath) in mesh.Material.Textures)
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    continue;    
                }
                
                var tempPath = Path.GetTempFileName();
                BtexConverter.Convert(filePath, tempPath,
                    textureId.ToLower().Contains("normal")
                        ? BtexConverter.TextureType.Normal
                        : BtexConverter.TextureType.Color);
                Thread.Sleep(100);
                var btexData = File.ReadAllBytes(tempPath);
                File.Delete(tempPath);
                var fileName = filePath.Split('/', '\\').Last();
                var extension = fileName.Split('.').Last();
                var btexFileName = fileName.Replace($".{extension}", ".btex");
                var uri = $"data://mod/{modName}/{btexFileName}";

                mesh.Material.TextureData.Add(new TextureData
                {
                    Id = textureId,
                    Uri = uri,
                    Data = btexData
                });
            }
            
            var name = mesh.Name.ToLower() switch
            {
                "shirtshape" => "shirt",
                "bootsshape" => "boots",
                "bodyshape" => "arm",
                "pantsshape" => "pants",
                "buttonshape" => "button",
                _ => "ERROR"
            };

            var material = MaterialBuilder.FromTemplate(
                mesh.Material.Id,
                name,
                $"data://mod/noctis_custom_2/main.fbxgmtl/{name}.gmtl.gfxbin",
                mesh.Material.Inputs.Select(p => new MaterialInputData
                {
                    Name = p.Key,
                    Values = p.Value
                }).ToList(),
                mesh.Material.TextureData.Select(p => new MaterialTextureData
                {
                    Name = p.Id,
                    Path = p.Uri
                }).ToList());

            var materialWriter = new MaterialWriter(material);
            
            builder.AddFile($"data://mod/noctis_custom_2/main.fbxgmtl/{name}.gmtl.gfxbin", materialWriter.Write());

            foreach (var texture in mesh.Material.TextureData)
            {
                builder.AddFile(texture.Uri, texture.Data);
            }

            gameAssets.AddRange(material.ShaderBinaries.Select(s => s.Path));
            gameAssets.AddRange(material.Textures
                .Where(t => !t.Path.StartsWith("data://mod"))
                .Select(t => t.Path));
        }

        // foreach (var file in Directory.EnumerateFiles(modDir))
        // {
        //     if (file.EndsWith(".btex") && !file.Contains("$preview"))
        //     {
        //         continue;
        //     }
        //
        //     if (file.EndsWith(".gmdl.gfxbin") || file.EndsWith(".gpubin"))
        //     {
        //         continue;
        //     }
        //     
        //     System.Console.WriteLine(file);
        //     
        //     builder.AddFile(file
        //         .Replace(modDir, "data://mod/noctis_custom_2")
        //         .Replace('\\', '/'),
        //         File.ReadAllBytes(file));
        // }

        // var dependencies = new List<string>();
        // foreach (var file in Directory.EnumerateFiles($"{modDir}\\main.fbxgmtl"))
        // {
        //     var materialBytes = File.ReadAllBytes(file);
        //     builder.AddFile(file
        //             .Replace(modDir, "data://mod/noctis_custom_2")
        //             .Replace('\\', '/'),
        //         materialBytes);
        //
        //     var reader = new MaterialReader(file);
        //     var material = reader.Read();
        //     
        //     dependencies.AddRange(material.ShaderBinaries.Select(s => s.Path));
        //     dependencies.AddRange(material.Textures
        //         .Where(s => !s.Path.StartsWith("data://mod"))
        //         .Select(s => s.Path));
        // }

        // foreach (var uri in dependencies.Distinct())
        // {
        //     var path = uri.Replace("data://shader", "C:\\Testing\\Gfxbin\\shader")
        //         .Replace(".tif", ".btex")
        //         .Replace(".tga", ".btex");
        //     builder.AddFile(uri.Replace(".tga", ".btex").Replace(".tif", ".btex"), File.ReadAllBytes(path));
        // }

        var moGfxbin = File.ReadAllBytes($"{modDir}\\main.gmdl.gfxbin");
        var moGpubin = File.ReadAllBytes($"{modDir}\\main.gpubin");
        var moReader = new ModelReader(moGfxbin, moGpubin);
        var moModel = moReader.Read();
        
        var model = OutfitTemplate.Build("test\\noctis_custom_2.ffxvbinmod", gpubin);
        //model.Header = moModel.Header;
        var replacer = new ModelReplacer(model, gpubin);
        model = replacer.Replace();
        var writer = new ModelWriter(model);
        var (gfxData, gpuData) = writer.Write();
        builder.AddModel("main", gfxData, gpuData);
        
        builder.AddGameAssets(gameAssets.Distinct());
        builder.WriteToFile("C:\\Testing\\de81d8a4-53d8-4ca9-bcf0-f9397e82db81.ffxvbinmod");
    }
    
    public static void Export()
    {
        var jsonPath = "C:\\Users\\Kieran\\Desktop\\Mods\\Noctis3\\noctis_custom_2.ffxvbinmod.json";
        var json = File.ReadAllText(jsonPath);
        var gpubin = JsonConvert.DeserializeObject<Gpubin>(json);
        //File.Delete(jsonPath);

        var exportPath = jsonPath.Replace(".json", "");
        var gameAssets = new List<string>();

        var modName = jsonPath.Split('/', '\\').Last().Split('.')[0];
        var builder = new BinmodBuilder(modName, "de81d8a4-53d8-4ca9-bcf0-f9397e82db81");

        foreach (var mesh in gpubin.Meshes)
        {
            foreach (var (textureId, filePath) in mesh.Material.Textures)
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    continue;    
                }
                
                var tempPath = Path.GetTempFileName();
                BtexConverter.Convert(filePath, tempPath,
                    textureId.ToLower().Contains("normal")
                        ? BtexConverter.TextureType.Normal
                        : BtexConverter.TextureType.Color);
                Thread.Sleep(100);
                var btexData = File.ReadAllBytes(tempPath);
                File.Delete(tempPath);
                var fileName = filePath.Split('/', '\\').Last();
                var extension = fileName.Split('.').Last();
                var btexFileName = fileName.Replace($".{extension}", ".btex");
                var uri = $"data://mod/{modName}/{btexFileName}";

                mesh.Material.TextureData.Add(new TextureData
                {
                    Id = textureId,
                    Uri = uri,
                    Data = btexData
                });
            }

            var material = MaterialBuilder.FromTemplate(
                mesh.Material.Id,
                mesh.Name,
                $"data://mod/{modName}/materials/{mesh.Name}.gmtl.gfxbin",
                mesh.Material.Inputs.Select(p => new MaterialInputData
                {
                    Name = p.Key,
                    Values = p.Value
                }).ToList(),
                mesh.Material.TextureData.Select(p => new MaterialTextureData
                {
                    Name = p.Id,
                    Path = p.Uri
                }).ToList());

            var materialWriter = new MaterialWriter(material);
            builder.AddMaterial(material.Name, materialWriter.Write());

            foreach (var texture in mesh.Material.TextureData)
            {
                builder.AddFile(texture.Uri, texture.Data);
            }

            gameAssets.AddRange(material.ShaderBinaries.Select(s => s.Path));
            gameAssets.AddRange(material.Textures
                .Where(t => !t.Path.StartsWith("data://mod"))
                .Select(t => t.Path));
        }

        var model = OutfitTemplate.Build(exportPath, gpubin);
        var replacer = new ModelReplacer(model, gpubin);
        model = replacer.Replace();
        var writer = new ModelWriter(model);
        var (gfxData, gpuData) = writer.Write();

        builder.AddModel(model.Name, gfxData, gpuData);
        builder.AddGameAssets(gameAssets.Distinct());
        builder.WriteToFile("C:\\Testing\\de81d8a4-53d8-4ca9-bcf0-f9397e82db81.ffxvbinmod");
    }
}