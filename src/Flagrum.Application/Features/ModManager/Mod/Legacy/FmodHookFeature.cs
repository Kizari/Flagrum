using System.IO;
using Flagrum.Abstractions.ModManager.Instructions;

namespace Flagrum.Application.Features.ModManager.Mod.Legacy;

public class FmodHookFeature
{
    public FlagrumHookFeature Feature { get; set; }

    public void Read(BinaryReader reader)
    {
        Feature = (FlagrumHookFeature)reader.ReadInt32();
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write((int)Feature);
    }
}