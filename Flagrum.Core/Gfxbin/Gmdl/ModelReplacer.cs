using System;
using System.Collections.Generic;
using System.Linq;
using Flagrum.Core.Archive;
using Flagrum.Core.Gfxbin.Gmdl.Components;
using Flagrum.Core.Gfxbin.Gmdl.Constructs;
using Flagrum.Core.Gfxbin.Gmdl.Templates;

namespace Flagrum.Core.Gfxbin.Gmdl;

/// <summary>
///     Temporary class to handle placing mesh data into an existing GFXBIN
///     to avoid manually filling out every piece of data
/// </summary>
public class ModelReplacer
{
    private readonly Gpubin _gpubin;
    private readonly Model _model;
    private readonly int _modTarget;
    private readonly int _modType;
    private readonly string _modVariant;

    public ModelReplacer(Model originalModel, Gpubin replacementData, int modType, int modTarget,
        string modVariant = null)
    {
        _model = originalModel;
        _gpubin = replacementData;
        _modType = modType;
        _modTarget = modTarget;
        _modVariant = modVariant;
    }

    public Model Replace()
    {
        var usedIndices = new List<ushort>();
        foreach (var mesh in _model.MeshObjects[0].Meshes)
        {
            var match = _gpubin.Meshes.FirstOrDefault(m => m.Name == mesh.Name);
            if (match != null)
            {
                mesh.VertexCount = (uint)match.VertexPositions.Count;

                var weightValues = match.WeightValues
                    .Select(m => m.Select(n => n.Select(o => (byte)o).ToArray()).ToList()).ToList();

                // This normalises weights to 255 over both weight maps
                // ffxvbinmods don't seem to support the second weight map, so we disable it
                // for (var i = 0; i < mesh.VertexCount; i++)
                // {
                //     var sum = weightValues[0][i].Sum(s => s) + weightValues[1][i].Sum(s => s);
                //     if (sum != 0 && sum != 255)
                //     {
                //         var difference = 255 - sum;
                //         var counter = 0;
                //
                //         while (difference > 0)
                //         {
                //             var weight = counter >= weightValues[0][i].Length
                //                 ? weightValues[1][i]
                //                 : weightValues[0][i];
                //
                //             var index = counter >= weightValues[0][i].Length
                //                 ? counter - weightValues[0][i].Length
                //                 : counter;
                //
                //             if (weight.Length > 0 && weight[index] > 0 && weight[index] < 255)
                //             {
                //                 weight[index]++;
                //                 difference--;
                //             }
                //
                //             counter++;
                //             if (counter == weightValues[0][i].Length + weightValues[1][i].Length)
                //             {
                //                 counter = 0;
                //             }
                //         }
                //
                //         while (difference < 0)
                //         {
                //             var weight = counter >= weightValues[0][i].Length
                //                 ? weightValues[1][i]
                //                 : weightValues[0][i];
                //
                //             var index = counter >= weightValues[0][i].Length
                //                 ? counter - weightValues[0][i].Length
                //                 : counter;
                //
                //             if (weight.Length > 0 && weight[index] > 0)
                //             {
                //                 weight[index]--;
                //                 difference++;
                //             }
                //
                //             counter++;
                //             if (counter == weightValues[0][i].Length + weightValues[1][i].Length - 1)
                //             {
                //                 counter = 0;
                //             }
                //         }
                //     }
                // }

                if (mesh.MaterialType == MaterialType.OneWeight)
                {
                    for (var i = 0; i < mesh.VertexCount; i++)
                    {
                        var weights = weightValues[0][i];
                        for (var j = 0; j < weights.Length; j++)
                        {
                            if (j == 0)
                            {
                                if (weights[j] > 0)
                                {
                                    weights[j] = 255;
                                }
                                else
                                {
                                    weights[j] = 0;
                                }
                            }

                            weights[j] = 0;
                        }
                    }
                }
                else
                {
                    // This normalises weights to 255 over the first weight map only
                    for (var i = 0; i < mesh.VertexCount; i++)
                    {
                        for (var j = 0; j < 2; j++)
                        {
                            var sum = weightValues[j][i].Sum(s => s);
                            if (sum != 0 && sum != 255)
                            {
                                var difference = 255 - sum;
                                var counter = 0;

                                while (difference > 0)
                                {
                                    var weight = weightValues[j][i];

                                    if (weight[counter] > 0 && weight[counter] < 255)
                                    {
                                        weight[counter]++;
                                        difference--;
                                    }

                                    counter++;
                                    if (counter == weight.Length)
                                    {
                                        counter = 0;
                                    }
                                }

                                while (difference < 0)
                                {
                                    var weight = weightValues[j][i];

                                    if (weight[counter] > 0)
                                    {
                                        weight[counter]--;
                                        difference++;
                                    }

                                    counter++;
                                    if (counter == weight.Length)
                                    {
                                        counter = 0;
                                    }
                                }
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
                mesh.WeightIndices = match.WeightIndices;
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

                // Generate list of Bone IDs used in this mesh
                var boneIds = new List<uint>();
                foreach (var weightIndexMap in mesh.WeightIndices)
                {
                    foreach (var weightList in weightIndexMap)
                    {
                        foreach (var weight in weightList)
                        {
                            boneIds.Add(weight);
                        }
                    }
                }

                if (_modType == (int)BinmodType.StyleEdit)
                {
                    if (_gpubin.BoneTable.Count > 1)
                    {
                        mesh.BoneIds = Enumerable.Range(0, _gpubin.BoneTable.Max(m => m.Key) - 1).Select(i => (uint)i);
                    }
                    else
                    {
                        mesh.BoneIds = new[] { 0u };
                    }
                }
                else
                {
                    mesh.BoneIds = boneIds.Distinct().OrderBy(i => i);
                    usedIndices.AddRange(boneIds.Distinct().Select(i => (ushort)i));
                }
            }
        }

        if (_modType == (int)BinmodType.StyleEdit)
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

            return _model;
        }

        // Create arbitrary indices for the bones
        // Start at 10000 to avoid conflicts with other loaded bones on the target model
        ushort count = 10000;
        usedIndices = usedIndices.Distinct().ToList();
        var indexMap = usedIndices.ToDictionary(i => i, i => count++);

        // Need to use indices of preloaded bones to prevent rigging issues
        foreach (var bone in PreloadedBones.Get(_modType, _modTarget, _modVariant))
        {
            var match = _gpubin.BoneTable.FirstOrDefault(p => p.Value == bone.Name);
            if (match.Value != null)
            {
                indexMap[(ushort)match.Key] = (ushort)(bone.LodIndex >> 16);
            }
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