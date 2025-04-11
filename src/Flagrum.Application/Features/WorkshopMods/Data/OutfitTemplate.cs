using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Flagrum.Core.Graphics.Models;
using Flagrum.Core.Mathematics;
using Flagrum.Core.Utilities;
using Flagrum.Core.Utilities.Extensions;
using Flagrum.Application.Features.WorkshopMods.Data.Model;

namespace Flagrum.Core.Gfxbin.Gmdl.Templates;

public static class OutfitTemplate
{
    public static GameModel Build(string modDirectoryName, string modelName, string modelNamePrefix,
        BinmodModelData gpubin)
    {
        var model = BuildGraphicsBinary(modDirectoryName, modelName, modelNamePrefix, gpubin);
        var allVertices = gpubin.Meshes
            .SelectMany(m => m.VertexPositions)
            .ToList();

        model.Name = modelName;
        model.GpubinHashes = new List<ulong>
            {ulong.Parse(model.Dependencies.FirstOrDefault(d => d.Value.EndsWith(".gpubin"))!.Key)};

        model.AxisAlignedBoundingBox = new AxisAlignedBoundingBox(
            new Vector3(
                allVertices.Min(v => v.X),
                allVertices.Min(v => v.Y),
                allVertices.Min(v => v.Z)),
            new Vector3(
                allVertices.Max(v => v.X),
                allVertices.Max(v => v.Y),
                allVertices.Max(v => v.Z)));

        model.Unknown2 = true;

        // These are always the same and should be left at these values
        model.ChildClassFormat = new List<uint>();
        model.InstanceNameFormat = "";
        model.ShaderClassFormat = "";
        model.ShaderParameterListFormat = new List<uint>();
        model.ShaderSamplerDescriptionFormat = new List<uint>();

        // These aren't used at this stage, unlikely they ever will be
        model.HasPsdPath = false;
        model.PsdPathHash = 0;

        model.MeshObjects = new List<GameModelMeshObject>
        {
            new()
            {
                // These values never seem to vary, so we just leave them as-is
                Name = "Base_Parts",
                Clusters = new List<string> {"CLUSTER_NAME"},
                Meshes = gpubin.Meshes.Select(m => BuildMesh(m,
                        Cryptography.HashFileUri64(
                            $"data://mod/{modDirectoryName}/materials/{modelNamePrefix}{m.Name.ToSafeString()}_mat.gmtl")))
                    .ToList()
            }
        };

        model.Nodes = BuildNodeTable(gpubin.Meshes.Select(m => m.Name));

        // We leave this empty as binmods don't support the parts system
        model.Parts = new List<GameModelPart>();

        return model;
    }

    private static GameModel BuildGraphicsBinary(string modDirectoryName, string modelName,
        string modelNamePrefix,
        BinmodModelData gpubin)
    {
        var basePath = $"data://mod/{modDirectoryName}";
        var gpubinUri = $"{basePath}/{modelName}.gpubin";
        var gpubinHash = Cryptography.HashFileUri64(gpubinUri);

        var materials = gpubin.Meshes.Select(m => (
                hash: Cryptography.HashFileUri64(
                    $"data://mod/{modDirectoryName}/materials/{modelNamePrefix}{m.Name.ToSafeString()}_mat.gmtl"),
                uri: $"{basePath}/materials/{modelNamePrefix}{m.Name.ToSafeString()}_mat.gmtl"
            ))
            .ToList();

        var dependencies = materials.ToDictionary(m => m.hash.ToString(), m => m.uri);
        dependencies[gpubinHash.ToString()] = gpubinUri;
        dependencies["asset_uri"] = basePath + "/";
        dependencies["ref"] = $"{basePath}/{modelName}.gmdl";

        var hashes = materials.Select(m => m.hash).ToList();
        hashes.Add(gpubinHash);

        return new GameModel
        {
            // Always use the latest version as we have no reason to use anything else
            Version = 20160705,
            Dependencies = dependencies,
            Hashes = hashes
        };
    }

    private static List<GameModelNode> BuildNodeTable(IEnumerable<string> meshNames)
    {
        var nodeTable = new List<GameModelNode>
        {
            BuildNode("Armature")
        };

        nodeTable.AddRange(meshNames.Select(BuildNode));
        return nodeTable;
    }

    private static GameModelNode BuildNode(string nodeName)
    {
        return new GameModelNode
        {
            Name = nodeName,

            // Does this even actually do anything in-game?
            Matrix = new Matrix3x4(
                new Vector3(1, 0, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, 0, 1),
                new Vector3(0, 0, 0))
        };
    }

    private static GameModelMesh BuildMesh(BinmodModelDataMesh binmodMesh, ulong materialHash)
    {
        var mesh = new GameModelMesh
        {
            Name = binmodMesh.Name,
            Flags = 262272,
            InstanceNumber = 0,                    // Seems to always be 0
            LodNear = 0,                           // Not currently dealing with LODs
            LodFar = 0,                            // Not currently dealing with LODs
            LodFade = 0,                           // Not currently dealing with LODs
            PartsId = 0,                           // Not currently supporting parts system
            Parts = new List<GameModelMeshPart>(), // Leave empty, not currently supporting parts system
            BreakableBoneIndex = 4294967295,       // This appears to always be the same, equivalent to ushort.MaxValue

            MaterialHash = materialHash,
            DrawPriorityOffset = 0, // Always 0

            // Using anything other than Skinning_4Bones appears to break things (no rigging and ghostlike appearance)
            // We suspect this is a limitation of binmods, but currently unsure
            VertexLayoutType = VertexLayoutType.Skinning_4Bones,

            LowLodShadowCascadeNo = 2, // Always 2
            IsOrientedBB = true,

            // These will always be the same, shouldn't need to ever change
            PrimitiveType = PrimitiveType.PrimitiveTypeTriangleList,

            // All models have one of these with everything set to 0, so we do the same so nothing breaks
            Subgeometries = new List<GameModelSubgeometry>
            {
                new()
                {
                    AABB = new AxisAlignedBoundingBox(new Vector3(0, 0, 0), new Vector3(0, 0, 0))
                }
            },

            VertexStreams = new List<VertexStream>()
        };

        return mesh;
    }
}