using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flagrum.Gfxbin.Data;
using Flagrum.Gfxbin.Gmdl.Components;
using Flagrum.Gfxbin.Gmdl.Constructs;

namespace Flagrum.Gfxbin.Gmdl.Templates;

public static class OutfitTemplate
{
    public static Model Build(string exportPath, Gpubin gpubin)
    {
        var modelName = exportPath.Split('/', '\\').Last().Split('.').First();
        var header = BuildHeader(modelName, exportPath);

        return new Model
        {
            Header = header,
            Name = modelName,
            AssetHash = ulong.Parse(header.Dependencies
                .FirstOrDefault(d => d.Path.EndsWith(".gpubin"))!.PathHash),

            // For now just using values from a random mod to get things working
            // NOTE: Unsure what this does but should probably be calculated for the specific model
            Aabb = (new Vector3(-0.6734764f, -0.0012439584f, -0.16458078f),
                new Vector3(0.6734764f, 1.6263884f, 0.22657359f)),

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
                    // TODO: Update this to use proper material hash
                    Meshes = gpubin.Meshes.Select(m => BuildMesh(m.Name, 12345)).ToList()
                }
            },

            NodeTable = BuildNodeTable(gpubin.Meshes.Select(m => m.Name)),

            // We leave this empty as binmods don't support the parts system
            Parts = new List<ModelPart>()
        };
    }

    // TODO: Finish this method
    private static GfxbinHeader BuildHeader(string modelName, string exportPath)
    {
        var basePath = $"data://mod/{Path.GetDirectoryName(exportPath)}/";

        // Generate the dependencies as (ulong, string)

        // Generate these from the tuple list and add the extra two with the string hashes
        var dependencies = new List<DependencyPath>
        {
            new() {PathHash = "", Path = $"{basePath}{modelName}.gpubin"},
            new() {PathHash = "asset_uri", Path = basePath},
            new() {PathHash = "ref", Path = $"{basePath}{modelName}.gmdl"}
        };

        // This will be generated from just the ulongs in the tuple list
        // Is just a repeat of the hashes except for the last two that are strings
        var hashes = new List<ulong>();

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
        };

        nodeTable.AddRange(meshNames.Select(BuildNode));
        return nodeTable;
    }

    private static NodeInformation BuildNode(string nodeName)
    {
        return new NodeInformation
        {
            Name = nodeName,

            // NOTE: Mods appear to use a scale factor of 100 to render correctly?
            // Does this even actually do anything in-game?
            Matrix = new Matrix(
                new Vector3(100, 0, 0),
                new Vector3(0, 100, 0),
                new Vector3(0, 0, 100),
                new Vector3(0, 0, 0))
        };
    }

    private static Mesh BuildMesh(string meshName, ulong materialHash)
    {
        return new Mesh
        {
            Name = meshName,

            // For now just using values from a random mod to get things working
            // NOTE: Unsure what this does but should probably be calculated for the specific mesh
            Aabb = new Aabb(new Vector3(-0.6734764f, -0.0012439584f, -0.16458078f),
                new Vector3(0.6734764f, 1.6263884f, 0.22657359f)),

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
            VertexLayoutType = VertexLayoutType.Skinning_4Bones,

            LowLodShadowCascadeNo = 2, // Always 2
            IsOrientedBB = true, // Always true

            // For now just using values from a random mod to get things working
            // NOTE: Unsure what this does but should probably be calculated for the specific mesh
            OrientedBB = new OrientedBB(
                new Vector3(0, 1.293f, 0.018f),
                new Vector3(0, 0, -0.123f),
                new Vector3(0, 0.272f, 0),
                new Vector3(0.301f, 0, 0)
            ),

            // These will always be the same, shouldn't need to ever change
            PrimitiveType = PrimitiveType.PrimitiveTypeTriangleList,

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