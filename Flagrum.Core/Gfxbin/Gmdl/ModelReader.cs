using System;
using System.Linq;
using Flagrum.Core.Gfxbin.Gmdl.Buffering;
using Flagrum.Core.Gfxbin.Gmdl.Components;
using Flagrum.Core.Gfxbin.Gmdl.Constructs;
using Flagrum.Core.Gfxbin.Serialization;

namespace Flagrum.Core.Gfxbin.Gmdl;

public class ModelReader
{
    private readonly Model _model = new();
    private readonly BinaryReader _reader;
    private readonly GpubinUnpacker _unpacker;

    public ModelReader(byte[] gfxbinData, byte[] gpubinData)
    {
        _reader = new BinaryReader(gfxbinData);
        _unpacker = new GpubinUnpacker(gpubinData);
    }

    public Model Read()
    {
        _model.Header.Read(_reader);

        _model.Aabb = new Aabb(_reader.ReadVector3(), _reader.ReadVector3());

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
        var meshObjectCountObject = _reader.Read();
        uint meshObjectCount;
        if (meshObjectCountObject is int integer)
        {
            meshObjectCount = Convert.ToUInt32(integer);
        }
        else if (meshObjectCountObject is ushort uint16)
        {
            meshObjectCount = Convert.ToUInt32(uint16);
        }
        else
        {
            meshObjectCount = (uint)meshObjectCountObject;
        }
        
        for (var i = 0; i < meshObjectCount; i++)
        {
            // NOTE: Seems to be a random bool here for MeshObjects after the first
            // Not sure what it does
            if (i > 0)
            {
                _ = _reader.ReadBool();
            }

            var meshObject = new MeshObject
            {
                Name = _reader.ReadString()
            };

            var clusterCount = _reader.Read();
            meshObject.ClusterCount = clusterCount is byte b ? b : (int)clusterCount;
            meshObject.ClusterName = _reader.ReadString();

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
                var faceIndicesCount = _reader.ReadUint();
                var faceIndexType = (IndexType)_reader.ReadByte();
                mesh.FaceIndexBufferOffset = _reader.ReadUint();
                mesh.FaceIndices = _unpacker.UnpackFaceIndices(
                    (int)mesh.FaceIndexBufferOffset,
                    (int)faceIndicesCount,
                    faceIndexType);

                // Size of the face indices buffer for this mesh
                // No point storing it as it is derived from the type and count
                _ = _reader.ReadUint();

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
                var vertexBufferSize = _reader.ReadUint();
                _unpacker.UnpackVertexStreams(mesh, vertexBufferSize);

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
}