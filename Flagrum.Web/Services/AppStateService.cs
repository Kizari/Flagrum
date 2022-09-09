using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;
using Newtonsoft.Json;

namespace Flagrum.Web.Services;

public class AppStateData
{
    public string CurrentAssetExplorerPath { get; set; }
}

public class AppStateService
{
    private readonly SettingsService _settings;
    private readonly FlagrumDbContext _context;

    public AppStateService(
        SettingsService settings,
        FlagrumDbContext context)
    {
        _settings = settings;
        _context = context;

        // Assimilate old state file into local DB
        if (File.Exists(_settings.StatePath))
        {
            var data = JsonConvert.DeserializeObject<AppStateData>(File.ReadAllText(_settings.StatePath))!;
            _context.SetString(StateKey.CurrentAssetExplorerPath, data.CurrentAssetExplorerPath);
            File.Delete(_settings.StatePath);
        }
    }

    public Binmod ActiveMod { get; set; }
    public IList<Binmod> Mods { get; set; } = new List<Binmod>();
    public IList<ModlistEntry> UnmanagedEntries { get; set; } = new List<ModlistEntry>();
    public bool IsModListInitialized { get; set; }

    public AssetExplorerNode Node { get; set; }
    public AssetExplorerNode RootModelBrowserNode { get; set; }

    public int ActiveCategoryFilter { get; set; } = 0;
    public int ActiveModTypeFilter { get; set; } = -1;

    public string GetCurrentAssetExplorerPath()
    {
        var path = _context.GetString(StateKey.CurrentAssetExplorerPath);
        if (path == null || !File.Exists(path))
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }

        return path;
    }

    public void SetCurrentAssetExplorerPath(string path)
    {
        _context.SetString(StateKey.CurrentAssetExplorerPath, path);
    }

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