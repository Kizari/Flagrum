using System.Collections.Generic;
using System.Linq;
using Flagrum.Core.Archive;
using Flagrum.Core.Archive.Binmod;

namespace Flagrum.Web.Services;

public class AppStateService
{
    private readonly Settings _settings;

    public AppStateService(Settings settings)
    {
        _settings = settings;
    }

    public Binmod ActiveMod { get; set; }
    public IList<Binmod> Mods { get; } = new List<Binmod>();
    public IList<ModlistEntry> UnmanagedEntries { get; set; } = new List<ModlistEntry>();
    public bool IsModListInitialized { get; set; }

    public void UpdateBinmodList()
    {
        var fakeMods = UnmanagedEntries.Select(e => new Binmod
        {
            // These are the only properties written to the list
            IsWorkshopMod = e.IsWorkshopMod,
            Path = e.Path,
            IsApplyToGame = e.IsEnabled,
            Index = e.Index
        });
        
        ModlistEntry.ToFile(_settings.BinmodListPath, Mods.Union(fakeMods));
    }
}