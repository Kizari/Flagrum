using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Core.Archive;
using Flagrum.Core.Ebex.Xmb2;
using Flagrum.Core.Gfxbin.Btex;
using Flagrum.Core.Utilities;
using Flagrum.Web.Features.AssetExplorer.Data;
using Flagrum.Web.Features.EarcMods.Data;
using Flagrum.Web.Resources;
using Flagrum.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Flagrum.Web.Persistence.Entities.ModManager;

public class EarcMod
{
    public int Id { get; set; }

    [Display(Name = nameof(DisplayNameResource.ModName), ResourceType = typeof(DisplayNameResource))]
    [Required(ErrorMessageResourceName = nameof(ErrorMessageResource.RequiredError),
        ErrorMessageResourceType = typeof(ErrorMessageResource))]
    [StringLength(37, ErrorMessageResourceName = nameof(ErrorMessageResource.MaxLengthError),
        ErrorMessageResourceType = typeof(ErrorMessageResource))]
    public string Name { get; set; }

    [Display(Name = nameof(DisplayNameResource.Author), ResourceType = typeof(DisplayNameResource))]
    [Required(ErrorMessageResourceName = nameof(ErrorMessageResource.RequiredError),
        ErrorMessageResourceType = typeof(ErrorMessageResource))]
    [StringLength(32, ErrorMessageResourceName = nameof(ErrorMessageResource.MaxLengthError),
        ErrorMessageResourceType = typeof(ErrorMessageResource))]
    public string Author { get; set; }

    [Display(Name = nameof(DisplayNameResource.Description), ResourceType = typeof(DisplayNameResource))]
    [Required(ErrorMessageResourceName = nameof(ErrorMessageResource.RequiredError),
        ErrorMessageResourceType = typeof(ErrorMessageResource))]
    [StringLength(1000, ErrorMessageResourceName = nameof(ErrorMessageResource.MaxLengthError),
        ErrorMessageResourceType = typeof(ErrorMessageResource))]
    public string Description { get; set; }

    public string Readme { get; set; }

    public bool IsActive { get; set; }
    public ModCategory Category { get; set; }
    public bool IsFavourite { get; set; }


    public ICollection<EarcModEarc> Earcs { get; set; } = new List<EarcModEarc>();
    public ICollection<EarcModLooseFile> LooseFiles { get; set; } = new List<EarcModLooseFile>();

    public bool HaveFilesChanged
    {
        get
        {
            var outdated = false;
            foreach (var earc in Earcs)
            {
                foreach (var file in earc.Files.Where(f =>
                             f.Type is EarcFileChangeType.Add or EarcFileChangeType.Replace
                                 or EarcFileChangeType.AddToTextureArray))
                {
                    if (file.FileLastModified != File.GetLastWriteTime(file.ReplacementFilePath).Ticks)
                    {
                        outdated = true;
                    }
                }
            }

            return outdated;
        }
    }

    public bool IsCached
    {
        get
        {
            var isCached = true;
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            foreach (var earc in Earcs)
            {
                foreach (var file in earc.Files.Where(f =>
                             f.Type is EarcFileChangeType.Add or EarcFileChangeType.Replace
                                 or EarcFileChangeType.AddToTextureArray))
                {
                    if (!file.ReplacementFilePath.EndsWith(".ffg") &&
                        !File.Exists(
                            $@"{appdata}\Temp\Flagrum\cache\{Id}{file.Id}{Cryptography.HashFileUri64(file.Uri)}.ffg"))
                    {
                        isCached = false;
                    }
                }
            }

            return isCached;
        }
    }

    public bool HasAnyCachedFiles => Earcs.Any(e => e.Files
        .Any(f => f.Type is EarcFileChangeType.Add or EarcFileChangeType.Replace or EarcFileChangeType.AddToTextureArray
                  && File.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}" +
                                 $@"\Temp\Flagrum\cache\{Id}{f.Id}{Cryptography.HashFileUri64(f.Uri)}.ffg")));

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

        earc.IsExpanded = true;

        earc.Files.Add(new EarcModFile
        {
            Uri = uri,
            Type = EarcFileChangeType.Remove
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

        earc.IsExpanded = true;
        earc.Files.Add(new EarcModFile
        {
            Uri = replacementUri,
            ReplacementFilePath = replacementFilePath,
            Type = EarcFileChangeType.Replace
        });
    }

    public void AddTextureArrayAddition(string earcPath, string textureArrayUri, string texturePath)
    {
        var earc = Earcs.FirstOrDefault(e => e.EarcRelativePath.Equals(earcPath, StringComparison.OrdinalIgnoreCase));

        if (earc == null)
        {
            earc = new EarcModEarc
            {
                EarcRelativePath = earcPath
            };

            Earcs.Add(earc);
        }

        earc.IsExpanded = true;
        earc.Files.Add(new EarcModFile
        {
            Uri = textureArrayUri,
            ReplacementFilePath = texturePath,
            Type = EarcFileChangeType.AddToTextureArray
        });
    }

    public async Task SaveCardOnly(FlagrumDbContext context)
    {
        var mod = context.EarcMods.FirstOrDefault(m => m.Id == Id) ?? new EarcMod();

        mod.Name = Name;
        mod.Description = Description;
        mod.Category = Category;
        mod.Author = Author;
        mod.Readme = Readme;

        if (mod.Id == 0)
        {
            await context.AddAsync(mod);
        }

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        Id = mod.Id;
        UpdateThumbnail();
    }

    public async Task SaveNoBuild(FlagrumDbContext context, bool hasBuildListChanged)
    {
        if (Id > 0 && IsActive)
        {
            // Need to revert first in-case the replacement list changed
            Revert(context);
        }

        IsActive = false;

        if (hasBuildListChanged)
        {
            await SaveToDatabase(context);
        }
        else
        {
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
        }
    }

    public async Task Save(FlagrumDbContext context, ILogger logger, bool hasBuildListChanged)
    {
        if (Id > 0 && IsActive)
        {
            // Need to revert first in-case the replacement list changed
            Revert(context);
        }

        var canBeApplied = CanBeApplied(context);
        IsActive = canBeApplied;

        if (hasBuildListChanged)
        {
            await SaveToDatabase(context);
        }
        else
        {
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
        }

        if (canBeApplied)
        {
            BuildAndApplyMod(context, logger, true);

            UpdateLastModifiedTimestamps(this);
            context.Update(this);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
        }
        else
        {
            throw new FileNotFoundException("At least one EARC containing files to modify was not found on disk");
        }
    }

    private bool CanBeApplied(FlagrumDbContext context)
    {
        return !Earcs.Where(e => e.Type == EarcChangeType.Change)
            .Select(earc => $@"{context.Settings.GameDataDirectory}\{earc.EarcRelativePath}")
            .Where(path => !File.Exists(path))
            .Any(path => !path.Contains(@"\highimages\"));
    }

    public void BuildCache(FlagrumDbContext context, ILogger logger)
    {
        Parallel.ForEach(Earcs.Where(e => e.Files.Any()), earc =>
        {
            var path = $@"{context.Settings.GameDataDirectory}\{earc.EarcRelativePath}";
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            IOHelper.EnsureDirectoryExists($@"{appdata}\Temp\Flagrum\cache");

            Unpacker unpacker = null;
            if (File.Exists(path))
            {
                unpacker = new Unpacker(path);
                _ = unpacker.Files; // Force file header read before parallelism to avoid multithreading issue
            }

            Parallel.ForEach(
                earc.Files.Where(f => f.Type is EarcFileChangeType.Add or EarcFileChangeType.Replace
                    or EarcFileChangeType.AddToTextureArray),
                file =>
                {
                    var hash = Cryptography.HashFileUri64(file.Uri);
                    var cachePath = $@"{appdata}\Temp\Flagrum\cache\{Id}{file.Id}{hash}.ffg";

                    // Only build files that are not already processed
                    if (!file.ReplacementFilePath.EndsWith(".ffg")
                        && (file.FileLastModified != File.GetLastWriteTime(file.ReplacementFilePath).Ticks
                            || !File.Exists(cachePath)))
                    {
                        FmodFragment fragment;
                        if (file.Type == EarcFileChangeType.Replace)
                        {
                            var original = unpacker!.Files.First(f => f.Uri == file.Uri);
                            var data = ConvertAsset(file, logger,
                                uri => unpacker.UnpackFileByQuery(file.Uri, out _));
                            var processedData = ArchiveFile.GetProcessedData(file.Uri,
                                original.Flags,
                                data,
                                original.Key,
                                unpacker.IsProtectedArchive,
                                out _);

                            fragment = new FmodFragment
                            {
                                OriginalSize = (uint)data.Length,
                                ProcessedSize = (uint)processedData.Length,
                                Flags = original.Flags,
                                Key = original.Key,
                                RelativePath = original.RelativePath,
                                Data = processedData
                            };
                        }
                        else
                        {
                            var data = ConvertAsset(file, logger, _ => null);
                            var processedData =
                                ArchiveFile.GetProcessedData(file.Uri, file.Flags, data, 0, true,
                                    out var archiveFile);

                            fragment = new FmodFragment
                            {
                                OriginalSize = (uint)data.Length,
                                ProcessedSize = (uint)processedData.Length,
                                Flags = archiveFile.Flags,
                                Key = archiveFile.Key,
                                RelativePath = archiveFile.RelativePath,
                                Data = processedData
                            };
                        }

                        fragment.Write(cachePath);
                    }
                });

            unpacker?.Dispose();
        });
    }

    private void BuildAndApplyMod(FlagrumDbContext context, ILogger logger, bool shouldCacheFiles)
    {
        var backupDirectory = $@"{context.Settings.FlagrumDirectory}\earc\backup";
        if (!Directory.Exists(backupDirectory))
        {
            Directory.CreateDirectory(backupDirectory);
        }

        var settings = new SettingsService();
        var earcs = new ConcurrentDictionary<string, string>();

        // Backup all files that will be replaced, altered, or removed
        Parallel.ForEach(
            Earcs.Where(e => e.Type == EarcChangeType.Change && e.Files.Any(f =>
                f.Type is EarcFileChangeType.Remove or EarcFileChangeType.Replace
                    or EarcFileChangeType.AddToTextureArray)), earc =>
            {
                var path = $@"{context.Settings.GameDataDirectory}\{earc.EarcRelativePath}";
                using var unpacker = new Unpacker(path);

                foreach (var replacement in earc.Files.Where(r =>
                             r.Type is EarcFileChangeType.Remove or EarcFileChangeType.Replace
                                 or EarcFileChangeType.AddToTextureArray))
                {
                    var hash = Cryptography.HashFileUri64(replacement.Uri).ToString();
                    var filePath = $@"{backupDirectory}\{hash}.ffg";
                    if (!File.Exists(filePath))
                    {
                        var file = unpacker.Files.FirstOrDefault(f => f.Uri == replacement.Uri)!;
                        var data = unpacker.UnpackRawByUri(replacement.Uri);
                        var fragment = new FmodFragment
                        {
                            OriginalSize = file.Size,
                            ProcessedSize = (uint)data.Length,
                            Flags = file.Flags,
                            Key = file.Key,
                            RelativePath = file.RelativePath,
                            Data = data
                        };

                        fragment.Write(filePath);
                    }
                }
            });

        Parallel.ForEach(Earcs.Where(e => e.Files.Any()), earc =>
        {
            var path = $@"{context.Settings.GameDataDirectory}\{earc.EarcRelativePath}";

            Packer packer;
            if (earc.Type == EarcChangeType.Create)
            {
                packer = new Packer();
                packer.SetFlags(earc.Flags);
            }
            else
            {
                var unpacker = new Unpacker(path);
                packer = unpacker.ToPacker();
            }

            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            IOHelper.EnsureDirectoryExists($@"{appdata}\Temp\Flagrum\cache");

            // Process physical files
            Parallel.Invoke(() =>
                {
                    Parallel.ForEach(
                        earc.Files.Where(f =>
                            f.Type is EarcFileChangeType.Add or EarcFileChangeType.Replace
                                or EarcFileChangeType.AddToTextureArray),
                        file =>
                        {
                            FmodFragment fragment;
                            var hash = Cryptography.HashFileUri64(file.Uri);
                            var cachePath = $@"{appdata}\Temp\Flagrum\cache\{Id}{file.Id}{hash}.ffg";
                            var needsRebuild = file.FileLastModified !=
                                               File.GetLastWriteTime(file.ReplacementFilePath).Ticks
                                               || !File.Exists(cachePath);

                            if (needsRebuild)
                            {
                                if (file.Type == EarcFileChangeType.Replace)
                                {
                                    var original = packer.Files.First(f => f.Uri == file.Uri);
                                    if (file.ReplacementFilePath.EndsWith(".ffg"))
                                    {
                                        var projectFragment = new FmodFragment();
                                        projectFragment.Read(file.ReplacementFilePath);
                                        fragment = projectFragment;
                                    }
                                    else
                                    {
                                        var data = ConvertAsset(file, logger,
                                            uri => packer.Files.First(f => f.Uri == uri).GetReadableData());
                                        var processedData = ArchiveFile.GetProcessedData(file.Uri,
                                            original.Flags,
                                            data,
                                            original.Key,
                                            packer.IsProtectedArchive,
                                            out _);

                                        fragment = new FmodFragment
                                        {
                                            OriginalSize = (uint)data.Length,
                                            ProcessedSize = (uint)processedData.Length,
                                            Flags = original.Flags,
                                            Key = original.Key,
                                            RelativePath = original.RelativePath,
                                            Data = processedData
                                        };
                                    }
                                }
                                else
                                {
                                    if (file.ReplacementFilePath.EndsWith(".ffg"))
                                    {
                                        var projectFragment = new FmodFragment();
                                        projectFragment.Read(file.ReplacementFilePath);
                                        fragment = projectFragment;
                                    }
                                    else
                                    {
                                        var data = ConvertAsset(file, logger, uri => null);
                                        var processedData =
                                            ArchiveFile.GetProcessedData(file.Uri, file.Flags, data, 0, true,
                                                out var archiveFile);

                                        fragment = new FmodFragment
                                        {
                                            OriginalSize = (uint)data.Length,
                                            ProcessedSize = (uint)processedData.Length,
                                            Flags = archiveFile.Flags,
                                            Key = archiveFile.Key,
                                            RelativePath = archiveFile.RelativePath,
                                            Data = processedData
                                        };
                                    }
                                }

                                if (!file.ReplacementFilePath.EndsWith(".ffg") && shouldCacheFiles)
                                {
                                    fragment.Write(cachePath);
                                }
                            }
                            else
                            {
                                fragment = new FmodFragment();
                                fragment.Read(file.ReplacementFilePath.EndsWith(".ffg")
                                    ? file.ReplacementFilePath
                                    : cachePath);
                            }

                            if (file.Type == EarcFileChangeType.Add)
                            {
                                packer.AddProcessedFile(file.Uri, fragment.Flags, fragment.Data,
                                    fragment.OriginalSize, fragment.Key);
                            }
                            else if (file.Type == EarcFileChangeType.AddToTextureArray)
                            {
                                var unprocessedData = ArchiveFile.GetUnprocessedData(fragment.Flags,
                                    fragment.OriginalSize, fragment.Key, fragment.Data);
                                var textureArray = packer.Files.First(f => f.Uri == file.Uri).GetReadableData();
                                var result = BtexConverter.AddTextureToArray(unprocessedData, textureArray);
                                packer.UpdateFile(file.Uri, result);
                            }
                            else
                            {
                                packer.UpdateFileWithProcessedData(file.Uri, fragment.OriginalSize,
                                    fragment.Data);
                            }
                        });
                },
                () =>
                {
                    foreach (var file in earc.Files)
                    {
                        if (file.Type == EarcFileChangeType.AddReference)
                        {
                            packer.AddFile(file.Uri, file.Flags, null);
                        }
                        else if (file.Type == EarcFileChangeType.Remove)
                        {
                            packer.RemoveFile(file.Uri);
                        }
                    }
                });

            // Pack the earc
            var outPath = $@"{settings.ModStagingDirectory}\{earc.Id}.earc";
            IOHelper.EnsureDirectoriesExistForFilePath(outPath);
            packer.WriteToFile(outPath);

            earcs.TryAdd(outPath, path);
        });

        // Move built earcs to the correct locations
        foreach (var (stagingPath, destinationPath) in earcs)
        {
            IOHelper.EnsureDirectoriesExistForFilePath(destinationPath);
            File.Move(stagingPath, destinationPath, true);
        }

        // Backup and apply loose files
        Parallel.ForEach(LooseFiles, file =>
        {
            if (file.Type == EarcChangeType.Change)
            {
                // Backup the original file
                var fileName = file.RelativePath.ToBase64();
                var backupPath = $@"{backupDirectory}\{fileName}";
                if (!File.Exists(backupPath))
                {
                    var originalFile = $@"{settings.GameDataDirectory}\{file.RelativePath}";
                    if (File.Exists(originalFile))
                    {
                        File.Copy(originalFile, backupPath);
                    }
                }
            }

            var destination = $@"{settings.GameDataDirectory}\{file.RelativePath}";
            File.Copy(file.FilePath, destination, true);
        });
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
                foreach (var replacement in earc.Files)
                {
                    replacement.Id = 0;
                }

                earc.Id = 0;
            }

            foreach (var file in context.EarcModLooseFile.Where(e => e.EarcModId == Id))
            {
                context.Remove(file);
            }

            foreach (var file in LooseFiles)
            {
                file.Id = 0;
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

    private byte[] ConvertAsset(EarcModFile file, ILogger logger, Func<string, byte[]> getOriginalData)
    {
        var data = File.ReadAllBytes(file.ReplacementFilePath);

        var assetType = AssetExplorerItem.GetType(file.Uri);

        // Convert any non-BTEX textures to BTEX
        if (assetType == ExplorerItemType.Texture &&
            !file.ReplacementFilePath.EndsWith(".btex", StringComparison.OrdinalIgnoreCase))
        {
            var originalName = file.Uri.Split('/').Last();
            var originalNameWithoutExtension = originalName[..originalName.LastIndexOf('.')];
            var extension =
                file.ReplacementFilePath[(file.ReplacementFilePath.LastIndexOf('.') + 1)..].ToLower();

            var originalType = TextureType.Undefined;
            var types = new Dictionary<string, TextureType>
            {
                {"_mrs", TextureType.Mrs},
                {"_n", TextureType.Normal},
                {"_a", TextureType.Opacity},
                {"_o", TextureType.AmbientOcclusion},
                {"_mro", TextureType.BaseColor},
                {"_hro", TextureType.BaseColor},
                {"_b", TextureType.BaseColor},
                {"_ba", TextureType.BaseColor},
                {"_e", TextureType.BaseColor},
                {"_mrgb", TextureType.MenuItem}
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

            if (file.Type == EarcFileChangeType.AddToTextureArray)
            {
                originalType = TextureType.BaseColor;
            }

            var converter = new TextureConverter();

            if (originalType == TextureType.Undefined || file.Type == EarcFileChangeType.Replace)
            {
                var btex = getOriginalData(file.Uri);

                if (btex == null || btex.Length == 0)
                {
                    originalType = TextureType.BaseColor;
                }
                else
                {
                    var withoutSedb = new byte[btex.Length - 128];
                    Array.Copy(btex, 128, withoutSedb, 0, withoutSedb.Length);
                    var btexHeader = BtexConverter.ReadBtexHeader(withoutSedb);

                    return converter.ToBtex(new BtexBuildRequest
                    {
                        Name = originalNameWithoutExtension,
                        PixelFormat = btexHeader.Format,
                        ImageFlags = btexHeader.p_ImageFlags,
                        MipLevels = btexHeader.MipMapCount,
                        SourceFormat = extension switch
                        {
                            "tga" => BtexSourceFormat.Targa,
                            "dds" => BtexSourceFormat.Dds,
                            _ => BtexSourceFormat.Wic
                        },
                        SourceData = data
                    });
                }
            }

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
                    logger.LogWarning("Failed to convert {File} to XMB2", file.ReplacementFilePath);
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

        ClearCachedFiles();

        // Remove any ffg files that were installed with this mod
        foreach (var file in context.EarcMods
                     .SelectMany(m => m.Earcs
                         .SelectMany(e => e.Files
                             .Where(f => f.Type == EarcFileChangeType.Add || f.Type == EarcFileChangeType.Replace)
                             .Select(f => f.ReplacementFilePath))))
        {
            var tokens = file.Split('\\');
            if (tokens[^2] == Id.ToString() && tokens[^1].EndsWith(".ffg"))
            {
                File.Delete(file);
            }
        }

        // Remove the project directory if it's empty
        var projectDirectory = $@"{context.Settings.EarcModsDirectory}\{Id}";
        if (Directory.Exists(projectDirectory)
            && !Directory.EnumerateDirectories(projectDirectory).Any()
            && !Directory.EnumerateFiles(projectDirectory).Any())
        {
            Directory.Delete(projectDirectory);
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

    private void UpdateLastModifiedTimestamps(EarcMod mod)
    {
        foreach (var earc in mod.Earcs)
        {
            foreach (var change in earc.Files.Where(r =>
                         r.Type is EarcFileChangeType.Add or EarcFileChangeType.Replace))
            {
                change.FileLastModified = File.GetLastWriteTime(change.ReplacementFilePath).Ticks;
            }
        }

        foreach (var file in mod.LooseFiles)
        {
            file.FileLastModified = File.GetLastWriteTime(file.FilePath).Ticks;
        }
    }

    public async Task Enable(FlagrumDbContext context, ILogger logger)
    {
        // Get the mod again since the sub collections aren't populated
        var mod = context.EarcMods
            .Include(m => m.Earcs)
            .ThenInclude(e => e.Files)
            .Include(m => m.LooseFiles)
            .Where(m => m.Id == Id)
            .AsNoTracking()
            .ToList()
            .FirstOrDefault()!;

        mod.BuildAndApplyMod(context, logger, false);
        mod.IsActive = true;
        UpdateLastModifiedTimestamps(mod);
        context.Update(mod);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
    }

    private void Revert(FlagrumDbContext context)
    {
        // Get the unmodified mod from the DB in case the user has made any changes
        var unmodified = context.EarcMods
            .Include(m => m.Earcs)
            .ThenInclude(e => e.Files)
            .Include(m => m.LooseFiles)
            .Where(m => m.Id == Id)
            .AsNoTracking()
            .ToList()
            .FirstOrDefault()!;

        var backupDirectory = $@"{context.Settings.FlagrumDirectory}\earc\backup";

        foreach (var earc in unmodified.Earcs)
        {
            var earcPath = $@"{context.Settings.GameDataDirectory}\{earc.EarcRelativePath}";

            if (earc.Type == EarcChangeType.Create)
            {
                File.Delete(earcPath);
                continue;
            }

            using var unpacker = new Unpacker(earcPath);
            var packer = unpacker.ToPacker();

            foreach (var replacement in earc.Files)
            {
                if (replacement.Type is EarcFileChangeType.AddReference or EarcFileChangeType.Add)
                {
                    if (packer.HasFile(replacement.Uri))
                    {
                        packer.RemoveFile(replacement.Uri);
                    }

                    continue;
                }

                var hash = Cryptography.HashFileUri64(replacement.Uri);
                var fragment = new FmodFragment();
                fragment.Read($@"{backupDirectory}\{hash}.ffg");

                switch (replacement.Type)
                {
                    case EarcFileChangeType.Replace or EarcFileChangeType.AddToTextureArray:
                        packer.UpdateFileWithProcessedData(replacement.Uri, fragment.OriginalSize, fragment.Data);
                        break;
                    case EarcFileChangeType.Remove:
                        packer.AddFileFromBackup(replacement.Uri, fragment.RelativePath, fragment.OriginalSize,
                            fragment.Flags, fragment.Key, fragment.Data);
                        break;
                }
            }

            packer.WriteToFile(earcPath);

            // Remove backups
            foreach (var replacement in earc.Files.Where(r =>
                         r.Type is EarcFileChangeType.Remove or EarcFileChangeType.Replace))
            {
                var hash = Cryptography.HashFileUri64(replacement.Uri).ToString();
                var backupFilePath = $@"{backupDirectory}\{hash}.ffg";
                File.Delete(backupFilePath);
            }

            context.ChangeTracker.Clear();
        }

        foreach (var file in unmodified.LooseFiles)
        {
            var path = $@"{context.Settings.GameDataDirectory}\{file.RelativePath}";
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            if (file.Type == EarcChangeType.Change)
            {
                // Restore the backup
                var fileName = file.RelativePath.ToBase64();
                var backupPath = $@"{backupDirectory}\{fileName}";
                File.Copy(backupPath, path);
                File.Delete(backupPath);
            }
        }
    }

    public static async Task<EarcLegacyConversionResult> ConvertLegacyZip(string path, FlagrumDbContext context,
        ILogger logger,
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
                        earcModEarc.Files.Add(new EarcModFile
                        {
                            Uri = match.Uri,
                            ReplacementFilePath = fileName,
                            Type = EarcFileChangeType.Replace
                        });
                    }
                }
            }

            foreach (var file in originalUnpacker.Files)
            {
                if (!unpacker.HasFile(file.Uri))
                {
                    earcModEarc.Files.Add(new EarcModFile
                    {
                        Uri = file.Uri,
                        Type = EarcFileChangeType.Remove
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
                        e.Files.Any(r => r.Type == EarcFileChangeType.Remove))
            .Select(e => new
            {
                ModId = e.EarcMod.Id,
                ModName = e.EarcMod.Name,
                Uris = e.Files
                    .Where(r => r.Type == EarcFileChangeType.Remove)
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

    public void ClearCachedFiles()
    {
        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        foreach (var path in Directory.EnumerateFiles($@"{appdata}\Temp\Flagrum\cache")
                     .Where(f => f.Split('\\').Last().StartsWith(Id.ToString())))
        {
            File.Delete(path);
        }
    }
}

public class EarcLegacyConversionResult
{
    public Dictionary<int, string> ModsToDisable { get; set; }
    public EarcLegacyConversionStatus Status { get; set; }
    public EarcMod Mod { get; set; }
}