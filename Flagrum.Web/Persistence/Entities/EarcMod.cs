using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Core.Archive;
using Flagrum.Core.Ebex.Xmb2;
using Flagrum.Core.Gfxbin.Btex;
using Flagrum.Core.Utilities;
using Flagrum.Web.Features.AssetExplorer.Data;
using Flagrum.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Flagrum.Web.Persistence.Entities;

public class EarcModReplacement
{
    public int Id { get; set; }

    public int EarcModEarcId { get; set; }
    public EarcModEarc EarcModEarc { get; set; }

    public string Uri { get; set; }
    public string ReplacementFilePath { get; set; }
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
    [Required] [StringLength(50)] public string Name { get; set; }
    [Required] [StringLength(50)] public string Author { get; set; }
    [Required] [StringLength(1000)] public string Description { get; set; }
    public bool IsActive { get; set; }

    public ICollection<EarcModEarc> Earcs { get; set; } = new List<EarcModEarc>();

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
            ReplacementFilePath = replacementFilePath
        });
    }

    public async Task Save(FlagrumDbContext context, ILogger logger)
    {
        if (Id > 0 && IsActive)
        {
            // Need to revert first in-case the replacement list changed
            Revert(context);
        }

        IsActive = true;
        await SaveToDatabase(context);
        UpdateThumbnail();
        BuildAndApplyMod(context, logger);
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
            using var unpacker = new Unpacker(path);
            foreach (var replacement in earc.Replacements)
            {
                var hash = Cryptography.HashFileUri64(replacement.Uri).ToString();
                var filePath = $@"{backupDirectory}\{hash}";
                if (!Directory.EnumerateFiles(backupDirectory).Any(f => f.Split('\\').Last().StartsWith(hash)))
                {
                    var data = unpacker.UnpackRawByUri(replacement.Uri, out var originalSize);
                    File.WriteAllBytes($"{filePath}+{originalSize}", data);
                }
            }

            // Apply each replacement
            var packer = unpacker.ToPacker();
            foreach (var replacement in earc.Replacements)
            {
                var data = ConvertAsset(replacement, logger, uri => unpacker.UnpackFileByQuery(uri, out _));
                packer.UpdateFile(replacement.Uri, data);
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
                if (btexHeader.Format == BtexFormat.B8G8R8A8_UNORM)
                {
                    originalType = TextureType.MenuSprites;
                }
                else
                {
                    originalType = TextureType.BaseColor;
                }
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
                var hash = Cryptography.HashFileUri64(replacement.Uri).ToString();
                var backupFilePath = Directory.EnumerateFiles(backupDirectory)
                    .FirstOrDefault(f => f.Split('\\').Last().StartsWith(hash))!;
                var originalSize = uint.Parse(backupFilePath.Split('+').Last());
                var original = File.ReadAllBytes(backupFilePath);
                packer.UpdateFileWithProcessedData(replacement.Uri, originalSize, original);
            }

            packer.WriteToFile(earcPath);

            var backupFiles = Directory.EnumerateFiles(backupDirectory).ToList();
            foreach (var replacement in earc.Replacements)
            {
                var hash = Cryptography.HashFileUri64(replacement.Uri).ToString();
                var backupFilePath = backupFiles.FirstOrDefault(f => f.Split('\\').Last().StartsWith(hash))!;
                File.Delete(backupFilePath);
            }
        }
    }
}