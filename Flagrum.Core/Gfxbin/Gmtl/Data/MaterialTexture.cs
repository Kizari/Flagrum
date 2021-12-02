namespace Flagrum.Gfxbin.Gmtl.Data
{
    public class MaterialTexture
    {
        public ulong ResourceFileHash { get; set; }

        public string Name { get; set; }
        public uint NameHash { get; set; }
        public ulong NameOffset { get; set; }

        public string ShaderGenName { get; set; }
        public uint ShaderGenNameHash { get; set; }
        public ulong ShaderGenNameOffset { get; set; }

        public string Path { get; set; }
        public uint PathHash { get; set; }
        public ulong PathOffset { get; set; }

        public uint Flags { get; set; }
        public sbyte HighTextureStreamingLevels { get; set; }
        public ulong Unknown1 { get; set; }
        public uint Unknown2 { get; set; }
    }
}