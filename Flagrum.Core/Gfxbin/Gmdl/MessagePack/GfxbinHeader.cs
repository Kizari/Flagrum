using System.Collections.Generic;

namespace Flagrum.Core.Gfxbin.Gmdl.MessagePack;

public class GfxbinHeader
{
    public uint Version { get; set; }
    public Dictionary<string, string> Dependencies { get; set; }
    public IList<ulong> Hashes { get; set; }

    public void Read(MessagePackReader reader)
    {
        Version = reader.Read<uint>();
        Dependencies = reader.Read<Dictionary<string, string>>();
        Hashes = reader.Read<List<ulong>>();
    }
}