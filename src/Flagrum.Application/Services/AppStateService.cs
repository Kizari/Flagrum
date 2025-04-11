using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Abstractions;
using Flagrum.Abstractions.AssetExplorer;
using Flagrum.Core.Archive;
using Flagrum.Core.Utilities;
using Flagrum.Application.Features.AssetExplorer.Indexing;
using FileIndexNode = Flagrum.Application.Features.AssetExplorer.Indexing.FileIndexNode;

namespace Flagrum.Application.Services;

public class AppStateService
{
    private readonly IConfiguration _configuration;
    private readonly IFileIndex _fileIndex;
    private readonly IProfileService _profile;

    public AppStateService(IProfileService profile, IConfiguration configuration, IFileIndex fileIndex)
    {
        _profile = profile;
        _configuration = configuration;
        _fileIndex = fileIndex;
        FileIndexNode.AppState = this;
    }

    public Binmod ActiveMod { get; set; }
    public IList<Binmod> Mods { get; set; } = new List<Binmod>();
    public IList<ModlistEntry> UnmanagedEntries { get; set; } = new List<ModlistEntry>();
    public bool IsModListInitialized { get; set; }

    public IAssetExplorerNode CurrentGameViewNode { get; set; }

    public int ActiveCategoryFilter { get; set; }
    public int ActiveModTypeFilter { get; set; } = -1;

    public bool Is3DViewerOpen { get; set; }
    public bool IsModalOpen { get; set; }

    public string GetCurrentAssetExplorerPath()
    {
        var path = _configuration.Get<string>(StateKey.CurrentAssetExplorerPath);
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
        var modList = ModlistEntry.FromFile(_profile.Current.BinmodListPath);

        var fixIdMap = modList
            .DistinctBy(m => m.Path)
            .ToDictionary(m => m.Path, m => m.Index);

        ModlistEntry.ToFile(_profile.Current.BinmodListPath, entries, fixIdMap);
    }

    public void LoadNodes()
    {
        if (_profile.IsReady)
        {
            // Run this separately as it shouldn't delay the startup
            ThreadHelper.RunOnNewThread(async () =>
            {
                if (_fileIndex.IsEmpty)
                {
                    _fileIndex.Regenerate();
                }
                else
                {
                    if (_profile.Current.Type == LuminousGame.Forspoken)
                    {
                        // Preload all earcs to speed up the Asset Explorer
                        await Task.Run(() =>
                        {
                            foreach (var path in ((FileIndex)_fileIndex).Archives
                                         .Select(archive =>
                                             Path.Combine(_profile.GameDataDirectory, archive.Value.RelativePath)))
                            {
                                ((ProfileService)_profile).OpenArchive(path);
                            }
                        });
                    }
                }
            });
        }
    }

    public byte[] GetFileByUri(string uri)
    {
        uri = uri.ToLower();

        var file = ((FileIndex)_fileIndex)[uri];
        if (file == null)
        {
            return [];
        }

        if (file.Archive == null)
        {
            var absolutePath = Path.Combine(_profile.GameDataDirectory, uri.Replace("data://", ""));
            return File.ReadAllBytes(absolutePath);
        }
        else
        {
            var relativePath = _fileIndex.GetArchiveRelativePathByUri(uri);
            if (relativePath == null)
            {
                return [];
            }

            var absolutePath = Path.Combine(_profile.GameDataDirectory, relativePath);
            using var archive = new EbonyArchive(absolutePath);
            return archive[uri].GetReadableData();
        }
    }
}