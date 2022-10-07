using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Core.Animation;
using Flagrum.Core.Animation.AnimationClip;
using Flagrum.Core.Gfxbin.Gmtl;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;
using Flagrum.Web.Services;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Animations;
using HelixToolkit.SharpDX.Core.Assimp;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using ModelReader = Flagrum.Core.Gfxbin.Gmdl.ModelReader;

namespace Flagrum.Desktop.Architecture;

public class BoneData
{
    private BoneData? _parent;

    public int Id { get; set; }
    public string Name { get; set; }
    public Matrix Matrix { get; set; }

    public BoneData? Parent
    {
        get => _parent;
        set
        {
            _parent = value;
            if (_parent?.Children.Contains(this) == false)
            {
                _parent.Children.Add(this);
            }
        }
    }

    public ICollection<BoneData> Children { get; set; } = new List<BoneData>();

    public GroupNode Node { get; set; }

    public void GenerateNode()
    {
        Node = new GroupNode
        {
            Name = Name,
            //ModelMatrix = Matrix.Identity,//Matrix,
            IsAnimationNode = true
        };
    }

    public void Traverse(Action<BoneData> action)
    {
        action(this);
        foreach (var child in Children)
        {
            child.Traverse(action);
        }
    }

    public void TraverseWithParent(BoneData? parent, Action<BoneData?, BoneData> action)
    {
        action(parent, this);

        foreach (var child in Children)
        {
            child.TraverseWithParent(this, action);
        }
    }
}

public class ViewportHelper
{
    private readonly FlagrumDbContext _context;
    private readonly MainViewModel _mainViewModel;

    private Dictionary<string, int> _boneIdMap;
    private List<BoneData?> _bones;

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
        var root = new GroupNode();
        var scene = new HelixToolkitScene(root);
        modelGroup.AddNode(scene.Root);
        var modelBag = new ConcurrentBag<SceneNode>();

        var armature = BuildArmature();
        root.AddChildNode(armature.Node);

        var gpubinUri = gmdlUri.Replace(".gmdl", ".gpubin");
        var modelData = _context.GetFilesByUri(new[] {gmdlUri, gpubinUri});
        var gmdl = modelData[gmdlUri];
        var gpubin = modelData[gpubinUri];
        var model = new ModelReader(gmdl, gpubin).Read();

        var boneNameMap = model.BoneHeaders.ToDictionary(b => b.UniqueIndex, b => b.Name);

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

                var meshNode = new BoneSkinMeshNode
                {
                    Name = mesh.Name
                };

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
                var skinnedMesh = new BoneSkinnedMeshGeometry3D(builder.ToMesh());

                var usedIds = mesh.WeightIndices[0].SelectMany(i => i.Select(j => j)).Distinct();
                var meshBones = usedIds.OrderBy(i => i).Select(i =>
                {
                    var boneHeader = model.BoneHeaders.FirstOrDefault(b => b.UniqueIndex == i);
                    var match = _bones.FirstOrDefault(b => b!.Name == boneHeader!.Name)!;
                    return new Bone
                    {
                        Name = boneHeader!.Name,
                        BoneLocalTransform = match.Matrix.Inverted(), //Matrix.Identity,
                        Node = match.Node,
                        ParentNode = match.Parent!.Node,
                        //BindPose = Matrix.Identity,
                        //InvBindPose = Matrix.Identity
                        BindPose = match.Matrix.Inverted(),
                        InvBindPose = match.Matrix
                        //BoneLocalTransform = _bones.FirstOrDefault(b => b.Name == boneHeader.Name).Matrix
                    };
                }).ToList();

                // var meshBones = _bones.Select(b => new Bone
                // {
                //     Name = b.Name,
                //     BoneLocalTransform = Matrix.Identity,
                //     Node = b.Node,
                //     ParentNode = b.Parent?.Node,
                //     BindPose = b.Matrix.Inverted(),
                //     InvBindPose = b.Matrix
                // }).ToList();

                meshBones = meshBones.Select(mb =>
                {
                    var bone = mb;
                    if (bone.ParentNode != null)
                    {
                        var parent = meshBones.FirstOrDefault(m => m.Node == bone.ParentNode);
                        bone.ParentIndex = meshBones.IndexOf(parent);
                    }

                    return bone;
                }).ToList();

                skinnedMesh.VertexBoneIds = new List<BoneIds>();
                for (var i = 0; i < mesh.WeightIndices[0].Count; i++)
                {
                    var i2 = i;
                    skinnedMesh.VertexBoneIds.Add(new BoneIds
                    {
                        // Bone1 = mesh.WeightIndices[0][i][0] == 0 ? 0 : _boneIdMap[boneNameMap[mesh.WeightIndices[0][i][0]]],
                        // Bone2 = mesh.WeightIndices[0][i][1] == 0 ? 0 : _boneIdMap[boneNameMap[mesh.WeightIndices[0][i][1]]],
                        // Bone3 = mesh.WeightIndices[0][i][2] == 0 ? 0 : _boneIdMap[boneNameMap[mesh.WeightIndices[0][i][2]]],
                        // Bone4 = mesh.WeightIndices[0][i][3] == 0 ? 0 : _boneIdMap[boneNameMap[mesh.WeightIndices[0][i][3]]],
                        Bone1 = meshBones.IndexOf(meshBones.First(m =>
                            m.Name == boneNameMap[mesh.WeightIndices[0][i2][0]])),
                        Bone2 = meshBones.IndexOf(meshBones.First(m =>
                            m.Name == boneNameMap[mesh.WeightIndices[0][i2][1]])),
                        Bone3 = meshBones.IndexOf(meshBones.First(m =>
                            m.Name == boneNameMap[mesh.WeightIndices[0][i2][2]])),
                        Bone4 = meshBones.IndexOf(meshBones.First(m =>
                            m.Name == boneNameMap[mesh.WeightIndices[0][i2][3]])),
                        Weights = new Vector4(mesh.WeightValues[0][i].Select(b => b / 255f).ToArray())
                    });
                }

                meshNode.Bones = meshBones.ToArray();
                meshNode.Geometry = skinnedMesh;

                modelBag.Add(meshNode);
                var skeletonNode = meshNode.CreateSkeletonNode(DiffuseMaterials.Red, "color", 0.04f);
                modelBag.Add(skeletonNode);
            });
        });

        foreach (var node in modelBag)
        {
            var groupNode = new GroupNode
            {
                Name = node.Name
            };

            groupNode.AddChildNode(node);
            root.AddChildNode(groupNode);
        }

        var importer = new Importer();
        importer.Configuration.CreateSkeletonForBoneSkinningMesh = true;
        var import = importer.Load(@"C:\Users\Kieran\Desktop\nh00_010_animated.fbx");

        import.Animations[0].BoneSkinMeshes =
            modelBag.Where(m => m is BoneSkinMeshNode).Select(m => (IBoneMatricesNode)m).ToList();
        import.Animations[0].RootNode = scene.Root;

        var collection = new List<NodeAnimation>();
        foreach (var item in import.Animations[0].NodeAnimationCollection)
        {
            var node = item;
            node.Node = _bones.FirstOrDefault(b => b.Name == item.Node.Name)?.Node;
            if (node.Node != null)
            {
                collection.Add(node);
            }
        }

        import.Animations[0].NodeAnimationCollection = collection;

        scene.Animations = import.Animations;

        // var animation = new Animation(AnimationType.Node)
        // {
        //     RootNode = scene.Root,
        //     BoneSkinMeshes = modelBag.Where(m => m is BoneSkinMeshNode)
        //         .Select(m => (IBoneMatricesNode)m)
        //         .ToList()
        // };

        // var collection = _bones.Select(b =>
        // {
        //     if (b.Name == "L_UpperLeg")
        //     {
        //         return new NodeAnimation
        //         {
        //             Node = b.Node,
        //             KeyFrames = new FastList<Keyframe>(Enumerable.Range(0, 60).Select(i =>
        //             {
        //                 var rotation = Quaternion.RotationYawPitchRoll(0, 0, 0.1f * i);
        //                 return new Keyframe
        //                 {
        //                     Rotation = rotation,
        //                     Scale = new Vector3(1, 1, 1),
        //                     Time = 0.03333333333f * i,
        //                     Translation = new Vector3(0, 0, 0)
        //                 };
        //             }))
        //         };
        //     }
        //     else
        //     {
        //         b.Matrix.Decompose(out var scale, out var rotation, out var translation);
        //         return new NodeAnimation
        //         {
        //             Node = b.Node,
        //             KeyFrames = new FastList<Keyframe>(new[]
        //             {
        //                 new Keyframe
        //                 {
        //                     Rotation = rotation, 
        //                     Scale = scale, 
        //                     Time = 0,
        //                     Translation = translation
        //                 },
        //                 new Keyframe
        //                 {
        //                     Rotation = rotation, 
        //                     Scale = scale, 
        //                     Time = 2,
        //                     Translation = translation
        //                 }
        //             })
        //         };
        //     }
        // }).ToList();

        // var nodeAnimation = new NodeAnimation
        // {
        //     Node = _bones.First(b => b.Name == "L_UpperLeg").Node,
        //     KeyFrames = new FastList<Keyframe>(Enumerable.Range(0, 60).Select(i =>
        //     {
        //         var rotation = Quaternion.RotationYawPitchRoll(0, 0, 0.1f * i);
        //         return new Keyframe
        //         {
        //             Rotation = rotation,
        //             Scale = new Vector3(1, 1, 1),
        //             Time = 0.03333333333f * i,
        //             Translation = new Vector3(0, 0, 0)
        //         };
        //     }))
        // };
        //
        // var collection = new List<NodeAnimation>();
        // //collection.Add(nodeAnimation);
        // animation.NodeAnimationCollection = collection;
        // animation.StartTime = 0;
        // animation.EndTime = 2f;
        // scene.Animations = new List<Animation> {animation};

        _mainViewModel.ModelGroup = modelGroup;
        _mainViewModel.Viewer.ZoomExtents(0);

        _mainViewModel.AnimationUpdater = new NodeAnimationUpdater(scene.Animations.First());


        // var importer = new Importer();
        // importer.Configuration.CreateSkeletonForBoneSkinningMesh = true;
        // importer.Configuration.SkeletonSizeScale = 0.04f;
        // importer.Configuration.GlobalScale = 0.1f;
        // var imported = importer.Load(@"C:\Users\Kieran\Desktop\nh00_010_animated.fbx");
        // _mainViewModel.ModelGroup = new SceneNodeGroupModel3D();
        //
        // foreach (var node in imported.Root.Traverse())
        // {
        //     if (node is BoneSkinMeshNode {IsSkeletonNode: false} n)
        //     {
        //         n.Visible = false;
        //     }
        // }
        //
        // _mainViewModel.ModelGroup.AddNode(imported.Root);
        // _mainViewModel.AnimationUpdater = new NodeAnimationUpdater(imported.Animations.First());
    }

    private BoneData BuildArmature()
    {
        var amdl = _context.GetFileByUri("data://character/nh/nh00/nh00.amdl");
        var animationModel = AnimationModel.FromData(amdl, false);

        var index = -1;
        for (var i = 0; i < animationModel.CustomUserData.CustomUserDataIndexNodes.Length; i++)
        {
            if (animationModel.CustomUserData.CustomUserDataIndexNodes[i].Type ==
                LmEAnimCustomDataType.eCustomUserDataType_SkeletalAnimInfo)
            {
                index = i;
                break;
            }
        }

        var armature = new BoneData
        {
            Id = -1,
            Matrix = Matrix.Identity,
            Name = "Armature"
        };

        _bones = new List<BoneData?>();
        var skeletalAnimInfo = (LmSkeletalAnimInfo)animationModel.CustomUserData.CustomUserDatas[index];

        var parentIndices = new List<short>();
        for (var i = 0; i < skeletalAnimInfo.ParentIndices.Length; i++)
        {
            var id = skeletalAnimInfo.ParentIndices[i];

            if (id == -1)
            {
                continue;
            }

            parentIndices.Add(id);
        }

        _boneIdMap = new Dictionary<string, int>();

        for (var i = 0; i < skeletalAnimInfo.ParentIndices.Length; i++)
        {
            _boneIdMap.Add(animationModel.BoneNames[i], i);
            _bones.Add(new BoneData
            {
                Id = i,
                Name = animationModel.BoneNames[i],
                Matrix = new Matrix(skeletalAnimInfo.BindPoseTransformations[i].Values)
                //Matrix = Matrix.Identity
            });
        }

        for (var i = 0; i < _bones.Count; i++)
        {
            if (i > 0)
            {
                _bones[i].Parent = _bones.FirstOrDefault(b => b?.Id == parentIndices[i - 1]) ?? armature;
            }
            else
            {
                _bones[i].Parent = armature;
            }
        }

        armature.GenerateNode();

        foreach (var bone in _bones)
        {
            bone.GenerateNode();
        }

        armature.TraverseWithParent(null, (parent, child) =>
        {
            if (parent == null)
            {
                child.Node.ModelMatrix = child.Matrix;
            }
            else
            {
                child.Node.ModelMatrix = parent.Node.ModelMatrix.Inverted() * child.Matrix;
            }
        });

        armature.Traverse(bone =>
        {
            foreach (var child in bone.Children)
            {
                bone.Node.AddChildNode(child.Node);
            }
        });

        return armature;
    }
}