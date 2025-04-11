using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Flagrum.Abstractions;
using Flagrum.Abstractions.AssetExplorer;
using Flagrum.Core.Entities.Xml2;
using Flagrum.Core.Graphics.Materials;
using Flagrum.Core.Graphics.Models;
using Flagrum.Core.Utilities;
using Flagrum.Generators;
using Flagrum.Application.Features.Shared;
using Flagrum.Application.Features.WorkshopMods.Data.Model;
using Injectio.Attributes;
using Newtonsoft.Json;

namespace Flagrum.Application.Services;

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

[RegisterScoped]
public partial class EnvironmentPacker
{
    [Inject] private readonly AppStateService _appState;
    [Inject] private readonly IFileIndex _fileIndex;
    private readonly ConcurrentBag<EnvironmentModelMetadata> _models = new();
    private readonly ConcurrentBag<string> _nodeTypes = new();
    [Inject] private readonly IProfileService _profile;

    private readonly List<string> _staticModelTypes = new()
    {
        "Black.Entity.Actor.TaggedStaticModelEntity",
        "Black.Entity.Shape.OceanFlowMapEntity",
        "Black.Entity.Shape.OceanPatchEntity",
        "Black.Entity.HairModelEntity",
        "Black.Entity.OceanFloatingModelEntity",
        "Black.Entity.SkeletalModelEntity",
        "Black.Entity.StaticModelEntity"
    };

    [Inject] private readonly TextureConverter _textureConverter;
    private readonly ConcurrentDictionary<string, bool> _textures = new();
    private readonly ConcurrentDictionary<string, bool> _unreadClassTypes = new();

    private string _modelsDirectory;
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
        _modelsDirectory = $"{basePath}\\{outputFileNameWithoutExtension}_models";
        _texturesDirectory = $"{basePath}\\{outputFileNameWithoutExtension}_textures";
        IOHelper.EnsureDirectoryExists(_modelsDirectory);
        IOHelper.EnsureDirectoryExists(_texturesDirectory);

        // Recurse through the scripts
        GetPathsRecursively(uri, _appState.GetFileByUri(uri),
            null,
            null,
            1.0f,
            new List<float[]>());

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
            var btexData = _appState.GetFileByUri(uri2);
            var pngData = _textureConverter.ToTarga(btexData);
            var fileName = uri2.Split('/').Last();
            var fileNameWithoutExtension = fileName[..fileName.LastIndexOf('.')];
            File.WriteAllBytes($"{_texturesDirectory}\\{fileNameWithoutExtension}.tga", pngData);
        }

        File.WriteAllText(outputPath, JsonConvert.SerializeObject(_models));

        _models.Clear();
        _textures.Clear();
        _nodeTypes.Clear();

        Thread.CurrentThread.CurrentCulture = previousCulture;
    }

    private void PackModel(string uri, string directory, int index)
    {
        var gfxbin = _appState.GetFileByUri(uri);
        var gpubinUri = uri.Replace(".gmdl", ".gpubin");
        var gpubin = _appState.GetFileByUri(gpubinUri);

        if (gfxbin.Length < 1 || gpubin.Length < 1)
        {
            return;
        }

        var model = new GameModel();
        model.Read(gfxbin);
        model.ReadVertexData(gpubin);

        Dictionary<int, string> boneTable;
        if (model.Bones.Count(b => b.UniqueIndex == ushort.MaxValue) > 1)
        {
            // Probably a broken MO gfxbin with all IDs set to this value
            var arbitraryIndex = 0;
            boneTable = model.Bones.ToDictionary(_ => arbitraryIndex++, b => b.Name);
        }
        else
        {
            boneTable = model.Bones.ToDictionary(b => (int)(b.UniqueIndex == 65535 ? 0 : b.UniqueIndex),
                b => b.Name);
        }

        var meshData = new BinmodModelData
        {
            BoneTable = boneTable,
            Meshes = model.MeshObjects.SelectMany(o => o.Meshes
                .Where(m => m.LodNear == 0)
                .Select(m =>
                {
                    var materialUri = model.Dependencies
                        .FirstOrDefault(d => d.Key == m.MaterialHash.ToString())
                        !.Value;

                    var materialData = _appState.GetFileByUri(materialUri);
                    GameMaterial material;
                    try
                    {
                        material = new GameMaterial();
                        material.Read(materialData);
                    }
                    catch
                    {
                        material = new GameMaterial();
                    }

                    return new BinmodModelDataMesh
                    {
                        Name = m.Name,
                        FaceIndices = m.FaceIndices,
                        VertexPositions = ((IList<float[]>)m.Semantics[VertexElementSemantic.Position0])
                            .Select(v => new BinmodVector3(v[0], v[1], v[2])).ToList(),
                        ColorMaps = m.Semantics
                            .Where(s => s.Key.Value.Contains("COLOR"))
                            .Select(s => new BinmodColorMap
                            {
                                Colors = ((IList<float[]>)s.Value)
                                    .Select(c => new BinmodColor4
                                    {
                                        R = (byte)(c[0] * 255.0f),
                                        G = (byte)(c[1] * 255.0f),
                                        B = (byte)(c[2] * 255.0f),
                                        A = (byte)(c[3] * 255.0f)
                                    }).ToList()
                            }).ToList(),
                        Normals = ((IList<float[]>)m.Semantics[VertexElementSemantic.Normal0]).Select(n =>
                            new BinmodNormal
                            {
                                X = (sbyte)(n[0] * 127.0f),
                                Y = (sbyte)(n[1] * 127.0f),
                                Z = (sbyte)(n[2] * 127.0f),
                                W = (sbyte)(n[3] * 127.0f)
                            }).ToList(),
                        UVMaps = m.Semantics
                            .Where(s => s.Key.Value.Contains("TEXCOORD"))
                            .Select(s => new BinmodUVMap32
                            {
                                UVs = ((IList<float[]>)s.Value).Select(uv => new BinmodUV32
                                {
                                    U = float.IsFinite(uv[0]) ? uv[0] : 0.0f,
                                    V = float.IsFinite(uv[1]) ? uv[1] : 0.0f
                                }).ToList()
                            }).ToList(),
                        WeightIndices = m.Semantics
                            .Where(s => s.Key.Value.Contains("BLENDINDICES"))
                            .Select(s =>
                                ((IList<uint[]>)s.Value).Select(a => a.Select(i => (ushort)i).ToArray()).ToList())
                            .ToList(),
                        WeightValues = m.Semantics
                            .Where(s => s.Key.Value.Contains("BLENDWEIGHT"))
                            .Select(s =>
                                ((IList<float[]>)s.Value).Select(a => a.Select(w => (int)(w * 255.0f)).ToArray())
                                .ToList())
                            .ToList(),
                        BlenderMaterial = new BlenderMaterialData
                        {
                            Hash = m.MaterialHash.ToString(),
                            Name = material.Name,
                            UVScale = material.Buffers
                                .Where(i => i.ShaderGenName.Equals("UVScale", StringComparison.OrdinalIgnoreCase))
                                .Select(i => i.Values)
                                .FirstOrDefault() ?? new[] {1.0f, 1.0f},
                            Textures = material.Textures
                                .Where(t => t.UriHash > 0 && t.Uri != null &&
                                            !t.Uri.Contains("magicdamage") && !t.Uri.Contains("magicdamege"))
                                .Select(t =>
                                {
                                    var textureUri = ResolveHighestResolutionTexture(t.Uri);
                                    var fileName = textureUri.Split('/').Last();

                                    if (!fileName.Contains('.'))
                                    {
                                        return null;
                                    }

                                    var fileNameWithoutExtension = fileName[..fileName.LastIndexOf('.')];

                                    return new BlenderTextureData
                                    {
                                        Hash = t.UriHash.ToString(),
                                        Name = fileNameWithoutExtension,
                                        Path = $"{_texturesDirectory}\\{fileNameWithoutExtension}.tga",
                                        Uri = textureUri,
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

    /// <summary>
    /// Finds the highest resolution version of the given standard resolution texture URI.
    /// </summary>
    /// <param name="uri">The URI of the texture.</param>
    /// <returns>The URI of the highest resolution version of the texture.</returns>
    private string ResolveHighestResolutionTexture(string uri)
    {
        var high = uri.Insert(uri.LastIndexOf('.'), "_$h");
        var highest = high.Replace("/sourceimages/", "/highimages/");

        string[] uris = [highest, high];
        foreach (var resolution in uris)
        {
            if (_fileIndex.Contains(resolution))
            {
                return resolution;
            }
        }

        return uri;
    }

    private void GetPathsRecursively(string uri, byte[] xmb2, float[] prefabPosition, float[] prefabRotation,
        float prefabScale, List<float[]> prefabRotations)
    {
        uri = uri.ToLower();
        using var stream = new MemoryStream(xmb2);
        var package = XmlBinary2Document.GetRootElement(stream);
        var objects = package.GetElementByName("objects");
        var elements = objects.GetElements();

        if (prefabPosition == null)
        {
            if (uri.EndsWith(".prefab"))
            {
                // Don't want to use the root transform if the root ebex is a prefab file
                // See https://github.com/Kizari/Flagrum/issues/51
                prefabPosition = new[] {0.0f, 0.0f, 0.0f, 0.0f};
                prefabRotation = new[] {0.0f, 0.0f, 0.0f, 0.0f};
                prefabScale = 1.0f;
            }
            else
            {
                try
                {
                    var element = elements.First();
                    prefabPosition = element.GetElementByName("position_").GetFloat4Value() ??
                                     new[] {0.0f, 0.0f, 0.0f, 0.0f};
                    prefabRotation = element.GetElementByName("rotation_").GetFloat4Value() ??
                                     new[] {0.0f, 0.0f, 0.0f, 0.0f};
                    prefabScale = element.GetElementByName("scaling_")?.GetFloatValue() ?? 1.0f;
                }
                catch
                {
                    prefabPosition = new[] {0.0f, 0.0f, 0.0f, 0.0f};
                    prefabRotation = new[] {0.0f, 0.0f, 0.0f, 0.0f};
                    prefabScale = 1.0f;
                }
            }
        }

        var quaternion = Quaternion.CreateFromYawPitchRoll(
            DegreesToRadians(prefabRotation[1]),
            DegreesToRadians(prefabRotation[0]),
            DegreesToRadians(prefabRotation[2]));

        Parallel.For(1, elements.Count, counter =>
        {
            var element = elements.ElementAt(counter);
            var typeAttribute = element.GetAttributeByName("type").GetTextValue();

            if (counter == 0 && !_nodeTypes.Contains(typeAttribute))
            {
                _nodeTypes.Add(typeAttribute);
            }

            if (_staticModelTypes.Contains(typeAttribute))
            {
                try
                {
                    var path = element.GetElementByName("sourcePath_");
                    var position = element.GetElementByName("position_")?.GetFloat4Value() ??
                                   new[] {0.0f, 0.0f, 0.0f, 0.0f};
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
                    // Ignore silently
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
                var innerXmb2 = _appState.GetFileByUri(combinedUriString);

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
                        new[] {positionAltered.X, positionAltered.Y, positionAltered.Z}, rotationAltered,
                        scaleAltered,
                        newPrefabRotations);
                }
            }
            else
            {
                _unreadClassTypes.TryAdd(typeAttribute, true);
            }
        });
    }

    private float DegreesToRadians(float degrees) => (float)(Math.PI / 180 * degrees);
}