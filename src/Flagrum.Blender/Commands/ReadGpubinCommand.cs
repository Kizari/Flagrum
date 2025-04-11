using Flagrum.Core.Graphics.Containers;
using Flagrum.Core.Graphics.Models;

namespace Flagrum.Blender.Commands;

public class ReadGpubinCommand : IConsoleCommand
{
    public string Command => "gpubin";

    public void Execute(string[] args)
    {
        var inputPath = args[0];
        var outputPath = args[1];
        var importLods = bool.Parse(args[2]);
        var importVems = bool.Parse(args[3]);
        
        var data = File.ReadAllBytes(inputPath);
        var model = new GameModel();
        model.Read(data);

        var fileName = inputPath.Split('\\', '/').Last().Split('.').First();
        var directory = Path.GetDirectoryName(inputPath)!;
        var gpubins = Directory.GetFiles(directory, "*.gpubin", SearchOption.TopDirectoryOnly);
        
        // TODO: Rewrite the match to handle filenName_0.gpubin etc for Forspoken
        var gpubinData = gpubins
            .Where(g => g.Contains("\\" + fileName + ".gpubin", StringComparison.OrdinalIgnoreCase))
            .OrderBy(g => g)
            .Select(File.ReadAllBytes)
            .ToList();

        model.ReadVertexData(gpubinData);

        using var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
        using var writer = new BinaryWriter(output);

        // Write all mesh data to the output stream
        foreach (var meshObject in model.MeshObjects)
        {
            foreach (var mesh in meshObject.Meshes)
            {
                // Skip LOD meshes if they are not to be imported
                if (!importLods && mesh.LodNear != 0)
                {
                    continue;
                }

                // Skip VEM meshes if they are not to be imported
                if (!importVems && (mesh.Flags & 67108864) > 0)
                {
                    continue;
                }

                Console.WriteLine(mesh.Name);

                // Write all faces to the output stream
                for (var i = 0; i < mesh.FaceIndexCount / 3; i++)
                {
                    writer.Write(mesh.FaceIndices[i, 0]);
                    writer.Write(mesh.FaceIndices[i, 1]);
                    writer.Write(mesh.FaceIndices[i, 2]);
                }

                // Write all vertex data to the output stream
                foreach (var (semantic, list) in mesh.Semantics.OrderBy(s => s.Key.Value))
                {
                    if (semantic.Equals(VertexElementSemantic.Position0)
                        || semantic.Value.StartsWith("COLOR")
                        || semantic.Value.StartsWith("TEXCOORD")
                        || semantic.Value.StartsWith("BLENDWEIGHT"))
                    {
                        var items = (IList<float[]>)list;
                        foreach (var vertex in items)
                        {
                            foreach (var position in vertex)
                            {
                                writer.Write(position);
                            }
                        }
                    }
                    else if (semantic.Equals(VertexElementSemantic.Normal0))
                    {
                        var items = (IList<float[]>)list;
                        foreach (var normal in items)
                        {
                            for (var i = 0; i < 3; i++)
                            {
                                writer.Write(normal[i]);
                            }
                        }
                    }
                    else if (semantic.Value.StartsWith("BLENDINDICES"))
                    {
                        var items = (IList<uint[]>)list;
                        foreach (var item in items)
                        {
                            foreach (var index in item)
                            {
                                writer.Write(index);
                            }
                        }
                    }
                }
            }
        }
    }
}