using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Flagrum.Core.Archive;
using Flagrum.Core.Ebex.Xmb2;
using Flagrum.Core.Gfxbin.Gmdl;
using Flagrum.Core.Gfxbin.Gmdl.Constructs;
using Flagrum.Core.Gfxbin.Gmtl;
using Flagrum.Core.Gfxbin.Gmtl.Data;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;
using Flagrum.Web.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Vector3 = System.Numerics.Vector3;

namespace Flagrum.Core.Ebex;

public class EnvironmentModelMetadata
{
    public string PrefabName { get; set; }
    public int Index { get; set; }
    public string Path { get; set; }
    public float[] Position { get; set; }
    public float[] Rotation { get; set; }
    public float Scale { get; set; }
    public List<float[]> PrefabRotations { get; set; }
}

public class EnvironmentPacker
{
    private readonly ILogger<EnvironmentPacker> _logger;

    private readonly ConcurrentBag<EnvironmentModelMetadata> _models = new();
    private readonly ConcurrentBag<string> _nodeTypes = new();
    private readonly SettingsService _settings;

    private readonly ConcurrentDictionary<string, bool> _textures = new();
    // private readonly ConcurrentDictionary<string, bool> _lowLodPrefabs = new();

    private string _modelsDirectory;
    private string _texturesDirectory;

    public EnvironmentPacker(
        ILogger<EnvironmentPacker> logger,
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
        _modelsDirectory = $"{basePath}\\{outputFileNameWithoutExtension}_models";
        _texturesDirectory = $"{basePath}\\{outputFileNameWithoutExtension}_textures";

        if (!Directory.Exists(_modelsDirectory))
        {
            Directory.CreateDirectory(_modelsDirectory);
        }

        if (!Directory.Exists(_texturesDirectory))
        {
            Directory.CreateDirectory(_texturesDirectory);
        }

        // This instance is needed because the injected one was rarely causing a concurrency exception for some reason
        using var outerContext = new FlagrumDbContext(_settings);
        GetPathsRecursively(uri, outerContext.GetFileByUri(uri), new[] {0.0f, 0.0f, 0.0f, 0.0f},
            new[] {0.0f, 0.0f, 0.0f, 0.0f},
            1.0f,
            new List<float[]>());

        // _models = new ConcurrentBag<EnvironmentModelMetadata>(_models.Where(m => !_lowLodPrefabs
        //     .Any(p => p.Key.Contains(m.PrefabName, StringComparison.OrdinalIgnoreCase))));

        var models = _models.DistinctBy(m => m.Path).ToList();

        Parallel.For(0, models.Count, index =>
        {
            var model = models.ElementAt(index);
            PackModel(model.Path, _modelsDirectory, index);
            foreach (var subModel in _models.Where(m => m.Path == model.Path))
            {
                subModel.Index = index;
            }
        });

        // Can't use multithreading here due to an issue where DirectXTexNet hits
        // an access violation exception because we can't clear the memory quickly enough
        foreach (var (uri2, _) in _textures)
        {
            using var context = new FlagrumDbContext(_settings);
            var btexData = context.GetFileByUri(uri2);
            var converter = new TextureConverter();
            var pngData = converter.BtexToTga(btexData);
            var fileName = uri2.Split('/').Last();
            var fileNameWithoutExtension = fileName[..fileName.LastIndexOf('.')];
            File.WriteAllBytes($"{_texturesDirectory}\\{fileNameWithoutExtension}.tga", pngData);
        }

        File.WriteAllText(outputPath, JsonConvert.SerializeObject(_models));

        foreach (var type in _nodeTypes)
        {
            _logger.LogInformation(type);
        }

        _models.Clear();
        _textures.Clear();
        _nodeTypes.Clear();

        Thread.CurrentThread.CurrentCulture = previousCulture;
    }

    private void PackModel(string uri, string directory, int index)
    {
        using var context = new FlagrumDbContext(_settings);

        var gfxbin = context.GetFileByUri(uri);
        var gpubinUri = uri.Replace(".gmdl", ".gpubin");
        var gpubin = context.GetFileByUri(gpubinUri);

        if (gfxbin.Length < 1 || gpubin.Length < 1)
        {
            return;
        }

        var reader = new ModelReader(gfxbin, gpubin);
        var model = reader.Read();

        Dictionary<int, string> boneTable;
        if (model.BoneHeaders.Count(b => b.UniqueIndex == ushort.MaxValue) > 1)
        {
            // Probably a broken MO gfxbin with all IDs set to this value
            var arbitraryIndex = 0;
            boneTable = model.BoneHeaders.ToDictionary(b => arbitraryIndex++, b => b.Name);
        }
        else
        {
            boneTable = model.BoneHeaders.ToDictionary(b => (int)(b.UniqueIndex == 65535 ? 0 : b.UniqueIndex),
                b => b.Name);
        }

        var meshData = new Gpubin
        {
            BoneTable = boneTable,
            Meshes = model.MeshObjects.SelectMany(o => o.Meshes
                .Where(m => m.LodNear == 0)
                .Select(m =>
                {
                    using var innerContext = new FlagrumDbContext(_settings);
                    var materialUri = model.Header.Dependencies
                        .FirstOrDefault(d => d.PathHash == m.DefaultMaterialHash.ToString())
                        !.Path;

                    var materialData = innerContext.GetFileByUri(materialUri);
                    Material material;
                    try
                    {
                        material = new MaterialReader(materialData).Read();
                    }
                    catch
                    {
                        material = new Material();
                        material.Textures = new List<MaterialTexture>();
                        _logger.LogInformation("Failed to read material for mesh {MeshName} on model {Uri}", m.Name,
                            uri);
                    }

                    var highResTextures = GetHighResTextures(material);

                    return new GpubinMesh
                    {
                        Name = m.Name,
                        FaceIndices = m.FaceIndices,
                        VertexPositions = m.VertexPositions,
                        ColorMaps = m.ColorMaps,
                        Normals = m.Normals,
                        UVMaps = m.UVMaps.Select(m => new UVMap32
                        {
                            UVs = m.UVs.Select(uv => new UV32
                            {
                                U = (float)uv.U,
                                V = (float)uv.V
                            }).ToList()
                        }).ToList(),
                        WeightIndices = m.WeightIndices,
                        WeightValues = m.WeightValues
                            .Select(n => n.Select(o => o.Select(p => (int)p).ToArray()).ToList())
                            .ToList(),
                        BlenderMaterial = new BlenderMaterialData
                        {
                            Hash = m.DefaultMaterialHash.ToString(),
                            Name = material.Name,
                            Textures = material.Textures
                                .Where(t => t.ResourceFileHash > 0 && t.Path != null &&
                                            !t.Path.Contains("magicdamage") && !t.Path.Contains("magicdamege"))
                                .Select(t =>
                                {
                                    var fileName = t.Path.Split('/').Last();

                                    if (!fileName.Contains('.'))
                                    {
                                        return null;
                                    }

                                    var fileNameWithoutExtension = fileName[..fileName.LastIndexOf('.')];
                                    var highResPath = highResTextures
                                        .FirstOrDefault(t =>
                                            t.Contains("highimages") && t.Contains(fileNameWithoutExtension + "_$h."));

                                    if (highResPath == null)
                                    {
                                        highResPath = highResTextures
                                            .FirstOrDefault(t => t.Contains(fileNameWithoutExtension + "_$h."));
                                    }

                                    if (highResPath != null)
                                    {
                                        fileNameWithoutExtension += "_$h";
                                    }

                                    return new BlenderTextureData
                                    {
                                        Hash = t.ResourceFileHash.ToString(),
                                        Name = fileNameWithoutExtension,
                                        Path = $"{_texturesDirectory}\\{fileNameWithoutExtension}.tga",
                                        Uri = highResPath ?? t.Path,
                                        Slot = t.ShaderGenName
                                    };
                                })
                                .Where(t => t != null)
                        }
                    };
                }))
        };

        foreach (var mesh in meshData.Meshes)
        {
            foreach (var texture in mesh.BlenderMaterial.Textures.DistinctBy(t => t.Uri))
            {
                _textures.TryAdd(texture.Uri, true);
            }
        }

        var json = JsonConvert.SerializeObject(meshData);
        File.WriteAllText($"{directory}\\{index}.json", json);
    }

    private IEnumerable<string> GetHighResTextures(Material material)
    {
        if (!string.IsNullOrWhiteSpace(material.HighTexturePackAsset))
        {
            var highTexturePackUri = material.HighTexturePackAsset.Replace(".htpk", ".autoext");

            using var context = new FlagrumDbContext(_settings);
            for (var i = 0; i < 2; i++)
            {
                // Check for 4K pack for this material, will be handled by the following code if not present
                if (i > 0)
                {
                    highTexturePackUri = highTexturePackUri.Insert(highTexturePackUri.LastIndexOf('.'), "2");
                }

                byte[] highTexturePack = null;
                if (context.ArchiveExistsForUri(highTexturePackUri))
                {
                    highTexturePack = context.GetFileByUri(highTexturePackUri);
                }
                else
                {
                    var archiveRelativePath = material.HighTexturePackAsset
                        .Replace("data://", "")
                        .Replace(".htpk", ".earc")
                        .Replace('/', '\\');

                    var archivePath = $"{_settings.GameDataDirectory}\\{archiveRelativePath}";

                    if (File.Exists(archivePath))
                    {
                        using var unpacker = new Unpacker(archivePath);
                        var files = string.Join(' ', unpacker.Files.Select(f => f.Uri));
                        highTexturePack = Encoding.UTF8.GetBytes(files);
                    }
                }

                if (highTexturePack != null)
                {
                    foreach (var highTexture in Encoding.UTF8.GetString(highTexturePack)
                                 .Split(' ')
                                 .Where(s => !string.IsNullOrWhiteSpace(s))
                                 .Select(s =>
                                 {
                                     var result = s.Trim();
                                     if (result.Last() == 0x00)
                                     {
                                         result = result[..^1];
                                     }

                                     return result;
                                 }))
                    {
                        yield return highTexture;
                    }
                }
            }
        }
    }

    private void GetPathsRecursively(string uri, byte[] xmb2, float[] prefabPosition, float[] prefabRotation,
        float prefabScale, List<float[]> prefabRotations)
    {
        using var stream = new MemoryStream(xmb2);
        var package = Xmb2Document.GetRootElement(stream);
        var objects = package.GetElementByName("objects");
        var quaternion = Quaternion.CreateFromYawPitchRoll(
            DegreesToRadians(prefabRotation[1]),
            DegreesToRadians(prefabRotation[0]),
            DegreesToRadians(prefabRotation[2]));

        var elements = objects.GetElements();
        Parallel.For(0, elements.Count, counter =>
        {
            var element = elements.ElementAt(counter);

            using var context = new FlagrumDbContext(_settings);
            var typeAttribute = element.GetAttributeByName("type").GetTextValue();

            if (counter == 0 && !_nodeTypes.Contains(typeAttribute))
            {
                _nodeTypes.Add(typeAttribute);
            }

            if (typeAttribute is "Black.Entity.StaticModelEntity" or "Black.Entity.Actor.HeightFieldEntity")
            {
                try
                {
                    var path = element.GetElementByName("sourcePath_");
                    var position = element.GetElementByName("position_")?.GetFloat4Value() ?? new[] {0.0f, 0.0f, 0.0f, 0.0f};
                    var rotation = element.GetElementByName("rotation_");
                    var scale = element.GetElementByName("scaling_");

                    var positionAltered =
                        Vector3.Add(Vector3.Transform(new Vector3(position), quaternion),
                            new Vector3(prefabPosition));

                    // Luminous ignores scale of 0, so we must too
                    var scaleFloat = scale?.GetFloatValue();
                    if (scaleFloat is null or > -0.0001f and < 0.0001f)
                    {
                        scaleFloat = 1.0f;
                    }

                    //var rotationAltered = rotationFloats.Select(p => p + prefabRotation[i++]).ToArray();
                    var scaleAltered = (float)scaleFloat * prefabScale;
                    var prefabFileName = uri.Split('\\', '/').Last();

                    _models.Add(new EnvironmentModelMetadata
                    {
                        PrefabName = prefabFileName[..prefabFileName.LastIndexOf('.')],
                        Path = $"data://{path.GetTextValue().Replace('\\', '/')}",
                        Position = new[] {positionAltered.X, positionAltered.Y, positionAltered.Z},
                        Rotation = rotation?.GetFloat4Value() ?? new[] {0.0f, 0.0f, 0.0f, 0.0f},
                        PrefabRotations = prefabRotations,
                        Scale = scaleAltered
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
                var position = element.GetElementByName("position_").GetFloat4Value();
                var rotation = element.GetElementByName("rotation_").GetFloat4Value();
                var scale = element.GetElementByName("scaling_")?.GetFloatValue() ?? 1.0f;

                var positionAltered =
                    Vector3.Add(Vector3.Transform(new Vector3(position), quaternion), new Vector3(prefabPosition));
                var i = 0;

                // Luminous ignores scale of 0, so we must too
                if (scale is > -0.0001f and < 0.0001f)
                {
                    scale = 1.0f;
                }

                var rotationAltered = rotation.Select(p => p + prefabRotation[i++]).ToArray();
                var scaleAltered = scale * prefabScale;

                var relativeUri = path.GetTextValue();
                var uriUri = new Uri(uri.Replace("data://", "data://data/"));
                var combinedUri = new Uri(uriUri, relativeUri);
                var combinedUriString = combinedUri.ToString().Replace("data://data/", "data://");
                var innerXmb2 = context.GetFileByUri(combinedUriString);

                if (innerXmb2.Length > 0)
                {
                    var newPrefabRotations = new List<float[]>(prefabRotations);
                    if (!(rotation[0] is > -0.0001f and < 0.0001f
                          && rotation[1] is > -0.0001f and < 0.0001f
                          && rotation[2] is > -0.0001f and < 0.0001f))
                    {
                        newPrefabRotations.Add(rotation);
                    }

                    GetPathsRecursively(combinedUriString, innerXmb2,
                        new[] {positionAltered.X, positionAltered.Y, positionAltered.Z}, rotationAltered, scaleAltered,
                        newPrefabRotations);
                }
                else
                {
                    _logger.LogInformation("Failed to load entity package from {Uri} at path {SourcePath}", uri,
                        path.GetTextValue());
                }
            }
            // else if (typeAttribute == "Black.Entity.Node.MapLodEntity")
            // {
            //     var lowLodList = element.GetElementByName("mapLodLowItemList_");
            //     var lowLods = lowLodList.GetElements();
            //     foreach (var lowLod in lowLods)
            //     {
            //         var filePath = lowLod.GetElementByName("filePath_");
            //         _lowLodPrefabs.TryAdd(filePath.GetTextValue(), true);
            //     }
            // }
        });
    }

    private float DegreesToRadians(float degrees)
    {
        return (float)(Math.PI / 180 * degrees);
    }
}