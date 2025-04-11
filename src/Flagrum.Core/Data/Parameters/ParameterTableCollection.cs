using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Flagrum.Core.Data.Bins;

public class ParameterTableCollection : BlackResourceBinary, IResourceBinaryItem, ISubresource
{
    public uint Version { get; set; }
    public uint TableCount { get; set; }
    public uint[] TableOffsets { get; set; }

    public List<ParameterTable> Tables { get; set; } = new();

    public DataIndex Index { get; set; }

    public void Read(BinaryReader reader)
    {
        throw new NotImplementedException();
    }

    public void Write(BinaryWriter writer)
    {
        throw new NotImplementedException();
    }

    public override void Read(Stream stream)
    {
        var start = stream.Position;

        base.Read(stream);

        using var reader = new BinaryReader(stream, Encoding.UTF8, true);

        Version = reader.ReadUInt32();
        TableCount = reader.ReadUInt32();
        TableOffsets = new uint[TableCount];

        for (var i = 0; i < TableCount; i++)
        {
            TableOffsets[i] = reader.ReadUInt32();
        }

        for (var i = 0; i < TableCount; i++)
        {
            var offset = TableOffsets[i];
            stream.Seek(start + 32 + offset, SeekOrigin.Begin);
            var table = new ParameterTable((uint)(i + 1 < TableCount
                ? start + 32 + TableOffsets[i + 1]
                : start + Size));

            table.Read(reader);
            Tables.Add(table);
        }
    }

    public override void Write(Stream stream)
    {
        var start = stream.Position;
        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);

        // Skip header for now
        var parameterTablesStart = stream.Position + 32;
        var parameterTableHeaderSize = 8 + 4 * TableCount;
        stream.Seek(parameterTablesStart + parameterTableHeaderSize, SeekOrigin.Begin);

        // Write each table
        TableCount = (uint)Tables.Count;
        for (var i = 0; i < TableCount; i++)
        {
            TableOffsets[i] = (uint)(stream.Position - parameterTablesStart);
            Tables[i].Write(writer);
        }

        // Update size
        Size = (int)(stream.Position - start);

        // Return to the start of the resource and write the header
        var returnAddress = stream.Position;
        stream.Seek(start, SeekOrigin.Begin);

        base.Write(stream);
        writer.Write(Version);
        writer.Write(TableCount);
        foreach (var tableOffset in TableOffsets)
        {
            writer.Write(tableOffset);
        }

        // Return to the end of this resource
        stream.Seek(returnAddress, SeekOrigin.Begin);
    }
}