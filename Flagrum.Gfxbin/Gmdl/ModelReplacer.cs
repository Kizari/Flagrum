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

                // Replace model data with the imported data
                mesh.Normals = match.Normals;
                mesh.Tangents = match.Tangents;
                mesh.FaceIndices = match.FaceIndices;
                mesh.VertexPositions = match.VertexPositions;
                mesh.WeightIndices = match.WeightIndices;

                // Convert weights back to bytes as JSON.NET doesn't handle byte numbers well
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

        // foreach (var mesh in _model.MeshObjects[0].Meshes)
        // {
        //     Console.WriteLine($"Mesh name: {mesh.Name}");
        //     for (var j = 0; j < mesh.WeightValues[0].Count; j++)
        //     {
        //         if (mesh.WeightIndices[1][j].Length < 1)
        //         {
        //             continue;
        //         }
        //
        //         var map1 = FixArraySize(mesh.WeightValues[0][j], 4);
        //         var map2 = FixArraySize(mesh.WeightValues[1][j], 4);
        //         var map1i = FixArraySize(mesh.WeightIndices[0][j], 4);
        //         var map2i = FixArraySize(mesh.WeightIndices[1][j], 4);
        //
        //         var sum = map1.Sum(s => s) + map2.Sum(s => s);
        //         Console.WriteLine($"[({map1i[0]}, {map1[0]}), " +
        //                           $"({map1i[1]}, {map1[1]}), " +
        //                           $"({map1i[2]}, {map1[2]}), " +
        //                           $"({map1i[3]}, {map1[3]})], " +
        //                           $"[({map2i[0]}, {map2[0]}), " +
        //                           $"({map2i[1]}, {map2[1]}), " +
        //                           $"({map2i[2]}, {map2[2]}), " +
        //                           $"({map2i[3]}, {map2[3]}]");
        //     }
        // }

        return _model;
    }

    private TValue[] FixArraySize<TValue>(TValue[] values, int desiredSize)
    {
        if (values.Length < desiredSize)
        {
            var newValues = new TValue[desiredSize];
            Array.Copy(values, 0, newValues, 0, values.Length);
            return newValues;
        }

        return values;
    }
}