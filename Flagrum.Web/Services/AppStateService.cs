using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Core.Utilities;
using Flagrum.Web.Features.AssetExplorer.Data;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Flagrum.Web.Services;

public class AppStateData
{
    public string CurrentAssetExplorerPath { get; set; }
}

public class AppStateService
{
    private readonly SettingsService _settings;

    public AppStateService(SettingsService settings)
    {
        _settings = settings;

        // Assimilate old state file into local DB
        if (File.Exists(_settings.StatePath))
        {
            using var context = new FlagrumDbContext(_settings);
            var data = JsonConvert.DeserializeObject<AppStateData>(File.ReadAllText(_settings.StatePath))!;
            context.SetString(StateKey.CurrentAssetExplorerPath, data.CurrentAssetExplorerPath);
            File.Delete(_settings.StatePath);
        }
    }

    public Binmod ActiveMod { get; set; }
    public IList<Binmod> Mods { get; set; } = new List<Binmod>();
    public IList<ModlistEntry> UnmanagedEntries { get; set; } = new List<ModlistEntry>();
    public bool IsModListInitialized { get; set; }
    public bool IsIndexing { get; set; }

    public AssetExplorerNode RootModelBrowserNode { get; set; }
    public IAssetExplorerNode RootFileSystemNode { get; set; }
    public IAssetExplorerNode RootGameViewNode { get; set; }
    public IAssetExplorerNode CurrentGameViewNode { get; set; }
    public Dictionary<ulong, string> PathMap { get; set; }

    public int ActiveCategoryFilter { get; set; } = 0;
    public int ActiveModTypeFilter { get; set; } = -1;

    public bool Is3DViewerOpen { get; set; }
    public bool IsModalOpen { get; set; }

    public string GetCurrentAssetExplorerPath(FlagrumDbContext context)
    {
        var path = context.GetString(StateKey.CurrentAssetExplorerPath);
        if (path == null || (!File.Exists(path) && !Directory.Exists(path)))
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }

        return path;
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
    
    public void LoadNodes()
    {
        // Run this separately as it shouldn't delay the startup
        Task.Run(() =>
        {
            using var context = new FlagrumDbContext(new SettingsService());
            if (!context.AssetExplorerNodes.Any())
            {
                new UriMapper(context, context.Settings).RegenerateMap();
            }

            // Don't wait for this as it won't be needed until it's long since finished
            Task.Run(LoadPathMap);

            LoadNodeTree();
        });
    }

    private void LoadNodeTree()
    {
        using var context = new FlagrumDbContext(new SettingsService());
        var currentNodeId = context.GetInt(StateKey.CurrentAssetNode);

        // Load the node tree from the DB
        var nodeTree = new ConcurrentDictionary<int, AssetExplorerNode>(context.AssetExplorerNodes
            .AsNoTracking()
            .ToList()
            .ToDictionary(n => n.Id, n => n));

        // Create the parent/child relationships
        foreach (var (_, node) in nodeTree)
        {
            if (node.ParentId > 0)
            {
                node.Parent = nodeTree[node.ParentId.Value];
                ((AssetExplorerNode)node.Parent).ChildNodes.Add(node);
            }

            if (node.Id == currentNodeId)
            {
                CurrentGameViewNode = node;
            }
        }

        // Sort the children first by directory status, then by name
        foreach (var (_, node) in nodeTree)
        {
            if (node.ChildNodes?.Any() == true)
            {
                var children = (List<AssetExplorerNode>)node.ChildNodes;
                children.Sort((first, second) =>
                {
                    var typeDifference = first.Children.Any().CompareTo(second.Children.Any()) * -1;
                    return typeDifference == 0 ? string.CompareOrdinal(first.Name, second.Name) : typeDifference;
                });
            }
        }

        RootGameViewNode = nodeTree[1];
        IsIndexing = false;
    }

    private void LoadPathMap()
    {
        using var context = new FlagrumDbContext(new SettingsService());
        PathMap = context.AssetUris
            .Select(a => new
            {
                a.Uri,
                a.ArchiveLocation.Path
            })
            .ToDictionary(a => Cryptography.HashFileUri64(a.Uri), a => a.Path);
    }
}