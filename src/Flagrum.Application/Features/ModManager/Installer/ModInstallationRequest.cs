using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Flagrum.Application.Features.ModManager.Mod;
using Microsoft.Extensions.Localization;

namespace Flagrum.Application.Features.ModManager.Installer;

public class ModInstallationRequest
{
    public string FilePath { get; set; }
    public Func<List<FlagrumMod>, Task<IEnumerable<FlagrumMod>>> GetModPackSelection { get; set; }
    public Func<Dictionary<string, List<string>>, Task> HandleLegacyConflicts { get; set; }
    public IStringLocalizer<Index> Localizer { get; set; }
}