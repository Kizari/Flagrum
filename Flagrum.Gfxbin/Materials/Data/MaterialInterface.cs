namespace Flagrum.Gfxbin.Materials.Data
{
    public class MaterialInterface
    {
        public string Name { get; set; }
        public uint NameHash { get; set; }
        public ulong NameOffset { get; set; }

        public string ShaderGenName { get; set; }
        public uint ShaderGenNameHash { get; set; }
        public ulong ShaderGenNameOffset { get; set; }

        public uint GpuOffset { get; set; }
        public ushort Size { get; set; }
        public ushort InputCount { get; set; }

        public uint Flags { get; set; }
        public ulong Unknown1 { get; set; }
        public uint Unknown2 { get; set; }
    }
}
