using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using Flagrum.Core.Archive;
using Flagrum.Core.Ebex.Xmb2;
using Flagrum.Core.Gfxbin.Gmdl;
using Flagrum.Core.Gfxbin.Gmdl.Constructs;
using Flagrum.Core.Gfxbin.Gmtl;
using Flagrum.Core.Gfxbin.Gmtl.Data;
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
}

public class EnvironmentPacker
{
    private readonly Func<string, bool> _archiveExistsForUri;
    private readonly Func<byte[], byte[]> _btexToPng;
    private readonly string _gameDataDirectory;
    private readonly Func<string, byte[]> _getFileByUri;
    private readonly ILogger _logger;
    private readonly List<EnvironmentModelMetadata> _models = new();
    private readonly List<string> _nodeTypes = new();
    private readonly Dictionary<string, string> _textures = new();
    private readonly string _uri;

    public EnvironmentPacker(ILogger logger, string uri, string gameDataDirectory, Func<string, byte[]> getFileByUri,
        Func<byte[], byte[]> btexToPng, Func<string, bool> archiveExistsForUri)
    {
        _logger = logger;
        _uri = uri;
        _gameDataDirectory = gameDataDirectory;
        _getFileByUri = getFileByUri;
        _btexToPng = btexToPng;
        _archiveExistsForUri = archiveExistsForUri;
    }

    public void Pack(string outputPath)
    {
        var basePathTokens = outputPath.Split('\\')[..^1];
        var basePath = string.Join('\\', basePathTokens);
        var modelsPath = $"{basePath}\\models";
        var texturesPath = $"{basePath}\\textures";

        if (!Directory.Exists(modelsPath))
        {
            Directory.CreateDirectory(modelsPath);
        }

        if (!Directory.Exists(texturesPath))
        {
            Directory.CreateDirectory(texturesPath);
        }

        GetPathsRecursively(_uri, _getFileByUri(_uri), new[] {0.0f, 0.0f, 0.0f, 0.0f}, new[] {0.0f, 0.0f, 0.0f, 0.0f},
            1.0f);

        var index = 0;
        foreach (var model in _models.DistinctBy(m => m.Path))
        {
            index++;
            PackModel(model.Path, modelsPath, index);
            foreach (var subModel in _models.Where(m => m.Path == model.Path))
            {
                subModel.Index = index;
            }
        }

        foreach (var (hash, uri) in _textures)
        {
            var btexData = _getFileByUri(uri);
            var pngData = _btexToPng(btexData);
            var fileName = uri.Split('/').Last();
            var fileNameWithoutExtension = fileName[..fileName.LastIndexOf('.')];
            File.WriteAllBytes($"{texturesPath}\\{fileNameWithoutExtension}.tga", pngData);
        }

        File.WriteAllText(outputPath, JsonConvert.SerializeObject(_models));

        foreach (var type in _nodeTypes)
        {
            _logger.LogInformation(type);
        }
    }

    private void PackModel(string uri, string directory, int index)
    {
        var gfxbin = _getFileByUri(uri);
        var gpubinUri = uri.Replace(".gmdl", ".gpubin");
        var gpubin = _getFileByUri(gpubinUri);

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
                    var materialUri = model.Header.Dependencies
                        .FirstOrDefault(d => d.PathHash == m.DefaultMaterialHash.ToString())
                        !.Path;

                    var materialData = _getFileByUri(materialUri);
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
                                    var fileNameWithoutExtension = fileName[..fileName.LastIndexOf('.')];
                                    var highResPath = highResTextures
                                        .FirstOrDefault(t => t.Contains("highimages") && t.Contains(fileNameWithoutExtension + "_$h."));

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
                                        Path = highResPath ?? t.Path,
                                        Slot = t.ShaderGenName
                                    };
                                })
                        }
                    };
                }))
        };

        foreach (var mesh in meshData.Meshes)
        {
            foreach (var texture in mesh.BlenderMaterial.Textures.DistinctBy(t => t.Hash))
            {
                _textures.TryAdd(texture.Hash, texture.Path);
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

            for (var i = 0; i < 2; i++)
            {
                // Check for 4K pack for this material, will be handled by the following code if not present
                if (i > 0)
                {
                    highTexturePackUri = highTexturePackUri.Insert(highTexturePackUri.LastIndexOf('.') - 1, "2");
                }

                byte[] highTexturePack = null;
                if (_archiveExistsForUri(highTexturePackUri))
                {
                    highTexturePack = _getFileByUri(highTexturePackUri);
                }
                else
                {
                    var archiveRelativePath = material.HighTexturePackAsset
                        .Replace("data://", "")
                        .Replace(".htpk", ".earc")
                        .Replace('/', '\\');

                    var archivePath = $"{_gameDataDirectory}\\{archiveRelativePath}";

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
        float prefabScale)
    {
        using var stream = new MemoryStream(xmb2);
        var package = Xmb2Document.GetRootElement(stream);
        var objects = package.GetElementByName("objects");
        var quaternion = Quaternion.CreateFromYawPitchRoll(
            DegreesToRadians(prefabRotation[1]),
            DegreesToRadians(prefabRotation[0]),
            DegreesToRadians(prefabRotation[2]));

        var counter = 0;
        foreach (var element in objects.GetElements())
        {
            var typeAttribute = (string)element.GetAttributeByName("type").Value;

            if (counter == 0 && !_nodeTypes.Contains(typeAttribute))
            {
                _nodeTypes.Add(typeAttribute);
            }

            counter++;

            if (typeAttribute is "Black.Entity.StaticModelEntity" or "Black.Entity.Actor.HeightFieldEntity")
            {
                try
                {
                    var path = element.GetElementByName("sourcePath_");
                    var position = element.GetElementByName("position_");
                    var rotation = element.GetElementByName("rotation_");
                    var scale = element.GetElementByName("scaling_");

                    var positionAltered =
                        Vector3.Add(Vector3.Transform(new Vector3(position.GetFloat4Value()), quaternion),
                            new Vector3(prefabPosition));
                    var i = 0;
                    var rotationAltered = rotation.GetFloat4Value().Select(p => p + prefabRotation[i++]).ToArray();
                    var scaleAltered = scale.GetFloatValue() * prefabScale;
                    var prefabFileName = uri.Split('\\', '/').Last();

                    _models.Add(new EnvironmentModelMetadata
                    {
                        PrefabName = prefabFileName[..prefabFileName.LastIndexOf('.')],
                        Path = $"data://{path.GetTextValue().Replace('\\', '/')}",
                        Position = new[] {positionAltered.X, positionAltered.Y, positionAltered.Z},
                        Rotation = rotationAltered,
                        Scale = scaleAltered
                    });
                }
                catch
                {
                    var path = element.GetElementByName("sourcePath_").GetTextValue();
                    _logger.LogInformation("Failed to handle model node with sourcePath {Path} from ebex {Uri}", path,
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
                var rotationAltered = rotation.Select(p => p + prefabRotation[i++]).ToArray();
                var scaleAltered = scale * prefabScale;

                var relativeUri = path.GetTextValue();
                var uriUri = new Uri(uri.Replace("data://", "data://data/"));
                var combinedUri = new Uri(uriUri, relativeUri);
                var combinedUriString = combinedUri.ToString().Replace("data://data/", "data://");
                var innerXmb2 = _getFileByUri(combinedUriString);

                if (innerXmb2.Length > 0)
                {
                    GetPathsRecursively(combinedUriString, innerXmb2,
                        new[] {positionAltered.X, positionAltered.Y, positionAltered.Z}, rotationAltered, scaleAltered);
                }
                else
                {
                    _logger.LogInformation("Failed to load entity package from {Uri} at path {SourcePath}", uri, path.GetTextValue());
                }
            }
        }
    }

    private float DegreesToRadians(float degrees)
    {
        return (float)(Math.PI / 180 * degrees);
    }
}