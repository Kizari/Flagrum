using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flagrum.Core.Gfxbin.Gmtl;
using Flagrum.Web.Features.AssetExplorer.Data;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;
using Flagrum.Web.Services;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Assimp;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using ModelReader = Flagrum.Core.Gfxbin.Gmdl.ModelReader;

namespace Flagrum.Desktop.Architecture;

public class ViewportHelper
{
    private readonly FlagrumDbContext _context;
    private readonly MainViewModel _mainViewModel;
    private ModelViewerTextureFidelity _fidelity;

    public ViewportHelper(
        MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
        _context = new FlagrumDbContext(new SettingsService());
    }

    public async Task ChangeModel(byte[] gmdl, byte[] gpubin, AssetExplorerView view, string? inputPath)
    {
        _mainViewModel.Viewer.Reset();

        var modelGroup = new SceneNodeGroupModel3D();
        var scene = new HelixToolkitScene(new GroupNode());
        modelGroup.AddNode(scene.Root);
        var modelBag = new ConcurrentBag<SceneNode>();

        // var amdl = _context.GetFileByUri("data://character/nh/nh00/nh00.amdl");
        // var armature = AnimationModel.FromData(amdl, false);
        //
        // var index = -1;
        // for (var i = 0; i < armature.CustomUserData.CustomUserDataIndexNodes.Length; i++)
        // {
        //     if (armature.CustomUserData.CustomUserDataIndexNodes[i].Type ==
        //         LmEAnimCustomDataType.eCustomUserDataType_SkeletalAnimInfo)
        //     {
        //         index = i;
        //         break;
        //     }
        // }
        //
        // var bones = new List<Bone>();
        // var skeletalAnimInfo = (LmSkeletalAnimInfo)armature.CustomUserData.CustomUserDatas[index];
        //
        // try
        // {
        //     for (var i = 0; i < skeletalAnimInfo.Transformations.Length; i++)
        //     {
        //         bones.Add(new Bone
        //         {
        //             Name = armature.BoneNames[i],
        //             BindPose = new Matrix(skeletalAnimInfo.Transformations[i].Values).Inverted(),
        //             BoneLocalTransform = Matrix.Identity,
        //             InvBindPose = new Matrix(skeletalAnimInfo.Transformations[i].Values)
        //         });
        //     }
        // }
        // catch (Exception e)
        // {
        //     Console.WriteLine(e);
        //     throw;
        // }

        var model = new ModelReader(gmdl, gpubin).Read();
        using var context = new FlagrumDbContext(_context.Settings);
        var fidelityInt = context.GetInt(StateKey.ViewportTextureFidelity);
        _fidelity = fidelityInt == -1 ? ModelViewerTextureFidelity.Low : (ModelViewerTextureFidelity)fidelityInt;

        // Calculate paths
        var gpubinUri = model.Header.Dependencies.First(d => d.Path.EndsWith(".gpubin")).Path;
        var modelContext = new FileSystemModelContext
        {
            RootUri = gpubinUri[..gpubinUri.LastIndexOf('/')],
            RootDirectory = inputPath == null ? null : string.Join('\\', inputPath.Split('\\')[..^1])
        };

        Parallel.ForEach(model.MeshObjects, meshObject =>
        {
            Parallel.ForEach(meshObject.Meshes.Where(m => m.LodNear == 0), mesh =>
            {
                FlagrumDbContext? context = null;
                if (view == AssetExplorerView.GameView)
                {
                    context = new FlagrumDbContext(_context.Settings);
                }

                object? contextObject = view == AssetExplorerView.FileSystem
                    ? modelContext
                    : context;

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
                
                var textureCoordinates = mesh.UVMaps.Count > 0
                    ? mesh.UVMaps[0].UVs.Select(uv => new Vector2((float)uv.U, (float)uv.V)).ToList()
                    : mesh.VertexPositions.Select(uv => new Vector2(0f, 0f)).ToList();

                builder.Append(
                    mesh.VertexPositions.Select(v => new Vector3(v.X, v.Y, v.Z)).ToList(),
                    faceIndices,
                    mesh.Normals.Select(n => new Vector3(n.X, n.Y, n.Z)).ToList(),
                    textureCoordinates);

                var meshNode = new BoneSkinMeshNode();
                var material = new PBRMaterial
                {
                    AlbedoColor = Color4.White
                };

                if (_fidelity != ModelViewerTextureFidelity.None)
                {
                    var materialUri = model.Header.Dependencies
                        .FirstOrDefault(d => d.PathHash == mesh.DefaultMaterialHash.ToString())
                        !.Path;

                    if (materialUri != null)
                    {
                        var materialData = GetDataByUri(materialUri, contextObject, view);
                        if (materialData == null)
                        {
                            return;
                        }

                        var gfxbin = new MaterialReader(materialData).Read();
                        var diffuseUri = gfxbin.Textures
                            .FirstOrDefault(t => t.ShaderGenName.Contains("BaseColor0"))
                            ?.Path;

                        var normalUri = gfxbin.Textures
                            .FirstOrDefault(t => t.ShaderGenName.Contains("Normal0"))
                            ?.Path;

                        if (diffuseUri != null && normalUri != null)
                        {
                            // var highDiffuseUri = diffuseUri.Insert(diffuseUri.LastIndexOf('.'), "_$h");
                            // var highNormalUri = normalUri.Insert(normalUri.LastIndexOf('.'), "_$h");
                            //
                            // var diffuse = _context.GetFileByUri(highDiffuseUri);
                            // var normal = _context.GetFileByUri(highNormalUri);
                            //
                            // if (diffuse.Length == 0)
                            // {
                            //     diffuse = _context.GetFileByUri(diffuseUri);
                            // }
                            //
                            // if (normal.Length == 0)
                            // {
                            //     normal = _context.GetFileByUri(normalUri);
                            // }

                            // var textures = context.GetFilesByUri(new[] {diffuseUri, normalUri});
                            // var diffuse = textures[diffuseUri];
                            // var normal = textures[normalUri];

                            var diffuse = GetDataByUri(diffuseUri, contextObject, view);
                            var normal = GetDataByUri(normalUri, contextObject, view);

                            var textureConverter = new TextureConverter();

                            TextureModel? albedoMap = null;
                            TextureModel? normalMap = null;

                            Parallel.Invoke(() =>
                                {
                                    if (diffuse is {Length: > 0})
                                    {
                                        var tag = Encoding.UTF8.GetString(diffuse[..8]);

                                        if (tag == "SEDBbtex")
                                        {
                                            diffuse = textureConverter.BtexToPng(diffuse);
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

                                        if (tag == "SEDBbtex")
                                        {
                                            normal = textureConverter.BtexToPng(normal);
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

                    if (view == AssetExplorerView.GameView)
                    {
                        context?.Dispose();
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

        _mainViewModel.ModelGroup = modelGroup;
        _mainViewModel.Viewer.ZoomExtents(0);
    }

    private byte[]? GetDataByUri(string uri, object? context, AssetExplorerView view)
    {
        if (view == AssetExplorerView.GameView)
        {
            var high = uri.Insert(uri.LastIndexOf('.'), "_$h");
            var highest = high.Replace("/sourceimages/", "/highimages/");

            if (_fidelity >= ModelViewerTextureFidelity.Highest)
            {
                var data = ((FlagrumDbContext?)context).GetFileByUri(highest);
                if (data.Length > 0)
                {
                    return data;
                }
            }

            if (_fidelity >= ModelViewerTextureFidelity.High)
            {
                var data = ((FlagrumDbContext?)context).GetFileByUri(high);
                if (data.Length > 0)
                {
                    return data;
                }
            }

            if (_fidelity >= ModelViewerTextureFidelity.Low)
            {
                var data = ((FlagrumDbContext?)context).GetFileByUri(uri);
                if (data.Length > 0)
                {
                    return data;
                }
            }

            return null;
        }

        var baseContext = (FileSystemModelContext)context;
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