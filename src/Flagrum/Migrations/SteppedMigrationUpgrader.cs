using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flagrum.Abstractions;
using Flagrum.Application.Persistence;
using Flagrum.Application.Persistence.Entities;
using Flagrum.Core.Utilities;
using Injectio.Attributes;
using MemoryPack;
using Microsoft.Extensions.Logging;
using ZstdSharp;

namespace Flagrum.Migrations;

[RegisterScoped<SteppedMigrationUpgrader>]
public partial class SteppedMigrationUpgrader(
    IConfiguration configuration,
    FlagrumDbContext context,
    ILogger<SteppedMigrationUpgrader> logger,
    LegacyMigrationService migrations,
    IProfileService profile)
{
    public void Run()
    {
        ResetUpgradeFlags();

        if (!profile.Current.HasUpgradedToSteppedMigrations)
        {
            MergeMigrationFile();
            CheckMigration00();
            CheckMigration01();
            CheckMigration02();
            profile.Current.HasUpgradedToSteppedMigrations = true;
            configuration.Save();
        }
    }

    private void CheckMigration00()
    {
        if (!migrations.Completed.Contains(BackupsMigration.ClearBackupsTableId))
        {
            if (migrations.Completed.Contains(RemoveSqliteMigration.DestroyDatabaseId)
                || !context.DoesTableExist(nameof(context.StatePairs))
                || context.GetBool(StateKey.HasMigratedBackups))
            {
                configuration.SetMigratedNoSave(BackupsMigration.ApplicationSteps);
                profile.Current.SetMigratedNoSave(BackupsMigration.ProfileSteps);
            }
        }
    }

    private void CheckMigration01()
    {
        if (!migrations.Completed.Contains(ProfilesMigration.MigrateId))
        {
            if (!profile.DidMigrateThisSession)
            {
                configuration.SetMigratedNoSave(ProfilesMigration.ApplicationSteps);
                profile.Current.SetMigratedNoSave(ProfilesMigration.ProfileSteps);
            }
        }
    }

    private void CheckMigration02()
    {
        if (!migrations.Completed.Contains(FileIndexMigration.CleanupId))
        {
            if (File.Exists(profile.FileIndexPath)
                || migrations.Completed.Contains(RemoveSqliteMigration.DestroyDatabaseId)
                || !context.DoesTableExist(nameof(context.AssetExplorerNodes))
                || !context.AssetExplorerNodes.Any())
            {
                configuration.SetMigratedNoSave(FileIndexMigration.ApplicationSteps);
                profile.Current.SetMigratedNoSave(FileIndexMigration.ProfileSteps);
            }
        }
    }

    /// <summary>
    /// There was one or two versions that stored the migration step status in a separate file
    /// this moves any completed steps from that file into the configuration file instead
    /// </summary>
    private void MergeMigrationFile()
    {
        configuration.SetMigratedNoSave(
            RemoveSqliteMigration.ApplicationSteps.Where(s => migrations.Completed.Contains(s)));
        profile.Current.SetMigratedNoSave(
            RemoveSqliteMigration.ProfileSteps.Where(s => migrations.Completed.Contains(s)));
        configuration.Save();

        try
        {
            migrations.Delete();
        }
        catch (Exception exception)
        {
            // Not a huge deal if this tiny file is left behind so just log it and move on
            logger.LogError(exception, "Failed to delete migration file");
        }
    }

    /// <summary>
    /// There was a bug with the data migrations where I didn't account for cases where only some legacy migrations
    /// had been performed (see https://github.com/Kizari/Flagrum/issues/132)
    /// This will reset the flag for anyone coming from pre-1.5.10 to run it again with the fixed checks
    /// </summary>
    private void ResetUpgradeFlags()
    {
        if (profile.LastVersion != null && profile.LastVersion < new Version(1, 5, 10))
        {
            // Reset flag for all profiles
            foreach (var profile in configuration.Profiles)
            {
                profile.HasUpgradedToSteppedMigrations = false;
            }

            configuration.Save();

            // Clean up these old files because they're annoying
            try
            {
                var logFile = Path.Combine(IOHelper.LocalApplicationData, "Flagrum", "Log.txt");
                var crashFile = Path.Combine(IOHelper.LocalApplicationData, "Flagrum", "crash.txt");
                IOHelper.DeleteFileIfExists(logFile);
                IOHelper.DeleteFileIfExists(crashFile);
            }
            catch (Exception exception)
            {
                // Not a huge deal if some txt files are left behind so just let it fail and move on
                logger.LogError(exception, "Failed to delete old log files");
            }
        }
    }
}

/// <summary>
/// Empty class used to differentiate parameterless constructors from service container constructors
/// in types that need a parameterless constructor for deserialization.
/// </summary>
public class DummyService;

/// <summary>
/// Old version of the migration service, retained for migration purposes.
/// </summary>
[MemoryPackable]
[RegisterSingleton<LegacyMigrationService>]
public partial class LegacyMigrationService
{
    [MemoryPackConstructor]
    public LegacyMigrationService() { }

    public LegacyMigrationService(DummyService dummy)
    {
        if (File.Exists(FilePath))
        {
            var buffer = File.ReadAllBytes(FilePath);
            var decompressor = new Decompressor();
            var self = this;
            MemoryPackSerializer.Deserialize(decompressor.Unwrap(buffer), ref self,
                MemoryPackSerializerOptions.Utf8);
        }
    }

    private static string FilePath => Path.Combine(IOHelper.LocalApplicationData, "Flagrum", "migrations.fms");

    [MemoryPackInclude] public HashSet<Guid> Completed { get; set; } = [];

    public void Delete()
    {
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
        }
    }
}