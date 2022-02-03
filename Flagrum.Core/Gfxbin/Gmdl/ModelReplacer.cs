using System;
using System.Collections.Generic;
using System.Linq;
using Flagrum.Core.Gfxbin.Gmdl.Components;
using Flagrum.Core.Gfxbin.Gmdl.Constructs;

namespace Flagrum.Core.Gfxbin.Gmdl;

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

    public Model Replace(bool isModelReplacement)
    {
        var usedIndices = new List<uint>();

        foreach (var mesh in _model.MeshObjects[0].Meshes)
        {
            var match = _gpubin.Meshes.FirstOrDefault(m => m.Name == mesh.Name);
            if (match != null)
            {
                mesh.VertexCount = (uint)match.VertexPositions.Count;

                var weightValues = match.WeightValues
                    .Select(m => m.Select(n => n.Select(o => (byte)o).ToArray()).ToList()).ToList();

                var weightIndices = match.WeightIndices
                    .Select(m => m.Select(n => n.Select(o => o).ToArray()).ToList()).ToList();

                var limit = mesh.WeightLimit;
                if (limit > 4)
                {
                    mesh.VertexLayoutType = VertexLayoutType.Skinning_6Bones;
                }

                for (var i = 0; i < mesh.VertexCount; i++)
                {
                    var sum = 0;
                    for (var s = 0; s < limit; s++)
                    {
                        if (s < 4)
                        {
                            if (s < weightValues[0][i].Length)
                            {
                                sum += weightValues[0][i][s];
                            }
                        }
                        else
                        {
                            if (s - 4 < weightValues[1][i].Length)
                            {
                                sum += weightValues[1][i][s - 4];
                            }
                        }
                    }

                    for (var s = limit; s < 8; s++)
                    {
                        if (s - 4 < weightValues[1][i].Length)
                        {
                            weightIndices[1][i][s - 4] = 0;
                            weightValues[1][i][s - 4] = 0;
                        }
                    }

                    if (sum != 0 && sum != 255)
                    {
                        var difference = 255 - sum;
                        var counter = 0;

                        while (difference > 0)
                        {
                            var weight = counter >= weightValues[0][i].Length
                                ? weightValues[1][i]
                                : weightValues[0][i];

                            var index = counter >= weightValues[0][i].Length
                                ? counter - weightValues[0][i].Length
                                : counter;

                            if (weight.Length > 0 && weight[index] > 0 && weight[index] < 255)
                            {
                                weight[index]++;
                                difference--;
                            }

                            counter++;
                            var threshold = weightValues[0][i].Length + weightValues[1][i].Length;
                            if (counter == (threshold < limit ? threshold : limit))
                            {
                                counter = 0;
                            }
                        }

                        while (difference < 0)
                        {
                            var weight = counter >= weightValues[0][i].Length
                                ? weightValues[1][i]
                                : weightValues[0][i];

                            var index = counter >= weightValues[0][i].Length
                                ? counter - weightValues[0][i].Length
                                : counter;

                            if (weight.Length > 0 && weight[index] > 0)
                            {
                                weight[index]--;
                                difference++;
                            }

                            counter++;
                            var threshold = weightValues[0][i].Length + weightValues[1][i].Length;
                            if (counter == (threshold < limit ? threshold : limit))
                            {
                                counter = 0;
                            }
                        }
                    }
                }

                // Replace model data with the imported data
                mesh.Normals = match.Normals;
                mesh.Tangents = match.Tangents;
                mesh.FaceIndices = match.FaceIndices;
                mesh.VertexPositions = match.VertexPositions;
                mesh.ColorMaps = match.ColorMaps;
                mesh.WeightIndices = weightIndices;
                mesh.WeightValues = weightValues;

                // Convert the UV coords back to half-precision floats
                mesh.UVMaps = match.UVMaps.Select(m => new UVMap
                {
                    UVs = m.UVs.Select(uv => new UV
                    {
                        U = (Half)uv.U,
                        V = (Half)uv.V
                    }).ToList()
                }).ToList();

                // Calculate the bounding box for this mesh
                mesh.Aabb = new Aabb(
                    new Vector3(
                        mesh.VertexPositions.Min(v => v.X),
                        mesh.VertexPositions.Min(v => v.Y),
                        mesh.VertexPositions.Min(v => v.Z)),
                    new Vector3(
                        mesh.VertexPositions.Max(v => v.X),
                        mesh.VertexPositions.Max(v => v.Y),
                        mesh.VertexPositions.Max(v => v.Z)
                    )
                );

                var center = new Vector3(
                    (mesh.Aabb.Min.X + mesh.Aabb.Max.X) / 2,
                    (mesh.Aabb.Min.Y + mesh.Aabb.Min.Y) / 2,
                    (mesh.Aabb.Min.Z + mesh.Aabb.Max.Z) / 2
                );

                mesh.OrientedBB = new OrientedBB(
                    center,
                    new Vector3(mesh.Aabb.Max.X - center.X, 0, 0),
                    new Vector3(0, mesh.Aabb.Max.Y - center.Y, 0),
                    new Vector3(0, 0, mesh.Aabb.Max.Z - center.Z)
                );

                if (isModelReplacement)
                {
                    mesh.BoneIds = mesh.WeightIndices
                        .SelectMany(w => w
                            .SelectMany(x => x
                                .Select(y => (uint)y)))
                        .Distinct()
                        .OrderBy(b => b);

                    usedIndices.AddRange(mesh.BoneIds);
                }
                else
                {
                    mesh.BoneIds = _gpubin.BoneTable.Count > 1
                        ? Enumerable.Range(0, _gpubin.BoneTable.Max(m => m.Key) - 1).Select(i => (uint)i)
                        : new[] {0u};
                }
            }
        }

        if (isModelReplacement)
        {
            // Create arbitrary indices for the bones
            // Start at 10000 to avoid conflicts with other loaded bones on the target model
            ushort count = 10000;
            usedIndices = usedIndices.Distinct().ToList();
            var indexMap = usedIndices.ToDictionary(i => i, i => count++);

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
                    LodIndex = ((uint)indexMap[(ushort)kvp.Key] << 16) | 0xFFFF
                })
                .OrderBy(b => b.LodIndex)
                .ToList();
        }
        else
        {
            _model.BoneHeaders = _gpubin.BoneTable
                .Select(kvp =>
                {
                    var (boneIndex, boneName) = kvp;
                    boneIndex = ushort.MaxValue;
                    var lodIndex = ((uint)boneIndex << 16) | 0xFFFF;
                    return new BoneHeader {LodIndex = lodIndex, Name = boneName};
                })
                .ToList();
        }

        return _model;
    }
}