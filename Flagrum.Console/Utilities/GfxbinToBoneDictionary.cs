using System.IO;
using System.Linq;
using System.Text;
using Flagrum.Core.Gfxbin.Gmdl;

namespace Flagrum.Console.Utilities;

public static class GfxbinToBoneDictionary
{
    public static void Run(string gfxbinPath)
    {
        var reader = new ModelReader(File.ReadAllBytes(gfxbinPath),
            File.ReadAllBytes(gfxbinPath.Replace(".gmdl.gfxbin", ".gpubin")));

        var model = reader.Read();

        var builder = new StringBuilder();
        builder.AppendLine("public static List<BoneHeader> PreloadedBones => new()");
        builder.AppendLine("{");

        foreach (var bone in model.BoneHeaders)
        {
            builder.Append("    new BoneHeader {Name = \"" + bone.Name + "\", LodIndex = " + bone.LodIndex + "}");

            if (bone != model.BoneHeaders.Last())
            {
                builder.Append(",\n");
            }
        }

        builder.Append("\n}");

        File.WriteAllText("C:\\Testing\\PreloadedBones.cs", builder.ToString());
    }
}