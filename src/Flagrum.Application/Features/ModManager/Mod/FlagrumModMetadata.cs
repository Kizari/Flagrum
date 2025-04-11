using System;
using System.Collections.Generic;
using Flagrum.Abstractions.ModManager.Instructions;
using Flagrum.Abstractions.ModManager.Project;
using MemoryPack;

namespace Flagrum.Application.Features.ModManager.Mod;

[MemoryPackable]
public partial class FlagrumModMetadata
{
    public Guid Guid { get; set; }
    public ModFlags Flags { get; set; }
    public List<IFlagrumProjectArchive> Archives { get; set; }
    public List<IModBuildInstruction> Instructions { get; set; }
    public string Name { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }
    public string Readme { get; set; }
}