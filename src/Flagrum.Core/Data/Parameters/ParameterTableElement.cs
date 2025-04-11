using System.Collections.Generic;
using System.IO;
using Flagrum.Core.Utilities;
using Flagrum.Core.Utilities.Extensions;

namespace Flagrum.Core.Data.Bins;

public class ParameterTableElement
{
    private readonly uint _valueCount;

    public ParameterTableElement(uint valueCount)
    {
        _valueCount = valueCount;
    }

    public uint Id { get; set; }

    // Values can be either int, uint, or float
    public List<object> Values { get; set; } = new();

    public void Read(BinaryReader reader)
    {
        Id = reader.ReadUInt32();
        for (var i = 0; i < _valueCount; i++)
        {
            Values.Add(reader.ReadUInt32());
        }
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(Id);

        foreach (var value in Values)
        {
            writer.WriteParameterTableValue(value);
        }
    }
}