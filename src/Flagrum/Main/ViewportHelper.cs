using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Flagrum.Abstractions;
using Flagrum.Abstractions.AssetExplorer;
using Flagrum.Core.Graphics.Materials;
using Flagrum.Core.Graphics.Models;
using Flagrum.Generators;
using Flagrum.Application.Features.AssetExplorer.Data;
using Flagrum.Application.Features.Shared;
using Flagrum.Application.Services;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Assimp;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf.SharpDX;
using Microsoft.Extensions.DependencyInjection;
using SharpDX;

namespace Flagrum.Main;

[InjectableDependency(ServiceLifetime.Singleton)]
public partial class ViewportHelper
{
    [Inject] private readonly AppStateService _appState;
    [Inject] private readonly IConfiguration _configuration;
    [Inject] private readonly TextureConverter _textureConverter;
    [Inject] private readonly ViewportViewModel _viewModel;

    private ModelViewerTextureFidelity _fidelity;

    public int ChangeModel(IAssetExplorerNode gmdlNode, AssetExplorerView view, int lodLevel)
    {
        _viewModel.Viewer!.Reset();

        var modelGroup = new SceneNodeGroupModel3D();
        var scene = new HelixToolkitScene(new GroupNode());
        modelGroup.AddNode(scene.Root);
        var modelBag = new ConcurrentBag<SceneNode>();

        // Read the model
        var gmdl = gmdlNode.Data;
        var model = new GameModel();
        model.Read(gmdl);

        // Get the gpubin uris
        var gpubinUris = model.Dependencies
            .Where(d => d.Value.EndsWith(".gpubin"))
            .Select(d => d.Value)
            .ToList();

        if (!gpubinUris.Any())
        {
            MessageBox.Show("This model doesn't have a corresponding gpubin file, so there is nothing to show.",
                "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return 0;
        }

        // Find the gpubin locations
        var gpubins = gpubinUris.Select(uri => gmdlNode.Parent.Children
                .Single(c => c.Path.Split('\\', '/').Last()
                    .Equals(uri.Split('\\', '/').Last(), StringComparison.OrdinalIgnoreCase)))
            .OrderBy(f => f.Name)
            .Select(gpubin => gpubin.Data)
            .ToList();

        // Read the gpubins
        model.ReadVertexData(gpubins);

        // Get the texture fidelity settings
        var fidelityInt = _configuration.Get<int>(StateKey.ViewportTextureFidelity);
        _fidelity = fidelityInt == -1 ? ModelViewerTextureFidelity.Low : (ModelViewerTextureFidelity)fidelityInt;

        // Calculate paths
        var gpubinUri = gpubinUris.First();
        var modelContext = new FileSystemModelContext
        {
            RootUri = gpubinUri[..gpubinUri.LastIndexOf('/')],
            RootDirectory = string.Join('\\', gmdlNode.Path.Split('\\')[..^1])
        };

        Parallel.ForEach(model.MeshObjects, meshObject =>
        {
            Parallel.ForEach(meshObject.Meshes.Where(m => m.LodLevel == lodLevel && (m.Flags & 67108864) == 0), mesh =>
            {
                object? contextObject = view == AssetExplorerView.FileSystem
                    ? modelContext
                    : null;

                var builder = new MeshBuilder();
                var faceIndices = new List<int>();

                for (var i = 0; i < mesh.FaceIndices.Length / 3; i++)
                {
                    var triangle = new List<int>();
                    for (var j = 0; j < 3; j++)
                    {
                        var index = (int)mesh.FaceIndices[i, j];

                        if (index < mesh.VertexCount)
                        {
                            triangle.Add(index);
                        }
                    }

                    if (triangle.Count == 3)
                    {
                        faceIndices.AddRange(triangle);
                    }
                }

                if (mesh.Semantics.TryGetValue(VertexElementSemantic.TexCoord0, out var semantic))
                {
                    builder.Append(
                        ((IList<float[]>)mesh.Semantics[VertexElementSemantic.Position0]).Select(v => new Vector3(v))
                        .ToList(),
                        faceIndices,
                        ((IList<float[]>)mesh.Semantics[VertexElementSemantic.Normal0]).Select(n => new Vector3(n[..3]))
                        .ToList(),
                        ((IList<float[]>)semantic).Select(c => new Vector2(c))
                        .ToList());
                }
                else
                {
                    builder.Append(
                        ((IList<float[]>)mesh.Semantics[VertexElementSemantic.Position0]).Select(v => new Vector3(v))
                        .ToList(),
                        faceIndices,
                        ((IList<float[]>)mesh.Semantics[VertexElementSemantic.Normal0]).Select(n => new Vector3(n[..3]))
                        .ToList(),
                        Enumerable.Range(0, (int)mesh.VertexCount).Select(i => new Vector2(0f, 0f))
                            .ToList());
                }

                var meshNode = new BoneSkinMeshNode();
                var material = new PBRMaterial
                {
                    AlbedoColor = Color4.White
                };

                if (_fidelity != ModelViewerTextureFidelity.None)
                {
                    var materialUri = model.Dependencies
                        .FirstOrDefault(d => d.Key == mesh.MaterialHash.ToString())
                        .Value;

                    if (materialUri != null)
                    {
                        var materialData = GetDataByUri(materialUri, contextObject, view);
                        if (materialData == null)
                        {
                            return;
                        }

                        var gameMaterial = new GameMaterial();
                        gameMaterial.Read(materialData);

                        var diffuseUri = gameMaterial.Textures
                            .FirstOrDefault(t =>
                                t.ShaderGenName.Contains("BaseColor0") || t.ShaderGenName.Contains("BaseColorTexture0"))
                            ?.Uri;

                        var normalUri = gameMaterial.Textures
                            .FirstOrDefault(t =>
                                t.ShaderGenName.Contains("Normal0") || t.ShaderGenName.Contains("NormalTexture0"))
                            ?.Uri;

                        if (diffuseUri != null && normalUri != null)
                        {
                            var diffuse = GetDataByUri(diffuseUri, contextObject, view);
                            var normal = GetDataByUri(normalUri, contextObject, view);

                            TextureModel? albedoMap = null;
                            TextureModel? normalMap = null;

                            Parallel.Invoke(() =>
                                {
                                    if (diffuse is {Length: > 0})
                                    {
                                        var tag = Encoding.UTF8.GetString(diffuse[..8]);

                                        if (tag == "SEDBbtex" || tag.StartsWith("BTEX"))
                                        {
                                            diffuse = _textureConverter.ToPng(diffuse);
                                        }

                                        var diffuseStream = new MemoryStream(diffuse);
                                        diffuseStream.Seek(0, SeekOrigin.Begin);
                                        albedoMap = TextureModel.Create(diffuseStream);
                                    }
                                },
                                () =>
                                {
                                    if (normal is {Length: > 0})
                                    {
                                        var tag = Encoding.UTF8.GetString(normal[..8]);

                                        if (tag == "SEDBbtex" || tag.StartsWith("BTEX"))
                                        {
                                            normal = _textureConverter.ToPng(normal);
                                        }

                                        var normalStream = new MemoryStream(normal);
                                        normalStream.Seek(0, SeekOrigin.Begin);
                                        normalMap = TextureModel.Create(normalStream);
                                    }
                                });

                            if (albedoMap != null)
                            {
                                material.AlbedoMap = albedoMap;
                            }

                            if (normalMap != null)
                            {
                                material.NormalMap = normalMap;
                            }
                        }
                    }
                }

                meshNode.Material = material;
                builder.ComputeTangents(MeshFaces.Default);
                meshNode.Geometry = builder.ToMesh();
                //meshNode.Bones = bones.ToArray();
                modelBag.Add(meshNode);
            });
        });

        foreach (var node in modelBag)
        {
            modelGroup.AddNode(node);
        }

        _viewModel.ModelGroup = modelGroup;
        _viewModel.Viewer.ZoomExtents(0);

        return model.LodLevels;
    }

    private byte[]? GetDataByUri(string uri, object? context, AssetExplorerView view)
    {
        if (view == AssetExplorerView.GameView)
        {
            var high = uri.Insert(uri.LastIndexOf('.'), "_$h");
            var highest = high.Replace("/sourceimages/", "/highimages/");

            if (_fidelity >= ModelViewerTextureFidelity.Highest)
            {
                var data = _appState.GetFileByUri(highest);
                if (data.Length > 0)
                {
                    return data;
                }
            }

            if (_fidelity >= ModelViewerTextureFidelity.High)
            {
                var data = _appState.GetFileByUri(high);
                if (data.Length > 0)
                {
                    return data;
                }
            }

            if (_fidelity >= ModelViewerTextureFidelity.Low)
            {
                var data = _appState.GetFileByUri(uri);
                if (data.Length > 0)
                {
                    return data;
                }
            }

            return null;
        }

        var baseContext = (FileSystemModelContext)context!;
        var modelContext = new FileSystemModelContext
        {
            Uri = uri,
            RootUri = baseContext.RootUri,
            RootDirectory = baseContext.RootDirectory
        };

        var path = uri.EndsWith(".gmtl")
            ? UriToRelativePath(modelContext)
            : ResolveTexturePath(modelContext);

        return path == null || !File.Exists(path) ? null : File.ReadAllBytes(path);
    }

    private string? UriToRelativePath(FileSystemModelContext context)
    {
        // Get path tokens
        var target = GetMatchingUriStart(context.Uri, context.RootUri);
        var rootUriTokens = context.RootUri.Replace("data://", "data/").Split('/');
        var rootPathTokens = context.RootDirectory.Split('\\');
        var targetTokens = target.Replace("data://", "data/").Split('/');
        var targetToken = targetTokens.Last();

        // Find index of the first folder the URIs have in common
        var index = -1;
        var counter = 0;
        for (var i = rootUriTokens.Length - 1; i >= 0; i--)
        {
            if (rootUriTokens[i].Equals(targetToken, StringComparison.OrdinalIgnoreCase))
            {
                index = i;
                break;
            }

            counter++;
        }

        if (index == -1)
        {
            return null;
        }

        // Calculate the final physical path
        var root = string.Join('\\', rootPathTokens[..^counter]);
        var remainingPath = context.Uri.Replace(target, "").Replace("://", "/");
        var path = $@"{root}{remainingPath.Replace('/', '\\')}";

        // Calculate the true extension
        path = path.Replace(".gmtl", ".gmtl.gfxbin");
        return path;
    }

    private string GetMatchingUriStart(string first, string second)
    {
        var tokens1 = first.Replace("data://", "data/").Split('/');
        var tokens2 = second.Replace("data://", "data/").Split('/');
        var tokens = new List<string>();

        var length = Math.Min(tokens1.Length, tokens2.Length);
        for (var i = 0; i < length; i++)
        {
            if (tokens1[i].Equals(tokens2[i], StringComparison.OrdinalIgnoreCase))
            {
                tokens.Add(tokens1[i]);
            }
            else
            {
                break;
            }
        }

        return string.Join('/', tokens).Replace("data/", "data://");
    }

    private string? ResolveTexturePath(FileSystemModelContext context)
    {
        var extensions = new[] {"dds", "tga", "png", "btex"};
        var high = context.Uri.Insert(context.Uri.LastIndexOf('.'), "_$h");
        var highest = high.Replace("/sourceimages/", "/highimages/");

        if (_fidelity >= ModelViewerTextureFidelity.Highest)
        {
            var highestPath = UriToRelativePath(new FileSystemModelContext
            {
                RootDirectory = context.RootDirectory,
                RootUri = context.RootUri,
                Uri = highest
            });

            if (highestPath != null)
            {
                foreach (var extension in extensions)
                {
                    var withoutExtension = highestPath[..highestPath.LastIndexOf('.')];
                    var withExtension = $"{withoutExtension}.{extension}";

                    if (File.Exists(withExtension))
                    {
                        return withExtension;
                    }
                }
            }
        }

        if (_fidelity >= ModelViewerTextureFidelity.High)
        {
            var highPath = UriToRelativePath(new FileSystemModelContext
            {
                RootDirectory = context.RootDirectory,
                RootUri = context.RootUri,
                Uri = high
            });

            if (highPath != null)
            {
                foreach (var extension in extensions)
                {
                    var withoutExtension = highPath[..highPath.LastIndexOf('.')];
                    var withExtension = $"{withoutExtension}.{extension}";

                    if (File.Exists(withExtension))
                    {
                        return withExtension;
                    }
                }
            }
        }

        if (_fidelity >= ModelViewerTextureFidelity.Low)
        {
            var lowPath = UriToRelativePath(new FileSystemModelContext
            {
                RootDirectory = context.RootDirectory,
                RootUri = context.RootUri,
                Uri = context.Uri
            });

            if (lowPath != null)
            {
                foreach (var extension in extensions)
                {
                    var withoutExtension = lowPath[..lowPath.LastIndexOf('.')];
                    var withExtension = $"{withoutExtension}.{extension}";

                    if (File.Exists(withExtension))
                    {
                        return withExtension;
                    }
                }
            }
        }

        return null;
    }

    private class FileSystemModelContext
    {
        public string Uri { get; set; }
        public string RootUri { get; set; }
        public string? RootDirectory { get; set; }
    }
}