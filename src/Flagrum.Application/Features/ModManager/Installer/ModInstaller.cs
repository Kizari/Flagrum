using System;
using System.IO.Compression;
using System.Threading.Tasks;
using Flagrum.Application.Features.ModManager.Legacy;
using Injectio.Attributes;

namespace Flagrum.Application.Features.ModManager.Installer;

[RegisterScoped<ModInstaller>]
public partial class ModInstaller(
    FlagrumModInstaller _flagrumModInstaller,
    LegacyModInstaller _legacyModInstaller,
    FlagrumZipModInstaller _flagrumZipModInstaller)
{
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