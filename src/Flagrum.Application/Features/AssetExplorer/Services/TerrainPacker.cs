using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Flagrum.Abstractions;
using Flagrum.Core.Entities.Xml2;
using Flagrum.Core.Graphics.Terrain;
using Flagrum.Core.Graphics.Textures;
using Flagrum.Core.Graphics.Textures.Luminous;
using Flagrum.Core.Graphics.Textures.Shared;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SixLabors.ImageSharp.Formats.Tga;

namespace Flagrum.Application.Services;

public class TerrainMetadata
{
    public string PrefabName { get; set; }
    public string Name { get; set; }
    public float[] Position { get; set; }
    public HeightMap HeightMap { get; set; }
}

// TODO: A lot of file paths that are not platform-agnostic in this file
public class TerrainPacker(
    ILogger<TerrainPacker> logger,
    IProfileService profile,
    AppStateService appState)
{
    private readonly ConcurrentBag<TerrainMetadata> _tiles = [];

    private string _texturesDirectory;

    public void Pack(string uri, string outputPath)
    {
        // Need to set to invariant culture as some cultures don't handle the
        // exponent portion when parsing floats
        var previousCulture = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        var basePathTokens = outputPath.Split(Path.DirectorySeparatorChar)[..^1];
        var basePath = string.Join(Path.DirectorySeparatorChar, basePathTokens);
        var outputFileName = outputPath.Split(Path.DirectorySeparatorChar).Last();
        var outputFileNameWithoutExtension = outputFileName[..outputFileName.LastIndexOf('.')];
        _texturesDirectory = Path.Combine(basePath, $"{outputFileNameWithoutExtension}_terrain_textures");
        var hebDirectory = Path.Combine(profile.GameDataDirectory, "environment", "world", "heightmaps");

        if (!Directory.Exists(_texturesDirectory))
        {
            Directory.CreateDirectory(_texturesDirectory);
        }

        // Export the texture arrays if they aren't present
        ExportTerrainTextures(basePath);

        GetPathsRecursively(uri, appState.GetFileByUri(uri));

        // Can't use multithreading here due to an issue where DirectXTexNet hits
        // an access violation exception because we can't clear the memory quickly enough
        foreach (var tile in _tiles)
        {
            var dimensions = 1024;
            while (!File.Exists(Path.Combine(hebDirectory, "diffuse", $"{tile.Name}.{dimensions}.heb")))
            {
                dimensions /= 2;
                if (dimensions < 256)
                {
                    break;
                }
            }

            if (dimensions >= 256)
            {
                var diffuseHeb = new HeightEntityBinary(
                    File.ReadAllBytes(Path.Combine(hebDirectory, "diffuse", $"{tile.Name}.{dimensions}.heb")));
                var diffuse = HebToImages(diffuseHeb).FirstOrDefault();
                if (diffuse != null)
                {
                    File.WriteAllBytes(Path.Combine(GetTileDirectory(tile.Name), $"diffuse.{diffuse.Extension}"),
                        ((HeightEntityBinaryImageData)diffuse).Data);
                }
            }

            dimensions = 1024;
            while (!File.Exists(Path.Combine(hebDirectory, "normal", $"{tile.Name}.{dimensions}.heb")))
            {
                dimensions /= 2;
                if (dimensions < 256)
                {
                    break;
                }
            }

            if (dimensions >= 256)
            {
                var normalHeb = new HeightEntityBinary(
                    File.ReadAllBytes(Path.Combine(hebDirectory, "normal", $"{tile.Name}.{dimensions}.heb")));
                var normal = HebToImages(normalHeb).FirstOrDefault();
                if (normal != null)
                {
                    File.WriteAllBytes(Path.Combine(GetTileDirectory(tile.Name), $"normal.{normal.Extension}"),
                        ((HeightEntityBinaryImageData)normal).Data);
                }
            }

            var lodIndex = 0;
            while (!File.Exists(Path.Combine(hebDirectory, $"lod0{lodIndex}", $"{tile.Name}.heb")))
            {
                lodIndex++;
                if (lodIndex > 6)
                {
                    break;
                }
            }

            if (lodIndex > 6)
            {
                continue;
            }

            var lodHeb = new HeightEntityBinary(File.ReadAllBytes(
                Path.Combine(hebDirectory, $"lod0{lodIndex}", $"{tile.Name}.heb")));
            var textures = HebToImages(lodHeb, [
                HeightEntityBinaryImageType.HEIGHT_MAP,
                HeightEntityBinaryImageType.MERGED_MASK_MAP,
                HeightEntityBinaryImageType.SLOPE_MAP
            ]);

            foreach (var texture in textures)
            {
                if (texture.Extension == "json")
                {
                    var data = ((HeightMapData)texture).Data;
                    if (tile.HeightMap == null || tile.HeightMap.Width < data.Width)
                    {
                        tile.HeightMap = data;
                    }
                }
                else
                {
                    var name = texture.Type == HeightEntityBinaryImageType.MERGED_MASK_MAP
                        ? "merged_mask_map"
                        : "slope_map";
                    File.WriteAllBytes(Path.Combine(GetTileDirectory(tile.Name), $"{name}.{texture.Extension}"),
                        ((HeightEntityBinaryImageData)texture).Data);
                }
            }
        }

        File.WriteAllText(outputPath, JsonConvert.SerializeObject(_tiles));

        _tiles.Clear();

        Thread.CurrentThread.CurrentCulture = previousCulture;
    }

    private void ExportTerrainTextures(string baseDirectory)
    {
        var directory = Path.Combine(baseDirectory, "common");
        var diffuse = Path.Combine(directory, "diffuse");
        var displacement = Path.Combine(directory, "displacement");
        var normal = Path.Combine(directory, "normal");
        var hro = Path.Combine(baseDirectory, "common", "hro");

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (!Directory.Exists(diffuse))
        {
            Directory.CreateDirectory(diffuse);
        }

        if (!Directory.Exists(displacement))
        {
            Directory.CreateDirectory(displacement);
        }

        if (!Directory.Exists(normal))
        {
            Directory.CreateDirectory(normal);
        }

        if (!Directory.Exists(hro))
        {
            Directory.CreateDirectory(hro);
        }

        var needsDiffuse = false;
        var needsDisplacement = false;
        var needsNormal = false;
        var needsHro = false;
        for (var i = 0; i < 26; i++)
        {
            if (!File.Exists(Path.Combine(diffuse, $"{i}.tga")))
            {
                needsDiffuse = true;
            }

            if (!File.Exists(Path.Combine(displacement, $"{i}.tga")))
            {
                needsDisplacement = true;
            }

            if (!File.Exists(Path.Combine(normal, $"{i}.tga")))
            {
                needsNormal = true;
            }

            if (!File.Exists(Path.Combine(hro, $"{i}.tga")))
            {
                needsHro = true;
            }
        }

        if (needsDiffuse)
        {
            ExportTextureArray(diffuse, "data://environment/world/sourceimages/terrainarraytex_00_b.tif");
        }

        if (needsDisplacement)
        {
            ExportTextureArray(displacement,
                "data://environment/world/sourceimages/terrainarraytex_displacement/terrainarraytex_00_h.png");
        }

        if (needsNormal)
        {
            ExportTextureArray(normal, "data://environment/world/sourceimages/terrainarraytex_00_n.tif");
        }

        if (needsHro)
        {
            ExportTextureArray(hro, "data://environment/world/sourceimages/terrainarraytex_00_hro.tif");
        }
    }

    private void ExportTextureArray(string outputDirectory, string uri)
    {
        var btex = appState.GetFileByUri(uri);
        var texture = new BlackTexture(btex);
        texture.Save($"{outputDirectory}.tga", ImageFileFormat.Targa, (i, _) => $"{i}.tga");
    }

    private string GetTileDirectory(string tileName)
    {
        var tileDirectory = Path.Combine(_texturesDirectory, tileName);
        if (!Directory.Exists(tileDirectory))
        {
            Directory.CreateDirectory(tileDirectory);
        }

        return tileDirectory;
    }

    private void GetPathsRecursively(string uri, byte[] xmb2)
    {
        using var stream = new MemoryStream(xmb2);
        var package = XmlBinary2Document.GetRootElement(stream);
        var objects = package.GetElementByName("objects");

        var elements = objects.GetElements();
        Parallel.For(0, elements.Count, counter =>
        {
            var element = elements.ElementAt(counter);
            var typeAttribute = element.GetAttributeByName("type").GetTextValue();

            if (typeAttribute == "Black.Entity.Actor.HeightFieldEntity")
            {
                try
                {
                    var name = element.GetAttributeByName("name").GetTextValue();
                    var position = element.GetElementByName("position_")?.GetFloat4Value() ??
                                   [0.0f, 0.0f, 0.0f, 0.0f];

                    var prefabFileName = uri.Split('\\', '/').Last();
                    _tiles.Add(new TerrainMetadata
                    {
                        Name = name,
                        Position = position[..3],
                        PrefabName = prefabFileName
                    });
                }
                catch
                {
                    var path = element.GetElementByName("sourcePath_")?.GetTextValue();
                    logger.LogInformation("Failed to handle model node with sourcePath {Path} from ebex {Uri}",
                        path ?? "NONE",
                        uri);
                }
            }
            else if (typeAttribute == "SQEX.Ebony.Framework.Entity.EntityPackageReference")
            {
                var path = element.GetElementByName("sourcePath_");
                var relativeUri = path.GetTextValue();
                var uriUri = new Uri(uri.Replace("data://", "data://data/"));
                var combinedUri = new Uri(uriUri, relativeUri);
                var combinedUriString = combinedUri.ToString().Replace("data://data/", "data://");
                var innerXmb2 = appState.GetFileByUri(combinedUriString);

                if (innerXmb2.Length > 0)
                {
                    GetPathsRecursively(combinedUriString, innerXmb2);
                }
                else
                {
                    logger.LogInformation("Failed to load entity package from {Uri} at path {SourcePath}", uri,
                        path.GetTextValue());
                }
            }
        });
    }

    public List<HeightEntityBinaryImageDataBase> HebToImages(HeightEntityBinary heb,
        HashSet<HeightEntityBinaryImageType>? allowTypes = null)
    {
        const float magic = 0.000015259022f;
        allowTypes ??= [..Enum.GetValues<HeightEntityBinaryImageType>()];

        var results = new List<HeightEntityBinaryImageDataBase>();
        for (var index = 0; index < heb.Images.Length; index++)
        {
            var header = heb.Images[index];
            if (!allowTypes.Contains(header.Type))
            {
                continue;
            }

            if (header.Type == HeightEntityBinaryImageType.HEIGHT_MAP)
            {
                var buffer = new float[header.Width * header.Height];
                var ddsData = heb.GetPixelData(index);
                var ddsSpan = MemoryMarshal.Cast<byte, ushort>(ddsData);

                for (var i = 0; i < header.Width * header.Height; i++)
                {
                    buffer[i] = ddsSpan[i] * magic * 4000f - 500f;
                }

                var heightMap = new HeightMap
                {
                    Width = header.Width,
                    Height = header.Height,
                    Altitudes = buffer
                };

                results.Add(new HeightMapData
                {
                    Index = index,
                    Extension = "json",
                    Data = heightMap
                });
            }
            else if (header.Format > 0)
            {
                results.Add(new HeightEntityBinaryImageData
                {
                    Type = header.Type,
                    Index = index,
                    Extension = "tga",
                    Data = heb.Convert(index, new TgaEncoder())
                });
            }
        }

        return results;
    }
}