namespace Flagrum.Core.Gfxbin.Gmtl.Data;

public class MaterialInterfaceInput
{
    public string Name { get; set; }
    public uint NameHash { get; set; }
    public ulong NameOffset { get; set; }

    public string ShaderGenName { get; set; }
    public uint ShaderGenNameHash { get; set; }
    public ulong ShaderGenNameOffset { get; set; }

    public ushort Type { get; set; }
    public ushort InterfaceIndex { get; set; }
    public ushort GpuOffset { get; set; }
    public ushort Size { get; set; }
    public uint Flags { get; set; }

    public float[] Values { get; set; }
}