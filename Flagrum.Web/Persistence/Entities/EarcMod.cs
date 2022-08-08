using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Flagrum.Core.Archive;
using Flagrum.Core.Ebex.Xmb2;
using Flagrum.Core.Gfxbin.Btex;
using Flagrum.Core.Services.Logging;
using Flagrum.Core.Utilities;
using Flagrum.Web.Features.AssetExplorer.Data;
using Flagrum.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Flagrum.Web.Persistence.Entities;

public enum EarcChangeType
{
    Replace,
    Add,
    Remove
}

public class EarcModReplacement
{
    public int Id { get; set; }

    public int EarcModEarcId { get; set; }
    public EarcModEarc EarcModEarc { get; set; }

    public string Uri { get; set; }
    public string ReplacementFilePath { get; set; }
    public EarcChangeType Type { get; set; }
}

public class EarcModEarc
{
    public int Id { get; set; }

    public int EarcModId { get; set; }
    public EarcMod EarcMod { get; set; }

    public string EarcRelativePath { get; set; }

    public ICollection<EarcModReplacement> Replacements { get; set; } = new List<EarcModReplacement>();
}

public class EarcMod
{
    public int Id { get; set; }
    [Required] [StringLength(37)] public string Name { get; set; }
    [Required] [StringLength(32)] public string Author { get; set; }
    [Required] [StringLength(1000)] public string Description { get; set; }
    public bool IsActive { get; set; }

    public ICollection<EarcModEarc> Earcs { get; set; } = new List<EarcModEarc>();

    public void AddRemoval(string earcPath, string uri)
    {
        var earc = Earcs
            .FirstOrDefault(e => e.EarcRelativePath.Equals(earcPath, StringComparison.OrdinalIgnoreCase));

        if (earc == null)
        {
            earc = new EarcModEarc
            {
                EarcRelativePath = earcPath
            };

            Earcs.Add(earc);
        }

        earc.Replacements.Add(new EarcModReplacement
        {
            Uri = uri,
            Type = EarcChangeType.Remove
        });
    }

    public void AddReplacement(string earcPath, string replacementUri, string replacementFilePath)
    {
        var earc = Earcs
            .FirstOrDefault(e => e.EarcRelativePath.Equals(earcPath, StringComparison.OrdinalIgnoreCase));

        if (earc == null)
        {
            earc = new EarcModEarc
            {
                EarcRelativePath = earcPath
            };

            Earcs.Add(earc);
        }

        earc.Replacements.Add(new EarcModReplacement
        {
            Uri = replacementUri,
            ReplacementFilePath = replacementFilePath,
            Type = EarcChangeType.Replace
        });
    }

    public async Task SaveNoBuild(FlagrumDbContext context)
    {
        if (Id > 0 && IsActive)
        {
            // Need to revert first in-case the replacement list changed
            Revert(context);
        }

        IsActive = false;
        await SaveToDatabase(context);
        UpdateThumbnail();
    }

    public async Task Save(FlagrumDbContext context, ILogger logger)
    {
        if (Id > 0 && IsActive)
        {
            // Need to revert first in-case the replacement list changed
            Revert(context);
        }

        var canBeApplied = CanBeApplied(context);
        IsActive = canBeApplied;
        await SaveToDatabase(context);
        UpdateThumbnail();

        if (canBeApplied)
        {
            BuildAndApplyMod(context, logger);
        }
        else
        {
            throw new FileNotFoundException("At least one EARC containing files to modify was not found on disk");
        }
    }

    private bool CanBeApplied(FlagrumDbContext context)
    {
        return !Earcs.Select(earc => $@"{context.Settings.GameDataDirectory}\{earc.EarcRelativePath}")
            .Where(path => !File.Exists(path))
            .Any(path => !path.Contains(@"\highimages\"));
    }

    private void BuildAndApplyMod(FlagrumDbContext context, ILogger logger)
    {
        foreach (var earc in Earcs)
        {
            var path = $@"{context.Settings.GameDataDirectory}\{earc.EarcRelativePath}";
            var backupDirectory = $@"{context.Settings.FlagrumDirectory}\earc\backup";

            if (!Directory.Exists(backupDirectory))
            {
                Directory.CreateDirectory(backupDirectory);
            }

            // Backup each file that is going to be replaced
            var unpacker = new Unpacker(path);
            foreach (var replacement in earc.Replacements)
            {
                var hash = Cryptography.HashFileUri64(replacement.Uri).ToString();
                var filePath = $@"{backupDirectory}\{hash}";
                if (!Directory.EnumerateFiles(backupDirectory).Any(f => f.Split('\\').Last().StartsWith(hash)))
                {
                    var file = unpacker.Files.FirstOrDefault(f => f.Uri == replacement.Uri)!;
                    context.EarcModBackups.Add(new EarcModBackup
                    {
                        Uri = replacement.Uri,
                        RelativePath = file.RelativePath,
                        Size = file.Size,
                        Flags = file.Flags,
                        LocalizationType = file.LocalizationType,
                        Locale = file.Locale,
                        Key = file.Key
                    });

                    var data = unpacker.UnpackRawByUri(replacement.Uri);
                    File.WriteAllBytes($"{filePath}", data);
                    context.SaveChanges();
                }
            }
            
            // Get data for each replacement
            var replacements = earc.Replacements.Where(replacement => replacement.Type == EarcChangeType.Replace)
                .ToDictionary(replacement => replacement, 
                    replacement => ConvertAsset(replacement, logger, 
                        uri => unpacker.UnpackFileByQuery(uri, out _)));

            // Apply each replacement
            var packer = unpacker.ToPacker();
            foreach (var replacement in earc.Replacements)
            {
                if (replacement.Type == EarcChangeType.Replace)
                {
                    packer.UpdateFile(replacement.Uri, replacements[replacement]);
                }
                else
                {
                    packer.RemoveFile(replacement.Uri);
                }
            }

            // Repack the EARC
            packer.WriteToFile(path);
        }
    }

    private void UpdateThumbnail()
    {
        var earcModDirectory = $@"{IOHelper.GetWebRoot()}\EarcMods";
        if (!Directory.Exists(earcModDirectory))
        {
            Directory.CreateDirectory(earcModDirectory);
        }

        File.Copy($@"{IOHelper.GetWebRoot()}\images\current_earc_preview.png",
            $@"{earcModDirectory}\{Id}.png",
            true);
    }

    private async Task SaveToDatabase(FlagrumDbContext context)
    {
        // Save the metadata to the database
        if (Id > 0)
        {
            foreach (var earc in context.EarcModEarcs.Where(e => e.EarcModId == Id))
            {
                foreach (var replacement in context.EarcModReplacements.Where(r => r.EarcModEarcId == earc.Id))
                {
                    context.Remove(replacement);
                }

                context.Remove(earc);
            }

            foreach (var earc in Earcs)
            {
                foreach (var replacement in earc.Replacements)
                {
                    replacement.Id = 0;
                }

                earc.Id = 0;
            }

            context.Update(this);
        }
        else
        {
            await context.AddAsync(this);
        }

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
    }

    private byte[] ConvertAsset(EarcModReplacement replacement, ILogger logger, Func<string, byte[]> getOriginalData)
    {
        var data = File.ReadAllBytes(replacement.ReplacementFilePath);
        var assetType = AssetExplorerItem.GetType(replacement.Uri);

        // Convert any non-BTEX textures to BTEX
        if (assetType == ExplorerItemType.Texture &&
            !replacement.ReplacementFilePath.EndsWith(".btex", StringComparison.OrdinalIgnoreCase))
        {
            var originalName = replacement.Uri.Split('/').Last();
            var originalNameWithoutExtension = originalName[..originalName.LastIndexOf('.')];
            var extension =
                replacement.ReplacementFilePath[(replacement.ReplacementFilePath.LastIndexOf('.') + 1)..];

            var originalType = TextureType.Undefined;
            var types = new Dictionary<string, TextureType>
            {
                {"_mrs", TextureType.Mrs},
                {"_n", TextureType.Normal},
                {"_a", TextureType.Opacity},
                {"_o", TextureType.AmbientOcclusion},
                {"_b", TextureType.BaseColor},
                {"_ba", TextureType.BaseColor},
                {"_e", TextureType.BaseColor}
            };

            foreach (var (suffix, type) in types)
            {
                if (originalNameWithoutExtension.EndsWith(suffix) ||
                    originalNameWithoutExtension.EndsWith(suffix + "_$h"))
                {
                    originalType = type;
                    break;
                }
            }

            if (originalType == TextureType.Undefined)
            {
                var btex = getOriginalData(replacement.Uri);
                var withoutSedb = new byte[btex.Length - 128];
                Array.Copy(btex, 128, withoutSedb, 0, withoutSedb.Length);
                var btexHeader = BtexConverter.ReadBtexHeader(withoutSedb);
                originalType = btexHeader.Format is BtexFormat.B8G8R8A8_UNORM or BtexFormat.BC7_UNORM 
                    ? TextureType.MenuSprites 
                    : TextureType.BaseColor;
            }

            var converter = new TextureConverter();
            data = converter.ToBtex(originalNameWithoutExtension, extension, originalType, data);
        }

        // Convert any XML files to XMB2
        else if (assetType == ExplorerItemType.Xml)
        {
            if (!(data[0] == 'X' && data[1] == 'M' && data[2] == 'B' && data[3] == '2'))
            {
                try
                {
                    data = new Xmb2Writer(data).Write();
                }
                catch
                {
                    // Can continue as normal after this since the game can read XML
                    logger.LogWarning("Failed to convert {File} to XMB2", replacement.ReplacementFilePath);
                }
            }
        }

        return data;
    }

    public async Task Delete(FlagrumDbContext context)
    {
        if (IsActive)
        {
            Revert(context);
        }

        context.Remove(this);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
    }

    public async Task Disable(FlagrumDbContext context)
    {
        Revert(context);
        IsActive = false;
        context.Update(this);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
    }

    public async Task Enable(FlagrumDbContext context, ILogger logger)
    {
        // Get the mod again since the sub collections aren't populated
        var mod = context.EarcMods
            .Include(m => m.Earcs)
            .ThenInclude(e => e.Replacements)
            .Where(m => m.Id == Id)
            .AsNoTracking()
            .ToList()
            .FirstOrDefault()!;

        mod.BuildAndApplyMod(context, logger);
        mod.IsActive = true;
        context.Update(mod);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
    }

    private void Revert(FlagrumDbContext context)
    {
        // Get the unmodified mod from the DB in case the user has made any changes
        var unmodified = context.EarcMods
            .Include(m => m.Earcs)
            .ThenInclude(e => e.Replacements)
            .Where(m => m.Id == Id)
            .AsNoTracking()
            .ToList()
            .FirstOrDefault()!;

        foreach (var earc in unmodified.Earcs)
        {
            var earcPath = $@"{context.Settings.GameDataDirectory}\{earc.EarcRelativePath}";
            var backupDirectory = $@"{context.Settings.FlagrumDirectory}\earc\backup";

            using var unpacker = new Unpacker(earcPath);
            var packer = unpacker.ToPacker();

            foreach (var replacement in earc.Replacements)
            {
                var hash = Cryptography.HashFileUri64(replacement.Uri);
                var backupFilePath = $@"{backupDirectory}\{hash}";
                var original = File.ReadAllBytes(backupFilePath);
                var backup = context.EarcModBackups.Find(replacement.Uri)!;

                if (replacement.Type == EarcChangeType.Replace)
                {
                    packer.UpdateFileWithProcessedData(replacement.Uri, backup.Size, original);
                }
                else
                {
                    packer.AddFileFromBackup(backup.Uri, backup.RelativePath, backup.Size, backup.Flags,
                        backup.LocalizationType, backup.Locale, backup.Key, original);
                }
            }

            packer.WriteToFile(earcPath);

            foreach (var replacement in earc.Replacements)
            {
                var hash = Cryptography.HashFileUri64(replacement.Uri).ToString();
                var backupFilePath = $@"{backupDirectory}\{hash}";
                File.Delete(backupFilePath);
                var backup = context.EarcModBackups.Find(replacement.Uri)!;
                context.EarcModBackups.Remove(backup);
            }

            context.SaveChanges();
            context.ChangeTracker.Clear();
        }
    }

    public static async Task<EarcLegacyConversionResult> ConvertLegacyZip(string path, FlagrumDbContext context, ILogger logger,
        Func<Dictionary<string, List<string>>, Task> handleConflicts)
    {
        // Get EARC files from the ZIP
        using var zip = ZipFile.OpenRead(path);
        var earcs = zip.Entries.Where(e => e.Name.EndsWith(".earc")).ToList();

        // Make sure there is at least one EARC present
        if (!earcs.Any())
        {
            return new EarcLegacyConversionResult {Status = EarcLegacyConversionStatus.NoEarcs};
        }

        // Check for conflicts
        var earcPaths = new Dictionary<string, List<string>>();
        foreach (var entry in earcs)
        {
            var earc = entry.ToArray();
            using var unpacker = new Unpacker(earc);
            string originalEarcPath = null;
            foreach (var sample in unpacker.Files.Select(_ =>
                         unpacker.Files.FirstOrDefault(f => !f.Flags.HasFlag(ArchiveFileFlag.Reference))!))
            {
                originalEarcPath = context.GetArchiveRelativeLocationByUri(sample.Uri);
                if (originalEarcPath != "UNKNOWN")
                {
                    break;
                }
            }

            if (originalEarcPath == "UNKNOWN")
            {
                var is4KRelated = unpacker.Files
                    .Select(_ => unpacker.Files.FirstOrDefault(f => !f.Flags.HasFlag(ArchiveFileFlag.Reference))!)
                    .Any(sample => sample.Uri.Contains("_$h2") || sample.Uri.Contains("/highimages/"));

                if (is4KRelated)
                {
                    // Skip comparison since there's no 4K pack to compare to
                    continue;
                }
                
                return new EarcLegacyConversionResult {Status = EarcLegacyConversionStatus.EarcNotFound};
            }

            using var originalUnpacker = new Unpacker($@"{context.Settings.GameDataDirectory}\{originalEarcPath}");

            if (!CompareArchives(context, originalEarcPath, originalUnpacker, unpacker, logger, out var result))
            {
                return result;
            }

            if (earcPaths.TryGetValue(originalEarcPath, out var zipPaths))
            {
                zipPaths.Add(entry.FullName);
            }
            else
            {
                earcPaths[originalEarcPath] = new List<string> {entry.FullName};
            }
        }

        if (earcPaths.Any(e => e.Value.Count > 1))
        {
            await handleConflicts(earcPaths);
        }

        // Create the mod metadata
        var earcMod = new EarcMod
        {
            Name = new string(path.Split('\\').Last().Take(37).ToArray()),
            Author = "Unknown",
            Description = "Legacy mod converted by Flagrum",
            IsActive = false
        };

        await context.AddAsync(earcMod);
        await context.SaveChangesAsync();

        // Create a folder for the mod files
        var directory = $@"{context.Settings.EarcModsDirectory}\{earcMod.Id}";
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Create default thumbnail for the mod
        var defaultPreviewPath = $@"{IOHelper.GetExecutingDirectory()}\Resources\earc.png";
        File.Copy(defaultPreviewPath, $@"{IOHelper.GetWebRoot()}\EarcMods\{earcMod.Id}.png", true);

        // Check which files have changed
        foreach (var (original, earc) in earcPaths)
        {
            var earcModEarc = new EarcModEarc {EarcRelativePath = original};
            var entry = zip.Entries.First(e => e.FullName == earc[0]);
            using var unpacker = new Unpacker(entry.ToArray());
            using var originalUnpacker = new Unpacker($@"{context.Settings.GameDataDirectory}\{original}");

            foreach (var file in unpacker.Files.Where(f => !f.Flags.HasFlag(ArchiveFileFlag.Reference)))
            {
                var match = originalUnpacker.Files.FirstOrDefault(f => f.Uri == file.Uri);
                if (match != null)
                {
                    if (file.Size != match.Size || !CompareFiles(originalUnpacker, match, unpacker, file))
                    {
                        // Save the file to the device
                        var fileName = $@"{directory}\{file.RelativePath.Split('/', '\\').Last()}";
                        var extension = fileName[fileName.LastIndexOf('.')..];
                        var fileNameWithoutExtension = fileName[..fileName.LastIndexOf('.')];
                        var counter = 2;

                        while (File.Exists(fileName))
                        {
                            fileName = $"{fileNameWithoutExtension}{counter++}{extension}";
                        }

                        unpacker.ReadFileData(file);
                        await File.WriteAllBytesAsync(fileName, file.GetReadableData());
                        earcModEarc.Replacements.Add(new EarcModReplacement
                        {
                            Uri = match.Uri,
                            ReplacementFilePath = fileName,
                            Type = EarcChangeType.Replace
                        });
                    }
                }
            }

            foreach (var file in originalUnpacker.Files)
            {
                if (!unpacker.HasFile(file.Uri))
                {
                    earcModEarc.Replacements.Add(new EarcModReplacement
                    {
                        Uri = file.Uri,
                        Type = EarcChangeType.Remove
                    });
                }
            }

            earcMod.Earcs.Add(earcModEarc);
        }

        await context.SaveChangesAsync();
        return new EarcLegacyConversionResult
        {
            Status = EarcLegacyConversionStatus.Success,
            Mod = earcMod
        };
    }

    private static bool CompareFiles(Unpacker originalUnpacker, ArchiveFile original, Unpacker unpacker,
        ArchiveFile file)
    {
        originalUnpacker.ReadFileData(original);
        unpacker.ReadFileData(file);

        var originalData = original.GetRawData();
        var data = file.GetRawData();

        for (var i = 0; i < originalData.Length; i++)
        {
            if (originalData[i] != data[i])
            {
                return false;
            }
        }

        return true;
    }

    private static bool CompareArchives(FlagrumDbContext context, string originalEarcPath, Unpacker original,
        Unpacker unpacker, ILogger logger, out EarcLegacyConversionResult result)
    {
        result = null;

        var modsThatRemoveFilesFromThisEarc = context.EarcModEarcs
            .Where(e => e.EarcMod.IsActive && e.EarcRelativePath == originalEarcPath &&
                        e.Replacements.Any(r => r.Type == EarcChangeType.Remove))
            .Select(e => new
            {
                ModId = e.EarcMod.Id,
                ModName = e.EarcMod.Name,
                Uris = e.Replacements
                    .Where(r => r.Type == EarcChangeType.Remove)
                    .Select(r => r.Uri)
            })
            .ToList();

        if (modsThatRemoveFilesFromThisEarc.Any())
        {
            result = new EarcLegacyConversionResult
            {
                Status = EarcLegacyConversionStatus.NeedsDisabling,
                ModsToDisable = modsThatRemoveFilesFromThisEarc.ToDictionary(m => m.ModId, m => m.ModName)
            };
        }
        else
        {
            var originalFileList = original.Files
                .Select(f => f.Uri)
                .ToList();

            if (unpacker.Files.Any(f => !originalFileList.Contains(f.Uri)
                && !f.Uri.Contains("/highimages/", StringComparison.OrdinalIgnoreCase)
                && !f.Uri.Contains("_$h2", StringComparison.OrdinalIgnoreCase)))
            {
                
                result = new EarcLegacyConversionResult {Status = EarcLegacyConversionStatus.NewFiles};
            }
        }

        return result == null;
    }
}

public class EarcLegacyConversionResult
{
    public Dictionary<int, string> ModsToDisable { get; set; }
    public EarcLegacyConversionStatus Status { get; set; }
    public EarcMod Mod { get; set; }
}

public enum EarcLegacyConversionStatus
{
    Success,
    NoEarcs,
    EarcNotFound,
    NewFiles,
    NeedsDisabling
}