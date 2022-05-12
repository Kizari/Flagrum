using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Core.Archive;
using Flagrum.Core.Utilities;

namespace Flagrum.Web.Persistence.Entities;

public class EarcMod
{
    public int Id { get; set; }
    [Required] [StringLength(50)] public string Name { get; set; }
    [Required] [StringLength(50)] public string Author { get; set; }
    [Required] [StringLength(150)] public string Description { get; set; }
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

    public async Task Save(FlagrumDbContext context)
    {
        IsActive = true;

        // Save the metadata to the database
        if (Id > 0)
        {
            context.Update(this);
        }
        else
        {
            await context.AddAsync(this);
        }

        await context.SaveChangesAsync();

        // Ensure the latest thumbnail is saved to the thumbnails directory
        var earcModDirectory = $@"{IOHelper.GetWebRoot()}\EarcMods";
        if (!Directory.Exists(earcModDirectory))
        {
            Directory.CreateDirectory(earcModDirectory);
        }

        File.Copy($@"{IOHelper.GetWebRoot()}\images\current_earc_preview.png",
            $@"{earcModDirectory}\{Id}.png",
            true);

        // Apply the mod
        foreach (var earc in Earcs)
        {
            // Ensure backup is present for the EARC
            var path = $@"{context.Settings.GameDataDirectory}\{earc.EarcRelativePath}";
            var backupPath = path.Replace(".earc", ".backup");
            if (!File.Exists(backupPath))
            {
                File.Copy(path, backupPath);
            }

            // Load the EARC into memory
            using var unpacker = new Unpacker(backupPath);
            var packer = unpacker.ToPacker();

            // Apply each replacement
            foreach (var replacement in earc.Replacements)
            {
                packer.UpdateFile(replacement.Uri, await File.ReadAllBytesAsync(replacement.ReplacementFilePath));
            }

            // Repack the EARC
            packer.WriteToFile(path);
        }
    }
}

public class EarcModEarc
{
    public int Id { get; set; }

    public int EarcModId { get; set; }
    public EarcMod EarcMod { get; set; }

    public string EarcRelativePath { get; set; }

    public ICollection<EarcModReplacement> Replacements { get; set; } = new List<EarcModReplacement>();
}

public class EarcModReplacement
{
    public int Id { get; set; }

    public int EarcModEarcId { get; set; }
    public EarcModEarc EarcModEarc { get; set; }

    public string Uri { get; set; }
    public string ReplacementFilePath { get; set; }
}