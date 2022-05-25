using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Flagrum.Core.Utilities;
using Flagrum.Web.Components.Modals;
using Flagrum.Web.Features.EarcMods.Data;
using Flagrum.Web.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Flagrum.Web.Features.EarcMods;

public partial class Index
{
    private Timer _timer;

    private bool IsLoading { get; set; }
    private string LoadingText { get; set; }

    private int Category { get; set; }
    private bool ShowActive => Category == 0;
    public PromptModal Prompt { get; private set; }
    private AlertModal Alert { get; set; }
    private AutosizeModal ConflictsModal { get; set; }
    private Dictionary<string, List<string>> LegacyConflicts { get; set; }
    private List<EarcConflictString> SelectedLegacyConflicts { get; set; }
    private TaskCompletionSource TaskCompletionSource { get; set; }
    private string DisplayText { get; set; }
    private string SubText { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (AppState.Node == null)
        {
            _timer = new Timer(1000);
            _timer.Elapsed += (_, _) =>
            {
                if (AppState.Node != null)
                {
                    _timer.Stop();
                    InvokeAsync(StateHasChanged);
                }
            };
            _timer.Start();
        }

        var category = Context.GetInt(StateKey.CurrentEarcCategory);
        Category = category > -1 ? category : 0;

        foreach (var file in Directory.EnumerateFiles($@"{IOHelper.GetWebRoot()}\EarcMods"))
        {
            var id = file.Split('\\').Last().Replace(".png", "");
            if (int.TryParse(id, out var idAsInt) && !Context.EarcMods.Any(m => m.Id == idAsInt))
            {
                try
                {
                    var thumbnail = $@"{IOHelper.GetWebRoot()}\EarcMods\{id}.png";
                    File.Delete(thumbnail);
                }
                catch
                {
                    // Ignore, try again next time
                }
            }
        }

        // Jank delay to stop this showing on every launch
        await Task.Delay(500);
        DisplayText = "Mod Manager Unavailable While Indexing Files";
        SubText = "Please wait until file indexing has completed";
    }

    public void SetLoading(bool isLoading, string message = null)
    {
        IsLoading = isLoading;
        LoadingText = message;
        StateHasChanged();
    }

    private void SetCategory(int category)
    {
        Category = category;
        StateHasChanged();
        Context.SetInt(StateKey.CurrentEarcCategory, category);
    }

    private async Task InstallFromZip()
    {
        await WpfService.OpenFileDialogAsync("ZIP Archive|*.zip", async path =>
        {
            await InvokeAsync(() => SetLoading(true, "Installing Mod"));

            await Task.Run(async () =>
            {
                try
                {
                    using var zip = ZipFile.OpenRead(path);
                    var jsonEntry = zip.GetEntry("flagrum.json")!;
                    EarcMod earcMod = null;

                    if (jsonEntry == null)
                    {
                        zip.Dispose();
                        var result = await EarcMod.ConvertLegacyZip(path, Context, conflicts =>
                        {
                            LegacyConflicts = conflicts;
                            SelectedLegacyConflicts = new string[conflicts.Count]
                                .Select(s => new EarcConflictString {Value = s}).ToList();
                            InvokeAsync(StateHasChanged);
                            TaskCompletionSource = new TaskCompletionSource();
                            InvokeAsync(ConflictsModal.Open);
                            return TaskCompletionSource.Task;
                        });

                        switch (result.Status)
                        {
                            case EarcLegacyConversionStatus.NoEarcs:
                                await InvokeAsync(() => Alert.Open("Error", "Invalid Mod Pack",
                                    "Could not find compatible mod data in this ZIP. Please refer to the mod author's original installation instructions.",
                                    null));
                                return;
                            case EarcLegacyConversionStatus.AlteredStructure:
                                await InvokeAsync(() => Alert.Open("Error", "Incompatible Mod Pack",
                                    "The ZIP contains valid mod data, but is not compatible with Flagrum's legacy mod converter at this time. Please refer to the mod author's original installation instructions.",
                                    null));
                                return;
                            case EarcLegacyConversionStatus.Success:
                                earcMod = result.Mod;
                                Context.ChangeTracker.Clear();
                                break;
                        }

                        Context.ChangeTracker.Clear();
                    }
                    else
                    {
                        await using var jsonStream = jsonEntry.Open();
                        await using var jsonMemoryStream = new MemoryStream();
                        await jsonStream.CopyToAsync(jsonMemoryStream);
                        var json = Encoding.UTF8.GetString(jsonMemoryStream.ToArray());
                        var metadata = JsonConvert.DeserializeObject<EarcModMetadata>(json)!;

                        earcMod = new EarcMod
                        {
                            Name = metadata.Name,
                            Author = metadata.Author,
                            Description = metadata.Description,
                            IsActive = false
                        };

                        await Context.EarcMods.AddAsync(earcMod);
                        await Context.SaveChangesAsync();

                        var thumbnailEntry = zip.GetEntry("flagrum.png")!;
                        await using var thumbnailStream = thumbnailEntry.Open();
                        await using var thumbnailMemoryStream = new MemoryStream();
                        await thumbnailStream.CopyToAsync(thumbnailMemoryStream);
                        await File.WriteAllBytesAsync($@"{IOHelper.GetWebRoot()}\EarcMods\{earcMod.Id}.png",
                            thumbnailMemoryStream.ToArray());

                        var directory = $@"{Settings.EarcModsDirectory}\{earcMod.Id}";
                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        foreach (var (earcPath, replacements) in metadata.Replacements)
                        {
                            var earc = new EarcModEarc {EarcRelativePath = earcPath};
                            foreach (var replacement in replacements)
                            {
                                var hash = Cryptography.HashFileUri64(replacement).ToString();
                                var matchEntry = zip.Entries.FirstOrDefault(e => e.Name.Contains(hash))!;
                                var filePath = $@"{Settings.FlagrumDirectory}\earc\{earcMod.Id}\{matchEntry.Name}";

                                await using var entryStream = matchEntry.Open();
                                await using var entryMemoryStream = new MemoryStream();
                                await entryStream.CopyToAsync(entryMemoryStream);
                                await File.WriteAllBytesAsync(filePath, entryMemoryStream.ToArray());

                                earc.Replacements.Add(new EarcModReplacement
                                {
                                    Uri = replacement,
                                    ReplacementFilePath = filePath
                                });
                            }

                            earcMod.Earcs.Add(earc);
                        }

                        await Context.SaveChangesAsync();
                        Context.ChangeTracker.Clear();
                    }

                    await CheckConflicts(earcMod, async () => { await earcMod!.Enable(Context, Logger); });
                }
                catch (Exception e)
                {
                    Logger.LogError("Failed to install mod from ZIP\r\n{Exception}\r\n{StackTrace}", e.Message,
                        e.StackTrace);
                }
            });

            await InvokeAsync(() => SetLoading(false));
        });
    }

    private async Task CheckConflicts(EarcMod mod, Func<Task> onContinue)
    {
        var uris = mod.Earcs
            .SelectMany(e => e.Replacements.Select(r => r.Uri))
            .ToList();

        var conflicts = Context.EarcMods
            .Where(m => m.IsActive
                        && m.Id != mod.Id
                        && m.Earcs.Any(e => e.Replacements
                            .Any(r => uris.Contains(r.Uri))))
            .Select(m => new {m.Id, m.Name})
            .ToList();

        if (conflicts.Any())
        {
            var message = conflicts.Aggregate("The following mod(s) conflict with this one:<br/><br/>",
                (previous, mod) => previous + $"<strong>{mod.Name}</strong><br/>");
            Prompt.Title = "Warning";
            Prompt.Heading = "Conflicts Detected";
            Prompt.Subtext = message + "<br/>Would you like to disable the above mod(s) now?";
            Prompt.OnYes = async () =>
            {
                foreach (var mod in conflicts)
                {
                    var match = Context.EarcMods
                        .Include(m => m.Earcs)
                        .ThenInclude(e => e.Replacements)
                        .Where(m => m.Id == mod.Id)
                        .AsNoTracking()
                        .ToList()
                        .FirstOrDefault()!;

                    await match.Disable(Context);
                }

                await onContinue();
            };

            await InvokeAsync(Prompt.Open);
        }
        else
        {
            await onContinue();
        }
    }

    private void ConfirmLegacyConflicts()
    {
        var i = 0;

        foreach (var (_, options) in LegacyConflicts.Where(c => c.Value.Count > 1))
        {
            options.Clear();
            options.Add(SelectedLegacyConflicts[i].Value);
            i++;
        }

        ConflictsModal.Close();
        TaskCompletionSource.SetResult();
    }

    private void CancelLegacyConflicts()
    {
        ConflictsModal.Close();
        TaskCompletionSource.SetCanceled();
    }
}