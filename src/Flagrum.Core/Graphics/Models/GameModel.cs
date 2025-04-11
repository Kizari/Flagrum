using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Core.Graphics.Containers;
using Flagrum.Core.Mathematics;
using Flagrum.Core.Serialization.MessagePack;
using Flagrum.Core.Utilities.Extensions;

namespace Flagrum.Core.Graphics.Models;

public class GameModel : GraphicsBinary
{
    public AxisAlignedBoundingBox AxisAlignedBoundingBox { get; set; }

    public string InstanceNameFormat { get; set; }
    public string ShaderClassFormat { get; set; }
    public IList<uint> ShaderSamplerDescriptionFormat { get; set; }
    public IList<uint> ShaderParameterListFormat { get; set; }
    public IList<uint> ChildClassFormat { get; set; }

    public IList<GameModelBone> Bones { get; set; }
    public IList<GameModelNode> Nodes { get; set; }

    public float Unknown { get; set; }

    public ulong GpubinCount { get; set; }
    public IList<ulong> GpubinHashes { get; set; }
    public IList<GameModelMeshObject> MeshObjects { get; set; }

    public bool Unknown2 { get; set; }
    public string Name { get; set; }

    public bool HasPsdPath { get; set; }
    public ulong PsdPathHash { get; set; }

    public IList<GameModelPart> Parts { get; set; }

    public float Unknown3 { get; set; }
    public float Unknown4 { get; set; }
    public float Unknown5 { get; set; }
    public float Unknown6 { get; set; }
    public float Unknown7 { get; set; }

    public uint GpubinSizeCount { get; set; }
    public IList<uint> GpubinSizes { get; set; }

    public int LodLevels { get; set; }
    public Dictionary<int, List<GameModelMesh>> LodMeshes { get; } = new();

    public static GameModel Deserialize(byte[] buffer)
    {
        var model = new GameModel();
        model.Read(buffer);
        return model;
    }

    public override void Read(Stream stream)
    {
        base.Read(stream);

        using var reader = new MessagePackReader(stream);
        reader.DataVersion = Version;

        AxisAlignedBoundingBox = new AxisAlignedBoundingBox();
        AxisAlignedBoundingBox.Read(reader);

        if (reader.DataVersion < 20220707)
        {
            InstanceNameFormat = reader.Read<string>();
            ShaderClassFormat = reader.Read<string>();
            ShaderSamplerDescriptionFormat = reader.Read<List<uint>>();
            ShaderParameterListFormat = reader.Read<List<uint>>();
            ChildClassFormat = reader.Read<List<uint>>();
        }

        Bones = reader.Read<List<GameModelBone>>();
        Nodes = reader.Read<List<GameModelNode>>();

        if (reader.DataVersion >= 20220707)
        {
            Unknown = reader.Read<float>();
            GpubinCount = reader.Read<uint>();
        }
        else
        {
            GpubinCount = 1;
        }

        if (reader.DataVersion >= 20141113)
        {
            GpubinHashes = new List<ulong>();

            try
            {
                for (var i = 0UL; i < GpubinCount; i++)
                {
                    GpubinHashes.Add(reader.Read<ulong>());
                }
            }
            catch
            {
                // This happens if there is no gpubin associated with the gfxbin
                reader.Seek(-1, SeekOrigin.Current);
            }
        }

        MeshObjects = reader.Read<List<GameModelMeshObject>>();

        if (reader.DataVersion >= 20140623)
        {
            if (GpubinHashes.Count > 0)
            {
                Unknown2 = reader.Read<bool>();
            }

            Name = reader.Read<string>();
        }

        if (GpubinHashes.Count > 0 && reader.DataVersion is >= 20140722 and < 20140929)
        {
            HasPsdPath = reader.Read<bool>();
            PsdPathHash = reader.Read<ulong>();
        }

        if (reader.DataVersion >= 20140815)
        {
            Parts = reader.Read<IList<GameModelPart>>();
        }

        if (GpubinHashes.Count > 0 && reader.DataVersion >= 20220707)
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

        // Calculate LOD information
        var lods = MeshObjects
            .SelectMany(mo => mo.Meshes.Select(m => m.LodNear))
            .Distinct()
            .ToList();

        var lodMap = new Dictionary<float, int>();
        LodLevels = lods.Count;

        for (var i = 0; i < LodLevels; i++)
        {
            lodMap[lods[i]] = i;
            LodMeshes[i] = new List<GameModelMesh>();
        }

        foreach (var meshObject in MeshObjects)
        {
            foreach (var mesh in meshObject.Meshes)
            {
                var lodLevel = lodMap[mesh.LodNear];
                mesh.LodLevel = lodLevel;
                LodMeshes[lodLevel].Add(mesh);
            }
        }
    }

    public override void Write(Stream stream)
    {
        base.Write(stream);

        using var writer = new MessagePackWriter(stream);
        writer.DataVersion = Version;

        AxisAlignedBoundingBox.Write(writer);

        if (writer.DataVersion < 20220707)
        {
            writer.Write(InstanceNameFormat);
            writer.Write(ShaderClassFormat);
            writer.Write(ShaderSamplerDescriptionFormat);
            writer.Write(ShaderParameterListFormat);
            writer.Write(ChildClassFormat);
        }

        writer.Write(Bones);
        writer.Write(Nodes);

        if (writer.DataVersion >= 20220707)
        {
            writer.Write(Unknown);
            writer.Write(GpubinCount);
        }

        foreach (var gpubinHash in GpubinHashes)
        {
            writer.Write(gpubinHash);
        }

        writer.Write(MeshObjects);
        writer.Write(Unknown2);
        writer.Write(Name);
        writer.Write(Parts);

        if (writer.DataVersion >= 20220707)
        {
            writer.Write(Unknown3);
            writer.Write(Unknown4);
            writer.Write(Unknown5);
            writer.Write(Unknown6);
            writer.Write(Unknown7);

            writer.Write(GpubinSizeCount);
            foreach (var gpubinSize in GpubinSizes)
            {
                writer.Write(gpubinSize);
            }
        }
    }

    public void ReadVertexData(byte[] gpubin)
    {
        ReadVertexData(new List<byte[]> {gpubin});
    }

    public void ReadVertexData(List<byte[]> gpubins)
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
                        case FaceIndexType.IndexType16:
                            for (var i = 0; i < faceCount; i++)
                            {
                                mesh.FaceIndices[i, 0] = reader.ReadUInt16();
                                mesh.FaceIndices[i, 1] = reader.ReadUInt16();
                                mesh.FaceIndices[i, 2] = reader.ReadUInt16();
                            }

                            break;
                        case FaceIndexType.IndexType32:
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
                                        semantics[element.Semantic].Add(new[]
                                        {
                                            (float)reader.ReadByte() / 0xFF,
                                            (float)reader.ReadByte() / 0xFF,
                                            (float)reader.ReadByte() / 0xFF,
                                            (float)reader.ReadByte() / 0xFF
                                        });
                                        break;
                                    case VertexElementFormat.XYZW8_Uint:
                                        semantics[element.Semantic].Add(new uint[]
                                        {
                                            reader.ReadByte(),
                                            reader.ReadByte(),
                                            reader.ReadByte(),
                                            reader.ReadByte()
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
                                        semantics[element.Semantic].Add(new uint[]
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
                                            (float)reader.ReadHalf(),
                                            (float)reader.ReadHalf(),
                                            (float)reader.ReadHalf(),
                                            (float)reader.ReadHalf()
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

    public byte[] WriteGpubin()
    {
        using var stream = new MemoryStream();
        WriteGpubin(stream);
        return stream.ToArray();
    }

    public void WriteGpubin(string path)
    {
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        WriteGpubin(stream);
    }

    public void WriteGpubin(Stream stream)
    {
        using var writer = new BinaryWriter(stream);

        foreach (var meshObject in MeshObjects)
        {
            foreach (var mesh in meshObject.Meshes)
            {
                mesh.FaceIndexOffset = (uint)stream.Position;
                mesh.VertexCount = (uint)mesh.Semantics[VertexElementSemantic.Position0].Count;
                mesh.FaceIndexCount = (uint)mesh.FaceIndices.LongLength;

                if (mesh.VertexCount > ushort.MaxValue)
                {
                    mesh.FaceIndexType = FaceIndexType.IndexType32;
                    mesh.FaceIndexSize = mesh.FaceIndexCount * 4;
                }
                else
                {
                    mesh.FaceIndexType = FaceIndexType.IndexType16;
                    mesh.FaceIndexSize = mesh.FaceIndexCount * 2;
                }

                switch (mesh.FaceIndexType)
                {
                    case FaceIndexType.IndexType16:
                        for (var i = 0; i < mesh.FaceIndices.Length / 3; i++)
                        {
                            writer.Write((ushort)mesh.FaceIndices[i, 0]);
                            writer.Write((ushort)mesh.FaceIndices[i, 1]);
                            writer.Write((ushort)mesh.FaceIndices[i, 2]);
                        }

                        break;
                    case FaceIndexType.IndexType32:
                        for (var i = 0; i < mesh.FaceIndices.Length / 3; i++)
                        {
                            writer.Write(mesh.FaceIndices[i, 0]);
                            writer.Write(mesh.FaceIndices[i, 1]);
                            writer.Write(mesh.FaceIndices[i, 2]);
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(mesh.FaceIndexType), mesh.FaceIndexType,
                            "Unsupported face index type");
                }

                writer.Align(128, 0x00);
                mesh.VertexBufferOffset = (uint)stream.Position;

                foreach (var vertexStream in mesh.VertexStreams)
                {
                    stream.Seek(mesh.VertexBufferOffset + vertexStream.Offset, SeekOrigin.Begin);

                    vertexStream.Elements = vertexStream.Elements.OrderBy(e => e.Offset).ToList();

                    for (var i = 0; i < mesh.VertexCount; i++)
                    {
                        foreach (var element in vertexStream.Elements)
                        {
                            switch (element.Format)
                            {
                                case VertexElementFormat.XYZ32_Float:
                                    var elements = (IList<float[]>)mesh.Semantics[element.Semantic];
                                    writer.Write(elements[i][0]);
                                    writer.Write(elements[i][1]);
                                    writer.Write(elements[i][2]);
                                    break;
                                case VertexElementFormat.XY16_SintN:
                                    var elements2 = (IList<float[]>)mesh.Semantics[element.Semantic];
                                    writer.Write((short)(elements2[i][0] * short.MaxValue));
                                    writer.Write((short)(elements2[i][1] * short.MaxValue));
                                    break;
                                case VertexElementFormat.XY16_UintN:
                                    var elements3 = (IList<float[]>)mesh.Semantics[element.Semantic];
                                    writer.Write((ushort)(elements3[i][0] * ushort.MaxValue));
                                    writer.Write((ushort)(elements3[i][1] * ushort.MaxValue));
                                    break;
                                case VertexElementFormat.XY16_Float:
                                    var elements4 = (IList<float[]>)mesh.Semantics[element.Semantic];
                                    writer.Write((Half)elements4[i][0]);
                                    writer.Write((Half)elements4[i][1]);
                                    break;
                                case VertexElementFormat.XY32_Float:
                                    var elements5 = (IList<float[]>)mesh.Semantics[element.Semantic];
                                    writer.Write(elements5[i][0]);
                                    writer.Write(elements5[i][1]);
                                    break;
                                case VertexElementFormat.XYZW8_UintN:
                                case VertexElementFormat.XYZW8_Uint:
                                    var elements6 = (IList<float[]>)mesh.Semantics[element.Semantic];
                                    writer.Write((byte)(elements6[i][0] * byte.MaxValue));
                                    writer.Write((byte)(elements6[i][1] * byte.MaxValue));
                                    writer.Write((byte)(elements6[i][2] * byte.MaxValue));
                                    writer.Write((byte)(elements6[i][3] * byte.MaxValue));
                                    break;
                                case VertexElementFormat.XYZW8_SintN:
                                case VertexElementFormat.XYZW8_Sint:
                                    var elements7 = (IList<float[]>)mesh.Semantics[element.Semantic];
                                    writer.Write((byte)(elements7[i][0] * sbyte.MaxValue));
                                    writer.Write((byte)(elements7[i][1] * sbyte.MaxValue));
                                    writer.Write((byte)(elements7[i][2] * sbyte.MaxValue));
                                    writer.Write((byte)(elements7[i][3] * sbyte.MaxValue));
                                    break;
                                case VertexElementFormat.XYZW16_Float:
                                    var elements8 = (IList<float[]>)mesh.Semantics[element.Semantic];
                                    writer.Write((Half)elements8[i][0]);
                                    writer.Write((Half)elements8[i][1]);
                                    writer.Write((Half)elements8[i][2]);
                                    writer.Write((Half)elements8[i][3]);
                                    break;
                                case VertexElementFormat.XYZW16_Uint:
                                    var elements9 = (IList<uint[]>)mesh.Semantics[element.Semantic];
                                    writer.Write((ushort)elements9[i][0]);
                                    writer.Write((ushort)elements9[i][1]);
                                    writer.Write((ushort)elements9[i][2]);
                                    writer.Write((ushort)elements9[i][3]);
                                    break;
                                case VertexElementFormat.XYZW32_Uint:
                                    var elements0 = (IList<uint[]>)mesh.Semantics[element.Semantic];
                                    writer.Write(elements0[i][0]);
                                    writer.Write(elements0[i][1]);
                                    writer.Write(elements0[i][2]);
                                    writer.Write(elements0[i][3]);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException(nameof(element.Format), element.Format,
                                        $"Unsupported vertex element format for semantic {element.Semantic}");
                            }
                        }
                    }
                }

                mesh.VertexBufferSize = (uint)(stream.Position - mesh.VertexBufferOffset);
                writer.Align(128, 0x00);
            }
        }
    }

    private static Type GetElementType(VertexElementFormat format)
    {
        return format switch
        {
            VertexElementFormat.XYZ32_Float => typeof(float[]),
            VertexElementFormat.XY16_SintN => typeof(float[]),
            VertexElementFormat.XY16_UintN => typeof(float[]),
            VertexElementFormat.XY16_Float => typeof(float[]),
            VertexElementFormat.XY32_Float => typeof(float[]),
            VertexElementFormat.XYZW8_UintN => typeof(float[]),
            VertexElementFormat.XYZW8_Uint => typeof(uint[]),
            VertexElementFormat.XYZW8_SintN => typeof(float[]),
            VertexElementFormat.XYZW8_Sint => typeof(float[]),
            VertexElementFormat.XYZW16_Uint => typeof(uint[]),
            VertexElementFormat.XYZW32_Uint => typeof(uint[]),
            VertexElementFormat.XYZW16_Float => typeof(float[]),
            _ => typeof(float[])
        };
    }
}