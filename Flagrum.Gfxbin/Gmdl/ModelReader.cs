using System;
using System.Linq;
using Flagrum.Gfxbin.Gmdl.Data;
using Flagrum.Gfxbin.Serialization;

namespace Flagrum.Gfxbin.Gmdl;

public class ModelReader
{
    private readonly byte[] _gpuBuffer;
    private readonly Model _model = new();
    private readonly BinaryReader _reader;

    public ModelReader(byte[] gfxbinData, byte[] gpubinData)
    {
        _reader = new BinaryReader(gfxbinData);
        _gpuBuffer = gpubinData;
    }

    public Model Read()
    {
        _model.Header.Read(_reader);

        _model.Aabb = (_reader.ReadVector3(), _reader.ReadVector3());

        _model.InstanceNameFormat = _reader.ReadByte();
        _model.ShaderClassFormat = _reader.ReadByte();
        _model.ShaderSamplerDescriptionFormat = _reader.ReadByte();
        _model.ShaderParameterListFormat = _reader.ReadByte();
        _model.ChildClassFormat = _reader.ReadByte();

        ReadBoneTable();
        ReadNodeTable();

        if (_model.Header.Version >= 20141113)
        {
            _model.AssetHash = (ulong)_reader.Read();
        }

        ReadMeshData();

        if (_model.Header.Version >= 20140623)
        {
            _model.Unknown1 = _reader.ReadBool();
            _model.Name = _reader.ReadString();
        }

        if (_model.Header.Version is >= 20140722 and < 20140929)
        {
            _model.HasPsdPath = _reader.ReadBool();
            _model.PsdPathHash = _reader.ReadUint64();
        }

        if (_model.Header.Version >= 20140815)
        {
            _model.Parts = Enumerable.Range(0, (int)_reader.ReadUint())
                .Select(_ => new ModelPart
                {
                    Name = _reader.ReadString(),
                    Id = _reader.ReadUint(),
                    Unknown = _reader.ReadString(),
                    Flags = _reader.ReadBool()
                })
                .ToList();
        }

        ProcessGpubin();

        return _model;
    }

    private void ReadBoneTable()
    {
        var boneCount = _reader.ReadUint();
        for (var _ = 0; _ < boneCount; _++)
        {
            var boneHeader = new BoneHeader
            {
                Name = _reader.ReadString()
            };

            var lodIndex = _reader.Read();

            if (lodIndex is ushort u)
            {
                boneHeader.LodIndex = u;
            }
            else
            {
                boneHeader.LodIndex = (uint)lodIndex;
            }

            boneHeader.UniqueIndex = _model.Header.Version < 20160407 ? 0xFFFFu : boneHeader.LodIndex >> 16;
            _model.BoneHeaders.Add(boneHeader);
        }
    }

    private void ReadNodeTable()
    {
        var nodeCount = _reader.ReadUint();

        for (var _ = 0; _ < nodeCount; _++)
        {
            _model.NodeTable.Add(new NodeInformation
            {
                Matrix = _reader.ReadMatrix(),
                Name = _reader.ReadString()
            });
        }
    }

    private void ReadMeshData()
    {
        var meshObjectCount = (int)_reader.Read();
        for (var _ = 0; _ < meshObjectCount; _++)
        {
            var meshObject = new MeshObject
            {
                Name = _reader.ReadString(),
                ClusterCount = (int)_reader.Read(),
                ClusterName = _reader.ReadString()
            };

            var meshCount = _reader.ReadUint();

            foreach (var x in Enumerable.Range(0, (int)meshCount))
            {
                var mesh = new Mesh
                {
                    Name = _reader.ReadString(),
                    Unknown1 = _reader.ReadByte()
                };

                var boneIdCount = _reader.ReadUint();

                mesh.BoneIds = Enumerable.Range(0, (int)boneIdCount)
                    .Select(r =>
                    {
                        var boneId = _reader.Read();
                        return boneId switch
                        {
                            byte b => b,
                            ushort u => u,
                            _ => (uint)boneId
                        };
                    })
                    .ToList();

                mesh.VertexLayoutType = (VertexLayoutType)_reader.ReadInt();

                // NOTE: Sai suspects this is an unnecessary isOBB check
                mesh.Unknown2 = (bool)_reader.Read();

                mesh.Aabb = new Aabb(_reader.ReadVector3(), _reader.ReadVector3());

                if (_model.Header.Version >= 20160705)
                {
                    mesh.IsOrientedBB = (bool)_reader.Read();

                    mesh.OrientedBB = new OrientedBB(
                        _reader.ReadVector3(),
                        _reader.ReadVector3(),
                        _reader.ReadVector3(),
                        _reader.ReadVector3());
                }

                mesh.PrimitiveType = (PrimitiveType)_reader.ReadByte();
                mesh.FaceIndicesCount = _reader.ReadUint();
                mesh.FaceIndicesType = (IndexType)_reader.ReadByte();
                mesh.FaceIndicesBufferOffset = _reader.ReadUint();
                mesh.FaceIndicesBufferSize = _reader.ReadUint();
                mesh.FaceIndicesBuffer = new int[mesh.FaceIndicesCount];

                foreach (var i in Enumerable.Range(0, (int)mesh.FaceIndicesCount))
                {
                    switch (mesh.FaceIndicesType)
                    {
                        case IndexType.IndexType32:
                            mesh.FaceIndicesBuffer[i] =
                                BitConverter.ToInt32(_gpuBuffer, (int)mesh.FaceIndicesBufferOffset + 4 * i);
                            break;
                        case IndexType.IndexType16:
                            mesh.FaceIndicesBuffer[i] =
                                BitConverter.ToInt16(_gpuBuffer, (int)mesh.FaceIndicesBufferOffset + 2 * i);
                            break;
                        default:
                            throw new InvalidOperationException(
                                $"Provided {nameof(mesh.FaceIndicesType)} not supported.");
                    }
                }

                mesh.VertexCount = _reader.ReadUint();
                mesh.VertexStreamDescriptions = Enumerable.Range(0, (int)_reader.ReadUint())
                    .Select(r => new VertexStreamDescription
                    {
                        Slot = (VertexStreamSlot)_reader.ReadUint(),
                        Type = (VertexStreamType)_reader.ReadUint(),
                        Stride = _reader.ReadUint(),
                        StartOffset = _reader.ReadUint(),
                        VertexElementDescriptions = Enumerable.Range(0, (int)_reader.ReadUint())
                            .Select(s => new VertexElementDescription
                            {
                                Offset = _reader.ReadUint(),
                                Semantic = _reader.ReadString(),
                                Format = (VertexElementFormat)_reader.ReadUint()
                            })
                            .ToList()
                    })
                    .ToList();

                mesh.VertexBufferOffset = _reader.ReadUint();
                mesh.VertexBufferSize = _reader.ReadUint();
                mesh.VertexBuffer = new byte[mesh.VertexBufferSize];

                Array.Copy(_gpuBuffer, mesh.VertexBufferOffset, mesh.VertexBuffer, 0, mesh.VertexBufferSize);

                if (_model.Header.Version >= 20150413)
                {
                    mesh.InstanceNumber = _reader.ReadUint();
                }

                mesh.SubGeometries = Enumerable.Range(0, (int)_reader.ReadUint())
                    .Select(r => new SubGeometry
                    {
                        Aabb = new Aabb(_reader.ReadVector3(), _reader.ReadVector3()),
                        StartIndex = _reader.ReadUint(),
                        PrimitiveCount = _reader.ReadUint(),
                        ClusterIndexBitFlag = _reader.ReadUint(),
                        DrawOrder = _reader.ReadUint()
                    })
                    .ToList();

                mesh.DefaultMaterialHash = _reader.ReadUint64();

                if (_model.Header.Version >= 20140623)
                {
                    mesh.DrawPriorityOffset = _reader.ReadInt();

                    if (_reader.ReadBool())
                    {
                        mesh.Flags |= 1u;
                    }

                    if (_reader.ReadBool())
                    {
                        mesh.Flags |= 2u;
                    }

                    mesh.LodNear = _reader.ReadFloat();
                    mesh.LodFar = _reader.ReadFloat();
                    mesh.LodFade = _reader.ReadFloat();

                    if (mesh.LodNear < mesh.LodFar
                        && (mesh.LodNear > 0.0 || mesh.LodFar < 3.4028235e38))
                    {
                        mesh.Flags |= 4u;
                    }

                    if (mesh.LodFade > 0.0)
                    {
                        mesh.Flags |= 0x100u;
                    }
                }

                if (_model.Header.Version >= 20140814)
                {
                    // NOTE: Unknown flag
                    mesh.Unknown6 = _reader.ReadBool();
                }

                if (_model.Header.Version >= 20141112 && _reader.ReadBool())
                {
                    mesh.Flags |= 0x10u;
                }

                if (_model.Header.Version >= 20140815)
                {
                    mesh.PartsId = _reader.ReadUint();
                }

                if (_model.Header.Version >= 20141115)
                {
                    mesh.MeshParts = Enumerable.Range(0, _reader.ReadInt())
                        .Select(r => new MeshPart
                        {
                            PartsId = _reader.ReadUint(),
                            StartIndex = _reader.ReadUint(),
                            IndexCount = _reader.ReadUint()
                        })
                        .ToList();
                }

                if (_model.Header.Version >= 20150413 && _reader.ReadBool())
                {
                    mesh.Flags |= 0x20u;
                }

                if (_model.Header.Version >= 20150430)
                {
                    mesh.Flag = _reader.ReadUint();

                    if (_model.Header.Version < 20151217)
                    {
                        mesh.Flags |= 0x200u;

                        if (!mesh.BoneIds.Any())
                        {
                            mesh.Flags |= 0x400u;
                        }
                    }

                    mesh.Flags |= mesh.Flag;
                }

                if (_model.Header.Version >= 20150512 && _reader.ReadBool())
                {
                    mesh.Flags |= 0x80u;
                }

                if (_model.Header.Version < 20160420)
                {
                    mesh.BreakableBoneIndex = 0xFFFFFFFF;
                    mesh.LowLodShadowCascadeNo = 2;
                }
                else
                {
                    mesh.BreakableBoneIndex = _reader.ReadUint();
                    mesh.LowLodShadowCascadeNo = (sbyte)_reader.ReadUint();
                }

                if ((mesh.Flags & 0x80000) != 0)
                {
                    mesh.Unknown3 = _reader.ReadBool();
                    mesh.Unknown4 = _reader.ReadUint();
                    mesh.Unknown5 = _reader.ReadUint();
                }

                meshObject.Meshes.Add(mesh);
            }

            _model.MeshObjects.Add(meshObject);
        }
    }

    private void ProcessGpubin()
    {
        var gpuBuffer = new GpuBuffer(_gpuBuffer);

        foreach (var meshObject in _model.MeshObjects)
        {
            foreach (var mesh in meshObject.Meshes.Where(m => m.LodNear == 0.0))
            {
                var faces = new int[mesh.FaceIndicesBuffer.Length / 3, 3];
                for (var f = 0; f < mesh.FaceIndicesBuffer.Length; f += 3)
                {
                    faces[f / 3, 0] = mesh.FaceIndicesBuffer[f];
                    faces[f / 3, 1] = mesh.FaceIndicesBuffer[f + 1];
                    faces[f / 3, 2] = mesh.FaceIndicesBuffer[f + 2];
                }

                gpuBuffer.SetBuffer(mesh.Name, faces, (int)mesh.VertexBufferOffset, (int)mesh.VertexBufferSize);

                foreach (var vertexStream in mesh.VertexStreamDescriptions)
                {
                    gpuBuffer.Position = (int)vertexStream.StartOffset;

                    for (var i = 0; i < mesh.VertexCount; i++)
                    {
                        foreach (var element in vertexStream.VertexElementDescriptions)
                        {
                            gpuBuffer.ReadVertexElement(element.Semantic, element.Format);
                        }
                    }
                }
            }
        }

        // Used for checking which semantics are present in the model when uncommented

        // var buffers = gpuBuffer._meshBuffers
        //     .Select(m => m.Value);
        //
        // foreach (var buffer in buffers)
        // {
        //     if (buffer.Data.TryGetValue(VertexElementDescription.Binormal0, out var list))
        //     {
        //         foreach (var value in list)
        //         {
        //             Console.WriteLine(value);
        //         }
        //     }
        //     else
        //     {
        //         Console.WriteLine("List does not exist.");
        //     }
        // }

        _model.Gpubin = gpuBuffer.ToGpubin();
    }
}