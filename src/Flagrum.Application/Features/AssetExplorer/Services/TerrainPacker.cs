using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DirectXTexNet;
using Flagrum.Abstractions;
using Flagrum.Core.Entities.Xml2;
using Flagrum.Core.Graphics.Terrain;
using Flagrum.Core.Graphics.Textures;
using Flagrum.Core.Graphics.Textures.DirectX;
using Flagrum.Application.Features.Shared;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Flagrum.Application.Services;

public class TerrainMetadata
{
    public string PrefabName { get; set; }
    public string Name { get; set; }
    public float[] Position { get; set; }
    public HeightMap HeightMap { get; set; }
}

public class TerrainPacker(
    ILogger<TerrainPacker> logger,
    IProfileService profile,
    TextureConverter textureConverter,
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

        var basePathTokens = outputPath.Split('\\')[..^1];
        var basePath = string.Join('\\', basePathTokens);
        var outputFileName = outputPath.Split('\\').Last();
        var outputFileNameWithoutExtension = outputFileName[..outputFileName.LastIndexOf('.')];
        _texturesDirectory = $"{basePath}\\{outputFileNameWithoutExtension}_terrain_textures";
        var hebDirectory = $@"{profile.GameDataDirectory}\environment\world\heightmaps";

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
            while (!File.Exists($@"{hebDirectory}\diffuse\{tile.Name}.{dimensions}.heb"))
            {
                dimensions /= 2;
                if (dimensions < 256)
                {
                    break;
                }
            }

            if (dimensions >= 256)
            {
                var diffuseHeb =
                    HeightEntityBinary.FromData(
                        File.ReadAllBytes($@"{hebDirectory}\diffuse\{tile.Name}.{dimensions}.heb"));
                var diffuse = HebToImages(diffuseHeb).FirstOrDefault();
                if (diffuse != null)
                {
                    File.WriteAllBytes($@"{GetTileDirectory(tile.Name)}\diffuse.{diffuse.Extension}",
                        ((HeightEntityBinaryImageData)diffuse).Data);
                }
            }

            dimensions = 1024;
            while (!File.Exists($@"{hebDirectory}\normal\{tile.Name}.{dimensions}.heb"))
            {
                dimensions /= 2;
                if (dimensions < 256)
                {
                    break;
                }
            }

            if (dimensions >= 256)
            {
                var normalHeb =
                    HeightEntityBinary.FromData(
                        File.ReadAllBytes($@"{hebDirectory}\normal\{tile.Name}.{dimensions}.heb"));
                var normal = HebToImages(normalHeb).FirstOrDefault();
                if (normal != null)
                {
                    File.WriteAllBytes($@"{GetTileDirectory(tile.Name)}\normal.{normal.Extension}",
                        ((HeightEntityBinaryImageData)normal).Data);
                }
            }

            var lodIndex = 0;
            while (!File.Exists($@"{hebDirectory}\lod0{lodIndex}\{tile.Name}.heb"))
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

            var lodHeb =
                HeightEntityBinary.FromData(File.ReadAllBytes($@"{hebDirectory}\lod0{lodIndex}\{tile.Name}.heb"));
            var textures = HebToImages(lodHeb, new[]
            {
                HeightEntityBinaryImageType.HEIGHT_MAP,
                HeightEntityBinaryImageType.MERGED_MASK_MAP,
                HeightEntityBinaryImageType.SLOPE_MAP
            });

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
                    File.WriteAllBytes($@"{GetTileDirectory(tile.Name)}\{name}.{texture.Extension}",
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
        var directory = $@"{baseDirectory}\common";
        var diffuse = $@"{baseDirectory}\common\diffuse";
        var displacement = $@"{baseDirectory}\common\displacement";
        var normal = $@"{baseDirectory}\common\normal";
        var hro = $@"{baseDirectory}\common\hro";

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
            if (!File.Exists($@"{diffuse}\{i}.tga"))
            {
                needsDiffuse = true;
            }

            if (!File.Exists($@"{displacement}\{i}.tga"))
            {
                needsDisplacement = true;
            }

            if (!File.Exists($@"{normal}\{i}.tga"))
            {
                needsNormal = true;
            }

            if (!File.Exists($@"{hro}\{i}.tga"))
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
        var targas = textureConverter.ToTargas(btex).ToList();

        for (var i = 0; i < targas.Count; i++)
        {
            File.WriteAllBytes($@"{outputDirectory}\{i}.tga", targas[i]);
        }
    }

    private string GetTileDirectory(string tileName)
    {
        var tileDirectory = $@"{_texturesDirectory}\{tileName}";
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
                                   new[] {0.0f, 0.0f, 0.0f, 0.0f};

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

    public IEnumerable<HeightEntityBinaryImageDataBase> HebToImages(HeightEntityBinary heb,
        IEnumerable<HeightEntityBinaryImageType> allowTypes = null)
    {
        const float magic = 0.000015259022f;
        allowTypes ??= Enum.GetValues<HeightEntityBinaryImageType>();

        foreach (var header in heb.ImageHeaders.Where(h => allowTypes.Contains(h.Type)))
        {
            if (header.Type == HeightEntityBinaryImageType.HEIGHT_MAP)
            {
                var buffer = new float[header.TextureWidth * header.TextureHeight];

                for (var i = 0; i < header.TextureWidth * header.TextureHeight; i++)
                {
                    var ushortValue = BitConverter.ToUInt16(header.DdsData, i * 2);
                    var value = ushortValue * magic * 4000f - 500f;
                    buffer[i] = value;
                }

                var heightMap = new HeightMap
                {
                    Width = header.TextureWidth,
                    Height = header.TextureHeight,
                    Altitudes = buffer
                };

                yield return new HeightMapData
                {
                    Index = heb.ImageHeaders.IndexOf(header),
                    Extension = "json",
                    Data = heightMap
                };
            }
            else if (header.TextureFormat > 0)
            {
                var dds = new DirectDrawSurface
                {
                    Height = header.TextureHeight,
                    Width = header.TextureWidth,
                    Pitch = header.TextureSizeBytes,
                    Depth = 1,
                    MipCount = header.MipCount > 0 ? header.MipCount : 1u,
                    Flags = DirectDrawSurfaceFlags.Texture | DirectDrawSurfaceFlags.Pitch |
                            DirectDrawSurfaceFlags.Depth | DirectDrawSurfaceFlags.MipMapCount,
                    Format = new DirectDrawSurfacePixelFormat(),
                    DirectX10Header = new DirectDrawSurfaceDirectX10Header
                    {
                        ArraySize = 1,
                        Format = TexturePixelFormatMap.Instance[header.TextureFormat],
                        ResourceDimension = 3
                    },
                    PixelData = header.DdsData
                };

                var ddsData = dds.Write();
                var pinnedData = GCHandle.Alloc(ddsData, GCHandleType.Pinned);
                var pointer = pinnedData.AddrOfPinnedObject();

                var image = TexHelper.Instance.LoadFromDDSMemory(pointer, ddsData.Length, DDS_FLAGS.NONE);

                pinnedData.Free();

                // This is required to prevent an access violation exception from DirectXTexNet
                // When converting a large number of textures at once
                GC.Collect();

                var metadata = image.GetMetadata();
                if (metadata.Format != DXGI_FORMAT.R8G8B8A8_UNORM)
                {
                    image = image.Decompress(DXGI_FORMAT.R8G8B8A8_UNORM);
                }

                using var stream = new MemoryStream();
                using var ddsStream = image.SaveToTGAMemory(0);
                //image.SaveToWICMemory(0, WIC_FLAGS.FORCE_SRGB, TexHelper.Instance.GetWICCodec(WICCodecs.PNG));
                ddsStream.CopyTo(stream);
                yield return new HeightEntityBinaryImageData
                {
                    Type = header.Type,
                    Index = heb.ImageHeaders.IndexOf(header),
                    Extension = "tga",
                    Data = stream.ToArray()
                };
            }
        }
    }
}