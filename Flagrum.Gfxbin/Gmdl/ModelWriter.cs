using System;
using System.Linq;
using Flagrum.Gfxbin.Gmdl.Data;
using Flagrum.Gfxbin.Serialization;

namespace Flagrum.Gfxbin.Gmdl
{
    public class ModelWriter
    {
        private readonly byte[] _gpuBuffer;
        private readonly Model _model;
        private readonly BinaryWriter _writer = new();

        public ModelWriter(Model model, byte[] gpubinData)
        {
            _model = model;
            _gpuBuffer = gpubinData;
        }

        public byte[] Write()
        {
            _model.Header.Write(_writer);

            _writer.WriteVector3(_model.Aabb.Min);
            _writer.WriteVector3(_model.Aabb.Max);

            _writer.WriteByte(_model.InstanceNameFormat);
            _writer.WriteByte(_model.ShaderClassFormat);
            _writer.WriteByte(_model.ShaderSamplerDescriptionFormat);
            _writer.WriteByte(_model.ShaderParameterListFormat);
            _writer.WriteByte(_model.ChildClassFormat);

            WriteBoneTable();
            WriteNodeTable();

            // TODO: Handle versioning here (see reader)
            _writer.WriteUInt(_model.AssetHash);

            WriteMeshData();

            // TODO: Version branching
            _writer.Write(_model.Unknown1);
            _writer.WriteString(_model.Name);
            _writer.WriteArraySize((uint)_model.Parts.Count());

            foreach (var part in _model.Parts)
            {
                _writer.WriteString(part.Name);
                _writer.WriteUInt(part.Id);
                _writer.WriteString(part.Unknown);
                Console.WriteLine(part.Name);
                _writer.Write(part.Flags);
            }

            return _writer.ToArray();
        }

        private void WriteBoneTable()
        {
            _writer.WriteArraySize((uint)_model.BoneHeaders.Count);
            foreach (var header in _model.BoneHeaders)
            {
                _writer.WriteString(header.Name);
                _writer.WriteUInt(header.LodIndex);
            }
        }

        private void WriteNodeTable()
        {
            _writer.WriteArraySize((uint)_model.NodeTable.Count);
            foreach (var node in _model.NodeTable)
            {
                _writer.WriteMatrix(node.Matrix);
                _writer.WriteString(node.Name);
            }
        }

        private void WriteMeshData()
        {
            _writer.WriteArraySize((uint)_model.MeshObjects.Count);
            foreach (var meshObject in _model.MeshObjects)
            {
                _writer.WriteString(meshObject.Name);
                _writer.WriteArraySize((uint)meshObject.ClusterCount);
                _writer.WriteString(meshObject.ClusterName);

                _writer.WriteArraySize((uint)meshObject.Meshes.Count);

                foreach (var mesh in meshObject.Meshes)
                {
                    _writer.WriteString(mesh.Name);
                    _writer.WriteByte(mesh.Unknown1);

                    _writer.WriteArraySize((uint)mesh.BoneIds.Count());

                    foreach (var boneId in mesh.BoneIds)
                    {
                        _writer.WriteUInt(boneId);
                    }

                    _writer.WriteUInt((byte)mesh.VertexLayoutType);
                    _writer.Write(mesh.Unknown2);

                    _writer.WriteVector3(mesh.Aabb.Min);
                    _writer.WriteVector3(mesh.Aabb.Max);

                    // TODO: Add versioning (see reader)
                    _writer.Write(mesh.IsOrientedBB);
                    _writer.WriteVector3(mesh.OrientedBB.Center);
                    _writer.WriteVector3(mesh.OrientedBB.XHalfExtent);
                    _writer.WriteVector3(mesh.OrientedBB.YHalfExtent);
                    _writer.WriteVector3(mesh.OrientedBB.ZHalfExtent);

                    _writer.WriteByte((byte)mesh.PrimitiveType);
                    _writer.WriteUInt(mesh.IndexCount);
                    _writer.WriteByte((byte)mesh.IndexType);
                    _writer.WriteUInt(mesh.IndexBufferOffset);
                    _writer.WriteUInt(mesh.IndexBufferSize);

                    _writer.WriteUInt(mesh.VertexCount);
                    _writer.WriteArraySize((uint)mesh.VertexStreamDescriptions.Count());

                    foreach (var vertex in mesh.VertexStreamDescriptions)
                    {
                        _writer.WriteUInt((uint)vertex.Slot);
                        _writer.WriteUInt((uint)vertex.Type);
                        _writer.WriteUInt(vertex.Stride);
                        _writer.WriteUInt(vertex.StartOffset);
                        _writer.WriteArraySize((uint)vertex.VertexElementDescriptions.Count());

                        foreach (var description in vertex.VertexElementDescriptions)
                        {
                            _writer.WriteUInt(description.Offset);
                            _writer.WriteString(description.Semantic);
                            _writer.WriteUInt((uint)description.Format);
                        }
                    }

                    _writer.WriteUInt(mesh.VertexBufferOffset);
                    _writer.WriteUInt(mesh.VertexBufferSize);

                    // TODO: Handle versioning (see reader)
                    _writer.WriteUInt(mesh.InstanceNumber);
                    _writer.WriteArraySize((uint)mesh.SubGeometries.Count());

                    foreach (var geometry in mesh.SubGeometries)
                    {
                        _writer.WriteVector3(geometry.Aabb.Min);
                        _writer.WriteVector3(geometry.Aabb.Max);
                        _writer.WriteUInt(geometry.StartIndex);
                        _writer.WriteUInt(geometry.PrimitiveCount);
                        _writer.WriteUInt(geometry.ClusterIndexBitFlag);
                        _writer.WriteUInt(geometry.DrawOrder);
                    }

                    _writer.WriteUInt(mesh.DefaultMaterialHash);

                    // TODO: More versioning for the rest of this method (see reader)

                    // FIXME: Should be written as an int, not uint
                    _writer.WriteUInt((uint)mesh.DrawPriorityOffset);

                    _writer.Write((mesh.Flags & 1u) != 0);
                    _writer.Write((mesh.Flags & 2u) != 0);

                    _writer.WriteFloat(mesh.LodNear);
                    _writer.WriteFloat(mesh.LodFar);
                    _writer.WriteFloat(mesh.LodFade);

                    _writer.Write(mesh.Unknown6);
                    _writer.Write((mesh.Flags & 0x10u) != 0);

                    _writer.WriteUInt(mesh.PartsId);
                    _writer.WriteArraySize((uint)mesh.MeshParts.Count());

                    foreach (var part in mesh.MeshParts)
                    {
                        Console.WriteLine($"{part.PartsId} - {part.PartsId}");
                        Console.WriteLine($"{part.StartIndex} - {part.StartIndex}");
                        Console.WriteLine($"{part.IndexCount} - {part.IndexCount}");
                        Console.WriteLine("");

                        _writer.WriteUInt(part.PartsId);
                        _writer.WriteUInt(part.StartIndex);
                        _writer.WriteUInt(part.IndexCount);
                    }

                    _writer.Write((mesh.Flags & 0x20u) != 0);
                    _writer.WriteUInt(mesh.Flag);
                    _writer.Write((mesh.Flags & 0x80u) != 0);

                    _writer.WriteUInt(mesh.BreakableBoneIndex);
                    _writer.WriteSByte(mesh.LowLodShadowCascadeNo);

                    if (mesh.Unknown3 || mesh.Unknown4 > 0 || mesh.Unknown5 > 0)
                    {
                        _writer.Write(mesh.Unknown3);
                        _writer.WriteUInt(mesh.Unknown4);
                        _writer.WriteUInt(mesh.Unknown5);
                    }
                }
            }
        }
    }
}