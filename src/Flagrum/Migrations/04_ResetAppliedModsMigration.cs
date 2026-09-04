using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Abstractions;
using Flagrum.Core.Utilities.Types;
using Flagrum.Generators;
using Flagrum.Application.Features.ModManager.Project;
using Flagrum.Application.Features.ModManager.Services;
using Flagrum.Application.Services;
using Flagrum.Host;

namespace Flagrum.Migrations;

/// <summary>
/// This migration disables all mods and re-enables them to apply fixes in the 1.5.11 mod manager
/// </summary>
[SteppedDataMigration(4)]
public partial class ResetAppliedModsMigration(
    ModManagerServiceBase modManager,
    IProfileService profile,
    IApplication application)
{
    private const string Warning = "An unexpected error occurred while attempting to repair potentially broken " +
                                   "modded files for a fix introduced in 1.5.11. Please manually reset your " +
                                   "mods by clicking the 'Force Reset/Cleanup' button in Flagrum's settings.";
    
    [MigrationStep(0, "042c0a72-62ca-466d-91a9-5c8d5ba9b1f2", MigrationScope.Profile, 
        MigrationStepMode.Warn, Warning)]
    private async Task ResetAppliedMods()
    {
        if (profile.Current.Type == LuminousGame.FFXV 
            && profile.Current.LastSeenVersion < new Version(1, 5, 11))
        {
            application.SetSplashText("Repairing broken applied mods");
            var enabledMods = modManager.Reset();
            foreach (var project in enabledMods)
            {
                await modManager.EnableMod(modManager.Projects[project]);
            }
        }
    }
}