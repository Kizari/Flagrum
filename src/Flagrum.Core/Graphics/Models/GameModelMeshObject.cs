using System.Collections.Generic;
using Flagrum.Core.Serialization.MessagePack;

namespace Flagrum.Core.Graphics.Models;

public class GameModelMeshObject : IMessagePackItem, IMessagePackDifferentFirstItem
{
    public bool Unknown { get; set; }
    public string Name { get; set; }
    public IList<string> Clusters { get; set; }
    public IList<GameModelMesh> Meshes { get; set; }
    public bool IsFirst { get; set; }

    public void Read(MessagePackReader reader)
    {
        if (!IsFirst)
        {
            Unknown = reader.Read<bool>();
        }

        Name = reader.Read<string>();
        Clusters = reader.Read<List<string>>();
        Meshes = reader.Read<List<GameModelMesh>>();
    }

    public void Write(MessagePackWriter writer)
    {
        if (!IsFirst)
        {
            writer.Write(Unknown);
        }

        writer.Write(Name);
        writer.Write(Clusters);
        writer.Write(Meshes);
    }
}