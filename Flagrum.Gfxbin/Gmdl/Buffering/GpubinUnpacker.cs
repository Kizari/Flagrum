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

    public GpubinUnpacker(byte[] gpubin)
    {
        _gpubin = gpubin;
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
}