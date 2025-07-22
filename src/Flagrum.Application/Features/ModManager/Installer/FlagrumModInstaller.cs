using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Abstractions;
using Flagrum.Abstractions.ModManager.Instructions;
using Flagrum.Core.Utilities;
using Flagrum.Core.Utilities.Exceptions;
using Flagrum.Generators;
using Flagrum.Application.Features.ModManager.Data;
using Flagrum.Application.Features.ModManager.Instructions.Abstractions;
using Flagrum.Application.Features.ModManager.Mod;
using Flagrum.Application.Features.ModManager.Services;
using Injectio.Attributes;

namespace Flagrum.Application.Features.ModManager.Installer;

[RegisterScoped]
public partial class FlagrumModInstaller
{
    [Inject] private readonly IModBuildInstructionFactory _instructionFactory;
    [Inject] private readonly ModManagerServiceBase _modManager;
    [Inject] private readonly IProfileService _profile;

    public async Task<ModInstallationResult> Install(ModInstallationRequest request)
    {
        using var modPack = new FlagrumModPack(request.FilePath);

        try
        {
            modPack.Read(request.FilePath, _instructionFactory);
        }
        catch (FileFormatException)
        {
            return new ModInstallationResult("Error", "Invalid FMOD", "The given file is not a valid FMOD.");
        }
        catch (FormatVersionException)
        {
            return new ModInstallationResult("Error", "Please Upgrade Flagrum",
                "The FMOD supplied is newer than this version of Flagrum can handle. Please upgrade to the latest version of Flagrum and try again.");
        }
        catch (FileTamperException)
        {
            return new ModInstallationResult("Error", "Corruption Detected",
                "The FMOD supplied is corrupted. Please get a new copy from the original source.");
        }
        catch (EarlyAccessException)
        {
            return new ModInstallationResult("Error", "Early Access Required",
                "This mod is currently in early access. Please ensure you are subscribed to the Flagrum" +
                " Patreon and have linked your Patreon account in the Settings tab.");
        }

        // Display mod selection modal if pack contains more than one mod
        List<FlagrumMod> selectedMods;
        if (modPack.Mods.Count < 2)
        {
            selectedMods = modPack.Mods;
        }
        else
        {
            var selectionResult = await request.GetModPackSelection(modPack.Mods);
            if (selectionResult == null)
            {
                return new ModInstallationResult(ModInstallationStatus.Cancelled);
            }

            selectedMods = selectionResult.ToList();
        }

        // Check if any of the mods are already installed
        var duplicates = selectedMods
            .Where(m => _modManager.Projects.ContainsKey(m.Metadata.Guid))
            .ToList();

        if (duplicates.Any())
        {
            return new ModInstallationResult("Error", "Duplicate Mods Detected",
                "You already have the following mods installed. If you wish to reinstall them, please " +
                "delete the copies you already have first and try again.<br/><br/>" +
                duplicates.Aggregate("", (previous, next) => $"{previous}{GetDuplicateText(next)}"));
        }

        var result = new ModInstallationResult(ModInstallationStatus.Success);

        // Process the selected mods
        foreach (var mod in selectedMods)
        {
            if (mod.Metadata.Guid == Guid.Empty)
            {
                mod.Metadata.Guid = Guid.NewGuid();
            }

            // Create a directory for the new mod
            var directory = Path.Combine(_profile.ModFilesDirectory, mod.Metadata.Guid.ToString());
            IOHelper.EnsureDirectoryExists(directory);

            // Unpack the thumbnail and make a copy in wwwroot
            var thumbnailPath = Path.Combine(directory, "thumbnail.jpg");
            mod.UnpackThumbnail(request.FilePath, thumbnailPath);
            File.Copy(thumbnailPath, Path.Combine(_profile.ImagesDirectory, $"{mod.Metadata.Guid}.jpg"), true);

            // Unpack the mod's files into the mod directory
            foreach (var (instruction, _) in mod.FileTable)
            {
                string fileName;
                switch (instruction)
                {
                    case PackedAssetBuildInstruction packed:
                        fileName = $"{Cryptography.HashFileUri64(packed.Uri)}.ffg";
                        packed.FilePath = Path.Combine(directory, fileName);
                        break;
                    case LooseAssetBuildInstruction loose:
                        fileName =
                            $"{Cryptography.HashFileUri64(loose.RelativePath)}.{loose.RelativePath.Split('.')[^1]}";
                        loose.FilePath = Path.Combine(directory, fileName);
                        break;
                    default:
                        throw new Exception("Invalid mod instruction type");
                }

                var outputPath = Path.Combine(directory, fileName);
                mod.Unpack(instruction, request.FilePath, outputPath);
            }

            // Create a Flagrum Project from the mod metadata
            var project = FmodExtensions.ToFlagrumProject(mod);
            _modManager.Projects.Add(project.Identifier, project);
            _modManager.ModsState.Add(project.Identifier, new ModState());
            await project.Save(Path.Combine(_profile.ModFilesDirectory, project.Identifier.ToString(),
                "project.fproj"));
            result.Projects.Add(project);
        }

        return result;
    }
    
    /// <summary>
    /// Gets the line of text to display in the mod installation modal if a mod with the same
    /// GUID as the target mod is already installed.
    /// </summary>
    /// <param name="target">The mod to generate duplicate text for.</param>
    private string GetDuplicateText(FlagrumMod target)
    {
        // Name of the new mod in bold
        var result = $"<strong>{target.Metadata.Name}</strong>";
        
        // Append the name of the conflicting installed mod if it isn't the same as the new mod
        var installed = _modManager.Projects[target.Metadata.Guid];
        if (!installed.Name.Equals(target.Metadata.Name, StringComparison.OrdinalIgnoreCase))
        {
            result += $" (installed as {installed.Name})";
        }

        // Append line break
        return result + "<br/>";
    }
}