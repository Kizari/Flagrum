using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using BlazorContextMenu;
using Flagrum.Core.Utilities;
using Flagrum.Core.Utilities.Exceptions;
using Flagrum.Web.Components.Modals;
using Flagrum.Web.Features.EarcMods.Data;
using Flagrum.Web.Features.EarcMods.Modals;
using Flagrum.Web.Persistence.Entities;
using Flagrum.Web.Persistence.Entities.ModManager;
using Flagrum.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Flagrum.Web.Features.EarcMods;

public partial class Index : ModComponentBase
{
    private Timer _timer;

    private bool IsLoading { get; set; }
    private string LoadingText { get; set; }

    private int EnabledState { get; set; }
    private int Category { get; set; } = -1;
    private bool ShowActive => EnabledState == 0;
    private AlertModal Alert { get; set; }
    private AutosizeModal ConflictsModal { get; set; }
    private AutosizeModal ReadmeModal { get; set; }
    private ModCardModal ModCardModal { get; set; }
    private ContextMenu ContextMenu { get; set; }
    private MarkupString CurrentReadme { get; set; }
    private Dictionary<string, List<string>> LegacyConflicts { get; set; }
    private List<EarcConflictString> SelectedLegacyConflicts { get; set; }
    private TaskCompletionSource TaskCompletionSource { get; set; }
    private string DisplayText { get; set; }
    private string SubText { get; set; }
    private List<string> TimestampsToUpdate { get; } = new();

    private Expression<Func<EarcMod, bool>> Filter => m =>
        (Category == -1 || m.Category == (ModCategory)Category) // Filter by selected category if applicable
        && m.IsActive == ShowActive                             // Filter by active/disabled tab
        && m.PrerequisiteId == null;                          // Don't show add-on mods

    protected override async Task OnInitializedAsync()
    {
        if (AppState.RootGameViewNode == null)
        {
            _timer = new Timer(1000);
            _timer.Elapsed += (_, _) =>
            {
                if (AppState.RootGameViewNode != null)
                {
                    _timer.Stop();
                    InvokeAsync(StateHasChanged);
                }
            };
            _timer.Start();
        }

        var enabledState = Context.GetInt(StateKey.CurrentEarcEnabledState);
        EnabledState = enabledState > -1 ? enabledState : 0;

        var category = Context.GetInt(StateKey.CurrentEarcCategory);
        Category = category;

        foreach (var file in Directory.EnumerateFiles($@"{IOHelper.GetWebRoot()}\EarcMods"))
        {
            var id = file.Split('\\').Last().Replace(".png", "");
            // TODO: Update to handle new IDs
            // if (int.TryParse(id, out var idAsInt) && !Context.EarcMods.Any(m => m.Id == idAsInt))
            // {
            //     try
            //     {
            //         var thumbnail = $@"{IOHelper.GetWebRoot()}\EarcMods\{id}.png";
            //         File.Delete(thumbnail);
            //     }
            //     catch
            //     {
            //         // Ignore, try again next time
            //     }
            // }
        }

        // Jank delay to stop this showing on every launch
        await Task.Delay(500);
        DisplayText = Localizer["DisplayText"];
        SubText = Localizer["SubText"];
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            var fmodPath = WpfService.GetFmodPath();
            if (fmodPath != null)
            {
                Prompt.Title = "Install Mod";
                Prompt.Heading = "Do you wish to install this mod?";
                Prompt.Subtext = fmodPath.Split('\\').Last();
                Prompt.OnYes = async () => await InstallMod(fmodPath);
                Prompt.Open();
                WpfService.ClearFmodPath();
            }
        }
    }

    public void AddTimestampToUpdate(string modId)
    {
        TimestampsToUpdate.Add(modId);
    }

    public bool CheckTimestampUpdate(string modId)
    {
        if (TimestampsToUpdate.Contains(modId))
        {
            TimestampsToUpdate.Remove(modId);
            return true;
        }

        return false;
    }

    public void CallStateHasChanged()
    {
        StateHasChanged();
    }

    public Task ShowModCardModal(string modId)
    {
        return ModCardModal.Open(modId);
    }

    public void SetLoading(bool isLoading, string message = null)
    {
        IsLoading = isLoading;
        LoadingText = Localizer[message ?? ""];
        StateHasChanged();
    }

    private void SetEnabledState(int state)
    {
        EnabledState = state;
        StateHasChanged();
        Context.SetInt(StateKey.CurrentEarcEnabledState, state);
    }

    private void SetCategory(int category)
    {
        Category = category;
        StateHasChanged();
        Context.SetInt(StateKey.CurrentEarcCategory, category);
    }

    private async Task Install()
    {
        if (Context.Settings.IsGameRunning())
        {
            Alert.Open("Error", "FFXV is Running",
                "Flagrum cannot install mods while the game is running. Please save and close down FFXV, then try again.",
                null);
            return;
        }

        await WpfService.OpenFileDialogAsync("Flagrum Mod|*.fmod;*.zip", async path => await InstallMod(path));
    }

    private async Task InstallMod(string path)
    {
        await InvokeAsync(() => SetLoading(true, Localizer["InstallingMod"]));

        await Task.Run(async () =>
        {
            EarcMod earcMod = null;

            if (path.EndsWith(".fmod", StringComparison.OrdinalIgnoreCase))
            {
                var id = Context.IndexCounts
                    .FromSqlRaw("SELECT * FROM sqlite_sequence WHERE name = 'EarcMods'")
                    .First().seq + 1;

                var fmod = new Fmod();

                var directory = $@"{Settings.FlagrumDirectory}\earc\{id}";
                IOHelper.EnsureDirectoryExists(directory);

                try
                {
                    fmod.Read(path, directory, $@"{IOHelper.GetWebRoot()}\EarcMods\{id}.png");
                }
                catch (FileFormatException e)
                {
                    await InvokeAsync(() =>
                        Alert.Open("Error", "Invalid FMOD", "The given file is not a valid FMOD.", null));
                    return;
                }
                catch (FormatVersionException e)
                {
                    await InvokeAsync(() => Alert.Open("Error", "Please Upgrade Flagrum",
                        "The FMOD supplied is newer than this version of Flagrum can handle. Please upgrade to the latest version of Flagrum and try again.",
                        null));
                    return;
                }

                earcMod = Fmod.ToEarcMod(fmod, directory);
                await Context.AddAsync(earcMod);
                await Context.SaveChangesAsync();
                Context.ChangeTracker.Clear();
            }
            else
            {
                using var zip = ZipFile.OpenRead(path);
                var jsonEntry = zip.GetEntry("flagrum.json")!;

                if (jsonEntry == null)
                {
                    zip.Dispose();
                    var result = await EarcMod.ConvertLegacyZip(path, Context, Logger, conflicts =>
                    {
                        LegacyConflicts = conflicts;
                        SelectedLegacyConflicts = new string[conflicts.Count(c => c.Value.Count > 1)]
                            .Select(s => new EarcConflictString {Value = s}).ToList();
                        InvokeAsync(StateHasChanged);
                        TaskCompletionSource = new TaskCompletionSource();
                        InvokeAsync(ConflictsModal.Open);
                        return TaskCompletionSource.Task;
                    });

                    switch (result.Status)
                    {
                        case EarcLegacyConversionStatus.NoEarcs:
                            await InvokeAsync(() => Alert.Open(Localizer["Error"], Localizer["InvalidModPack"],
                                Localizer["InvalidModPackDescription"],
                                null));
                            return;
                        case EarcLegacyConversionStatus.NewFiles:
                            await InvokeAsync(() => Alert.Open(Localizer["Error"], Localizer["IncompatibleModPack"],
                                Localizer["IncompatibleNewFilesDescription"],
                                null, 500, 400));
                            return;
                        case EarcLegacyConversionStatus.EarcNotFound:
                            await InvokeAsync(() => Alert.Open(Localizer["Error"], Localizer["IncompatibleModPack"],
                                Localizer["IncompatibleNewEarcDescription"],
                                null, 500, 400));
                            return;
                        case EarcLegacyConversionStatus.NeedsDisabling:
                            var modsToDisable = result.ModsToDisable
                                .Aggregate("", (previous, kvp) => $"{previous}<strong>{kvp.Value}</strong>");
                            await InvokeAsync(() => Alert.Open(Localizer["Error"], Localizer["ErrorDataCompare"],
                                Localizer["ErrorDataCompareDescription"] +
                                modsToDisable,
                                null, 500, 400));
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
                    var converter = new TextureConverter();
                    var newThumbnail = converter.ProcessEarcModThumbnail(thumbnailMemoryStream.ToArray());
                    await File.WriteAllBytesAsync($@"{IOHelper.GetWebRoot()}\EarcMods\{earcMod.Id}.png",
                        newThumbnail);

                    var directory = $@"{Settings.EarcModsDirectory}\{earcMod.Id}";
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    if (metadata.Version == 0)
                    {
                        foreach (var (earcPath, replacements) in metadata.Replacements)
                        {
                            var earc = new EarcModEarc {EarcRelativePath = earcPath};
                            foreach (var replacement in replacements)
                            {
                                var hash = Cryptography.HashFileUri64(replacement).ToString();
                                var matchEntry = zip.Entries.FirstOrDefault(e => e.Name.Contains(hash))!;
                                var filePath =
                                    $@"{Settings.FlagrumDirectory}\earc\{earcMod.Id}\{matchEntry.Name}";

                                await using var entryStream = matchEntry.Open();
                                await using var entryMemoryStream = new MemoryStream();
                                await entryStream.CopyToAsync(entryMemoryStream);
                                await File.WriteAllBytesAsync(filePath, entryMemoryStream.ToArray());

                                earc.Files.Add(new EarcModFile
                                {
                                    Uri = replacement,
                                    ReplacementFilePath = filePath,
                                    FileLastModified = File.GetLastWriteTime(filePath).Ticks
                                });
                            }

                            earcMod.Earcs.Add(earc);
                        }
                    }
                    else
                    {
                        foreach (var (earcPath, changes) in metadata.Changes)
                        {
                            var earc = new EarcModEarc {EarcRelativePath = earcPath};
                            foreach (var change in changes)
                            {
                                string filePath = null;

                                if (change.Type is EarcFileChangeType.Replace or EarcFileChangeType.Add)
                                {
                                    var hash = Cryptography.HashFileUri64(change.Uri).ToString();
                                    var matchEntry = zip.Entries.FirstOrDefault(e => e.Name.Contains(hash))!;
                                    filePath =
                                        $@"{Settings.FlagrumDirectory}\earc\{earcMod.Id}\{matchEntry.Name}";

                                    await using var entryStream = matchEntry.Open();
                                    await using var entryMemoryStream = new MemoryStream();
                                    await entryStream.CopyToAsync(entryMemoryStream);
                                    await File.WriteAllBytesAsync(filePath, entryMemoryStream.ToArray());
                                }

                                earc.Files.Add(new EarcModFile
                                {
                                    Uri = change.Uri,
                                    ReplacementFilePath = filePath,
                                    Type = change.Type,
                                    FileLastModified = filePath == null ? 0 : File.GetLastWriteTime(filePath).Ticks
                                });

                                earcMod.Earcs.Add(earc);
                            }
                        }
                    }

                    await Context.SaveChangesAsync();
                    Context.ChangeTracker.Clear();
                }
            }

            await CheckConflicts(earcMod, async () => { await earcMod!.Enable(Context, Logger); });
        });

        await InvokeAsync(() => SetLoading(false));
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

    public void ShowReadme(string readme)
    {
        CurrentReadme = (MarkupString)readme;
        ReadmeModal.Open();
    }
}