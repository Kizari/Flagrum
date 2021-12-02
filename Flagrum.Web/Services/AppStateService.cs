using System.Collections.Generic;
using Flagrum.Core.Archive;

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
    public bool IsModListInitialized { get; set; }

    public void UpdateBinmodList()
    {
        ModlistEntry.ToFile(_settings.BinmodListPath, Mods);
    }
}