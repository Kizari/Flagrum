using System.Collections.Generic;
using System.Linq;

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

    public int ActiveCategoryFilter { get; set; } = 0;
    public int ActiveModTypeFilter { get; set; } = -1;

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

        var entries = Mods.Union(fakeMods);

        var modList = ModlistEntry.FromFile(_settings.BinmodListPath);
        var fixIdMap = modList.ToDictionary(m => m.Path, m => m.Index);

        ModlistEntry.ToFile(_settings.BinmodListPath, entries, fixIdMap);
    }
}