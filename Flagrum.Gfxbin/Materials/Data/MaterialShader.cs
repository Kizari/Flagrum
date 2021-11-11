namespace Flagrum.Gfxbin.Materials.Data
{
    public class MaterialShader
    {
        public ulong ResourceFileHash { get; set; }

        public string Path { get; set; }
        public ulong PathOffset { get; set; }

        public ushort LowKey { get; set; }
        public ushort HighKey { get; set; }
        public ushort CsBinaryIndex { get; set; }
        public ushort VsBinaryIndex { get; set; }
        public ushort HsBinaryIndex { get; set; }
        public ushort DsBinaryIndex { get; set; }
        public ushort GsBinaryIndex { get; set; }
        public ushort PsBinaryIndex { get; set; }
    }
}
