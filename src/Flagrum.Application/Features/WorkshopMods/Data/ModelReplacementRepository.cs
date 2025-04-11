using System.Collections.Generic;
using Flagrum.Core.Utilities;
using Flagrum.Application.Persistence.Entities;
using MemoryPack;

namespace Flagrum.Application.Features.WorkshopMods.Data;

[MemoryPackable]
public partial class ModelReplacementRepository
{
    public static string Path => System.IO.Path.Combine(IOHelper.LocalApplicationData, "Flagrum", "workshop.fwp");

    public List<ModelReplacementFavourite> Favourites { get; set; } = new();
    public List<ModelReplacementPreset> Presets { get; set; } = new();
}