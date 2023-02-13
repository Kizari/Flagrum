using System.Collections.Generic;

namespace Flagrum.Core.Gfxbin.Gmdl.MessagePack;

public class GfxbinMeshObject : IMessagePackItem, IMessagePackDifferentFirstItem
{
    public bool Unknown { get; set; }
    public string Name { get; set; }
    public IList<string> Clusters { get; set; }
    public IList<GfxbinMesh> Meshes { get; set; }
    public bool IsFirst { get; set; }

    public void Read(MessagePackReader reader)
    {
        if (!IsFirst)
        {
            Unknown = reader.Read<bool>();
        }

        Name = reader.Read<string>();
        Clusters = reader.Read<List<string>>();
        Meshes = reader.Read<List<GfxbinMesh>>();
    }
}