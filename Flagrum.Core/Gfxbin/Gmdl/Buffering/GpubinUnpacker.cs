using System;
using System.Collections.Generic;
using Flagrum.Gfxbin.Gmdl.Components;
using Flagrum.Gfxbin.Gmdl.Constructs;

namespace Flagrum.Gfxbin.Gmdl.Buffering;

public class GpubinUnpacker
{
    private readonly byte[] _gpubin;

    private byte[] _currentBuffer;
    private int _currentBufferPosition;
    private readonly Dictionary<VertexElementFormat, (Type type, int elementCount)> _formatParameters;

    public GpubinUnpacker(byte[] gpubin)
    {
        _gpubin = gpubin;

        _formatParameters = new Dictionary<VertexElementFormat, (Type type, int elementCount)>
        {
            {VertexElementFormat.X8_Uint, (typeof(byte), 1)},
            {VertexElementFormat.XYZW8_Uint, (typeof(byte), 4)},
            {VertexElementFormat.X8_Sint, (typeof(sbyte), 1)},
            {VertexElementFormat.XYZW8_Sint, (typeof(sbyte), 4)},

            {VertexElementFormat.X8_UintN, (typeof(byte), 1)},
            {VertexElementFormat.XYZW8_UintN, (typeof(byte), 4)},
            {VertexElementFormat.X8_SintN, (typeof(sbyte), 1)},
            {VertexElementFormat.XYZW8_SintN, (typeof(sbyte), 4)},

            {VertexElementFormat.X16_Sint, (typeof(short), 1)},
            {VertexElementFormat.XY16_Sint, (typeof(short), 2)},
            {VertexElementFormat.XYZW16_Sint, (typeof(short), 4)},

            {VertexElementFormat.X16_Uint, (typeof(ushort), 1)},
            {VertexElementFormat.XY16_Uint, (typeof(ushort), 2)},
            {VertexElementFormat.XYZW16_Uint, (typeof(ushort), 4)},

            {VertexElementFormat.X16_SintN, (typeof(short), 1)},
            {VertexElementFormat.XY16_SintN, (typeof(short), 2)},
            {VertexElementFormat.XYZW16_SintN, (typeof(short), 4)},

            {VertexElementFormat.X16_UintN, (typeof(ushort), 1)},
            {VertexElementFormat.XY16_UintN, (typeof(ushort), 2)},
            {VertexElementFormat.XYZW16_UintN, (typeof(ushort), 4)},

            {VertexElementFormat.X32_Sint, (typeof(int), 1)},
            {VertexElementFormat.XY32_Sint, (typeof(int), 2)},
            {VertexElementFormat.XYZ32_Sint, (typeof(int), 3)},
            {VertexElementFormat.XYZW32_Sint, (typeof(int), 4)},

            {VertexElementFormat.X32_Uint, (typeof(uint), 1)},
            {VertexElementFormat.XY32_Uint, (typeof(uint), 2)},
            {VertexElementFormat.XYZ32_Uint, (typeof(uint), 3)},
            {VertexElementFormat.XYZW32_Uint, (typeof(uint), 4)},

            {VertexElementFormat.X32_SintN, (typeof(int), 1)},
            {VertexElementFormat.XY32_SintN, (typeof(int), 2)},
            {VertexElementFormat.XYZ32_SintN, (typeof(int), 3)},
            {VertexElementFormat.XYZW32_SintN, (typeof(int), 4)},

            {VertexElementFormat.X32_UintN, (typeof(uint), 1)},
            {VertexElementFormat.XY32_UintN, (typeof(uint), 2)},
            {VertexElementFormat.XYZ32_UintN, (typeof(uint), 3)},
            {VertexElementFormat.XYZW32_UintN, (typeof(uint), 4)},

            {VertexElementFormat.X16_Float, (typeof(Half), 1)},
            {VertexElementFormat.XY16_Float, (typeof(Half), 2)},
            {VertexElementFormat.XYZW16_Float, (typeof(Half), 4)},

            {VertexElementFormat.X32_Float, (typeof(float), 1)},
            {VertexElementFormat.XY32_Float, (typeof(float), 2)},
            {VertexElementFormat.XYZ32_Float, (typeof(float), 3)},
            {VertexElementFormat.XYZW32_Float, (typeof(float), 4)}
        };
    }

    public int[,] UnpackFaceIndices(int gpubinOffset, int faceIndicesCount, IndexType faceIndexType)
    {
        var faceCount = faceIndicesCount / 3; // All faces are tris on gmdl meshes
        var faceIndices = new int[faceCount, 3];

        Func<int, int> unpack = faceIndexType switch
        {
            IndexType.IndexType32 => index => BitConverter.ToInt32(_gpubin, gpubinOffset + 4 * index),
            IndexType.IndexType16 => index => BitConverter.ToInt16(_gpubin, gpubinOffset + 2 * index),
            _ => throw new ArgumentException("Provided face index type not supported.", nameof(faceIndexType))
        };

        for (var i = 0; i < faceCount; i++)
        {
            faceIndices[i, 0] = unpack(i * 3);
            faceIndices[i, 1] = unpack(i * 3 + 1);
            faceIndices[i, 2] = unpack(i * 3 + 2);
        }

        return faceIndices;
    }

    public void UnpackVertexStreams(Mesh mesh, uint vertexBufferSize)
    {
        _currentBuffer = new byte[vertexBufferSize];
        Array.Copy(_gpubin, mesh.VertexBufferOffset, _currentBuffer, 0, vertexBufferSize);

        foreach (var vertexStream in mesh.VertexStreamDescriptions)
        {
            _currentBufferPosition = (int)vertexStream.StartOffset;

            for (var i = 0; i < mesh.VertexCount; i++)
            {
                foreach (var element in vertexStream.VertexElementDescriptions)
                {
                    switch (element.Semantic)
                    {
                        case VertexElementDescription.Position0:
                            var position = new Vector3
                            {
                                X = ReadFloat(),
                                Y = ReadFloat(),
                                Z = ReadFloat()
                            };
                            mesh.VertexPositions.Add(position);
                            break;
                        case VertexElementDescription.Normal0:
                            var normal = new Normal
                            {
                                X = ReadSByte(),
                                Y = ReadSByte(),
                                Z = ReadSByte(),
                                W = ReadSByte()
                            };
                            mesh.Normals.Add(normal);
                            break;
                        case VertexElementDescription.Tangent0:
                            var tangent = new Normal
                            {
                                X = ReadSByte(),
                                Y = ReadSByte(),
                                Z = ReadSByte(),
                                W = ReadSByte()
                            };
                            mesh.Tangents.Add(tangent);
                            break;
                        case VertexElementDescription.Color0:
                        case VertexElementDescription.Color1:
                        case VertexElementDescription.Color2:
                        case VertexElementDescription.Color3:
                            var colorIndex = int.Parse(element.Semantic[^1].ToString());
                            while (mesh.ColorMaps.Count - 1 < colorIndex)
                            {
                                mesh.ColorMaps.Add(new ColorMap {Colors = new List<Color4>()});
                            }

                            var colorMap = mesh.ColorMaps[colorIndex];
                            var color = new Color4
                            {
                                R = ReadByte(),
                                G = ReadByte(),
                                B = ReadByte(),
                                A = ReadByte()
                            };
                            colorMap.Colors.Add(color);
                            break;
                        case VertexElementDescription.TexCoord0:
                        case VertexElementDescription.TexCoord1:
                        case VertexElementDescription.TexCoord2:
                        case VertexElementDescription.TexCoord3:
                        case VertexElementDescription.TexCoord4:
                        case VertexElementDescription.TexCoord5:
                        case VertexElementDescription.TexCoord6:
                        case VertexElementDescription.TexCoord7:
                            var texCoordIndex = int.Parse(element.Semantic[^1].ToString());
                            while (mesh.UVMaps.Count - 1 < texCoordIndex)
                            {
                                mesh.UVMaps.Add(new UVMap {UVs = new List<UV>()});
                            }

                            var uvMap = mesh.UVMaps[texCoordIndex];
                            var uv = new UV
                            {
                                U = ReadHalf(),
                                V = ReadHalf()
                            };
                            uvMap.UVs.Add(uv);
                            break;
                        case VertexElementDescription.BlendWeight0:
                        case VertexElementDescription.BlendWeight1:
                            var weightIndex = int.Parse(element.Semantic[^1].ToString());
                            while (mesh.WeightValues.Count - 1 < weightIndex)
                            {
                                mesh.WeightValues.Add(new List<byte[]>());
                            }

                            var weightMap = mesh.WeightValues[weightIndex];
                            var weight = new[] {ReadByte(), ReadByte(), ReadByte(), ReadByte()};
                            weightMap.Add(weight);
                            break;
                        case VertexElementDescription.BlendIndices0:
                        case VertexElementDescription.BlendIndices1:
                            var weightIndicesIndex = int.Parse(element.Semantic[^1].ToString());
                            while (mesh.WeightIndices.Count - 1 < weightIndicesIndex)
                            {
                                mesh.WeightIndices.Add(new List<ushort[]>());
                            }

                            var weightIndexMap = mesh.WeightIndices[weightIndicesIndex];
                            var weightIndices = new[] {ReadUShort(), ReadUShort(), ReadUShort(), ReadUShort()};
                            weightIndexMap.Add(weightIndices);
                            break;
                        default:
                            // TODO: Read the remaining semantics into proper data structures
                            ReadPastVertexElement(element.Format);
                            break;
                    }
                }
            }
        }
    }

    private float ReadFloat()
    {
        var value = BitConverter.ToSingle(_currentBuffer, _currentBufferPosition);
        _currentBufferPosition += sizeof(float);
        return value;
    }

    private Half ReadHalf()
    {
        var value = BitConverter.ToHalf(_currentBuffer, _currentBufferPosition);
        _currentBufferPosition += 2; // 2 bytes in a half
        return value;
    }

    private int ReadInt()
    {
        var value = BitConverter.ToInt32(_currentBuffer, _currentBufferPosition);
        _currentBufferPosition += sizeof(int);
        return value;
    }

    private uint ReadUInt()
    {
        var value = BitConverter.ToUInt32(_currentBuffer, _currentBufferPosition);
        _currentBufferPosition += sizeof(uint);
        return value;
    }

    private ushort ReadUShort()
    {
        var value = BitConverter.ToUInt16(_currentBuffer, _currentBufferPosition);
        _currentBufferPosition += sizeof(ushort);
        return value;
    }

    private short ReadShort()
    {
        var value = BitConverter.ToInt16(_currentBuffer, _currentBufferPosition);
        _currentBufferPosition += sizeof(short);
        return value;
    }

    private byte ReadByte()
    {
        var value = _currentBuffer[_currentBufferPosition];
        _currentBufferPosition += sizeof(byte);
        return value;
    }

    private sbyte ReadSByte()
    {
        var value = unchecked((sbyte)_currentBuffer[_currentBufferPosition]);
        _currentBufferPosition += sizeof(byte);
        return value;
    }

    private void ReadPastVertexElement(VertexElementFormat format)
    {
        var (type, elementCount) = _formatParameters[format];

        for (var i = 0; i < elementCount; i++)
        {
            if (type == typeof(byte))
            {
                ReadByte();
            }
            else if (type == typeof(sbyte))
            {
                ReadSByte();
            }
            else if (type == typeof(short))
            {
                ReadShort();
            }
            else if (type == typeof(ushort))
            {
                ReadUShort();
            }
            else if (type == typeof(int))
            {
                ReadInt();
            }
            else if (type == typeof(uint))
            {
                ReadUInt();
            }
            else if (type == typeof(Half))
            {
                ReadHalf();
            }
            else if (type == typeof(float))
            {
                ReadFloat();
            }
        }
    }
}