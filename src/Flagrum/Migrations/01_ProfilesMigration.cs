using System.Linq;
using System.Threading.Tasks;
using Flagrum.Abstractions;
using Flagrum.Generators;
using Flagrum.Application.Features.Settings.Data;
using Flagrum.Application.Persistence;
using Flagrum.Application.Persistence.Entities;
using Flagrum.Application.Services;

namespace Flagrum.Migrations;

[SteppedDataMigration(1)]
public partial class ProfilesMigration
{
    [Inject] private readonly FlagrumDbContext _context;
    [Inject] private readonly IProfileService _profile;
    [Inject] private readonly IConfiguration _configuration;

    [MigrationStep(0, "73beb165-31e2-4400-8ae8-93c8c0c0dbf9", MigrationScope.Application)]
    public async Task Migrate()
    {
        if (_profile.DidMigrateThisSession)
        {
            SplashViewModel.Instance.SetLoadingText("Migrating to the profile system");

            var profile = _context.Profile;

            // If this is the first time using the profiles system
            if (profile.DidMigrateThisSession)
            {
                // Move the paths out into the profiles file
                var gamePath = _context.GetString(StateKey.GamePath);
                var binmodListPath = _context.GetString(StateKey.BinmodListPath);

                profile.Current.GamePath = gamePath;
                profile.Current.BinmodListPath = binmodListPath;

                _context.DeleteStateKey(StateKey.GamePath);
                _context.DeleteStateKey(StateKey.BinmodListPath);

                var oldRoot = _context.AssetExplorerNodes
                    .Where(n => n.ParentId == null)
                    .Select(n => n.Id)
                    .First();

                var newRoot = new AssetExplorerNode {Name = ""};
                _context.AssetExplorerNodes.Add(newRoot);
                await _context.SaveChangesAsync();

                var oldRootEntity = _context.AssetExplorerNodes.Find(oldRoot)!;
                oldRootEntity.ParentId = newRoot.Id;
                oldRootEntity.Name = "data:";
                await _context.SaveChangesAsync();

                // Update the mod paths to point to the new locations
                foreach (var file in _context.EarcModReplacements)
                {
                    if (file.ReplacementFilePath?.StartsWith($@"{profile.FlagrumDirectory}\earc") == true)
                    {
                        file.ReplacementFilePath = file.ReplacementFilePath.Replace($@"{profile.FlagrumDirectory}\earc",
                            profile.ModFilesDirectory);
                    }
                }

                await _context.SaveChangesAsync();

                foreach (var file in _context.EarcModLooseFile)
                {
                    if (file.FilePath?.StartsWith($@"{profile.FlagrumDirectory}\earc") == true)
                    {
                        file.FilePath =
                            file.FilePath.Replace($@"{profile.FlagrumDirectory}\earc", profile.ModFilesDirectory);
                    }
                }

                await _context.SaveChangesAsync();
            }
        }
    }
}