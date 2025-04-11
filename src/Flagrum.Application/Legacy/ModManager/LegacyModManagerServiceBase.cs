using System.Linq;
using System.Threading.Tasks;
using Flagrum.Abstractions;
using Flagrum.Core.Archive;
using Flagrum.Application.Persistence;
using Flagrum.Application.Persistence.Entities.ModManager;
using Flagrum.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace Flagrum.Application.Features.ModManager.Services;

public abstract class LegacyModManagerServiceBase(
    IProfileService profile,
    FlagrumDbContext context)
{
    protected readonly FlagrumDbContext _context = context;
    protected readonly IProfileService _profile = profile;

    public abstract Task BuildAndApplyMod(EarcMod mod, EbonyArchiveManager archiveManager);
    public abstract Task RevertMod(EarcMod mod);

    public void DisableMod(int modId)
    {
        // Need to pull this again with includes as they won't be there in the Mod Manager
        var mod = _context.EarcMods
            .Include(m => m.Earcs)
            .ThenInclude(e => e.Files)
            .Include(m => m.LooseFiles)
            .Where(e => e.Id == modId)
            .AsNoTracking()
            .ToList()
            .First();

        RevertMod(mod);

        // Now that the mod has been reverted, it can be marked as disabled
        mod.IsActive = false;
        _context.Update(mod);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
    }
}