namespace Flagrum.Core.Gfxbin.Gmtl.Data;

public class MaterialShaderProgram
{
    public ushort LowKey { get; set; }
    public ushort HighKey { get; set; }
    public ushort CsBinaryIndex { get; set; }
    public ushort VsBinaryIndex { get; set; }
    public ushort HsBinaryIndex { get; set; }
    public ushort DsBinaryIndex { get; set; }
    public ushort GsBinaryIndex { get; set; }
    public ushort PsBinaryIndex { get; set; }
}