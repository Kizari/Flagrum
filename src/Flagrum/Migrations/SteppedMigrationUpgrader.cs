using System;
using System.IO;
using System.Linq;
using Flagrum.Abstractions;
using Flagrum.Core.Utilities;
using Flagrum.Generators;
using Flagrum.Application.Persistence;
using Flagrum.Application.Persistence.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Flagrum.Migrations;

[InjectableDependency(ServiceLifetime.Scoped)]
public partial class SteppedMigrationUpgrader
{
    [Inject] private readonly IConfiguration _configuration;
    [Inject] private readonly FlagrumDbContext _context;
    [Inject] private readonly ILogger<SteppedMigrationUpgrader> _logger;
    [Inject] private readonly MigrationService _migrations;
    [Inject] private readonly IProfileService _profile;

    public void Run()
    {
        ResetUpgradeFlags();

        if (!_profile.Current.HasUpgradedToSteppedMigrations)
        {
            MergeMigrationFile();
            CheckMigration00();
            CheckMigration01();
            CheckMigration02();
            _profile.Current.HasUpgradedToSteppedMigrations = true;
            _configuration.Save();
        }
    }

    private void CheckMigration00()
    {
        if (!_migrations.Completed.Contains(BackupsMigration.ClearBackupsTableId))
        {
            if (_migrations.Completed.Contains(RemoveSqliteMigration.DestroyDatabaseId)
                || !_context.DoesTableExist(nameof(_context.StatePairs))
                || _context.GetBool(StateKey.HasMigratedBackups))
            {
                _configuration.SetMigratedNoSave(BackupsMigration.ApplicationSteps);
                _profile.Current.SetMigratedNoSave(BackupsMigration.ProfileSteps);
            }
        }
    }

    private void CheckMigration01()
    {
        if (!_migrations.Completed.Contains(ProfilesMigration.MigrateId))
        {
            if (!_profile.DidMigrateThisSession)
            {
                _configuration.SetMigratedNoSave(ProfilesMigration.ApplicationSteps);
                _profile.Current.SetMigratedNoSave(ProfilesMigration.ProfileSteps);
            }
        }
    }

    private void CheckMigration02()
    {
        if (!_migrations.Completed.Contains(FileIndexMigration.CleanupId))
        {
            if (File.Exists(_profile.FileIndexPath)
                || _migrations.Completed.Contains(RemoveSqliteMigration.DestroyDatabaseId)
                || !_context.DoesTableExist(nameof(_context.AssetExplorerNodes))
                || !_context.AssetExplorerNodes.Any())
            {
                _configuration.SetMigratedNoSave(FileIndexMigration.ApplicationSteps);
                _profile.Current.SetMigratedNoSave(FileIndexMigration.ProfileSteps);
            }
        }
    }

    /// <summary>
    /// There was one or two versions that stored the migration step status in a separate file
    /// this moves any completed steps from that file into the configuration file instead
    /// </summary>
    private void MergeMigrationFile()
    {
        _configuration.SetMigratedNoSave(
            RemoveSqliteMigration.ApplicationSteps.Where(s => _migrations.Completed.Contains(s)));
        _profile.Current.SetMigratedNoSave(
            RemoveSqliteMigration.ProfileSteps.Where(s => _migrations.Completed.Contains(s)));
        _configuration.Save();

        try
        {
            _migrations.Delete();
        }
        catch (Exception exception)
        {
            // Not a huge deal if this tiny file is left behind so just log it and move on
            _logger.LogError(exception, "Failed to delete migration file");
        }
    }

    /// <summary>
    /// There was a bug with the data migrations where I didn't account for cases where only some legacy migrations
    /// had been performed (see https://github.com/Kizari/Flagrum/issues/132)
    /// This will reset the flag for anyone coming from pre-1.5.10 to run it again with the fixed checks
    /// </summary>
    private void ResetUpgradeFlags()
    {
        if (_profile.LastVersion < new Version(1, 5, 10))
        {
            // Reset flag for all profiles
            foreach (var profile in _configuration.Profiles)
            {
                profile.HasUpgradedToSteppedMigrations = false;
            }

            _configuration.Save();

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
                _logger.LogError(exception, "Failed to delete old log files");
            }
        }
    }
}