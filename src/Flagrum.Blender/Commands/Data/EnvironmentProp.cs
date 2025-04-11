namespace Flagrum.Blender.Commands.Data;

public class EnvironmentProp : EnvironmentItem
{
    public string Type { get; set; } = "EnvironmentProp";
    public string? Uri { get; set; }
    public string? RelativePath { get; set; }
    public float[]? Position { get; set; }
    public float[]? Rotation { get; set; }
    public float Scale { get; set; }
    public List<float[]>? PrefabRotations { get; set; }
}