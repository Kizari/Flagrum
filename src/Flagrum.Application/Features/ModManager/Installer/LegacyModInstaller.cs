using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Abstractions;
using Flagrum.Abstractions.Archive;
using Flagrum.Abstractions.AssetExplorer;
using Flagrum.Abstractions.ModManager.Instructions;
using Flagrum.Core.Archive;
using Flagrum.Core.Utilities;
using Flagrum.Core.Utilities.Extensions;
using Flagrum.Generators;
using Flagrum.Application.Features.ModManager.Data;
using Flagrum.Application.Features.ModManager.Installer;
using Flagrum.Application.Features.ModManager.Instructions;
using Flagrum.Application.Features.ModManager.Project;
using Flagrum.Application.Features.ModManager.Services;
using Injectio.Attributes;
using Microsoft.Extensions.Localization;

namespace Flagrum.Application.Features.ModManager.Legacy;

[RegisterScoped]
public partial class LegacyModInstaller
{
    [Inject] private readonly IFileIndex _fileIndex;
    [Inject] private readonly IModBuildInstructionFactory _instructionFactory;
    [Inject] private readonly ModManagerServiceBase _modManager;
    [Inject] private readonly IProfileService _profile;
    private IStringLocalizer<Index> _localizer;

    public async Task<ModInstallationResult> Install(ModInstallationRequest request)
    {
        _localizer = request.Localizer;

        using var zip = ZipFile.OpenRead(request.FilePath);
        var earcs = zip.Entries.Where(e => e.Name.EndsWith(".earc")).ToList();

        // Make sure there is at least one EARC present
        if (!earcs.Any())
        {
            return new ModInstallationResult(_localizer["Error"], _localizer["InvalidModPack"],
                _localizer["InvalidModPackDescription"]);
        }

        // Check for conflicts
        var earcPaths = new Dictionary<string, List<string>>();
        foreach (var entry in earcs)
        {
            var earc = entry.ToArray();
            using var unpacker = new EbonyArchive(earc);
            string originalEarcPath = null;
            foreach (var sample in unpacker.Files
                         .Select(_ => unpacker.Files.First(f => !f.Value.Flags
                             .HasFlag(EbonyArchiveFileFlags.Reference)).Value))
            {
                originalEarcPath = _fileIndex.GetArchiveRelativePathByUri(sample.Uri);
                if (originalEarcPath != null)
                {
                    break;
                }
            }

            if (originalEarcPath == null)
            {
                var is4KRelated = unpacker.Files
                    .Select(_ =>
                        unpacker.Files.First(f => !f.Value.Flags.HasFlag(EbonyArchiveFileFlags.Reference)).Value)
                    .Any(sample => sample.Uri.Contains("_$h2") || sample.Uri.Contains("/highimages/"));

                if (is4KRelated)
                {
                    // Skip comparison since there's no 4K pack to compare to
                    continue;
                }

                return new ModInstallationResult(_localizer["Error"], _localizer["IncompatibleModPack"],
                    _localizer["IncompatibleNewEarcDescription"]);
            }

            using var originalUnpacker = new EbonyArchive(Path.Combine(_profile.GameDataDirectory, originalEarcPath));

            if (!CompareArchives(originalEarcPath, originalUnpacker, unpacker, out var result))
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
            await request.HandleLegacyConflicts(earcPaths);
        }

        // Create the mod metadata
        var project = new FlagrumProject
        {
            Identifier = Guid.NewGuid(),
            Name = new string(request.FilePath.Split('\\').Last().Take(37).ToArray()),
            Author = "Unknown",
            Description = "Legacy mod converted by Flagrum"
        };

        // Create a folder for the mod files
        var directory = Path.Combine(_profile.ModFilesDirectory, project.Identifier.ToString());
        IOHelper.EnsureDirectoryExists(directory);

        // Create default thumbnail for the mod
        var defaultPreviewPath = Path.Combine(IOHelper.GetExecutingDirectory(), "Resources", "earc.png");
        var previewPath = Path.Combine(_profile.ImagesDirectory, $"{project.Identifier}.jpg");
        File.Copy(defaultPreviewPath, previewPath, true);
        var thumbnailPath = Path.Combine(directory, "thumbnail.jpg");
        File.Copy(defaultPreviewPath, thumbnailPath, true);

        // Check which files have changed
        foreach (var (original, earc) in earcPaths)
        {
            var earcModEarc = new FlagrumProjectArchive {RelativePath = original.Replace('\\', '/')};
            var entry = zip.Entries.First(e => e.FullName == earc[0]);
            using var unpacker = new EbonyArchive(entry.ToArray());
            using var originalUnpacker = new EbonyArchive(Path.Combine(_profile.GameDataDirectory, original));

            foreach (var file in unpacker.Files
                         .Where(f => !f.Value.Flags.HasFlag(EbonyArchiveFileFlags.Reference))
                         .Select(f => f.Value))
            {
                var match = originalUnpacker[file.Uri];
                if (match != null)
                {
                    if (file.Size != match.Size || !CompareFiles(match, file))
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

                        await File.WriteAllBytesAsync(fileName, file.GetReadableData());
                        var instruction = _instructionFactory.Create<ReplacePackedFileBuildInstruction>();
                        instruction.Uri = match.Uri;
                        instruction.FilePath = fileName;
                        earcModEarc.Instructions.Add(instruction);
                    }
                }
            }

            foreach (var (_, file) in originalUnpacker.Files)
            {
                if (!unpacker.HasFile(file.Uri))
                {
                    var instruction = _instructionFactory.Create<RemovePackedFileBuildInstruction>();
                    instruction.Uri = file.Uri;
                    earcModEarc.Instructions.Add(instruction);
                }
            }

            project.Archives.Add(earcModEarc);
        }

        // Save the mod and add it to the app state
        await project.Save(Path.Combine(directory, "project.fproj"));
        _modManager.Projects[project.Identifier] = project;
        _modManager.ModsState.Add(project.Identifier, new ModState());

        return new ModInstallationResult(project);
    }

    private bool CompareFiles(EbonyArchiveFile original, EbonyArchiveFile file)
    {
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

    private bool CompareArchives(string originalEarcPath, EbonyArchive original, EbonyArchive unpacker,
        out ModInstallationResult result)
    {
        result = null;

        var modsThatRemoveFilesFromThisEarc = _modManager.Projects.Values
            .Where(m => _modManager.ModsState.GetActive(m.Identifier)
                        && m.Archives.Any(a => a.RelativePath == originalEarcPath
                                               && a.Instructions.Any(r => r is RemovePackedFileBuildInstruction)))
            .Select(m => new
            {
                ModId = m.Identifier,
                ModName = m.Name,
                Uris = m.Archives.Where(a => a.RelativePath == originalEarcPath)
                    .SelectMany(a => a.Instructions.Where(i => i is RemovePackedFileBuildInstruction))
                    .Select(r => r.Uri)
            })
            .ToList();

        if (modsThatRemoveFilesFromThisEarc.Any())
        {
            result = new ModInstallationResult(_localizer["Error"], _localizer["ErrorDataCompare"],
                _localizer["ErrorDataCompareDescription"] + modsThatRemoveFilesFromThisEarc
                    .Aggregate("", (previous, next) => $"{previous}<strong>{next.ModName}</strong>"));
        }
        else
        {
            var originalFileList = original.Files
                .Select(f => f.Value.Uri)
                .ToList();

            if (unpacker.Files.Any(f => !originalFileList.Contains(f.Value.Uri)
                                        && !f.Value.Uri.Contains("/highimages/", StringComparison.OrdinalIgnoreCase)
                                        && !f.Value.Uri.Contains("_$h2", StringComparison.OrdinalIgnoreCase)))
            {
                result = new ModInstallationResult(_localizer["Error"], _localizer["IncompatibleModPack"],
                    _localizer["IncompatibleNewFilesDescription"]);
            }
        }

        return result == null;
    }
}