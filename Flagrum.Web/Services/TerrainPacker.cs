using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Black.Entity.Actor;
using DirectXTexNet;
using Flagrum.Core.Ebex.Xmb2;
using Flagrum.Core.Gfxbin.Btex;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Flagrum.Web.Services;

public class TerrainMetadata
{
    public string PrefabName { get; set; }
    public string Name { get; set; }
    public float[] Position { get; set; }
    public HebHeightMap HeightMap { get; set; }
}

public class TerrainPacker
{
    private readonly ILogger<TerrainPacker> _logger;
    private readonly SettingsService _settings;

    private readonly ConcurrentBag<TerrainMetadata> _tiles = new();

    public TerrainPacker(
        ILogger<TerrainPacker> logger,
        SettingsService settings)
    {
        _logger = logger;
        _settings = settings;
    }

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
        var texturesDirectory = $"{basePath}\\{outputFileNameWithoutExtension}_terrain_textures";
        var hebDirectory = $@"{_settings.GameDataDirectory}\environment\world\heightmaps";

        if (!Directory.Exists(texturesDirectory))
        {
            Directory.CreateDirectory(texturesDirectory);
        }

        // This instance is needed because the injected one was rarely causing a concurrency exception for some reason
        using var outerContext = new FlagrumDbContext(_settings);

        GetPathsRecursively(uri, outerContext.GetFileByUri(uri));

        // Can't use multithreading here due to an issue where DirectXTexNet hits
        // an access violation exception because we can't clear the memory quickly enough
        foreach (var tile in _tiles)
        {
            var tileDirectory = $@"{texturesDirectory}\{tile.Name}";
            if (!Directory.Exists(tileDirectory))
            {
                Directory.CreateDirectory(tileDirectory);
            }

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
                    HebHeader.FromData(File.ReadAllBytes($@"{hebDirectory}\diffuse\{tile.Name}.{dimensions}.heb"));
                var diffuse = HebToImages(diffuseHeb).First();
                File.WriteAllBytes($@"{tileDirectory}\diffuse.{diffuse.Extension}", ((HebImageData)diffuse).Data);
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
                    HebHeader.FromData(File.ReadAllBytes($@"{hebDirectory}\normal\{tile.Name}.{dimensions}.heb"));
                var normal = HebToImages(normalHeb).First();
                File.WriteAllBytes($@"{tileDirectory}\normal.{normal.Extension}", ((HebImageData)normal).Data);
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

            var lodHeb = HebHeader.FromData(File.ReadAllBytes($@"{hebDirectory}\lod0{lodIndex}\{tile.Name}.heb"));
            var textures = HebToImages(lodHeb);

            foreach (var texture in textures)
            {
                if (texture.Extension == "json")
                {
                    var data = ((HebHeightMapData)texture).Data;
                    if (tile.HeightMap == null || tile.HeightMap.Width < data.Width)
                    {
                        tile.HeightMap = data;
                    }
                }
                else
                {
                    File.WriteAllBytes($@"{tileDirectory}\{texture.Index}.{texture.Extension}",
                        ((HebImageData)texture).Data);
                }
            }
        }

        File.WriteAllText(outputPath, JsonConvert.SerializeObject(_tiles));

        _tiles.Clear();

        Thread.CurrentThread.CurrentCulture = previousCulture;
    }

    private void GetPathsRecursively(string uri, byte[] xmb2)
    {
        using var stream = new MemoryStream(xmb2);
        var package = Xmb2Document.GetRootElement(stream);
        var objects = package.GetElementByName("objects");

        var elements = objects.GetElements();
        Parallel.For(0, elements.Count, counter =>
        {
            var element = elements.ElementAt(counter);

            using var context = new FlagrumDbContext(_settings);
            var typeAttribute = element.GetAttributeByName("type").GetTextValue();

            var type = Assembly.GetExecutingAssembly().GetTypes()
                .FirstOrDefault(t => t.FullName?.Contains(typeAttribute) == true);

            if (type?.IsAssignableTo(typeof(HeightFieldEntity)) == true)
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
                    _logger.LogInformation("Failed to handle model node with sourcePath {Path} from ebex {Uri}",
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
                var innerXmb2 = context.GetFileByUri(combinedUriString);

                if (innerXmb2.Length > 0)
                {
                    GetPathsRecursively(combinedUriString, innerXmb2);
                }
                else
                {
                    _logger.LogInformation("Failed to load entity package from {Uri} at path {SourcePath}", uri,
                        path.GetTextValue());
                }
            }
        });
    }

    private IEnumerable<HebImageDataBase> HebToImages(HebHeader heb)
    {
        const float magic = 0.000015259022f;

        foreach (var header in heb.ImageHeaders)
        {
            if (header.Type == HebImageType.TYPE_HEIGHT_MAP)
            {
                var buffer = new float[header.TextureWidth * header.TextureHeight];

                for (var i = 0; i < header.TextureWidth * header.TextureHeight; i++)
                {
                    var ushortValue = BitConverter.ToUInt16(header.DdsData, i * 2);
                    var value = ushortValue * magic * 4000f - 500f;
                    buffer[i] = value;
                }

                var heightMap = new HebHeightMap
                {
                    Width = header.TextureWidth,
                    Height = header.TextureHeight,
                    Altitudes = buffer
                };

                yield return new HebHeightMapData
                {
                    Index = heb.ImageHeaders.IndexOf(header),
                    Extension = "json",
                    Data = heightMap
                };
            }
            else
            {
                var ddsHeader = new DdsHeader
                {
                    Height = header.TextureHeight,
                    Width = header.TextureWidth,
                    PitchOrLinearSize = header.TextureSizeBytes,
                    Depth = 1,
                    MipMapCount = header.MipCount > 0 ? header.MipCount : 1u,
                    Flags = DDSFlags.Texture | DDSFlags.Pitch | DDSFlags.Depth | DDSFlags.MipMapCount,
                    PixelFormat = new PixelFormat(),
                    DX10Header = new DX10
                    {
                        ArraySize = 1,
                        Format = BtexConverter.BtexFormatToDX10Format(header.TextureFormat),
                        ResourceDimension = 3
                    }
                };

                var ddsData = BtexConverter.WriteDds(ddsHeader, header.DdsData);
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
                yield return new HebImageData
                {
                    Index = heb.ImageHeaders.IndexOf(header),
                    Extension = "tga",
                    Data = stream.ToArray()
                };
            }
        }
    }
}