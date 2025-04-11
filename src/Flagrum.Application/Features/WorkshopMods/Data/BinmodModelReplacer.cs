using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Flagrum.Core.Archive.Mod;
using Flagrum.Core.Graphics.Models;
using Flagrum.Core.Mathematics;
using Flagrum.Application.Features.WorkshopMods.Data.Model;

namespace Flagrum.Core.Gfxbin.Gmdl;

/// <summary>
/// Temporary class to handle placing mesh data into an existing GFXBIN
/// to avoid manually filling out every piece of data
/// </summary>
public class ModelReplacer
{
    private readonly BinmodModelData _gpubin;
    private readonly GameModel _model;

    public ModelReplacer(GameModel originalModel, BinmodModelData replacementData)
    {
        _model = originalModel;
        _gpubin = replacementData;
    }

    public GameModel Replace(WorkshopModType modType)
    {
        var isModelReplacement = modType == WorkshopModType.Character;
        var usedIndices = new List<uint>();

        // Weapon model replacements (usually only have 1 or 2 bones) don't seem to work correctly on the
        // same system as other model replacements, so we'll just let it use the standard bone system
        // if we have detected this armature
        if (_gpubin.BoneTable.Count < 6 && _gpubin.BoneTable.Any(b => b.Value == "C_Body"))
        {
            isModelReplacement = false;
        }

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

                var limit = match.Material.WeightLimit;
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
                mesh.FaceIndices = match.FaceIndices;

                // TODO: Normals need to come in from Blender as floats instead of sbyte
                mesh.Semantics[VertexElementSemantic.Normal0] =
                    match.Normals.Select(n => new[]
                    {
                        n.X * (1f / 0x7F),
                        n.Y * (1f / 0x7F),
                        n.Z * (1f / 0x7F),
                        n.W * (1f / 0x7F)
                    }).ToList();

                // TODO: Tangents need to come in from Blender as floats instead of sbyte
                mesh.Semantics[VertexElementSemantic.Tangent0] =
                    match.Tangents.Select(n => new[]
                    {
                        n.X * (1f / 0x7F),
                        n.Y * (1f / 0x7F),
                        n.Z * (1f / 0x7F),
                        n.W * (1f / 0x7F)
                    }).ToList();

                mesh.Semantics[VertexElementSemantic.Position0] =
                    match.VertexPositions.Select(v => new[] {v.X, v.Y, v.Z})
                        .ToList();

                for (var i = 0; i < match.ColorMaps.Count; i++)
                {
                    if (match.ColorMaps[i].Colors.Count > 0)
                    {
                        var key = (VertexElementSemantic)$"COLOR{i}";
                        // TODO: Colours need to come in from Blender as floats instead of byte
                        mesh.Semantics[key] = match.ColorMaps[i].Colors.Select(c => new[]
                        {
                            c.R / 255f,
                            c.G / 255f,
                            c.B / 255f,
                            c.A / 255f
                        }).ToList();
                    }
                }
                
                // Automatic white COLOR2 for mods that need it if no colorSet2 was present, prevents blackout models
                if (modType != WorkshopModType.Weapon && modType != WorkshopModType.Multi_Weapon &&
                    !mesh.Semantics.ContainsKey(VertexElementSemantic.Color2))
                {
                    mesh.Semantics[VertexElementSemantic.Color2] = new List<float[]>();
                    for (var j = 0; j < mesh.Semantics[VertexElementSemantic.Position0].Count; j++)
                    {
                        mesh.Semantics[VertexElementSemantic.Color2].Add(new[] {1f, 1f, 1f, 1f});
                    }
                }

                for (var i = 0; i < (limit > 4 ? 2 : 1); i++)
                {
                    var key = (VertexElementSemantic)$"BLENDINDICES{i}";
                    mesh.Semantics[key] = weightIndices[i]
                        .Select(j => FixArraySize(j, 4)
                            .Select(k => (uint)k).ToArray())
                        .ToList();
                }

                for (var i = 0; i < (limit > 4 ? 2 : 1); i++)
                {
                    var key = (VertexElementSemantic)$"BLENDWEIGHT{i}";
                    mesh.Semantics[key] = weightValues[i].Select(u =>
                    {
                        var w = FixArraySize(u, 4);
                        return new[]
                        {
                            w[0] / 255f,
                            w[1] / 255f,
                            w[2] / 255f,
                            w[3] / 255f
                        };
                    }).ToList();
                }

                for (var i = 0; i < match.UVMaps.Count; i++)
                {
                    var key = (VertexElementSemantic)$"TEXCOORD{i}";
                    mesh.Semantics[key] = match.UVMaps[i].UVs.Select(uv => new[] {uv.U, uv.V}).ToList();
                }

                // Calculate the bounding box for this mesh
                mesh.AABB = new AxisAlignedBoundingBox(
                    new Vector3(
                        ((IList<float[]>)mesh.Semantics[VertexElementSemantic.Position0]).Min(v => v[0]),
                        ((IList<float[]>)mesh.Semantics[VertexElementSemantic.Position0]).Min(v => v[1]),
                        ((IList<float[]>)mesh.Semantics[VertexElementSemantic.Position0]).Min(v => v[2])),
                    new Vector3(
                        ((IList<float[]>)mesh.Semantics[VertexElementSemantic.Position0]).Max(v => v[0]),
                        ((IList<float[]>)mesh.Semantics[VertexElementSemantic.Position0]).Max(v => v[1]),
                        ((IList<float[]>)mesh.Semantics[VertexElementSemantic.Position0]).Max(v => v[2])
                    )
                );

                var center = new Vector3(
                    (mesh.AABB.Start.X + mesh.AABB.End.X) / 2,
                    (mesh.AABB.Start.Y + mesh.AABB.End.Y) / 2,
                    (mesh.AABB.Start.Z + mesh.AABB.End.Z) / 2
                );

                mesh.OrientedBoundingBox = new OrientedBoundingBox(
                    center,
                    new Vector3(mesh.AABB.End.X - center.X, 0, 0),
                    new Vector3(0, mesh.AABB.End.Y - center.Y, 0),
                    new Vector3(0, 0, mesh.AABB.End.Z - center.Z)
                );

                if (isModelReplacement)
                {
                    var indices = ((IList<uint[]>)mesh.Semantics[VertexElementSemantic.BlendIndices0])
                        .SelectMany(w => w.Select(i => i))
                        .ToList();

                    if (mesh.Semantics.TryGetValue(VertexElementSemantic.BlendIndices1, out var blendIndices1))
                    {
                        var indices2 = ((IList<uint[]>)blendIndices1)
                            .SelectMany(w => w.Select(i => i));
                        indices.AddRange(indices2);
                    }

                    mesh.BoneIds = indices.Distinct().OrderBy(i => i).ToList();
                    usedIndices.AddRange(mesh.BoneIds);
                }
                else
                {
                    mesh.BoneIds = _gpubin.BoneTable.Count > 1
                        ? Enumerable.Range(0, _gpubin.BoneTable.Max(m => m.Key) - 1).Select(i => (uint)i).ToList()
                        : new[] {0u};
                }

                UpdateVertexStreams(mesh, match);
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
                var weightIndices = new List<IList<uint[]>>
                    {(IList<uint[]>)mesh.Semantics[VertexElementSemantic.BlendIndices0]};
                if (mesh.Semantics.TryGetValue(VertexElementSemantic.BlendIndices1, out var blendIndices1))
                {
                    weightIndices.Add((IList<uint[]>)blendIndices1);
                }

                foreach (var indexList in weightIndices)
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
            _model.Bones = _gpubin.BoneTable
                .Where(d => usedIndices.Contains((ushort)d.Key))
                .Select(kvp => new GameModelBone
                {
                    Name = kvp.Value,
                    LodIndex = ((uint)indexMap[(ushort)kvp.Key] << 16) | 0xFFFF
                })
                .OrderBy(b => b.LodIndex)
                .ToList();
        }
        else
        {
            _model.Bones = _gpubin.BoneTable
                .Select(kvp =>
                {
                    var (boneIndex, boneName) = kvp;
                    boneIndex = ushort.MaxValue;
                    var lodIndex = ((uint)boneIndex << 16) | 0xFFFF;
                    return new GameModelBone {LodIndex = lodIndex, Name = boneName};
                })
                .ToList();
        }

        return _model;
    }

    private void UpdateVertexStreams(GameModelMesh mesh, BinmodModelDataMesh binmodMesh)
    {
        var stride = 0u;
        var elements1 = new List<VertexElement>();

        elements1.Add(new VertexElement
        {
            Format = VertexElementFormat.XYZ32_Float,
            Semantic = VertexElementSemantic.Position0,
            Offset = stride
        });

        stride += 12;

        elements1.Add(new VertexElement
        {
            Format = VertexElementFormat.XYZW16_Uint,
            Semantic = VertexElementSemantic.BlendIndices0,
            Offset = stride
        });

        stride += 8;

        if (binmodMesh.Material.WeightLimit > 4)
        {
            elements1.Add(new VertexElement
            {
                Format = VertexElementFormat.XYZW16_Uint,
                Semantic = VertexElementSemantic.BlendIndices1,
                Offset = stride
            });

            stride += 8;
        }

        elements1.Add(new VertexElement
        {
            Format = VertexElementFormat.XYZW8_UintN,
            Semantic = VertexElementSemantic.BlendWeight0,
            Offset = stride
        });

        stride += 4;

        if (binmodMesh.Material.WeightLimit > 4)
        {
            elements1.Add(new VertexElement
            {
                Format = VertexElementFormat.XYZW8_UintN,
                Semantic = VertexElementSemantic.BlendWeight1,
                Offset = stride
            });

            stride += 4;
        }

        mesh.VertexStreams.Add(new VertexStream
        {
            Slot = VertexStreamSlot.Slot_0,
            Type = VertexStreamType.Vertex,
            Stride = stride,
            Elements = elements1
        });

        stride = 0;
        var elements2 = new List<VertexElement>();

        elements2.Add(new VertexElement
        {
            Format = VertexElementFormat.XYZW8_SintN,
            Semantic = VertexElementSemantic.Normal0,
            Offset = stride
        });

        stride += 4;

        elements2.Add(new VertexElement
        {
            Format = VertexElementFormat.XYZW8_SintN,
            Semantic = VertexElementSemantic.Tangent0,
            Offset = stride
        });

        stride += 4;

        var uvCount = 0;
        foreach (var uvMap in mesh.Semantics.Keys
                     .Where(k => k.Value.StartsWith("TEXCOORD"))
                     .OrderBy(k => k.Value))
        {
            elements2.Add(new VertexElement
            {
                Format = VertexElementFormat.XY16_Float,
                Semantic = uvMap,
                Offset = stride
            });

            stride += 4;
            uvCount++;

            // Luminous only supports up to TEXCOORD7
            if (uvCount > 7)
            {
                break;
            }
        }

        var colorCount = 0;
        foreach (var colorMap in mesh.Semantics.Keys
                     .Where(k => k.Value.StartsWith("COLOR"))
                     .OrderBy(k => k.Value))
        {
            elements2.Add(new VertexElement
            {
                Format = VertexElementFormat.XYZW8_UintN,
                Semantic = colorMap,
                Offset = stride
            });

            stride += 4;
            colorCount++;

            // Luminous only supports up to COLOR3
            if (colorCount > 3)
            {
                break;
            }
        }

        mesh.VertexStreams.Add(new VertexStream
        {
            Slot = VertexStreamSlot.Slot_1,
            Type = VertexStreamType.Vertex,
            Stride = stride,
            Offset = mesh.VertexStreams[0].Stride * mesh.VertexCount,
            Elements = elements2
        });
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