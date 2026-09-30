using System.Linq;
using System.Threading.Tasks;
using Flagrum.Abstractions;
using Flagrum.Generators;
using Flagrum.Application.Features.Settings.Data;
using Flagrum.Application.Persistence;
using Flagrum.Application.Persistence.Entities;
using Flagrum.Application.Services;
using Flagrum.Host;

namespace Flagrum.Migrations;

[SteppedDataMigration(1)]
public partial class ProfilesMigration(
    FlagrumDbContext context,
    IProfileService profile,
    IConfiguration configuration,
    IApplication application)
{
    [MigrationStep(0, "73beb165-31e2-4400-8ae8-93c8c0c0dbf9", MigrationScope.Application)]
    public async Task Migrate()
    {
        if (profile.DidMigrateThisSession)
        {
            application.SetSplashText("Migrating to the profile system");

            var profile = context.Profile;

            // If this is the first time using the profiles system
            if (profile.DidMigrateThisSession)
            {
                // Move the paths out into the profiles file
                var gamePath = context.GetString(StateKey.GamePath);
                var binmodListPath = context.GetString(StateKey.BinmodListPath);

                profile.Current.GamePath = gamePath;
                profile.Current.BinmodListPath = binmodListPath;

                context.DeleteStateKey(StateKey.GamePath);
                context.DeleteStateKey(StateKey.BinmodListPath);

                var oldRoot = context.AssetExplorerNodes
                    .Where(n => n.ParentId == null)
                    .Select(n => n.Id)
                    .First();

                var newRoot = new AssetExplorerNode {Name = ""};
                context.AssetExplorerNodes.Add(newRoot);
                await context.SaveChangesAsync();

                var oldRootEntity = context.AssetExplorerNodes.Find(oldRoot)!;
                oldRootEntity.ParentId = newRoot.Id;
                oldRootEntity.Name = "data:";
                await context.SaveChangesAsync();

                // Update the mod paths to point to the new locations
                foreach (var file in context.EarcModReplacements)
                {
                    if (file.ReplacementFilePath?.StartsWith($@"{profile.FlagrumDirectory}\earc") == true)
                    {
                        file.ReplacementFilePath = file.ReplacementFilePath.Replace($@"{profile.FlagrumDirectory}\earc",
                            profile.ModFilesDirectory);
                    }
                }

                await context.SaveChangesAsync();

                foreach (var file in context.EarcModLooseFile)
                {
                    if (file.FilePath?.StartsWith($@"{profile.FlagrumDirectory}\earc") == true)
                    {
                        file.FilePath =
                            file.FilePath.Replace($@"{profile.FlagrumDirectory}\earc", profile.ModFilesDirectory);
                    }
                }

                await context.SaveChangesAsync();
            }
        }
    }
}