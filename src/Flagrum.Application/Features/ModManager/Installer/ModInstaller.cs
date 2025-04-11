using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Threading.Tasks;
using Flagrum.Generators;
using Flagrum.Application.Features.ModManager.Legacy;
using Flagrum.Application.Features.ModManager.Mod;
using Flagrum.Application.Utilities;
using Injectio.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Flagrum.Application.Features.ModManager.Installer;

[RegisterScoped]
public partial class ModInstaller
{
    [Inject] private readonly FlagrumModInstaller _flagrumModInstaller;
    [Inject] private readonly LegacyModInstaller _legacyModInstaller;
    [Inject] private readonly FlagrumZipModInstaller _flagrumZipModInstaller;
    
    public Task<ModInstallationResult> Install(ModInstallationRequest request)
    {
        if (request.FilePath.EndsWith(".fmod", StringComparison.OrdinalIgnoreCase))
        {
            return _flagrumModInstaller.Install(request);
        }
        
        if (request.FilePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            bool isFlagrumZip;
            using (var zip = ZipFile.OpenRead(request.FilePath))
            {
                isFlagrumZip = zip.GetEntry("flagrum.json") != null;
            }
            
            return isFlagrumZip
                ? _flagrumZipModInstaller.Install(request)
                : _legacyModInstaller.Install(request);
        }

        return Task.FromResult(new ModInstallationResult("Error", "Invalid File Format",
            "Flagrum can only install mods from .fmod files or .zip files."));
    }
}