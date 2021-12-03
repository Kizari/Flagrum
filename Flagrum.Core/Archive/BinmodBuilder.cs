using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Flagrum.Gfxbin.Btex;
using Flagrum.Gfxbin.Gmdl;
using Flagrum.Gfxbin.Gmdl.Constructs;
using Flagrum.Gfxbin.Gmdl.Templates;
using Flagrum.Gfxbin.Gmtl;
using Newtonsoft.Json;

namespace Flagrum.Core.Archive;

public enum Boye
{
    Noctis,
    Prompto,
    Ignis,
    Gladiolus
}

public class BinmodBuilder
{
    private readonly Binmod _mod;
    private readonly Packer _packer;

    public BinmodBuilder(Binmod mod, byte[] previewImage)
    {
        _mod = mod;
        _packer = new Packer();
        _packer.AddFile(_mod.ToModmeta(), GetDataPath("index.modmeta"));

        var exml = EntityPackageBuilder.BuildExml(_mod.ModelName, _mod.ModDirectoryName, _mod.Target);
        _packer.AddFile(exml, "data://$mod/temp.exml");

        var tempFile = Path.GetTempFileName();
        var tempFile2 = Path.GetTempFileName();
        File.WriteAllBytes(tempFile, previewImage);
        BtexConverter.Convert(tempFile, tempFile2, BtexConverter.TextureType.Color);
        var btex = File.ReadAllBytes(tempFile2);

        _packer.AddFile(previewImage, GetDataPath("$preview.png.bin"));
        _packer.AddFile(btex, GetDataPath("$preview.btex"));
    }

    public void WriteToFile(string outPath)
    {
        _packer.WriteToFile(outPath);
    }

    public void AddFile(string uri, byte[] data)
    {
        _packer.AddFile(data, uri);
    }

    public void AddFmd(byte[] fmd)
    {
        using var memoryStream = new MemoryStream(fmd);
        using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read);

        var dataEntry = archive.GetEntry("data.json");
        using var dataStream = new MemoryStream();
        var stream = dataEntry.Open();
        stream.CopyTo(dataStream);

        var json = Encoding.UTF8.GetString(dataStream.ToArray());
        var gpubin = JsonConvert.DeserializeObject<Gpubin>(json);

        var gameAssets = new List<string>();

        foreach (var mesh in gpubin.Meshes)
        {
            foreach (var (textureId, filePath) in mesh.Material.Textures)
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    if (MaterialBuilder.DefaultTextures.TryGetValue(textureId, out var replacement))
                    {
                        mesh.Material.TextureData.Add(new TextureData
                        {
                            Id = textureId,
                            Uri = $"data://mod/{_mod.ModDirectoryName}/sourceimages/{replacement}",
                            Data = MaterialBuilder.GetDefaultTextureData(replacement)
                        });
                    }
                }
                else
                {
                    var query = $"{mesh.Name}/{textureId}";
                    var entry = archive.Entries.FirstOrDefault(e => e.FullName.Contains(query));
                    using var textureStream = new MemoryStream();
                    var tempStream = entry.Open();
                    tempStream.CopyTo(textureStream);

                    var fileName = filePath.Split('/', '\\').Last();
                    var extension = fileName.Split('.').Last();
                    string btexFileName;
                    byte[] btexData;

                    if (extension.ToLower() == "btex")
                    {
                        btexFileName = fileName;
                        btexData = textureStream.ToArray();
                    }
                    else
                    {
                        var tempPathOriginal = Path.GetTempFileName();
                        File.WriteAllBytes(tempPathOriginal, textureStream.ToArray());

                        BtexConverter.TextureType textureType;
                        if (textureId.Contains("normal", StringComparison.OrdinalIgnoreCase))
                        {
                            textureType = BtexConverter.TextureType.Normal;
                        }
                        else if (textureId.Contains("basecolor", StringComparison.OrdinalIgnoreCase) ||
                                 textureId.Contains("mrs", StringComparison.OrdinalIgnoreCase))
                        {
                            textureType = BtexConverter.TextureType.Color;
                        }
                        else
                        {
                            textureType = BtexConverter.TextureType.Greyscale;
                        }

                        var tempPath = Path.GetTempFileName();
                        BtexConverter.Convert(tempPathOriginal, tempPath, textureType);

                        btexData = File.ReadAllBytes(tempPath);
                        File.Delete(tempPathOriginal);
                        File.Delete(tempPath);

                        btexFileName = fileName.Replace($".{extension}", ".btex");
                    }

                    var uri = $"data://mod/{_mod.ModDirectoryName}/sourceimages/{btexFileName}";

                    mesh.Material.TextureData.Add(new TextureData
                    {
                        Id = textureId,
                        Uri = uri,
                        Data = btexData
                    });
                }
            }

            var material = MaterialBuilder.FromTemplate(
                mesh.Material.Id,
                $"{mesh.Name.ToLower()}_mat",
                _mod.ModDirectoryName,
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

            AddFile(material.Uri, materialWriter.Write());

            foreach (var texture in mesh.Material.TextureData)
            {
                AddFile(texture.Uri, texture.Data);
            }

            gameAssets.AddRange(material.ShaderBinaries.Select(s => s.Path));
            gameAssets.AddRange(material.Textures
                .Where(t => !t.Path.StartsWith("data://mod"))
                .Select(t => t.Path));
        }

        var model = OutfitTemplate.Build(_mod.ModDirectoryName, _mod.ModelName, gpubin);
        var replacer = new ModelReplacer(model, gpubin, _mod.Target.ToString());
        model = replacer.Replace();
        var writer = new ModelWriter(model);
        var (gfxData, gpuData) = writer.Write();

        AddFile(GetDataPath($"{_mod.ModelName}.gmdl.gfxbin"), gfxData);
        AddFile(GetDataPath($"{_mod.ModelName}.gpubin"), gpuData);

        AddGameAssets(gameAssets.Distinct());
    }

    /// <summary>
    ///     Add a copy of a game asset to the archive
    ///     Asset will be read from the EARC and copied to the archive
    /// </summary>
    public void AddGameAssets(IEnumerable<string> paths)
    {
        var dataDirectory = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Final Fantasy XV\\datas\\";
        var archiveDictionary = new Dictionary<string, List<string>>();

        foreach (var uri in paths)
        {
            var path = uri.Replace("data://", dataDirectory).Replace('/', '\\');
            var fileName = path.Split('\\').Last();
            path = path.Replace(fileName, "autoexternal.earc");

            while (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                var newPath = "";
                var tokens = path.Split('\\');
                for (var i = 0; i < tokens.Length - 2; i++)
                {
                    newPath += tokens[i];

                    if (i != tokens.Length - 3)
                    {
                        newPath += '\\';
                    }
                }

                path = newPath + "\\autoexternal.earc";
            }

            if (!archiveDictionary.ContainsKey(path))
            {
                archiveDictionary.Add(path, new List<string>());
            }

            archiveDictionary[path].Add(uri);
        }

        foreach (var (archivePath, uriList) in archiveDictionary)
        {
            var unpacker = new Unpacker(archivePath);

            foreach (var uri in uriList)
            {
                var fileData = unpacker.UnpackFileByQuery(uri);
                if (fileData.Length == 0)
                {
                    throw new InvalidOperationException($"URI {uri} must exist in game files!");
                }

                _packer.AddFile(fileData, uri);
            }
        }
    }

    private string GetDataPath(string relativePath)
    {
        return $"data://mod/{_mod.ModDirectoryName}/{relativePath}";
    }
}