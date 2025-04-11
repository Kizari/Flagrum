using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Abstractions;
using Flagrum.Abstractions.AssetExplorer;
using Flagrum.Abstractions.ModManager.Instructions;
using Flagrum.Abstractions.ModManager.Project;
using Flagrum.Components.Modals;
using Flagrum.Core.Utilities;
using Flagrum.Application.Features.ModManager.Data;
using Flagrum.Application.Features.ModManager.Installer;
using Flagrum.Application.Features.ModManager.Instructions.Builders;
using Flagrum.Application.Features.ModManager.Launcher;
using Flagrum.Application.Features.ModManager.Modals;
using Flagrum.Application.Features.ModManager.Services;
using Flagrum.Application.Features.Shared;
using Microsoft.AspNetCore.Components;

namespace Flagrum.Application.Features.ModManager;

public sealed partial class Index : ModComponentBase
{
    private IFlagrumProject _contextMod;

    [Inject] private IConfiguration Configuration { get; set; }
    [Inject] private TextureConverter TextureConverter { get; set; }
    [Inject] private DataIndexBinaryDifferenceBuilder DifferenceBuilder { get; set; }
    [Inject] private IModBuildInstructionFactory InstructionFactory { get; set; }
    [Inject] private ModInstaller ModInstaller { get; set; }
    [Inject] private ModManagerServiceBase ModManager { get; set; }
    [Inject] private IFileIndex FileIndex { get; set; }

    public bool IsLoading { get; set; }
    public string LoadingText { get; set; }

    private int EnabledState { get; set; }
    private int Category { get; set; } = -1;
    private bool ShowActive => EnabledState == 0;
    private AutosizeModal ConflictsModal { get; set; }
    private AutosizeModal ReadmeModal { get; set; }
    private ModCardModal ModCardModal { get; set; }
    public ExportModal ExportModal { get; set; }
    private ModPackInstallModal ModPackInstallModal { get; set; }
    private MarkupString CurrentReadme { get; set; }
    private Dictionary<string, List<string>> LegacyConflicts { get; set; }
    private List<EarcConflictString> SelectedLegacyConflicts { get; set; }
    private TaskCompletionSource TaskCompletionSource { get; set; }
    private string DisplayText { get; set; }
    private string SubText { get; set; }
    private List<Guid> TimestampsToUpdate { get; } = new();

    private Func<IFlagrumProject, bool> Filter => m =>
        ModManager.ModsState.GetActive(m.Identifier) == ShowActive;

    /// <inheritdoc />
    public async Task LaunchGameAsync(bool isDebug)
    {
        var result = Launcher.TryLaunch(isDebug);
        if (result == GameLaunchResult.GameAlreadyRunning)
        {
            Alert.Open("Error", "Game Already Running",
                "Flagrum has detected that the game is already running " +
                "and cannot launch again until the game is closed.", null);
        }
        else if (result == GameLaunchResult.UnsupportedExecutable)
        {
            Alert.Open("Error", "Unsupported Game Version",
                "Flagrum does not recognize the game executable, " +
                "only the latest Steam release is supported.", null);
        }
        else if (result == GameLaunchResult.AccessDenied)
        {
            Alert.Open("Error", "Access Denied",
                "Flagrum was unable to launch FFXV due to insufficient permissions. " +
                "Please relaunch Flagrum as administrator and try again.", null);
        }

        await Task.Delay(5000); // Prevent spam launching
    }

    protected override async Task OnInitializedAsync()
    {
        FileIndex.OnIsRegeneratingChanged += _ => InvokeAsync(StateHasChanged);

        var enabledState = Configuration.Get<int>(StateKey.CurrentEarcEnabledState);
        EnabledState = enabledState > -1 ? enabledState : 0;

        var category = Configuration.Get<int>(StateKey.CurrentEarcCategory);
        Category = category;

        foreach (var file in Directory.EnumerateFiles($@"{IOHelper.GetWebRoot()}\EarcMods"))
        {
            var id = file.Split('\\').Last().Replace(".png", "");
            var guid = new Guid(id);
            if (!ModManager.Projects.ContainsKey(guid))
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
        DisplayText = Localizer["DisplayText"];
        SubText = Localizer["SubText"];
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            var fmodPath = PlatformService.GetFmodPath();
            if (fmodPath != null)
            {
                Prompt.Title = "Install Mod";
                Prompt.Heading = "Do you wish to install this mod?";
                Prompt.Subtext = fmodPath.Split('\\').Last();
                Prompt.OnYes = async () => await InstallMod(fmodPath);
                Prompt.Open();
                PlatformService.ClearFmodPath();
            }
        }
    }

    public void AddTimestampToUpdate(Guid modId)
    {
        TimestampsToUpdate.Add(modId);
    }

    public bool CheckTimestampUpdate(Guid modId)
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

    public Task ShowModCardModal() => ModCardModal.Open(_contextMod.Identifier);

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
        Configuration.Set(StateKey.CurrentEarcEnabledState, state);
    }

    private Task Install()
    {
        if (Profile.IsGameRunning())
        {
            Alert.Open("Error", "The Game is Running",
                "Flagrum cannot install mods while the game is running. Please save and close down the game, then try again.",
                null);
            return Task.CompletedTask;
        }

        return PlatformService.OpenFileDialogAsync("Flagrum Mod|*.fmod;*.zip", async path => await InstallMod(path));
    }

    private async Task InstallMod(string path)
    {
        await InvokeAsync(() => SetLoading(true, Localizer["InstallingMod"]));

        await Task.Run(async () =>
        {
            var result = await ModInstaller.Install(new ModInstallationRequest
            {
                FilePath = path,
                GetModPackSelection = async mods =>
                    await ModPackInstallModal.TryGetResult(mods, out var selectedMods)
                        ? selectedMods
                        : null,
                HandleLegacyConflicts = conflicts =>
                {
                    LegacyConflicts = conflicts;
                    SelectedLegacyConflicts = new string[conflicts.Count(c => c.Value.Count > 1)]
                        .Select(s => new EarcConflictString {Value = s}).ToList();
                    InvokeAsync(StateHasChanged);
                    TaskCompletionSource = new TaskCompletionSource();
                    InvokeAsync(ConflictsModal.Open);
                    return TaskCompletionSource.Task;
                },
                Localizer = Localizer
            });

            switch (result.Status)
            {
                case ModInstallationStatus.Error:
                    await InvokeAsync(() => Alert.Open(result.ErrorTitle, result.ErrorHeading, result.ErrorText, null));
                    return;
                case ModInstallationStatus.Success:
                    break;
                case ModInstallationStatus.Cancelled:
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            foreach (var project in result.Projects)
            {
                if (CheckReferencesValid(project.Identifier))
                {
                    await CheckConflicts(project, () => ModManagerService.EnableMod(project));
                }
            }
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

    private void OpenModSite()
    {
        var url = Profile.Current.Type == LuminousGame.FFXV
            ? "https://www.curseforge.com/final-fantasy-xv"
            : "https://modworkshop.net/game/forspoken";

        Process.Start(new ProcessStartInfo(url)
        {
            UseShellExecute = true
        });
    }

    public void SetContextMod(IFlagrumProject mod)
    {
        _contextMod = mod;
    }

    public IFlagrumProject GetContextMod() => _contextMod;

    private void OnStateHasChanged(object sender, EventArgs e)
    {
        _ = InvokeAsync(StateHasChanged);
    }
}