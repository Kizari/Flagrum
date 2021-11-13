namespace Flagrum.Gfxbin.Gmtl.Data
{
    public class MaterialShaderBinary
    {
        public ulong ResourceFileHash { get; set; }

        public string Path { get; set; }
        public ulong PathOffset { get; set; }
    }
}