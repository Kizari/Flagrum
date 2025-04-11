using System;
using System.Collections.Generic;
using System.IO;
using Flagrum.Core.Data.Bins;
using Flagrum.Core.Physics.Collision;
using Flagrum.Core.Utilities.Extensions;

namespace Flagrum.Core.Data;

public class DataIndexBinary : BlackResourceBinary
{
    private readonly Dictionary<uint, Type> _resourceTypes = new()
    {
        {67110350, typeof(PhysicsBinary)},
        {67118428, typeof(MeshCollisionBinary)},
        {67110385, typeof(ParameterTableCollection)},
        {67111838, typeof(MessageBinary)}
    };

    public uint Count { get; set; }
    public List<DataIndex> DataIndices { get; set; } = new();
    public List<IResourceBinaryItem> Items { get; set; } = new();

    public override void Read(Stream stream)
    {
        base.Read(stream);

        using var reader = new BinaryReader(stream);

        Count = reader.ReadUInt32();

        for (var i = 0; i < Count; i++)
        {
            var index = new DataIndex();
            index.Read(reader);
            DataIndices.Add(index);
        }

        reader.Align(256);

        foreach (var index in DataIndices)
        {
            var type = _resourceTypes[index.ResourceId.Type];
            var item = (IResourceBinaryItem)Activator.CreateInstance(type)!;
            item.Index = index;

            if (item is ISubresource subresource)
            {
                subresource.Read(stream);
            }
            else
            {
                item.Read(reader);
            }

            Items.Add(item);
            reader.Align(256);
        }
    }

    public override void Write(Stream stream)
    {
        using var writer = new BinaryWriter(stream);

        // Skip header for now
        var dataIndexSize = DataIndices.Count * 24;
        writer.Seek(32 + dataIndexSize, SeekOrigin.Begin);
        writer.Align(256, 0x00);

        // Write each resource
        foreach (var item in Items)
        {
            item.Index.Offset = (uint)stream.Position;

            if (item is ISubresource subresource)
            {
                subresource.Write(stream);
            }
            else
            {
                item.Write(writer);
            }

            item.Index.Size = (uint)(stream.Position - item.Index.Offset);
            writer.Align(256, 0x00);
        }

        // Write the BdevResource header
        Size = (int)stream.Position;
        stream.Seek(0, SeekOrigin.Begin);
        base.Write(stream);

        // Write the data index table
        Count = (uint)DataIndices.Count;
        writer.Write(Count);

        foreach (var index in DataIndices)
        {
            index.Write(writer);
        }
    }
}