using System.Collections.Generic;
using System.Linq;
using Flagrum.Core.Gfxbin.Data;
using Flagrum.Core.Gfxbin.Gmdl.Components;
using Flagrum.Core.Gfxbin.Gmdl.Constructs;
using Flagrum.Core.Utilities;

namespace Flagrum.Core.Gfxbin.Gmdl.Templates;

public static class OutfitTemplate
{
    public static Model Build(string modDirectoryName, string modelName, Gpubin gpubin)
    {
        var header = BuildHeader(modDirectoryName, modelName, gpubin);
        var allVertices = gpubin.Meshes
            .SelectMany(m => m.VertexPositions)
            .ToList();

        return new Model
        {
            Header = header,
            Name = modelName,
            AssetHash = ulong.Parse(header.Dependencies
                .FirstOrDefault(d => d.Path.EndsWith(".gpubin"))!.PathHash),

            // NOTE: Unsure what the bounding box is used for in-game, possibly culling?
            Aabb = new Aabb(
                new Vector3(
                    allVertices.Min(v => v.X),
                    allVertices.Min(v => v.Y),
                    allVertices.Min(v => v.Z)),
                new Vector3(
                    allVertices.Max(v => v.X),
                    allVertices.Max(v => v.Y),
                    allVertices.Max(v => v.Z))),

            // Unsure what this is, but appears to always be true, so we use that
            Unknown1 = true,

            // These are always the same and should be left at these values
            ChildClassFormat = 144,
            InstanceNameFormat = 160,
            ShaderClassFormat = 160,
            ShaderParameterListFormat = 144,
            ShaderSamplerDescriptionFormat = 144,

            // These aren't used at this stage, unlikely they ever will be
            HasPsdPath = false,
            PsdPathHash = 0,

            // Models seem to only ever have one mesh object
            MeshObjects = new List<MeshObject>
            {
                new()
                {
                    // These values never seem to vary, so we just leave them as-is
                    Name = "Base_Parts",
                    ClusterCount = 1,
                    ClusterName = "CLUSTER_NAME",
                    Meshes = gpubin.Meshes.Select(m => BuildMesh(m.Name,
                            Cryptography.HashFileUri64($"data://mod/{modDirectoryName}/materials/{m.Name.ToLower()}_mat.gmtl"),
                            m.MaterialType))
                        .ToList()
                }
            },

            NodeTable = BuildNodeTable(gpubin.Meshes.Select(m => m.Name)),

            // We leave this empty as binmods don't support the parts system
            Parts = new List<ModelPart>()
        };
    }

    private static GfxbinHeader BuildHeader(string modDirectoryName, string modelName, Gpubin gpubin)
    {
        var basePath = $"data://mod/{modDirectoryName}";
        var gpubinUri = $"{basePath}/{modelName}.gpubin";
        var gpubinHash = Cryptography.HashFileUri64(gpubinUri);

        var materials =
            gpubin.Meshes.Select(m => (
                    hash: Cryptography.HashFileUri64($"data://mod/{modDirectoryName}/materials/{m.Name.ToLower()}_mat.gmtl"),
                    uri: $"{basePath}/materials/{m.Name.ToLower()}_mat.gmtl"
            ));

        var dependencies = new List<DependencyPath>();
        dependencies.AddRange(materials.Select(m => new DependencyPath
        {
            PathHash = m.hash.ToString(),
            Path = m.uri
        }));

        dependencies.Add(new DependencyPath {PathHash = gpubinHash.ToString(), Path = gpubinUri});
        dependencies.Add(new DependencyPath {PathHash = "asset_uri", Path = basePath + "/"});
        dependencies.Add(new DependencyPath {PathHash = "ref", Path = $"{basePath}/{modelName}.gmdl"});

        var hashes = materials.Select(m => m.hash).ToList();
        hashes.Add(gpubinHash);

        return new GfxbinHeader
        {
            // Always use the latest version as we have no reason to use anything else
            Version = 20160705,
            Dependencies = dependencies,
            Hashes = hashes
        };
    }

    private static List<NodeInformation> BuildNodeTable(IEnumerable<string> meshNames)
    {
        var nodeTable = new List<NodeInformation>
        {
            BuildNode("Armature")
            // BuildNode("Root"),
            // BuildNode("Trans"),
            // BuildNode("Proxy"),
            // BuildNode("Interest"),
            // BuildNode("Mesh")
        };

        nodeTable.AddRange(meshNames.Select(BuildNode));
        return nodeTable;
    }

    private static NodeInformation BuildNode(string nodeName)
    {
        return new NodeInformation
        {
            Name = nodeName,

            // Does this even actually do anything in-game?
            Matrix = new Matrix(
                new Vector3(1, 0, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, 0, 1),
                new Vector3(0, 0, 0))
        };
    }

    private static Mesh BuildMesh(string meshName, ulong materialHash, MaterialType materialType)
    {
        return new Mesh
        {
            Name = meshName,

            // Not really sure why these appear twice, but not a problem for now
            // These defaults work fine for all outfit meshes for now
            Flag = 262272,
            // NOTE: This is 262276 on nh00_010, this may be relevant in some capacity
            Flags = 262272,

            InstanceNumber = 0, // Seems to always be 0
            LodNear = 0, // Not currently dealing with LODs
            LodFar = 0, // Not currently dealing with LODs
            LodFade = 0, // Not currently dealing with LODs
            PartsId = 0, // Not used by binmods
            MeshParts = new List<MeshPart>(), // Leave empty, not used by binmods
            BreakableBoneIndex = 4294967295, // Not sure what this does, leave as default

            // Will need material editor to generate a hash for us to put here
            // These will almost certainly correspond to the hashes in the GfxbinHeader of this file
            DefaultMaterialHash = materialHash,
            DrawPriorityOffset = 0, // Always 0

            // NOTE: Currently suspecting that this may control the weight limit of the vertices
            // i.e. Skinning_8Bones may allow 8 weights per vertex (e.g. BLENDWEIGHTS1 may work?)
            // Using the above types currently breaks the mod (no rigging and ghostlike appearance)
            // Perhaps other changes are needed to enable this?
            VertexLayoutType = materialType switch
            {
                MaterialType.OneWeight => VertexLayoutType.Skinning_1Bones,
                _ => VertexLayoutType.Skinning_4Bones
            },

            LowLodShadowCascadeNo = 2, // Always 2
            IsOrientedBB = true, // Always true

            // These will always be the same, shouldn't need to ever change
            PrimitiveType = PrimitiveType.PrimitiveTypeTriangleList,
            
            MaterialType = materialType,

            // These shouldn't ever need to be changed
            Unknown1 = 0,
            Unknown2 = false,
            Unknown3 = false,
            Unknown4 = 0,
            Unknown5 = 0,
            Unknown6 = false,

            // All models have one of these with everything set to 0, so we do the same so nothing breaks
            SubGeometries = new List<SubGeometry>
            {
                new()
                {
                    Aabb = new Aabb(new Vector3(0, 0, 0), new Vector3(0, 0, 0))
                }
            }
        };
    }
}