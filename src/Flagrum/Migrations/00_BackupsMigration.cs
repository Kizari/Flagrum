using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Abstractions;
using Flagrum.Core.Utilities;
using Flagrum.Generators;
using Flagrum.Application.Features.ModManager.Mod;
using Flagrum.Application.Features.Settings.Data;
using Flagrum.Application.Persistence;
using Flagrum.Application.Persistence.Entities;
using Flagrum.Application.Services;
using Flagrum.Application.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Flagrum.Migrations;

[SteppedDataMigration(0)]
public partial class BackupsMigration
{
    [Inject] private readonly IConfiguration _configuration;
    [Inject] private readonly IProfileService _profile;
    [Inject] private readonly FlagrumDbContext _context;
    [Inject] private readonly MigrationService _migrations;

    [MigrationStep(0, "9b931f84-480f-497e-b788-b14167d715d2", MigrationScope.Profile)]
    private void ConvertBackups()
    {
        // Iterate each backup record in the DB
        foreach (var backup in _context.EarcModBackups)
        {
            // Compute the file path of the old backup data for this record
            var hash = Cryptography.HashFileUri64(backup.Uri);
            var backupPath = Path.Combine(_profile.EarcModBackupsDirectory, hash.ToString());

            // Convert the backup to a fragment if it exists on disk
            if (File.Exists(backupPath))
            {
                var data = File.ReadAllBytes(backupPath);
                var fragment = new FmodFragment
                {
                    OriginalSize = backup.Size,
                    ProcessedSize = (uint)data.Length,
                    Flags = backup.Flags,
                    Key = backup.Key,
                    RelativePath = backup.RelativePath,
                    Data = data
                };

                fragment.Write(Path.Combine(_profile.EarcModBackupsDirectory, $"{hash}.ffg"));
                File.Delete(backupPath);
            }
        }
    }

    [MigrationStep(1, "f4b46c31-4980-4045-9aa5-9c4d2f03fb47", MigrationScope.Profile, MigrationStepMode.Retry)]
    private void ClearBackupsTable()
    {
        // Clear the table as it's no longer needed
        _context.Database.ExecuteSqlRaw($"DELETE FROM {nameof(_context.EarcModBackups)}");
    }
}