using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Flagrum.Gfxbin.Gmdl.Data;

namespace Flagrum.Gfxbin.Serialization;

public class GpuBuffer
{
    private readonly Dictionary<VertexElementFormat, Type> _formatTypes;
    private readonly byte[] _gpuBuffer;
    private readonly Dictionary<string, MeshBuffer> _meshBuffers;
    private readonly Dictionary<Type, Func<object>> _typeFunctions;

    /// <summary>
    ///     Passing in a <see cref="VertexElementFormat" /> will return the count of components for that format
    /// </summary>
    private readonly uint[] _vertexComponentCounts;

    private MeshBuffer _currentBuffer;

    public GpuBuffer(byte[] gpuBuffer)
    {
        _gpuBuffer = gpuBuffer;
        _meshBuffers = new Dictionary<string, MeshBuffer>();

        _formatTypes = new Dictionary<VertexElementFormat, Type>
        {
            {VertexElementFormat.X8_Uint, typeof(byte)},
            {VertexElementFormat.XYZW8_Uint, typeof(byte)},
            {VertexElementFormat.X8_Sint, typeof(sbyte)},
            {VertexElementFormat.XYZW8_Sint, typeof(sbyte)},

            {VertexElementFormat.X8_UintN, typeof(byte)},
            {VertexElementFormat.XYZW8_UintN, typeof(byte)},
            {VertexElementFormat.X8_SintN, typeof(sbyte)},
            {VertexElementFormat.XYZW8_SintN, typeof(sbyte)},

            {VertexElementFormat.X16_Sint, typeof(short)},
            {VertexElementFormat.XY16_Sint, typeof(short)},
            {VertexElementFormat.XYZW16_Sint, typeof(short)},

            {VertexElementFormat.X16_Uint, typeof(ushort)},
            {VertexElementFormat.XY16_Uint, typeof(ushort)},
            {VertexElementFormat.XYZW16_Uint, typeof(ushort)},

            {VertexElementFormat.X16_SintN, typeof(short)},
            {VertexElementFormat.XY16_SintN, typeof(short)},
            {VertexElementFormat.XYZW16_SintN, typeof(short)},

            {VertexElementFormat.X16_UintN, typeof(ushort)},
            {VertexElementFormat.XY16_UintN, typeof(ushort)},
            {VertexElementFormat.XYZW16_UintN, typeof(ushort)},

            {VertexElementFormat.X32_Sint, typeof(int)},
            {VertexElementFormat.XY32_Sint, typeof(int)},
            {VertexElementFormat.XYZ32_Sint, typeof(int)},
            {VertexElementFormat.XYZW32_Sint, typeof(int)},

            {VertexElementFormat.X32_Uint, typeof(uint)},
            {VertexElementFormat.XY32_Uint, typeof(uint)},
            {VertexElementFormat.XYZ32_Uint, typeof(uint)},
            {VertexElementFormat.XYZW32_Uint, typeof(uint)},

            {VertexElementFormat.X32_SintN, typeof(int)},
            {VertexElementFormat.XY32_SintN, typeof(int)},
            {VertexElementFormat.XYZ32_SintN, typeof(int)},
            {VertexElementFormat.XYZW32_SintN, typeof(int)},

            {VertexElementFormat.X32_UintN, typeof(uint)},
            {VertexElementFormat.XY32_UintN, typeof(uint)},
            {VertexElementFormat.XYZ32_UintN, typeof(uint)},
            {VertexElementFormat.XYZW32_UintN, typeof(uint)},

            {VertexElementFormat.X16_Float, typeof(Half)},
            {VertexElementFormat.XY16_Float, typeof(Half)},
            {VertexElementFormat.XYZW16_Float, typeof(Half)},

            {VertexElementFormat.X32_Float, typeof(float)},
            {VertexElementFormat.XY32_Float, typeof(float)},
            {VertexElementFormat.XYZ32_Float, typeof(float)},
            {VertexElementFormat.XYZW32_Float, typeof(float)}
        };

        _typeFunctions = new Dictionary<Type, Func<object>>
        {
            {typeof(sbyte), ReadSByte},
            {typeof(byte), ReadByte},
            {typeof(short), ReadShort},
            {typeof(ushort), ReadUShort},
            {typeof(int), ReadInt},
            {typeof(uint), ReadUInt},
            {typeof(float), ReadFloat},
            {typeof(Half), ReadHalf}
        };

        _vertexComponentCounts = new[]
        {
            0u,
            4u,
            0u,
            0u,
            0u,
            0u,
            4u,
            4u,
            4u,
            4u,
            0u,
            0u,
            4u,
            4u,
            4u,
            0u,
            3u,
            0u,
            0u,
            0u,
            0u,
            2u,
            0u,
            0u,
            0u,
            0u,
            2u,
            0u,
            2u,
            2u,
            2u,
            1u,
            0u,
            0u,
            0u,
            0u,
            1u,
            0u,
            1u,
            1u,
            1u,
            0u,
            1u,
            1u,
            0u,
            0u,
            429065506u,
            4u,
            77171364u,
            0u,
            0u,
            9u,
            77171396u,
            120u,
            0u,
            1u,
            429065506u,
            2u,
            77171580u,
            0u,
            0u,
            4u,
            77171596u,
            32u,
            0u,
            1u,
            429065506u,
            2u,
            77171664u,
            0u,
            0u,
            5u,
            77171680u,
            32u,
            0u,
            1u,
            429065506u,
            2u,
            77171748u,
            0u,
            0u,
            4u,
            77171764u,
            32u,
            0u,
            1u,
            429065506u,
            1u,
            74407332u,
            0u,
            0u,
            3u,
            77172504u,
            32u,
            0u,
            1u,
            429065506u,
            0u,
            0u,
            0u,
            0u,
            1u,
            77172584u,
            32u,
            0u,
            5u,
            429065506u,
            5u,
            75694096u,
            0u,
            0u,
            11u,
            77172624u,
            32u,
            0u,
            1u
        };
    }

    public int Position { get; set; }

    public void SetBuffer(string meshName, int[,] faces, int offset, int size)
    {
        if (_meshBuffers.TryGetValue(meshName, out var meshBuffer))
        {
            // If the buffer already exists, this is the same mesh for another LOD
            // For now we just discard this since we are just trying to get LOD0 working
            _currentBuffer = new MeshBuffer
            {
                Buffer = new byte[size],
                Data = new Dictionary<string, IList>(),
                Faces = faces
            };
        }
        else
        {
            meshBuffer = new MeshBuffer
            {
                Buffer = new byte[size],
                Data = new Dictionary<string, IList>(),
                Faces = faces
            };

            _meshBuffers.Add(meshName, meshBuffer);
            _currentBuffer = meshBuffer;

            Array.Copy(_gpuBuffer, offset, _currentBuffer.Buffer, 0, size);
        }
    }

    public void ReadVertexElement(string semantic, VertexElementFormat format)
    {
        if (!_formatTypes.TryGetValue(format, out var type))
        {
            throw new ArgumentException(
                $"Vertex format {format.ToString()} does not have an associated type " +
                $"in {nameof(GpuBuffer)}.{nameof(_formatTypes)}", nameof(format));
        }

        if (_currentBuffer.Data.TryGetValue(semantic, out var list))
        {
            ProcessVertexElement(list, format, type);
        }
        else
        {
            var listType = typeof(List<>).MakeGenericType(type);
            var instance = (IList)Activator.CreateInstance(listType);
            _currentBuffer.Data.Add(semantic, instance);
            ProcessVertexElement(instance, format, type);
        }
    }

    private void ProcessVertexElement(IList list, VertexElementFormat format, Type type)
    {
        var count = _vertexComponentCounts[(int)format];
        for (var _ = 0; _ < count; _++)
        {
            var readFunction = _typeFunctions[type];
            var result = readFunction();
            list.Add(result);
        }
    }

    private object ReadFloat()
    {
        var value = BitConverter.ToSingle(_currentBuffer.Buffer, Position);
        Position += sizeof(float);
        return value;
    }

    private object ReadHalf()
    {
        var value = BitConverter.ToHalf(_currentBuffer.Buffer, Position);
        Position += 2; // 2 bytes in a half
        return value;
    }

    private object ReadInt()
    {
        var value = BitConverter.ToInt32(_currentBuffer.Buffer, Position);
        Position += sizeof(int);
        return value;
    }

    private object ReadUInt()
    {
        var value = BitConverter.ToUInt32(_currentBuffer.Buffer, Position);
        Position += sizeof(uint);
        return value;
    }

    private object ReadUShort()
    {
        var value = BitConverter.ToUInt16(_currentBuffer.Buffer, Position);
        Position += sizeof(ushort);
        return value;
    }

    private object ReadShort()
    {
        var value = BitConverter.ToInt16(_currentBuffer.Buffer, Position);
        Position += sizeof(short);
        return value;
    }

    private object ReadByte()
    {
        var value = _currentBuffer.Buffer[Position];
        Position += sizeof(byte);
        return value;
    }

    private object ReadSByte()
    {
        var value = unchecked((sbyte)_currentBuffer.Buffer[Position]);
        Position += sizeof(byte);
        return value;
    }

    public Gpubin ToGpubin()
    {
        var gpubin = new Gpubin();

        foreach (var meshBuffer in _meshBuffers)
        {
            var mesh = new MeshData
            {
                Name = meshBuffer.Key,
                Faces = meshBuffer.Value.Faces
            };

            var positions = (IList<float>)meshBuffer.Value.Data
                .FirstOrDefault(d => d.Key == VertexElementDescription.Position0).Value;

            var normals = (IList<sbyte>)meshBuffer.Value.Data
                .FirstOrDefault(d => d.Key == VertexElementDescription.Normal0).Value;

            var colors = meshBuffer.Value.Data
                .OrderBy(d => d.Key)
                .Where(d => d.Key is VertexElementDescription.Color0
                    or VertexElementDescription.Color1
                    or VertexElementDescription.Color2
                    or VertexElementDescription.Color3)
                .Select(d => (IList<byte>)d.Value);

            var weights0 = meshBuffer.Value.Data
                .OrderBy(d => d.Key)
                .Where(d => d.Key is VertexElementDescription.BlendWeight0)
                .Select(d => (IList<byte>)d.Value)
                .ToList();

            var weightIndices0 = meshBuffer.Value.Data
                .OrderBy(d => d.Key)
                .Where(d => d.Key is VertexElementDescription.BlendIndices0)
                .Select(d => (IList<ushort>)d.Value)
                .ToList();

            var weights1 = meshBuffer.Value.Data
                .OrderBy(d => d.Key)
                .Where(d => d.Key is VertexElementDescription.BlendWeight1)
                .Select(d => (IList<byte>)d.Value)
                .ToList();

            var weightIndices1 = meshBuffer.Value.Data
                .OrderBy(d => d.Key)
                .Where(d => d.Key is VertexElementDescription.BlendIndices1)
                .Select(d => (IList<ushort>)d.Value)
                .ToList();

            var weightDictionaries = new List<List<VertexWeight>>();
            for (var i = 0; i < weights0.Count; i++)
            {
                var vertexIndex = 0;
                var vertexCounter = 0;

                var dictionary = new List<VertexWeight>();
                for (var j = 0; j < weights0[i].Count; j++)
                {
                    dictionary.Add(new VertexWeight
                    {
                        VertexIndex = vertexIndex,
                        BoneIndex = weightIndices0[i][j],
                        Weight = weights0[i][j]
                    });

                    if (weights1.Any())
                    {
                        dictionary.Add(new VertexWeight
                        {
                            VertexIndex = vertexIndex,
                            BoneIndex = weightIndices1[i][j],
                            Weight = weights1[i][j]
                        });
                    }

                    vertexCounter++;
                    if (vertexCounter > 3)
                    {
                        vertexCounter = 0;
                        vertexIndex++;
                    }
                }

                weightDictionaries.Add(dictionary);
            }

            var uvMaps = meshBuffer.Value.Data
                .OrderBy(d => d.Key)
                .Where(d => d.Key is VertexElementDescription.TexCoord0
                    or VertexElementDescription.TexCoord1
                    or VertexElementDescription.TexCoord2
                    or VertexElementDescription.TexCoord3
                    or VertexElementDescription.TexCoord4
                    or VertexElementDescription.TexCoord5
                    or VertexElementDescription.TexCoord6
                    or VertexElementDescription.TexCoord7)
                .Select(d => (IList<Half>)d.Value);

            mesh.VertexPositions = new Vector3[positions.Count / 3];
            for (var i = 0; i < positions.Count; i += 3)
            {
                mesh.VertexPositions[i / 3] = new Vector3(positions[i], positions[i + 1], positions[i + 2]);
            }

            mesh.Normals = new Vector4[normals.Count / 4];
            for (var i = 0; i < normals.Count; i += 4)
            {
                mesh.Normals[i / 4] = new Vector4
                {
                    X = normals[i],
                    Y = normals[i + 1],
                    Z = normals[i + 2],
                    W = normals[i + 3]
                };
            }

            var colorMapsCount = colors.Count();
            mesh.ColorMaps = new ColorMap[colorMapsCount];
            for (var colorMapIndex = 0; colorMapIndex < colorMapsCount; colorMapIndex++)
            {
                var colorList = colors.ElementAt(colorMapIndex);
                mesh.ColorMaps[colorMapIndex] = new ColorMap
                {
                    Colors = new Color4[colorList.Count / 4]
                };

                for (var i = 0; i < colorList.Count; i += 4)
                {
                    mesh.ColorMaps[colorMapIndex].Colors[i / 4] = new Color4
                    {
                        R = colorList[i],
                        G = colorList[i + 1],
                        B = colorList[i + 2],
                        A = colorList[i + 3]
                    };
                }
            }

            weightDictionaries = weightDictionaries
                .Select(d => d.Where(e => e.Weight != 0).ToList())
                .ToList();

            mesh.WeightMaps = weightDictionaries;
            // mesh.WeightMaps = new WeightMap[weightDictionaries.Count()];
            // for (var weightMapIndex = 0; weightMapIndex < weightDictionaries.Count(); weightMapIndex++)
            // {
            //     var weightList = weightDictionaries[weightMapIndex];
            //     
            //     mesh.WeightMaps[weightMapIndex] = new WeightMap
            //     {
            //         VertexWeights = new VertexWeights[weightList.Count / 4]
            //     };
            //
            //     for (var i = 0; i < weightList.Count; i += 4)
            //     {
            //         mesh.WeightMaps[weightMapIndex].VertexWeights[i / 4].VertexWeight = new VertexWeight[4];
            //
            //         for (var j = 0; j < 4; j++)
            //         {
            //             mesh.WeightMaps[weightMapIndex].VertexWeights[i / 4].VertexWeight[j] = new VertexWeight
            //             {
            //                 BoneIndex = weightList[i + j].Item1,
            //                 Weight = weightList[i + j].Item2
            //             };
            //         }
            //     }
            // }

            mesh.UVMaps = new UVMap[uvMaps.Count()];
            for (var uvMapIndex = 0; uvMapIndex < uvMaps.Count(); uvMapIndex++)
            {
                var uvMap = uvMaps.ElementAt(uvMapIndex);
                mesh.UVMaps[uvMapIndex] = new UVMap
                {
                    Coords = new Vector2[uvMap.Count / 2]
                };

                for (var i = 0; i < uvMap.Count; i += 2)
                {
                    mesh.UVMaps[uvMapIndex].Coords[i / 2] = new Vector2
                    {
                        X = (float)uvMap[i],
                        Y = (float)uvMap[i + 1]
                    };
                }
            }

            gpubin.Meshes.Add(mesh);
        }

        return gpubin;
    }
}