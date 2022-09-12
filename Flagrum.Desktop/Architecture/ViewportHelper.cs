using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Core.Gfxbin.Gmtl;
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

    public ViewportHelper(
        MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
        _context = new FlagrumDbContext(new SettingsService());
    }

    public async Task ChangeModel(string gmdlUri)
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

        var gpubinUri = gmdlUri.Replace(".gmdl", ".gpubin");
        var modelData = _context.GetFilesByUri(new[] {gmdlUri, gpubinUri});
        var gmdl = modelData[gmdlUri];
        var gpubin = modelData[gpubinUri];
        var model = new ModelReader(gmdl, gpubin).Read();

        Parallel.ForEach(model.MeshObjects, meshObject =>
        {
            Parallel.ForEach(meshObject.Meshes.Where(m => m.LodNear == 0), mesh =>
            {
                using var context = new FlagrumDbContext(_context.Settings);

                var builder = new MeshBuilder();
                var faceIndices = new List<int>();

                for (var i = 0; i < mesh.FaceIndices.Length / 3; i++)
                {
                    for (var j = 0; j < 3; j++)
                    {
                        faceIndices.Add((int)mesh.FaceIndices[i, j]);
                    }
                }

                builder.Append(
                    mesh.VertexPositions.Select(v => new Vector3(v.X, v.Y, v.Z)).ToList(),
                    faceIndices,
                    mesh.Normals.Select(n => new Vector3(n.X, n.Y, n.Z)).ToList(),
                    mesh.UVMaps[0].UVs.Select(uv => new Vector2((float)uv.U, (float)uv.V)).ToList());

                var meshNode = new BoneSkinMeshNode();
                var material = new PBRMaterial
                {
                    AlbedoColor = Color4.White
                };

                var materialUri = model.Header.Dependencies
                    .FirstOrDefault(d => d.PathHash == mesh.DefaultMaterialHash.ToString())
                    !.Path;

                if (materialUri != null)
                {
                    var materialData = context.GetFileByUri(materialUri);
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

                        var textures = context.GetFilesByUri(new[] {diffuseUri, normalUri});
                        var diffuse = textures[diffuseUri];
                        var normal = textures[normalUri];

                        var textureConverter = new TextureConverter();

                        TextureModel albedoMap = null;
                        TextureModel normalMap = null;

                        Parallel.Invoke(() =>
                            {
                                if (diffuse.Length > 0)
                                {
                                    diffuse = textureConverter.BtexToPng(diffuse);
                                    var diffuseStream = new MemoryStream(diffuse);
                                    diffuseStream.Seek(0, SeekOrigin.Begin);
                                    albedoMap = TextureModel.Create(diffuseStream);
                                }
                            },
                            () =>
                            {
                                if (normal.Length > 0)
                                {
                                    normal = textureConverter.BtexToPng(normal);
                                    var normalStream = new MemoryStream(normal);
                                    normalStream.Seek(0, SeekOrigin.Begin);
                                    normalMap = TextureModel.Create(normalStream);
                                }
                            });

                        material.AlbedoMap = albedoMap;
                        material.NormalMap = normalMap;
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
}