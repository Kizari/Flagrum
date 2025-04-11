using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Abstractions;
using Flagrum.Abstractions.ModManager.Project;
using Flagrum.Components.Modals;
using Flagrum.Application.Features.ModManager.Instructions;
using Flagrum.Application.Features.ModManager.Services;
using Flagrum.Application.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Flagrum.Application.Features.ModManager;

public class ModComponentBase : ComponentBase
{
    public Dictionary<Guid, TaskCompletionSource<bool>> TaskCompletionSources { get; } = new();

    [Inject] protected IStringLocalizer<Index> Localizer { get; set; }

    [Inject] protected ModManagerServiceBase ModManagerService { get; set; }

    [Inject] protected IProfileService Profile { get; set; }

    [Inject] protected AppStateService AppState { get; set; }

    [Parameter] public IFlagrumProject Mod { get; set; }

    public PromptModal Prompt { get; set; }
    public AlertModal Alert { get; set; }

    public bool CheckReferencesValid(Guid modId)
    {
        var mod = ModManagerService.Projects[modId];
        return CheckReferencesValid(mod);
    }

    protected bool CheckReferencesValid(IFlagrumProject mod)
    {
        if (!mod.AreReferencesValid())
        {
            InvokeAsync(() => Alert.Open("Error",
                "Invalid References Detected",
                "This mod could not be enabled because one or more references in the build list are invalid.",
                null));
            return false;
        }

        return true;
    }

    public Task<Guid> CheckConflicts(Guid modId, Func<Task> onYes, Func<Task> onNo = null)
    {
        var mod = ModManagerService.Projects[modId];
        return CheckConflicts(mod, onYes, onNo);
    }

    protected async Task<Guid> CheckConflicts(IFlagrumProject mod, Func<Task> onYes, Func<Task> onNo = null)
    {
        var taskId = Guid.NewGuid();
        var taskCompletionSource = new TaskCompletionSource<bool>();
        TaskCompletionSources[taskId] = taskCompletionSource;

        var uris = mod.Archives
            .SelectMany(e => e.Instructions.Select(r => r.Uri))
            .ToList();

        var conflicts = ModManagerService.Projects.Values
            .Where(m => ModManagerService.ModsState.GetActive(m.Identifier) && m.Identifier != mod.Identifier)
            .Select(m => new
            {
                m.Identifier,
                m.Name,
                Mod = m,
                Conflicts = m.Archives.SelectMany(e => e.Instructions
                        .Where(f => f is not AddReferenceBuildInstruction && uris.Contains(f.Uri))
                        .Select(f => f.Uri))
                    .ToList()
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
            Prompt.YesText = "Yes";
            Prompt.NoText = "No";
            Prompt.OnYes = () => Task.Run(async () =>
            {
                foreach (var conflict in conflicts)
                {
                    ModManagerService.DisableMod(conflict.Mod);
                }

                await onYes();
                taskCompletionSource.SetResult(true);
            });

            Prompt.OnNo = async () =>
            {
                if (onNo != null)
                {
                    await onNo();
                }

                taskCompletionSource.SetResult(false);
            };

            await InvokeAsync(Prompt.Open);
        }
        else
        {
            await onYes();
            taskCompletionSource.SetResult(true);
        }

        return taskId;
    }
}