// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Threading.Tasks;
// using Flagrum.Core.Gfxbin.Gmdl.Components;
// using Flagrum.Core.Gfxbin.Gmdl.Constructs;
//
// namespace Flagrum.Core.Gfxbin.Gmdl.MessagePack;
//
// public class Gpubin
// {
//     private readonly List<byte[]> _buffers;
//
//     private readonly Dictionary<VertexElementFormat, (Type type, int elementCount)> _formatParameters;
//
//     public Gpubin(List<byte[]> gpubins)
//     {
//         _buffers = gpubins;
//         _formatParameters = new Dictionary<VertexElementFormat, (Type type, int elementCount)>
//         {
//             {VertexElementFormat.X8_Uint, (typeof(byte), 1)},
//             {VertexElementFormat.XYZW8_Uint, (typeof(byte), 4)},
//             {VertexElementFormat.X8_Sint, (typeof(sbyte), 1)},
//             {VertexElementFormat.XYZW8_Sint, (typeof(sbyte), 4)},
//
//             {VertexElementFormat.X8_UintN, (typeof(byte), 1)},
//             {VertexElementFormat.XYZW8_UintN, (typeof(byte), 4)},
//             {VertexElementFormat.X8_SintN, (typeof(sbyte), 1)},
//             {VertexElementFormat.XYZW8_SintN, (typeof(sbyte), 4)},
//
//             {VertexElementFormat.X16_Sint, (typeof(short), 1)},
//             {VertexElementFormat.XY16_Sint, (typeof(short), 2)},
//             {VertexElementFormat.XYZW16_Sint, (typeof(short), 4)},
//
//             {VertexElementFormat.X16_Uint, (typeof(ushort), 1)},
//             {VertexElementFormat.XY16_Uint, (typeof(ushort), 2)},
//             {VertexElementFormat.XYZW16_Uint, (typeof(ushort), 4)},
//
//             {VertexElementFormat.X16_SintN, (typeof(short), 1)},
//             {VertexElementFormat.XY16_SintN, (typeof(short), 2)},
//             {VertexElementFormat.XYZW16_SintN, (typeof(short), 4)},
//
//             {VertexElementFormat.X16_UintN, (typeof(ushort), 1)},
//             {VertexElementFormat.XY16_UintN, (typeof(ushort), 2)},
//             {VertexElementFormat.XYZW16_UintN, (typeof(ushort), 4)},
//
//             {VertexElementFormat.X32_Sint, (typeof(int), 1)},
//             {VertexElementFormat.XY32_Sint, (typeof(int), 2)},
//             {VertexElementFormat.XYZ32_Sint, (typeof(int), 3)},
//             {VertexElementFormat.XYZW32_Sint, (typeof(int), 4)},
//
//             {VertexElementFormat.X32_Uint, (typeof(uint), 1)},
//             {VertexElementFormat.XY32_Uint, (typeof(uint), 2)},
//             {VertexElementFormat.XYZ32_Uint, (typeof(uint), 3)},
//             {VertexElementFormat.XYZW32_Uint, (typeof(uint), 4)},
//
//             {VertexElementFormat.X32_SintN, (typeof(int), 1)},
//             {VertexElementFormat.XY32_SintN, (typeof(int), 2)},
//             {VertexElementFormat.XYZ32_SintN, (typeof(int), 3)},
//             {VertexElementFormat.XYZW32_SintN, (typeof(int), 4)},
//
//             {VertexElementFormat.X32_UintN, (typeof(uint), 1)},
//             {VertexElementFormat.XY32_UintN, (typeof(uint), 2)},
//             {VertexElementFormat.XYZ32_UintN, (typeof(uint), 3)},
//             {VertexElementFormat.XYZW32_UintN, (typeof(uint), 4)},
//
//             {VertexElementFormat.X16_Float, (typeof(Half), 1)},
//             {VertexElementFormat.XY16_Float, (typeof(Half), 2)},
//             {VertexElementFormat.XYZW16_Float, (typeof(Half), 4)},
//
//             {VertexElementFormat.X32_Float, (typeof(float), 1)},
//             {VertexElementFormat.XY32_Float, (typeof(float), 2)},
//             {VertexElementFormat.XYZ32_Float, (typeof(float), 3)},
//             {VertexElementFormat.XYZW32_Float, (typeof(float), 4)}
//         };
//     }
//
//     public uint[,] UnpackFaceIndices(uint gpubinOffset, uint faceIndicesCount, IndexType faceIndexType)
//     {
//         using var stream = new MemoryStream(_buffers[0]);
//         using var reader = new BinaryReader(stream);
//
//         var faceCount = faceIndicesCount / 3; // All faces are tris on gmdl meshes
//         var faceIndices = new uint[faceCount, 3];
//
//         stream.Seek(gpubinOffset, SeekOrigin.Begin);
//
//         // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
//         switch (faceIndexType)
//         {
//             case IndexType.IndexType16:
//                 for (var i = 0; i < faceCount; i++)
//                 {
//                     faceIndices[i, 0] = reader.ReadUInt16();
//                     faceIndices[i, 1] = reader.ReadUInt16();
//                     faceIndices[i, 2] = reader.ReadUInt16();
//                 }
//
//                 break;
//             case IndexType.IndexType32:
//                 for (var i = 0; i < faceCount; i++)
//                 {
//                     faceIndices[i, 0] = reader.ReadUInt32();
//                     faceIndices[i, 1] = reader.ReadUInt32();
//                     faceIndices[i, 2] = reader.ReadUInt32();
//                 }
//
//                 break;
//             default:
//                 throw new ArgumentOutOfRangeException(nameof(faceIndexType), faceIndexType, null);
//         }
//
//         return faceIndices;
//     }
//
//     public void UnpackVertexStreams(GfxbinMesh mesh)
//     {
//         Parallel.ForEach(mesh.VertexStreams, vertexStream =>
//         {
//             using var stream = new MemoryStream(_buffers[(int)mesh.GpubinIndex]);
//             using var reader = new BinaryReader(stream);
//
//             stream.Seek(mesh.VertexBufferOffset + vertexStream.Offset, SeekOrigin.Begin);
//
//             for (var i = 0; i < mesh.VertexCount; i++)
//             {
//                 foreach (var element in vertexStream.Elements)
//                 {
//                     switch (element.Semantic)
//                     {
//                         case VertexElementDescription.Position0:
//                             var position = new Vector3
//                             {
//                                 X = ReadSingle(reader),
//                                 Y = ReadSingle(reader),
//                                 Z = ReadSingle(reader)
//                             };
//                             mesh.VertexPositions.Add(position);
//                             break;
//                         case VertexElementDescription.Normal0:
//                             var normal = new Normal
//                             {
//                                 X = reader.ReadSByte(),
//                                 Y = reader.ReadSByte(),
//                                 Z = reader.ReadSByte(),
//                                 W = reader.ReadSByte()
//                             };
//                             mesh.Normals.Add(normal);
//                             break;
//                         case VertexElementDescription.Tangent0:
//                             var tangent = new Normal
//                             {
//                                 X = reader.ReadSByte(),
//                                 Y = reader.ReadSByte(),
//                                 Z = reader.ReadSByte(),
//                                 W = reader.ReadSByte()
//                             };
//                             mesh.Tangents.Add(tangent);
//                             break;
//                         case VertexElementDescription.Color0:
//                         case VertexElementDescription.Color1:
//                         case VertexElementDescription.Color2:
//                         case VertexElementDescription.Color3:
//                             var colorIndex = int.Parse(element.Semantic[^1].ToString());
//                             while (mesh.ColorMaps.Count - 1 < colorIndex)
//                             {
//                                 mesh.ColorMaps.Add(new ColorMap {Colors = new List<Color4>()});
//                             }
//
//                             var colorMap = mesh.ColorMaps[colorIndex];
//                             var color = new Color4
//                             {
//                                 R = reader.ReadByte(),
//                                 G = reader.ReadByte(),
//                                 B = reader.ReadByte(),
//                                 A = reader.ReadByte()
//                             };
//                             colorMap.Colors.Add(color);
//                             break;
//                         case VertexElementDescription.TexCoord0:
//                         case VertexElementDescription.TexCoord1:
//                         case VertexElementDescription.TexCoord2:
//                         case VertexElementDescription.TexCoord3:
//                         case VertexElementDescription.TexCoord4:
//                         case VertexElementDescription.TexCoord5:
//                         case VertexElementDescription.TexCoord6:
//                         case VertexElementDescription.TexCoord7:
//                             var texCoordIndex = int.Parse(element.Semantic[^1].ToString());
//                             while (mesh.UVMaps.Count - 1 < texCoordIndex)
//                             {
//                                 mesh.UVMaps.Add(new UVMap {UVs = new List<UV>()});
//                             }
//
//                             var uvMap = mesh.UVMaps[texCoordIndex];
//                             var uv = new UV
//                             {
//                                 U = ReadHalf(reader),
//                                 V = ReadHalf(reader)
//                             };
//
//                             uvMap.UVs.Add(uv);
//                             break;
//                         case VertexElementDescription.BlendWeight0:
//                         case VertexElementDescription.BlendWeight1:
//                             var weightIndex = int.Parse(element.Semantic[^1].ToString());
//                             while (mesh.WeightValues.Count - 1 < weightIndex)
//                             {
//                                 mesh.WeightValues.Add(new List<byte[]>());
//                             }
//
//                             var weightMap = mesh.WeightValues[weightIndex];
//                             var weight = new[]
//                                 {reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte()};
//                             weightMap.Add(weight);
//                             break;
//                         case VertexElementDescription.BlendIndices0:
//                         case VertexElementDescription.BlendIndices1:
//                             var weightIndicesIndex = int.Parse(element.Semantic[^1].ToString());
//                             while (mesh.WeightIndices.Count - 1 < weightIndicesIndex)
//                             {
//                                 mesh.WeightIndices.Add(new List<ushort[]>());
//                             }
//
//                             var weightIndexMap = mesh.WeightIndices[weightIndicesIndex];
//                             var weightIndices = new[]
//                             {
//                                 ReadWeightIndex(reader, element.Format),
//                                 ReadWeightIndex(reader, element.Format),
//                                 ReadWeightIndex(reader, element.Format),
//                                 ReadWeightIndex(reader, element.Format)
//                             };
//                             weightIndexMap.Add(weightIndices);
//                             break;
//                         default:
//                             // TODO: Read the remaining semantics into proper data structures
//                             ReadPastVertexElement(reader, element.Format);
//                             break;
//                     }
//                 }
//             }
//         });
//     }
//
//     private ushort ReadWeightIndex(BinaryReader reader, VertexElementFormat format)
//     {
//         var (type, _) = _formatParameters[format];
//         return type == typeof(byte) ? reader.ReadByte() : reader.ReadUInt16();
//     }
//
//     private void ReadPastVertexElement(BinaryReader reader, VertexElementFormat format)
//     {
//         var (type, elementCount) = _formatParameters[format];
//
//         for (var i = 0; i < elementCount; i++)
//         {
//             if (type == typeof(byte))
//             {
//                 reader.ReadByte();
//             }
//             else if (type == typeof(sbyte))
//             {
//                 reader.ReadSByte();
//             }
//             else if (type == typeof(short))
//             {
//                 reader.ReadInt16();
//             }
//             else if (type == typeof(ushort))
//             {
//                 reader.ReadUInt16();
//             }
//             else if (type == typeof(int))
//             {
//                 reader.ReadInt32();
//             }
//             else if (type == typeof(uint))
//             {
//                 reader.ReadUInt32();
//             }
//             else if (type == typeof(Half))
//             {
//                 ReadHalf(reader);
//             }
//             else if (type == typeof(float))
//             {
//                 ReadSingle(reader);
//             }
//         }
//     }
//
//     private float ReadSingle(BinaryReader reader)
//     {
//         var value = reader.ReadSingle();
//
//         if (float.IsNaN(value)
//             || float.IsInfinity(value)
//             || float.IsNegativeInfinity(value)
//             || float.IsPositiveInfinity(value))
//         {
//             return 0.0f;
//         }
//
//         return value;
//     }
//
//     private Half ReadHalf(BinaryReader reader)
//     {
//         var value = reader.ReadHalf();
//
//         if (Half.IsNaN(value)
//             || Half.IsInfinity(value)
//             || Half.IsNegativeInfinity(value)
//             || Half.IsPositiveInfinity(value))
//         {
//             return (Half)0;
//         }
//
//         return value;
//     }
// }

