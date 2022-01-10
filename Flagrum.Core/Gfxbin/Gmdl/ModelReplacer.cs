using System;
using System.IO;
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

    public Model Replace()
    {
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

                for (var i = 0; i < mesh.VertexCount; i++)
                {
                    if (mesh.WeightLimit == 6)
                    {
                        var weights = 0;
                        for (var j = 0; j < 4; j++)
                        {
                            if (j < weightValues[0][i].Length && weightValues[0][i][j] > 0)
                            {
                                weights++;
                            }

                            if (j < weightValues[1][i].Length && weightValues[1][i][j] > 0)
                            {
                                weights++;
                            }
                        }

                        // var sum1 = weightValues[0][i].Sum(w => w);
                        // var sum2 = weightValues[1][i].Sum(w => w);
                        if (weights > 6)
                        {
                            File.AppendAllText(@"C:\Modding\MaterialTesting\log.txt", "REEEEEEEEE\r\n");
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

                mesh.BoneIds = _gpubin.BoneTable.Count > 1
                    ? Enumerable.Range(0, _gpubin.BoneTable.Max(m => m.Key) - 1).Select(i => (uint)i)
                    : new[] {0u};
            }
        }

        _model.BoneHeaders = _gpubin.BoneTable
            .Select(kvp =>
            {
                var (boneIndex, boneName) = kvp;
                boneIndex = ushort.MaxValue;
                var lodIndex = ((uint)boneIndex << 16) | 0xFFFF;
                return new BoneHeader {LodIndex = lodIndex, Name = boneName};
            })
            .ToList();

        return _model;
    }
}