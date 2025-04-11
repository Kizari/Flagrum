using System.Collections.Generic;
using System.IO;
using Flagrum.Core.Serialization;
using Flagrum.Core.Serialization.MessagePack;

namespace Flagrum.Core.Graphics.Containers;

public class GraphicsBinary : BinaryReaderWriterBase
{
    public uint Version { get; set; }
    public uint Unknown { get; set; }
    public Dictionary<string, string> Dependencies { get; set; }
    public IList<ulong> Hashes { get; set; }

    public override void Read(Stream stream)
    {
        using var reader = new MessagePackReader(stream, true);

        Version = reader.Read<uint>();
        reader.DataVersion = Version;

        // This property only exists in Episode Duscae
        if (Version <= 20141115)
        {
            Unknown = reader.Read<uint>();
        }

        Dependencies = reader.Read<Dictionary<string, string>>();
        Hashes = reader.Read<List<ulong>>();
    }

    public override void Write(Stream stream)
    {
        using var writer = new MessagePackWriter(stream, true);
        writer.DataVersion = Version;

        writer.Write(Version);
        writer.Write(Dependencies);
        writer.Write(Hashes);
    }
}