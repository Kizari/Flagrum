using System;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Web.Components.Modals;
using Flagrum.Web.Persistence;
using Flagrum.Web.Persistence.Entities.ModManager;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Flagrum.Web.Features.EarcMods;

public class ModComponentBase : ComponentBase
{
    protected TaskCompletionSource<bool> _taskCompletionSource;

    [Inject] protected FlagrumDbContext Context { get; set; }
    [Inject] protected IStringLocalizer<Index> Localizer { get; set; }
    [Parameter] public EarcMod Mod { get; set; }

    public PromptModal Prompt { get; set; }

    protected Task CheckConflicts(int modId, Func<Task> onYes, Func<Task> onNo = null)
    {
        var mod = Context.EarcMods
            .Include(m => m.Earcs)
            .ThenInclude(e => e.Files)
            .Include(m => m.LooseFiles)
            .Where(m => m.Id == modId)
            .AsNoTracking()
            .ToList()
            .FirstOrDefault()!;

        return CheckConflicts(mod, onYes, onNo);
    }

    protected async Task CheckConflicts(EarcMod mod, Func<Task> onYes, Func<Task> onNo = null)
    {
        _taskCompletionSource = new TaskCompletionSource<bool>();

        var uris = mod.Earcs
            .SelectMany(e => e.Files.Select(r => r.Uri))
            .ToList();

        var conflicts = Context.EarcMods
            .Where(m => m.IsActive && m.Id != mod.Id)
            .Select(m => new
            {
                m.Id,
                m.Name,
                Conflicts = m.Earcs.SelectMany(e => e.Files
                    .Where(f => f.Type != EarcFileChangeType.AddReference && uris.Contains(f.Uri))
                    .Select(f => f.Uri))
            })
            .Where(m => m.Conflicts.Any())
            .ToList();

        if (conflicts.Any())
        {
            var message = conflicts.Aggregate(Localizer["Conflicts"].Value,
                (previous, m) =>
                    previous + $"<br/><br/><strong>{m.Name}</strong><br/>{string.Join("<br/>", m.Conflicts)}");
            Prompt.Title = Localizer["Warning"];
            Prompt.Heading = Localizer["Conflicts Detected"];
            Prompt.Subtext = message + "<br/><br/>" + Localizer["DisableNow"];
            Prompt.OnYes = () => Task.Run(async () =>
            {
                foreach (var mod in conflicts)
                {
                    var match = Context.EarcMods
                        .Include(m => m.Earcs)
                        .ThenInclude(e => e.Files)
                        .Include(m => m.LooseFiles)
                        .Where(m => m.Id == mod.Id)
                        .AsNoTracking()
                        .ToList()
                        .FirstOrDefault()!;

                    await match.Disable(Context);
                }

                await onYes();
                _taskCompletionSource.SetResult(true);
            });

            Prompt.OnNo = async () =>
            {
                if (onNo != null)
                {
                    await onNo();
                }

                _taskCompletionSource.SetResult(false);
            };

            await InvokeAsync(Prompt.Open);
        }
        else
        {
            await onYes();
            _taskCompletionSource.SetResult(true);
        }
    }
}