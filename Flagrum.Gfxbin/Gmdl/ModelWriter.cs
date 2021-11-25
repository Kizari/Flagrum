using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flagrum.Gfxbin.Gmdl.Buffering;
using Flagrum.Gfxbin.Gmdl.Components;
using BinaryWriter = Flagrum.Gfxbin.Serialization.BinaryWriter;

namespace Flagrum.Gfxbin.Gmdl;

public class ModelWriter
{
    private readonly Model _model;
    private readonly GpubinPacker _packer = new();
    private readonly BinaryWriter _writer = new();

    public ModelWriter(Model model)
    {
        _model = model;
    }

    public (byte[] gfxbin, byte[] gpubin) Write()
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
            _writer.Write(part.Flags);
        }

        return (_writer.ToArray(), _packer.ToArray());
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
                _writer.WriteUInt((uint)mesh.FaceIndices.Length);

                var indexType = mesh.VertexCount > ushort.MaxValue
                    ? IndexType.IndexType32
                    : IndexType.IndexType16;

                _writer.WriteByte((byte)indexType);

                var (faceIndexBufferOffset, faceIndexBufferSize) =
                    _packer.PackFaceIndices(mesh.FaceIndices, indexType);

                _writer.WriteUInt(faceIndexBufferOffset);
                _writer.WriteUInt(faceIndexBufferSize);

                _writer.WriteUInt((uint)mesh.VertexPositions.Count);
                _writer.WriteArraySize(2);

                var vertexBufferStream = new MemoryStream();

                WriteVertexStream1(vertexBufferStream, mesh);
                WriteVertexStream2(vertexBufferStream, mesh);

                // TODO: Write the second weight map
                // TODO: Write binormals, normalXfactors, fogcoord0 and psize0

                var vertexBufferSize = vertexBufferStream.Length;
                var vertexBufferOffset = _packer.PackVertexBuffer(vertexBufferStream);

                _writer.WriteUInt(vertexBufferOffset);
                _writer.WriteUInt((uint)vertexBufferSize);

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

    /// <summary>
    ///     The first vertex stream is the vertex positions and weight data
    /// </summary>
    private void WriteVertexStream1(Stream vertexBufferStream, Mesh mesh)
    {
        uint stride = 0;
        var vertexStream = new MemoryStream();
        var elements = new List<VertexElementDescription>();

        elements.Add(new VertexElementDescription
        {
            Format = VertexElementFormat.XYZ32_Float,
            Semantic = VertexElementDescription.Position0,
            Offset = stride
        });

        stride += 12;

        elements.Add(new VertexElementDescription
        {
            Format = VertexElementFormat.XYZW16_Uint,
            Semantic = VertexElementDescription.BlendIndices0,
            Offset = stride
        });

        stride += 8;

        // ffxvbinmods don't seem to support the second weight map, so we disable it
        // elements.Add(new VertexElementDescription
        // {
        //     Format = VertexElementFormat.XYZW16_Uint,
        //     Semantic = VertexElementDescription.BlendIndices1,
        //     Offset = stride
        // });
        //
        // stride += 8;

        elements.Add(new VertexElementDescription
        {
            Format = VertexElementFormat.XYZW8_UintN,
            Semantic = VertexElementDescription.BlendWeight0,
            Offset = stride
        });

        stride += 4;

        // ffxvbinmods don't seem to support the second weight map, so we disable it
        // elements.Add(new VertexElementDescription
        // {
        //     Format = VertexElementFormat.XYZW8_UintN,
        //     Semantic = VertexElementDescription.BlendWeight1,
        //     Offset = stride
        // });
        //
        // stride += 4;

        for (var i = 0; i < mesh.VertexCount; i++)
        {
            var position = mesh.VertexPositions[i];
            vertexStream.Write(BitConverter.GetBytes(position.X));
            vertexStream.Write(BitConverter.GetBytes(position.Y));
            vertexStream.Write(BitConverter.GetBytes(position.Z));

            foreach (var index in mesh.WeightIndices
                         .Take(1)
                         .Select(weightIndicesMap => FixArraySize(weightIndicesMap[i], 4)))
            {
                vertexStream.Write(BitConverter.GetBytes(index[0]));
                vertexStream.Write(BitConverter.GetBytes(index[1]));
                vertexStream.Write(BitConverter.GetBytes(index[2]));
                vertexStream.Write(BitConverter.GetBytes(index[3]));
            }

            foreach (var weight in mesh.WeightValues
                         .Take(1)
                         .Select(weightValuesMap => FixArraySize(weightValuesMap[i], 4)))
            {
                vertexStream.WriteByte(weight[0]);
                vertexStream.WriteByte(weight[1]);
                vertexStream.WriteByte(weight[2]);
                vertexStream.WriteByte(weight[3]);
            }
        }

        // Write the stream description
        var description = new VertexStreamDescription
        {
            Slot = VertexStreamSlot.Slot_0,
            Type = VertexStreamType.Vertex,
            Stride = stride,
            StartOffset = (uint)vertexBufferStream.Position,
            VertexElementDescriptions = elements
        };

        vertexStream.Seek(0, SeekOrigin.Begin);
        vertexStream.CopyTo(vertexBufferStream);
        WriteVertexStream(description);
    }

    /// <summary>
    ///     The second vertex stream is everything not in the first
    /// </summary>
    private void WriteVertexStream2(Stream vertexBufferStream, Mesh mesh)
    {
        uint stride = 0;
        var vertexStream = new MemoryStream();
        var elements = new List<VertexElementDescription>();

        elements.Add(new VertexElementDescription
        {
            Format = VertexElementFormat.XYZW8_SintN,
            Semantic = VertexElementDescription.Normal0,
            Offset = stride
        });

        stride += 4;

        elements.Add(new VertexElementDescription
        {
            Format = VertexElementFormat.XYZW8_SintN,
            Semantic = VertexElementDescription.Tangent0,
            Offset = stride
        });

        stride += 4;

        var count = 0;
        foreach (var uvMap in mesh.UVMaps)
        {
            elements.Add(new VertexElementDescription
            {
                Format = VertexElementFormat.XY16_Float,
                Semantic = $"TEXCOORD{count}",
                Offset = stride
            });

            stride += 4;
            count++;

            // Luminous only supports up to TEXCOORD7
            if (count > 7)
            {
                break;
            }
        }

        count = 0;
        foreach (var colorMap in mesh.ColorMaps)
        {
            elements.Add(new VertexElementDescription
            {
                Format = VertexElementFormat.XYZW8_UintN,
                Semantic = $"COLOR{count}",
                Offset = stride
            });

            stride += 4;
            count++;
            
            // Luminous only supports up to COLOR3
            if (count > 3)
            {
                break;
            }
        }

        for (var i = 0; i < mesh.VertexCount; i++)
        {
            var normal = mesh.Normals[i];
            vertexStream.WriteByte((byte)normal.X);
            vertexStream.WriteByte((byte)normal.Y);
            vertexStream.WriteByte((byte)normal.Z);
            vertexStream.WriteByte((byte)normal.W);

            var tangent = mesh.Tangents[i];
            vertexStream.WriteByte((byte)tangent.X);
            vertexStream.WriteByte((byte)tangent.Y);
            vertexStream.WriteByte((byte)tangent.Z);
            vertexStream.WriteByte((byte)tangent.W);

            foreach (var uvMap in mesh.UVMaps)
            {
                var uv = uvMap.UVs[i];
                vertexStream.Write(BitConverter.GetBytes(uv.U));
                vertexStream.Write(BitConverter.GetBytes(uv.V));
            }

            foreach (var colorMap in mesh.ColorMaps)
            {
                var color = colorMap.Colors[i];
                vertexStream.WriteByte(color.R);
                vertexStream.WriteByte(color.G);
                vertexStream.WriteByte(color.B);
                vertexStream.WriteByte(color.A);
            }
        }

        // Write the stream description
        var description = new VertexStreamDescription
        {
            Slot = VertexStreamSlot.Slot_1,
            Type = VertexStreamType.Vertex,
            Stride = stride,
            StartOffset = (uint)vertexBufferStream.Position,
            VertexElementDescriptions = elements
        };

        vertexStream.Seek(0, SeekOrigin.Begin);
        vertexStream.CopyTo(vertexBufferStream);
        WriteVertexStream(description);
    }

    private void WriteVertexStream(VertexStreamDescription vertexStream)
    {
        _writer.WriteUInt((uint)vertexStream.Slot);
        _writer.WriteUInt((uint)vertexStream.Type);
        _writer.WriteUInt(vertexStream.Stride);
        _writer.WriteUInt(vertexStream.StartOffset);
        _writer.WriteArraySize((uint)vertexStream.VertexElementDescriptions.Count());

        foreach (var description in vertexStream.VertexElementDescriptions)
        {
            _writer.WriteUInt(description.Offset);
            _writer.WriteString(description.Semantic);
            _writer.WriteUInt((uint)description.Format);
        }
    }
}