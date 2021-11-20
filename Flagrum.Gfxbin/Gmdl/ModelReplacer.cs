using System;
using System.Collections.Generic;
using System.Linq;
using Flagrum.Gfxbin.Gmdl.Components;
using Flagrum.Gfxbin.Gmdl.Constructs;

namespace Flagrum.Gfxbin.Gmdl;

/// <summary>
///     Temporary class to handle placing mesh data into an existing GFXBIN
///     to avoid manually filling out every piece of data
/// </summary>
public class ModelReplacer
{
    private readonly Gpubin _gpubin;
    private readonly Model _model;

    public ModelReplacer(Model originalModel, Gpubin replacementData)
    {
        _model = originalModel;
        _gpubin = replacementData;
    }

    public Model Replace()
    {
        var usedIndices = new List<ushort>();
        foreach (var mesh in _model.MeshObjects[0].Meshes)
        {
            var match = _gpubin.Meshes.FirstOrDefault(m => m.Name == mesh.Name);
            if (match != null)
            {
                // Replace model data with the imported data
                mesh.Normals = match.Normals;
                mesh.Tangents = match.Tangents;
                mesh.FaceIndices = match.FaceIndices;
                mesh.VertexCount = (uint)match.VertexPositions.Count;
                mesh.VertexPositions = match.VertexPositions;
                mesh.WeightIndices = match.WeightIndices;

                // Convert weights back to bytes as JSON.NET doesn't handle byte numbers well
                mesh.WeightValues = match.WeightValues
                    .Select(m => m.Select(n => n.Select(o => (byte)o).ToArray()).ToList()).ToList();

                // Convert the UV coords back to half-precision floats
                mesh.UVMaps = match.UVMaps.Select(m => new UVMap
                {
                    UVs = m.UVs.Select(uv => new UV
                    {
                        U = (Half)uv.U,
                        V = (Half)uv.V
                    }).ToList()
                }).ToList();

                // Generate list of Bone IDs used in this mesh
                var boneIds = new List<uint>();
                foreach (var weightIndexArray in mesh.WeightIndices.SelectMany(weightIndexMap => weightIndexMap))
                {
                    boneIds.AddRange(weightIndexArray.Select(w => (uint)w));
                }

                mesh.BoneIds = boneIds.Distinct().OrderBy(i => i);
                usedIndices.AddRange(boneIds.Distinct().Select(i => (ushort)i));
            }
        }

        // Create arbitrary indices for the bones
        // Start at 10000 to avoid conflicts with other loaded bones on the target model
        ushort count = 10000;
        usedIndices = usedIndices.Distinct().ToList();
        var indexMap = usedIndices.ToDictionary(i => i, i => count++);

        // Need to hardcode this index to prevent Noct's finger from deforming incorrectly
        // Presuming this is because this bone is assigned this index on another one of his models
        var lucii = _gpubin.BoneTable.FirstOrDefault(p => p.Value == "R_Middle1");
        if (lucii.Value != null)
        {
            indexMap[(ushort)lucii.Key] = 341;
        }

        // Update each weight index in the mesh to match the new index map
        foreach (var mesh in _model.MeshObjects[0].Meshes)
        {
            foreach (var indexList in mesh.WeightIndices)
            {
                foreach (var indices in indexList)
                {
                    for (var i = 0; i < indices.Length; i++)
                    {
                        indices[i] = indexMap[indices[i]];
                    }
                }
            }
        }

        // Generate the fixed bone table and apply it to the model
        _model.BoneHeaders = _gpubin.BoneTable
            .Where(d => usedIndices.Contains((ushort)d.Key))
            .Select(kvp => new BoneHeader
            {
                Name = kvp.Value,
                // Indices are stored in the bone table as the original number shifted left 16 bits
                // With the last 16 bits all set to 1
                // Unsure why this is, but we do the same to ensure the model loads correctly
                LodIndex = ((uint)indexMap[(ushort)kvp.Key] << 16) | 0xFFFF
            })
            .OrderBy(b => b.LodIndex)
            .ToList();

        return _model;
    }
}