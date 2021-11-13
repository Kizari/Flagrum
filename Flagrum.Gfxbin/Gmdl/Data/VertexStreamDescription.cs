using System.Collections.Generic;

namespace Flagrum.Gfxbin.Gmdl.Data
{
    public enum VertexStreamSlot
    {
        Slot_0 = 0x0,
        Slot_1 = 0x1,
        Slot_2 = 0x2,
        Slot_3 = 0x3,
        Slot_Num = 0x4
    }

    public enum VertexStreamType
    {
        Vertex = 0x0,
        Instance = 0x1,
        Index = 0x2,
        Patch = 0x3,
        Num = 0x4,
        Dummy = 0xFF
    }

    public class VertexStreamDescription
    {
        public VertexStreamSlot Slot { get; set; }
        public VertexStreamType Type { get; set; }

        public uint Stride { get; set; }
        public uint StartOffset { get; set; }

        public IEnumerable<VertexElementDescription> VertexElementDescriptions { get; set; }
    }
}