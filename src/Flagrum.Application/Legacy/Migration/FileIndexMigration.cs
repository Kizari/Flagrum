using System;
using System.IO;
using System.Linq;
using Flagrum.Abstractions;
using Flagrum.Core.Archive;
using Flagrum.Core.Persistence;
using Flagrum.Core.Utilities;
using Flagrum.Application.Features.AssetExplorer.Indexing;
using Flagrum.Application.Legacy.AssetExplorer.Indexing;
using Flagrum.Application.Services;

namespace Flagrum.Application.Legacy.Migration;

/// <summary>
/// Migrates <see cref="LegacyFileIndex" /> into the new <see cref="FileIndex" /> class.
/// </summary>
public class FileIndexMigration
{
    private static Guid Identifier => new("baeddc13-e7c6-4bc7-96f3-ece1c076041c");

    /// <summary>
    /// Runs the migration if it has not already been completed previously.
    /// </summary>
    /// <param name="profile">The profile whose index is to be migrated.</param>
    public static void Run(IProfileService profile)
    {
        if (!profile.Current.HasMigrated(Identifier))
        {
            if (File.Exists(profile.FileIndexPath))
            {
                // Read the old file index
                var legacy = Repository.Load<LegacyFileIndex>(profile.FileIndexPath);

                // Migrate the data into the new file index class
                var index = new FileIndex
                {
                    RootNode = legacy.RootNode,
                    Archives = legacy.Archives.ToDictionary
                    (
                        archive => Cryptography.Hash64(archive.RelativePath),
                        archive => archive
                    ),
                    Files = legacy.Files.ToDictionary
                    (
                        file => new AssetId(file.Uri),
                        file => file
                    )
                };

                // Overwrite the old file index with the new one
                Repository.Save(index, profile.FileIndexPath);
            }

            // Mark the migration as completed
            profile.Current.SetMigrated(Identifier);
        }
    }
}