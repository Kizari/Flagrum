using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Core.Gfxbin.Gmdl.Components;

namespace Flagrum.Core.Gfxbin.Gmdl.MessagePack;

public class Gfxbin
{
    public Gfxbin(byte[] gfxbinData)
    {
        using var stream = new MemoryStream(gfxbinData);
        using var reader = new MessagePackReader(stream);

        Header = new GfxbinHeader();
        Header.Read(reader);

        reader.DataVersion = Header.Version;

        Aabb = reader.ReadAABB();

        if (Header.Version < 20220707)
        {
            InstanceNameFormat = reader.Read<string>();
            ShaderClassFormat = reader.Read<string>();
            ShaderSamplerDescriptionFormat = reader.Read<List<uint>>();
            ShaderParameterListFormat = reader.Read<List<uint>>();
            ChildClassFormat = reader.Read<List<uint>>();
        }

        Bones = reader.Read<List<GfxbinBone>>();
        Nodes = reader.Read<List<GfxbinNode>>();

        if (Header.Version >= 20220707)
        {
            Unknown = reader.Read<float>();
            GpubinCount = reader.Read<uint>();
        }
        else
        {
            GpubinCount = 1;
        }

        GpubinHashes = new List<ulong>();
        for (var i = 0UL; i < GpubinCount; i++)
        {
            GpubinHashes.Add(reader.Read<ulong>());
        }

        MeshObjects = reader.Read<List<GfxbinMeshObject>>();

        Unknown2 = reader.Read<bool>();
        Name = reader.Read<string>();

        Parts = reader.Read<IList<GfxbinModelPart>>();

        if (Header.Version >= 20220707)
        {
            Unknown3 = reader.Read<float>();
            Unknown4 = reader.Read<float>();
            Unknown5 = reader.Read<float>();
            Unknown6 = reader.Read<float>();
            Unknown7 = reader.Read<float>();

            GpubinSizeCount = reader.Read<uint>();
            GpubinSizes = new List<uint>();
            for (var i = 0; i < GpubinSizeCount; i++)
            {
                GpubinSizes.Add(reader.Read<uint>());
            }
        }
    }

    public GfxbinHeader Header { get; set; }
    public GfxbinAABB Aabb { get; set; }

    public string InstanceNameFormat { get; set; }
    public string ShaderClassFormat { get; set; }
    public IList<uint> ShaderSamplerDescriptionFormat { get; set; }
    public IList<uint> ShaderParameterListFormat { get; set; }
    public IList<uint> ChildClassFormat { get; set; }

    public IList<GfxbinBone> Bones { get; set; }
    public IList<GfxbinNode> Nodes { get; set; }

    public float Unknown { get; set; }

    public ulong GpubinCount { get; set; }
    public IList<ulong> GpubinHashes { get; set; }
    public IList<GfxbinMeshObject> MeshObjects { get; set; }

    public bool Unknown2 { get; set; }
    public string Name { get; set; }

    public IList<GfxbinModelPart> Parts { get; set; }

    public float Unknown3 { get; set; }
    public float Unknown4 { get; set; }
    public float Unknown5 { get; set; }
    public float Unknown6 { get; set; }
    public float Unknown7 { get; set; }

    public uint GpubinSizeCount { get; set; }
    public IList<uint> GpubinSizes { get; set; }

    // public void ReadVertexData(List<byte[]> gpubins)
    // {
    //     //var gpubin = new Gpubin(gpubins);
    //     // foreach (var meshObject in MeshObjects)
    //     // {
    //     //     foreach (var mesh in meshObject.Meshes)
    //     //     {
    //     //         mesh.FaceIndices = gpubin.UnpackFaceIndices(mesh.FaceIndexOffset, mesh.FaceIndexCount, mesh.FaceIndexType);
    //     //         gpubin.UnpackVertexStreams(mesh);
    //     //     }
    //     // }
    //     
    //     Parallel.ForEach(MeshObjects,
    //         meshObject =>
    //         {
    //             Parallel.ForEach(meshObject.Meshes,
    //                 mesh =>
    //                 {
    //                     Parallel.Invoke(
    //                         () =>
    //                         {
    //                             mesh.FaceIndices = gpubin.UnpackFaceIndices(mesh.FaceIndexOffset, mesh.FaceIndexCount,
    //                                 mesh.FaceIndexType);
    //                         }, () => gpubin.UnpackVertexStreams(mesh));
    //                 });
    //         });
    // }

    public void ReadVertexData2(List<byte[]> gpubins)
    {
        Parallel.ForEach(MeshObjects, meshObject =>
        {
            Parallel.ForEach(meshObject.Meshes, mesh =>
            {
                Parallel.Invoke(() =>
                {
                    using var stream = new MemoryStream(gpubins[(int)mesh.GpubinIndex]);
                    using var reader = new BinaryReader(stream);

                    var faceCount = mesh.FaceIndexCount / 3;
                    mesh.FaceIndices = new uint[faceCount, 3];

                    stream.Seek(mesh.FaceIndexOffset, SeekOrigin.Begin);

                    switch (mesh.FaceIndexType)
                    {
                        case IndexType.IndexType16:
                            for (var i = 0; i < faceCount; i++)
                            {
                                mesh.FaceIndices[i, 0] = reader.ReadUInt16();
                                mesh.FaceIndices[i, 1] = reader.ReadUInt16();
                                mesh.FaceIndices[i, 2] = reader.ReadUInt16();
                            }

                            break;
                        case IndexType.IndexType32:
                            for (var i = 0; i < faceCount; i++)
                            {
                                mesh.FaceIndices[i, 0] = reader.ReadUInt32();
                                mesh.FaceIndices[i, 1] = reader.ReadUInt32();
                                mesh.FaceIndices[i, 2] = reader.ReadUInt32();
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(mesh.FaceIndexType), mesh.FaceIndexType,
                                "Unsupported face index type");
                    }
                }, () =>
                {
                    Parallel.ForEach(mesh.VertexStreams, vertexStream =>
                    {
                        var semantics = new Dictionary<VertexElementSemantic, IList>();

                        using var stream = new MemoryStream(gpubins[(int)mesh.GpubinIndex]);
                        using var reader = new BinaryReader(stream);

                        stream.Seek(mesh.VertexBufferOffset + vertexStream.Offset, SeekOrigin.Begin);

                        vertexStream.Elements = vertexStream.Elements.OrderBy(e => e.Offset).ToList();
                        foreach (var element in vertexStream.Elements)
                        {
                            var type = GetElementType(element.Format);
                            var listType = typeof(List<>).MakeGenericType(type);
                            semantics[element.Semantic] = (IList)Activator.CreateInstance(listType);
                        }

                        for (var i = 0; i < mesh.VertexCount; i++)
                        {
                            foreach (var element in vertexStream.Elements)
                            {
                                switch (element.Format)
                                {
                                    case VertexElementFormat.XYZ32_Float:
                                        semantics[element.Semantic].Add(new[]
                                        {
                                            reader.ReadSingle(),
                                            reader.ReadSingle(),
                                            reader.ReadSingle()
                                        });
                                        break;
                                    case VertexElementFormat.XY16_SintN:
                                        semantics[element.Semantic].Add(new[]
                                        {
                                            reader.ReadInt16() * (1f / 0x7FFF),
                                            reader.ReadInt16() * (1f / 0x7FFF)
                                        });
                                        break;
                                    case VertexElementFormat.XY16_UintN:
                                        semantics[element.Semantic].Add(new[]
                                        {
                                            (float)reader.ReadUInt16() / 0xFFFF,
                                            (float)reader.ReadUInt16() / 0xFFFF
                                        });
                                        break;
                                    case VertexElementFormat.XY16_Float:
                                        semantics[element.Semantic].Add(new[]
                                        {
                                            (float)reader.ReadHalf(),
                                            (float)reader.ReadHalf()
                                        });
                                        break;
                                    case VertexElementFormat.XY32_Float:
                                        semantics[element.Semantic].Add(new[]
                                        {
                                            reader.ReadSingle(),
                                            reader.ReadSingle()
                                        });
                                        break;
                                    case VertexElementFormat.XYZW8_UintN:
                                    case VertexElementFormat.XYZW8_Uint:
                                        semantics[element.Semantic].Add(new[]
                                        {
                                            (float)reader.ReadByte() / 0xFF,
                                            (float)reader.ReadByte() / 0xFF,
                                            (float)reader.ReadByte() / 0xFF,
                                            (float)reader.ReadByte() / 0xFF
                                        });
                                        break;
                                    case VertexElementFormat.XYZW8_SintN:
                                    case VertexElementFormat.XYZW8_Sint:
                                        semantics[element.Semantic].Add(new[]
                                        {
                                            reader.ReadSByte() * (1f / 0x7F),
                                            reader.ReadSByte() * (1f / 0x7F),
                                            reader.ReadSByte() * (1f / 0x7F),
                                            reader.ReadSByte() * (1f / 0x7F)
                                        });
                                        break;
                                    case VertexElementFormat.XYZW16_Uint:
                                        semantics[element.Semantic].Add(new[]
                                        {
                                            reader.ReadUInt16(),
                                            reader.ReadUInt16(),
                                            reader.ReadUInt16(),
                                            reader.ReadUInt16()
                                        });
                                        break;
                                    case VertexElementFormat.XYZW32_Uint:
                                        semantics[element.Semantic].Add(new[]
                                        {
                                            reader.ReadUInt32(),
                                            reader.ReadUInt32(),
                                            reader.ReadUInt32(),
                                            reader.ReadUInt32()
                                        });
                                        break;
                                    case VertexElementFormat.XYZW16_Float:
                                        semantics[element.Semantic].Add(new[]
                                        {
                                            reader.ReadHalf(),
                                            reader.ReadHalf(),
                                            reader.ReadHalf(),
                                            reader.ReadHalf()
                                        });
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException(nameof(element.Format), element.Format,
                                            $"Unsupported vertex element format for semantic {element.Semantic}");
                                }
                            }
                        }

                        foreach (var (semantic, list) in semantics)
                        {
                            mesh.Semantics[semantic] = list;
                        }
                    });
                });
            });
        });
    }

    private Type GetElementType(VertexElementFormat format)
    {
        return format switch
        {
            VertexElementFormat.XYZ32_Float => typeof(float[]),
            VertexElementFormat.XY16_SintN => typeof(float[]),
            VertexElementFormat.XY16_UintN => typeof(float[]),
            VertexElementFormat.XY16_Float => typeof(float[]),
            VertexElementFormat.XY32_Float => typeof(float[]),
            VertexElementFormat.XYZW8_UintN => typeof(float[]),
            VertexElementFormat.XYZW8_Uint => typeof(float[]),
            VertexElementFormat.XYZW8_SintN => typeof(float[]),
            VertexElementFormat.XYZW8_Sint => typeof(float[]),
            VertexElementFormat.XYZW16_Uint => typeof(ushort[]),
            VertexElementFormat.XYZW32_Uint => typeof(uint[]),
            VertexElementFormat.XYZW16_Float => typeof(Half[]),
            _ => typeof(float[])
        };
    }
}