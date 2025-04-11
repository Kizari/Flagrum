using System.IO;
using System.Linq;
using Flagrum.Abstractions;
using Flagrum.Core.Utilities;
using Flagrum.Generators;
using Flagrum.Application.Features.Settings.Data;
using Flagrum.Application.Services;

namespace Flagrum.Migrations;

/// <summary>
/// Removes residual data from the old proxy DLL system, and from AppCenter which was also removed in this version.
/// </summary>
[SteppedDataMigration(5)]
public partial class RemoveProxyDllMigration
{
    [Inject] private readonly IConfiguration _configuration;
    [Inject] private readonly IProfileService _profile;
    
    [MigrationStep(0, "f6a4d64b-799a-4cde-a0b7-aca654052cf1",
        MigrationScope.Application, 
        MigrationStepMode.Retry)]
    private void RemoveResidualAppCenterData()
    {
        foreach (var directory in Directory.EnumerateDirectories(_profile.FlagrumDirectory)
                     .Where(d => d.StartsWith("Flagrum_Url_")))
        {
            Directory.Delete(directory, true);
        }
    }

    [MigrationStep(1, "f2467fdd-ca43-432d-be35-4f9e1f6691bf", 
        MigrationScope.Profile, 
        MigrationStepMode.Retry)]
    private void RemoveProxyAndHookConfig()
    {
        IOHelper.DeleteFileIfExists(Path.Combine(_profile.GameDirectory, "hid.dll"));
        IOHelper.DeleteFileIfExists(Path.Combine(_profile.GameDirectory, "hook.fhc"));
    }
}