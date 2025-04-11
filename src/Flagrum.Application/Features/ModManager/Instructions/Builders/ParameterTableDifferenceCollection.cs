// Have had to use MessagePack here instead of MemoryPack because MemoryPack doesn't support serializing System.Object
// which is necessary as column values of parameter tables have variable types.
// Fortunately MessagePack is a perfectly performant binary format, so there isn't really a downside to this.

using System.Collections.Generic;
using MessagePack;

namespace Flagrum.Application.Features.ModManager.Instructions.Builders;

[MessagePackObject]
public class ParameterTableDifference
{
    [Key(0)] public int Index { get; set; }
    [Key(1)] public uint Fixid { get; set; }
    [Key(2)] public List<ParameterTableRecordDifference> NewRecords { get; set; } = new();
    [Key(3)] public List<ParameterTableRecordDifference> ModifiedRecords { get; set; } = new();
    [Key(4)] public List<uint> RemovedRecords { get; set; } = new();
}

[MessagePackObject]
public class ParameterTableRecordDifference
{
    [Key(0)] public uint Fixid { get; set; }
    [Key(1)] public Dictionary<uint, object> ColumnValues { get; set; } = new();
}