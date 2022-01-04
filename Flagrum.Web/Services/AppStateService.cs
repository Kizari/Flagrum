using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Flagrum.Web.Services;

public class AppStateData
{
    public string CurrentAssetExplorerPath { get; set; }
}

public class AppStateService
{
    private readonly AppStateData _data;
    private readonly SettingsService _settings;

    public AppStateService(SettingsService settings)
    {
        _settings = settings;

        if (File.Exists(_settings.StatePath))
        {
            _data = JsonConvert.DeserializeObject<AppStateData>(File.ReadAllText(_settings.StatePath));
        }
        else
        {
            _data = new AppStateData
            {
                CurrentAssetExplorerPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };
        }
    }

    public Binmod ActiveMod { get; set; }
    public IList<Binmod> Mods { get; set; } = new List<Binmod>();
    public IList<ModlistEntry> UnmanagedEntries { get; set; } = new List<ModlistEntry>();
    public bool IsModListInitialized { get; set; }

    public int ActiveCategoryFilter { get; set; } = 0;
    public int ActiveModTypeFilter { get; set; } = -1;

    private void SaveData()
    {
        File.WriteAllText(_settings.StatePath, JsonConvert.SerializeObject(_data));
    }

    public string GetCurrentAssetExplorerPath()
    {
        return _data.CurrentAssetExplorerPath;
    }

    public void SetCurrentAssetExplorerPath(string path)
    {
        _data.CurrentAssetExplorerPath = path;
        SaveData();
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