using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Flagrum.Abstractions.ModManager;
using Flagrum.Abstractions.ModManager.Instructions;
using Flagrum.Abstractions.ModManager.Project;
using Flagrum.Core.Utilities;
using Flagrum.Application.Features.ModManager.Instructions;
using Flagrum.Application.Features.ModManager.Instructions.Abstractions;
using Flagrum.Application.Features.ModManager.Modals;
using Flagrum.Application.Features.ModManager.Project;
using Flagrum.Application.Utilities;
using Microsoft.AspNetCore.Components;

namespace Flagrum.Application.Features.ModManager.Editor;

public partial class Editor
{
    private const string AllFilesFilter = "All Files|*.*";

    private readonly Timer _timer = new(300);

    private string _filterQuery;

    [Parameter] public string ModId { get; set; }

    private bool IsLoading { get; set; }
    private string LoadingText { get; set; }

    private UriSelectModal Modal { get; set; }
    private bool HasChanged { get; set; }

    public string FilterQuery
    {
        get => _filterQuery;
        set
        {
            _timer.Stop();
            _filterQuery = value;
            _timer.Start();
        }
    }

    public List<IModEditorInstructionGroup> InstructionGroups { get; set; } = [];

    /// <inheritdoc />
    public void OpenReplaceModal() => Modal.Open(OnReplacementSelected);

    /// <inheritdoc />
    public void OpenRemoveModal() => Modal.Open(OnRemovalSelected);

    /// <inheritdoc />
    public async Task OnModChangedAsync()
    {
        HasChanged = true;
        await InvokeAsync(StateHasChanged);
    }

    /// <inheritdoc />
    public void OpenUriSelectModal(Func<string, Task> callback) => Modal.Open(callback);

    /// <inheritdoc />
    public Task CloseUriSelectModalAsync() => InvokeAsync(Modal.Close);

    /// <inheritdoc />
    public IFlagrumProjectArchive TryAddArchiveByAsset(string uri)
    {
        // Get the archive associated with the target asset
        var relativePath = FileIndex.GetArchiveRelativePathByUri(uri);
        var archive = Mod.Archives.FirstOrDefault(e =>
            e.RelativePath.Equals(relativePath, StringComparison.OrdinalIgnoreCase));

        // Ensure the archive is added to the project if it doesn't already exist
        if (archive == null)
        {
            archive = new FlagrumProjectArchive {RelativePath = relativePath};

            Mod.Archives.Add(archive);
            InstructionGroups.Add(new ModEditorInstructionGroup
            {
                Text = relativePath,
                Archive = archive,
                Type = ModEditorInstructionGroupType.Archive,
                Instructions = archive.Instructions.Where(i => i.ShouldShowInBuildList)
            });
        }

        // Expand the UI for the archive
        InstructionGroups.First(g => g.Archive == archive).IsExpanded = true;

        return archive;
    }

    protected override void OnInitialized()
    {
        _timer.Elapsed += (_, _) =>
        {
            InvokeAsync(StateHasChanged);
            _timer.Stop();
        };

        Mod = Provider.CloneFlagrumProject(ModManager.Projects[new Guid(ModId)]);
        InstructionGroups = ModEditorInstructionGroup.CreateGroups(Mod.Archives, Mod.Instructions);
    }

    /// <summary>
    /// Checks if an instruction already exists in this mod to alter the file at the given URI.
    /// </summary>
    /// <param name="uri">The URI of the asset to modify.</param>
    /// <returns><c>true</c> if the file is a duplicate, otherwise <c>false</c>.</returns>
    private bool CheckDuplicateFile(string uri)
    {
        if (Mod.Archives.Any(e => e.Instructions
                .Any(r => r.Uri.Equals(uri, StringComparison.OrdinalIgnoreCase))))
        {
            Alert.Open("Warning", "Invalid Action", "You cannot alter the same file twice.", null);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Callback for when a file to replace is selected from <see cref="UriSelectModal" />.
    /// </summary>
    /// <param name="uri">URI of the asset to replace.</param>
    private async Task OnReplacementSelected(string uri)
    {
        if (!CheckDuplicateFile(uri))
        {
            await PlatformService.OpenFileDialogAsync(AllFilesFilter, async file =>
            {
                // Create the replacement instruction
                var archive = TryAddArchiveByAsset(uri);
                var instruction = InstructionFactory.Create<ReplacePackedFileBuildInstruction>();
                instruction.Uri = uri;
                instruction.FilePath = file;
                archive.Instructions.Add(instruction);

                // Update state and close the modal
                HasChanged = true;
                await InvokeAsync(() =>
                {
                    Modal.Close();
                    StateHasChanged();
                });
            });
        }
    }

    /// <summary>
    /// Callback for when a file to delete is selected from <see cref="UriSelectModal" />.
    /// </summary>
    /// <param name="uri">URI of the asset to delete.</param>
    private async Task OnRemovalSelected(string uri)
    {
        if (!CheckDuplicateFile(uri))
        {
            // Create the removal instruction
            var archive = TryAddArchiveByAsset(uri);
            var instruction = InstructionFactory.Create<RemovePackedFileBuildInstruction>();
            instruction.Uri = uri;
            archive.Instructions.Add(instruction);

            // Update state and close the modal
            HasChanged = true;
            await InvokeAsync(() =>
            {
                Modal.Close();
                StateHasChanged();
            });
        }
    }

    public void RemoveBuildInstruction(IModEditorInstructionGroup group,
        IModBuildInstruction instruction)
    {
        if (group.Type == ModEditorInstructionGroupType.Archive)
        {
            group.Archive.Instructions.Remove((PackedBuildInstruction)instruction);

            if (!group.Instructions.Any())
            {
                Mod.Archives.Remove(group.Archive);
                InstructionGroups.Remove(group);
            }
        }
        else
        {
            Mod.Instructions.Remove(instruction);
        }

        HasChanged = true;
        StateHasChanged();
    }

    public void SetHasChanged()
    {
        HasChanged = true;
    }

    private void Save()
    {
        if (Profile.IsGameRunning())
        {
            Alert.Open("Error", "The Game is Running",
                "Flagrum cannot apply mods while the game is running. Please save and close down the game, then try again.",
                null);
            return;
        }

        LoadingText = "Saving Mod";
        IsLoading = true;
        StateHasChanged();

        if (HasChanged || Mod.HaveFilesChanged)
        {
            var deadFiles = Mod.GetDeadFiles();

            if (deadFiles.Any())
            {
                var message = deadFiles.Aggregate(
                    "The mod could not be built as the following files were missing:<ul class='mt-4'>",
                    (current, file) => current + $"<li>{file}</li>");
                Alert.Open("Error", "Missing Files", message + "</ul>", null, 400, 300, true);
                IsLoading = false;
                StateHasChanged();
            }
            else if (!CheckReferencesValid(Mod))
            {
                IsLoading = false;
                StateHasChanged();
            }
            else
            {
                ThreadHelper.RunOnNewThread(async () =>
                {
                    await Task.Run(async () => await CheckConflicts(Mod,
                        async () =>
                        {
                            if (ModManager.ModsState.GetActive(Mod.Identifier))
                            {
                                var mod = ModManager.Projects[Mod.Identifier];
                                await ModManagerService.DisableMod(mod);
                            }

                            if (HasChanged)
                            {
                                await ModManagerService.SaveBuildList(Mod);
                            }

                            await ModManagerService.EnableMod(Mod);

                            Navigation.NavigateTo("/");
                        },
                        async () =>
                        {
                            if (ModManager.ModsState.GetActive(Mod.Identifier))
                            {
                                var mod = ModManager.Projects[Mod.Identifier];
                                await ModManagerService.DisableMod(mod);
                            }

                            if (HasChanged)
                            {
                                await ModManagerService.SaveBuildList(Mod);
                            }

                            Navigation.NavigateTo("/");
                        }));
                });
            }
        }
        else
        {
            if (ModManager.ModsState.GetActive(Mod.Identifier))
            {
                Navigation.NavigateTo("/");
            }
            else if (!CheckReferencesValid(Mod))
            {
                IsLoading = false;
                StateHasChanged();
            }
            else
            {
                ThreadHelper.RunOnNewThread(async () =>
                {
                    await Task.Run(async () => await CheckConflicts(Mod,
                        async () =>
                        {
                            await ModManagerService.EnableMod(Mod);
                            Navigation.NavigateTo("/");
                        },
                        () =>
                        {
                            Navigation.NavigateTo("/");
                            return Task.CompletedTask;
                        }));
                });
            }
        }
    }

    private void ClearCache()
    {
        ModManagerService.ClearCachedFilesForMod(Mod.Identifier);
        Alert.Open("Success", "Cache Cleared", "The cached build files for this mod have been cleared.", null, 300,
            200);
        StateHasChanged();
    }

    public void LaunchRelativePath(string relativePath)
    {
        var uri = Path.GetDirectoryName(Path.Combine(Profile.GameDataDirectory, relativePath))!;
        Process.Start(new ProcessStartInfo(uri) {UseShellExecute = true});
    }

    public void LaunchAbsolutePath(string path)
    {
        Process.Start(new ProcessStartInfo(path) {UseShellExecute = true});
    }
}