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

namespace Flagrum.Migrations;

/// <summary>
/// This migration disables all mods and re-enables them to apply fixes in the 1.5.11 mod manager
/// </summary>
[SteppedDataMigration(4)]
public partial class ResetAppliedModsMigration
{
    private const string Warning = "An unexpected error occurred while attempting to repair potentially broken " +
                                   "modded files for a fix introduced in 1.5.11. Please manually reset your " +
                                   "mods by clicking the 'Force Reset/Cleanup' button in Flagrum's settings.";
    
    [Inject] private readonly ModManagerServiceBase _modManager;
    [Inject] private readonly IProfileService _profile;
    
    [MigrationStep(0, "042c0a72-62ca-466d-91a9-5c8d5ba9b1f2", MigrationScope.Profile, 
        MigrationStepMode.Warn, Warning)]
    private async Task ResetAppliedMods()
    {
        if (_profile.Current.Type == LuminousGame.FFXV 
            && _profile.Current.LastSeenVersion < new Version(1, 5, 11))
        {
            SplashViewModel.Instance.SetLoadingText("Repairing broken applied mods");
            var enabledMods = _modManager.Reset();
            foreach (var project in enabledMods)
            {
                await _modManager.EnableMod(_modManager.Projects[project]);
            }
        }
    }
}