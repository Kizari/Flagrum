using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Flagrum.Abstractions;
using Flagrum.Abstractions.AssetExplorer;
using Flagrum.Abstractions.ModManager.Project;
using Flagrum.Core.Persistence;
using Flagrum.Generators;
using Flagrum.Application.Features.AssetExplorer.Data;
using Flagrum.Application.Features.AssetExplorer.Indexing;
using Flagrum.Application.Features.ModManager.Project;
using Flagrum.Application.Features.ModManager.Services;
using Flagrum.Application.Features.Settings.Data;
using Flagrum.Application.Features.WorkshopMods.Data;
using Flagrum.Application.Persistence;
using Flagrum.Application.Persistence.Entities;
using Flagrum.Application.Services;
using Flagrum.Application.Utilities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ModifierKeys = System.Windows.Input.ModifierKeys;
using MouseAction = System.Windows.Input.MouseAction;

namespace Flagrum.Migrations;

[SteppedDataMigration(3)]
public partial class RemoveSqliteMigration
{
    private const string ReindexWarning = "An unexpected error occurred while attempting to index loose game " +
                                          "files for the 1.5.6 feature update. If this is something you wish to " +
                                          "make use of, please manually reindex your game files from the settings tab.";
    
    [Inject] private readonly IProfileService _profile;
    [Inject] private readonly FlagrumDbContext _context;
    [Inject] private readonly IConfiguration _configuration;
    [Inject] private readonly ModManagerServiceBase _modManager;
    [Inject] private readonly IFileIndex _fileIndex;
    [Inject] private readonly MigrationService _migrations;

    [MigrationStep(0, "d9079848-e368-4207-90fd-edffd5ffee4f", MigrationScope.Application)]
    private async Task MigrateStatePairs()
    {
        SplashViewModel.Instance.SetLoadingText("Migrating application preferences");
        
        // Ensure the DB is up to date
        await _context.Database.MigrateAsync();
        
        // Create a enum map for the state pairs
        var map = new Dictionary<StateKey, Type>
        {
            {StateKey.ViewportRotateModifierKey, typeof(ModifierKeys)},
            {StateKey.ViewportRotateMouseAction, typeof(MouseAction)},
            {StateKey.ViewportPanModifierKey, typeof(ModifierKeys)},
            {StateKey.ViewportPanMouseAction, typeof(MouseAction)},
            {StateKey.CurrentAssetExplorerView, typeof(AssetExplorerView)},
            {StateKey.CurrentAssetExplorerLayout, typeof(FileListLayout)}
        };

        // Copy the state pairs over to the standalone configuration
        foreach (var statePair in _context.StatePairs)
        {
            if (map.ContainsKey(statePair.Key))
            {
                // Enumerations need to be parsed so they can be converted to their integer representations
                var enumType = map[statePair.Key];
                if (Enum.TryParse(enumType, statePair.Value, out var value))
                {
                    dynamic result = Convert.ChangeType(value, enumType);
                    _configuration.Set(statePair.Key, result);
                }
            }
            else
            {
                // Everything else can be passed through as is
                _configuration.Set(statePair.Key, statePair.Value);
            }
        }
    }

    [MigrationStep(1, "9904759b-cdc3-4381-8362-47519e0a8323", MigrationScope.Application)]
    private async Task MigrateWorkshopModelReplacementPresets()
    {
        SplashViewModel.Instance.SetLoadingText("Migrating Workshop model replacement presets");
        
        // Ensure the DB is up to date
        await _context.Database.MigrateAsync();
        
        // Create the new repository data from the old DB records
        var repository = new ModelReplacementRepository
        {
            Favourites = _context.ModelReplacementFavourites
                .Select(f => new ModelReplacementFavourite
                {
                    Id = f.Id,
                    IsDefault = f.IsDefault
                })
                .ToList(),

            Presets = _context.ModelReplacementPresets
                .Select(p => new ModelReplacementPreset
                {
                    Id = p.Id,
                    Name = p.Name,
                    ReplacementPaths = p.ReplacementPaths.Select(rp => rp.Path).ToList()
                })
                .ToList()
        };
        
        // Save the repository to disk
        Repository.Save(repository, ModelReplacementRepository.Path);
    }

    [MigrationStep(2, "9e4a4b5e-0c95-4566-98e2-6f576cc6dd62", MigrationScope.Profile, MigrationStepMode.Retry)]
    public async Task DestroyDatabase()
    {
        // Destroy the DB as this was the last thing it was still used for
        await _context.DisposeAsync();
        SqliteConnection.ClearAllPools();
        File.Delete(_profile.DatabasePath);
    }
    
    [MigrationStep(3, "748726d9-b1f4-4de7-a2f2-071a9439b5fb", MigrationScope.Profile, MigrationStepMode.Warn, ReindexWarning)]
    private async Task IndexLooseFiles()
    {
        SplashViewModel.Instance.SetLoadingText("Temporarily disabling active mods");
        
        // Disable all mods so the file indexer doesn't index any mod files
        var modsToEnable = new List<IFlagrumProject>();
        foreach (var (guid, project) in _modManager.Projects
                     .Where(kvp => _modManager.ModsState.GetActive(kvp.Key)))
        {
            await _modManager.DisableMod(project);
            modsToEnable.Add(project);
        }
        
        // Regenerate the file index
        SplashViewModel.Instance.SetLoadingText("Indexing loose files");
        _fileIndex.Regenerate();
        
        // Reenable all mods now that the index has regenerated
        SplashViewModel.Instance.SetLoadingText("Reenabling active mods");
        foreach (var project in modsToEnable)
        {
            await _modManager.EnableMod(project);
        }
    }
}