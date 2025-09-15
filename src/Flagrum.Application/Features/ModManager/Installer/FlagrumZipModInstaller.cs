using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flagrum.Abstractions;
using Flagrum.Abstractions.ModManager.Instructions;
using Flagrum.Application.Features.ModManager.Data;
using Flagrum.Application.Features.ModManager.Instructions;
using Flagrum.Application.Features.ModManager.Instructions.Abstractions;
using Flagrum.Application.Features.ModManager.Mod;
using Flagrum.Application.Features.ModManager.Project;
using Flagrum.Application.Features.ModManager.Services;
using Flagrum.Application.Utilities;
using Flagrum.Core.Utilities;
using Flagrum.Generators;
using Injectio.Attributes;
using Newtonsoft.Json;
using SixLabors.ImageSharp;

namespace Flagrum.Application.Features.ModManager.Installer;

[RegisterScoped]
public partial class FlagrumZipModInstaller
{
    [Inject] private readonly IModBuildInstructionFactory _instructionFactory;
    [Inject] private readonly ModManagerServiceBase _modManager;
    [Inject] private readonly IProfileService _profile;

    public async Task<ModInstallationResult> Install(ModInstallationRequest request)
    {
        using var zip = ZipFile.OpenRead(request.FilePath);
        var jsonEntry = zip.GetEntry("flagrum.json")!;
        await using var jsonStream = jsonEntry.Open();
        await using var jsonMemoryStream = new MemoryStream();
        await jsonStream.CopyToAsync(jsonMemoryStream);
        var json = Encoding.UTF8.GetString(jsonMemoryStream.ToArray());
        var metadata = JsonConvert.DeserializeObject<EarcModMetadata>(json)!;

        var project = new FlagrumProject
        {
            Identifier = Guid.NewGuid(),
            Name = metadata.Name,
            Author = metadata.Author,
            Description = metadata.Description
        };

        var directory = Path.Combine(_profile.ModFilesDirectory, project.Identifier.ToString());
        IOHelper.EnsureDirectoryExists(directory);

        var thumbnailEntry = zip.GetEntry("flagrum.png")!;
        await using var thumbnailStream = thumbnailEntry.Open();
        using var image = await Image.LoadAsync(thumbnailStream);
        image.ResizeFill(326, 170);
        await image.SaveAsJpegAsync(Path.Combine(directory, "thumbnail.jpg"));
        File.Copy(Path.Combine(directory, "thumbnail.jpg"),
            Path.Combine(_profile.ImagesDirectory, $"{project.Identifier}.jpg"));

        if (metadata.Version == 0)
        {
            foreach (var (earcPath, replacements) in metadata.Replacements)
            {
                var earc = new FlagrumProjectArchive {RelativePath = earcPath.Replace('\\', '/')};
                foreach (var replacement in replacements)
                {
                    var hash = Cryptography.HashFileUri64(replacement).ToString();
                    var matchEntry = zip.Entries.FirstOrDefault(e => e.Name.Contains(hash))!;
                    var filePath = Path.Combine(directory, matchEntry.Name);

                    await using var entryStream = matchEntry.Open();
                    await using var entryMemoryStream = new MemoryStream();
                    await entryStream.CopyToAsync(entryMemoryStream);
                    await File.WriteAllBytesAsync(filePath, entryMemoryStream.ToArray());

                    var instruction = _instructionFactory.Create<ReplacePackedFileBuildInstruction>();
                    instruction.Uri = replacement;
                    instruction.FilePath = filePath;
                    instruction.FileLastModified = File.GetLastWriteTime(filePath).Ticks;
                    earc.Instructions.Add(instruction);
                }

                project.Archives.Add(earc);
            }
        }
        else
        {
            foreach (var (earcPath, changes) in metadata.Changes)
            {
                var earc = new FlagrumProjectArchive {RelativePath = earcPath.Replace('\\', '/')};
                foreach (var change in changes)
                {
                    string filePath = null;

                    if (change.Type is LegacyModBuildInstruction.ReplacePackedFile
                        or LegacyModBuildInstruction.AddPackedFile)
                    {
                        var hash = Cryptography.HashFileUri64(change.Uri).ToString();
                        var matchEntry = zip.Entries.FirstOrDefault(e => e.Name.Contains(hash))!;
                        filePath = Path.Combine(directory, matchEntry.Name);

                        await using var entryStream = matchEntry.Open();
                        await using var entryMemoryStream = new MemoryStream();
                        await entryStream.CopyToAsync(entryMemoryStream);
                        await File.WriteAllBytesAsync(filePath, entryMemoryStream.ToArray());
                    }

                    PackedBuildInstruction instruction = change.Type switch
                    {
                        LegacyModBuildInstruction.ReplacePackedFile => _instructionFactory
                            .Create<ReplacePackedFileBuildInstruction>(),
                        LegacyModBuildInstruction.RemovePackedFile => _instructionFactory
                            .Create<RemovePackedFileBuildInstruction>(),
                        _ => throw new Exception(
                            "Pre-FMOD mods didn't support any instructions other than those above")
                    };

                    instruction.Uri = change.Uri;

                    if (instruction is PackedAssetBuildInstruction packedAssetBuildInstruction)
                    {
                        packedAssetBuildInstruction.FilePath = filePath;
                        packedAssetBuildInstruction.FileLastModified =
                            filePath == null ? 0 : File.GetLastWriteTime(filePath).Ticks;
                    }

                    earc.Instructions.Add(instruction);
                }

                project.Archives.Add(earc);
            }
        }

        await project.Save(Path.Combine(directory, "project.fproj"));
        _modManager.Projects[project.Identifier] = project;
        _modManager.ModsState.Add(project.Identifier, new ModState());

        return new ModInstallationResult(project);
    }
}