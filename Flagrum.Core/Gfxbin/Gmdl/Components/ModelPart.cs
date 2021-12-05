namespace Flagrum.Core.Gfxbin.Gmdl.Components;

public class ModelPart
{
    public string Name { get; set; }
    public uint Id { get; set; }
    public bool Flags { get; set; }


    // NOTE: This class has unknowns
    public string Unknown { get; set; }
}