using System;
using System.IO;
using Flagrum.Abstractions;
using Flagrum.Core.Utilities;
using Flagrum.Generators;

namespace Flagrum.Migrations;

/// <summary>
/// Moves user-specific assets from the wwwroot directory to the user data directory.
/// </summary>
[SteppedDataMigration(6)]
public partial class MoveUserAssetsMigration(
    IConfiguration configuration,
    IProfileService profile)
{
    [MigrationStep(0, "cdbec2c0-305b-49a4-a768-77897830fa03",
        MigrationScope.Application,
        MigrationStepMode.Retry)]
    private void MoveDirectories()
    {
        // Ensure the new user assets directory exists
        var userAssetsRoot = profile.UserAssetsDirectory;
        if (!Directory.Exists(userAssetsRoot))
        {
            Directory.CreateDirectory(userAssetsRoot);
        }
        
        // Determine directory paths
        var imagesDirectoryOld = Path.Combine(IOHelper.GetWebRoot(), "images");
        var imagesDirectoryNew = Path.Combine(profile.UserAssetsDirectory, "images");
        var modsDirectoryOld = Path.Combine(IOHelper.GetWebRoot(), "EarcMods");
        var modsDirectoryNew = Path.Combine(profile.UserAssetsDirectory, "EarcMods");

        // Catch error here, as the migration should not retry in future in case it replaces
        // the directories once they are already altered under the new system
        try
        {
            // Move images directory if present
            if (Directory.Exists(imagesDirectoryOld))
            {
                Directory.Move(imagesDirectoryOld, imagesDirectoryNew);
            }

            // Move mods directory if present
            if (Directory.Exists(modsDirectoryOld))
            {
                Directory.Move(modsDirectoryOld, modsDirectoryNew);
            }
        }
        catch
        {
            // Ignore if this fails, as it is not worth retrieving a small amount of disk space
            // at risk of causing issues
        }
    }
}