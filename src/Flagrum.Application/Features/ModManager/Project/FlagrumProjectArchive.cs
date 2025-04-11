using System.Collections.Generic;
using Flagrum.Abstractions.Archive;
using Flagrum.Abstractions.ModManager.Instructions;
using Flagrum.Abstractions.ModManager.Project;
using MemoryPack;

namespace Flagrum.Application.Features.ModManager.Project;

/// <inheritdoc cref="IFlagrumProjectArchive" />
[MemoryPackable]
public partial class FlagrumProjectArchive : IFlagrumProjectArchive
{
    /// <inheritdoc />
    public ModChangeType Type { get; set; }

    /// <inheritdoc />
    public string RelativePath { get; set; }

    /// <inheritdoc />
    public EbonyArchiveFlags Flags { get; set; }

    /// <inheritdoc />
    public List<IPackedBuildInstruction> Instructions { get; set; } = [];
    
    public class Formatter : MemoryPackFormatter<IFlagrumProjectArchive>
    {
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, 
            scoped ref IFlagrumProjectArchive value) => writer.WritePackable((FlagrumProjectArchive)value);

        public override void Deserialize(ref MemoryPackReader reader, scoped ref IFlagrumProjectArchive value) =>
            value = reader.ReadPackable<FlagrumProjectArchive>();
    }
}