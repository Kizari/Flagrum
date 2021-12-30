using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flagrum.Core.Archive;
using Flagrum.Core.Utilities;
using Flagrum.Web.Features.ModLibrary.Data;
using Flagrum.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Flagrum.Web.Features.ModLibrary.Editor;

public partial class Index : ComponentBase
{
    [Inject] private ILogger<Index> Logger { get; set; }
    [Inject] private AppStateService AppState { get; set; }
    [Inject] private BinmodTypeHelper BinmodTypeHelper { get; set; }
    [Inject] private SettingsService Settings { get; set; }
    [Inject] private NavigationManager Navigation { get; set; }
    [Inject] private IWpfService WpfService { get; set; }
    [Inject] private ModelReplacementPresets ReplacementPresets { get; set; }
    [Inject] private BinmodBuilder BinmodBuilder { get; set; }
    [Inject] private Modmeta Modmeta { get; set; }

    public Binmod Mod { get; set; }
    public int StatsTotal { get; set; }
    private bool IsNew { get; set; }
    private bool CanSave { get; set; }
    private string LoadingText { get; set; }
    private bool IsLoading { get; set; }
    private string ImageName { get; set; } = "current_preview";
    private string ThumbnailName { get; set; } = "current_thumbnail";
    private Dictionary<int, string> ModTypes { get; set; }
    private Dictionary<int, string> ModTargets { get; set; }
    private BuildContext BuildContext { get; set; }
    private int ModelCount { get; set; } = 1;
    private Dictionary<int, string> ModelNames { get; set; }
    private bool[] HasSelectedDataForModel { get; } = new bool[2];
    private string[] FmdFileNames { get; } = new string[2];

    protected override void OnInitialized()
    {
        BuildContext = new BuildContext(Logger, StateHasChanged);
        ModTypes = Enum.GetValues<BinmodType>().ToDictionary(t => (int)t, BinmodTypeHelper.GetDisplayName);

        Mod = AppState.ActiveMod?.Clone();

        if (Mod == null)
        {
            InitializeNewMod();
        }
        else
        {
            InitializeExistingMod();
        }

        ModTargets = BinmodTypeHelper.GetTargets(Mod.Type);
    }

    private void SetLoading(string text)
    {
        LoadingText = text;
        IsLoading = true;
        StateHasChanged();
    }

    private void InitializeNewMod()
    {
        IsNew = true;
        BuildContext.Flags |= BuildContextFlags.NeedsBuild | BuildContextFlags.PreviewImageChanged;

        var defaultPreviewPath = $"{IOHelper.GetExecutingDirectory()}\\Resources\\preview.png";
        var currentPreviewPath = $"{IOHelper.GetWebRoot()}\\images\\current_preview.png";
        File.Copy(defaultPreviewPath, currentPreviewPath, true);
        var previewBytes = File.ReadAllBytes(defaultPreviewPath);
        BuildContext.ProcessPreviewImage(previewBytes);

        Mod = new Binmod
        {
            Uuid = Guid.NewGuid().ToString(),
            IsApplyToGame = true
        };

        Mod.Path = $"{Settings.ModDirectory}\\{Mod.Uuid}.ffxvbinmod";
    }

    private void InitializeExistingMod()
    {
        ModelNames = BinmodTypeHelper.GetModelNames(Mod.Type, Mod.Target);
        ModelCount = BinmodTypeHelper.GetModelCount(Mod.Type, Mod.Target);

        if (ModelCount > 1)
        {
            Mod.Model1Name = ModelNames[0].ToSafeString();
            Mod.Model2Name = ModelNames[1].ToSafeString();
        }

        var previewBytes = Mod.GetPreviewPng();
        BuildContext.ProcessPreviewImage(previewBytes);
        File.WriteAllBytes($"{IOHelper.GetWebRoot()}\\images\\current_preview.png", previewBytes);

        if (Mod.Type == (int)BinmodType.StyleEdit)
        {
            if (Mod.HasThumbnailPng(out var thumbnailBytes))
            {
                BuildContext.ProcessThumbnailImage(thumbnailBytes);
                File.WriteAllBytes($"{IOHelper.GetWebRoot()}\\images\\current_thumbnail.png", thumbnailBytes);
            }
            else
            {
                try
                {
                    var converter = new TextureConverter();
                    var jpgBytes = converter.BtexToJpg(thumbnailBytes);
                    BuildContext.ProcessThumbnailImage(jpgBytes);
                    File.WriteAllBytes($"{IOHelper.GetWebRoot()}\\images\\current_thumbnail.png", jpgBytes);
                }
                catch
                {
                    // Must be a mod made with a previous version of Flagrum if the Btex conversion is failing
                    var defaultThumbnailPath = $"{IOHelper.GetExecutingDirectory()}\\Resources\\default.png";
                    var pngBytes = File.ReadAllBytes(defaultThumbnailPath);
                    BuildContext.ProcessThumbnailImage(pngBytes);
                    File.WriteAllBytes($"{IOHelper.GetWebRoot()}\\images\\current_thumbnail.png", pngBytes);
                }
            }
        }

        HasSelectedDataForModel[0] = true;
        HasSelectedDataForModel[1] = true;
        CanSave = true;
        StateHasChanged();
    }

    private void Cancel()
    {
        Navigation.NavigateTo("/");
    }

    private async Task SelectImage()
    {
        await WpfService.OpenFileDialogAsync(
            "Image Files|*.png;*.jpg;*.jpeg;*.tif;*.tiff;*.gif",
            path =>
            {
                BuildContext.ProcessPreviewImage(path, async () =>
                {
                    // This jank is required or the UI won't update the image if the value hasn't changed
                    ImageName = ImageName == "current_preview" ? "Current_Preview" : "current_preview";
                    await InvokeAsync(StateHasChanged);
                });
            });
    }

    private async Task SelectThumbnail()
    {
        await WpfService.OpenFileDialogAsync(
            "Image Files|*.png;*.jpg;*.jpeg;*.tif;*.tiff;*.gif",
            path =>
            {
                BuildContext.ProcessThumbnailImage(path, async () =>
                {
                    // This jank is required or the UI won't update the image if the value hasn't changed
                    ThumbnailName = ThumbnailName == "current_thumbnail" ? "Current_Thumbnail" : "current_thumbnail";
                    await InvokeAsync(StateHasChanged);
                });
            });
    }

    private void Delete()
    {
        AppState.Mods.Remove(AppState.ActiveMod);
        AppState.UpdateBinmodList();
        File.Delete(Mod.Path);

        Navigation.NavigateTo("/");
    }

    private void Save()
    {
        if (Mod.Type == (int)BinmodType.Character)
        {
            Mod.GameMenuTitle = null;
        }

        File.WriteAllBytes($"{IOHelper.GetWebRoot()}\\images\\{Mod.Uuid}.png", BuildContext.PreviewImage);

        if (BuildContext.Flags.HasFlag(BuildContextFlags.NeedsBuild))
        {
            SetLoading("Building Mod");
            BuildAsync();
        }
        else
        {
            SetLoading("Saving");
            SaveAsync();
        }
    }

    private async void SaveAsync()
    {
        var unpacker = new Unpacker(Mod.Path);
        var packer = unpacker.ToPacker();

        packer.UpdateFile("index.modmeta", Modmeta.Build(Mod));

        if (BuildContext.Flags.HasFlag(BuildContextFlags.PreviewImageChanged))
        {
            await BuildContext.WaitForPreviewData(Mod.Type == (int)BinmodType.StyleEdit);
            packer.UpdateFile("$preview.png", BuildContext.PreviewBtex);
            packer.UpdateFile("$preview.png.bin", BuildContext.PreviewImage);

            if (Mod.Type == (int)BinmodType.StyleEdit)
            {
                packer.UpdateFile("default.png", BuildContext.ThumbnailBtex);

                if (packer.HasFile($"data://mod/{Mod.ModDirectoryName}/default.png.bin"))
                {
                    packer.UpdateFile("default.png.bin", BuildContext.ThumbnailImage);
                }
                else
                {
                    packer.AddFile(BuildContext.ThumbnailImage, $"data://mod/{Mod.ModDirectoryName}/default.png.bin");
                }
            }
        }

        packer.WriteToFile(Mod.Path);

        AppState.ActiveMod.UpdateFrom(Mod);
        Navigation.NavigateTo("/");
    }

    private async void BuildAsync()
    {
        await BuildContext.WaitForBuildData(Mod.Type == (int)BinmodType.StyleEdit);

        Mod.ModelExtension = "gmdl";
        BinmodBuilder.Initialise(Mod, BuildContext.PreviewImage, BuildContext.PreviewBtex, BuildContext.ThumbnailImage,
            BuildContext.ThumbnailBtex);

        if (ModelCount > 1)
        {
            if (BuildContext.Fmds[0] == null)
            {
                BinmodBuilder.AddModelFromExisting(Mod, 0);
            }
            else
            {
                BinmodBuilder.AddFmd(0, BuildContext.Fmds[0].Gpubin, BuildContext.Fmds[0].Textures);
            }

            if (BuildContext.Fmds[1] == null)
            {
                BinmodBuilder.AddModelFromExisting(Mod, 1);
            }
            else
            {
                BinmodBuilder.AddFmd(1, BuildContext.Fmds[1].Gpubin, BuildContext.Fmds[1].Textures);
            }
        }
        else
        {
            BinmodBuilder.AddFmd(-1, BuildContext.Fmds[0].Gpubin, BuildContext.Fmds[0].Textures);
        }

        BinmodBuilder.WriteToFile(Mod.Path);

        if (AppState.ActiveMod == null)
        {
            AppState.Mods.Add(Mod);
        }
        else
        {
            AppState.ActiveMod.UpdateFrom(Mod);
        }

        AppState.UpdateBinmodList();
        Navigation.NavigateTo("/");
    }

    private void Upload()
    {
        Navigation.NavigateTo("/mod/upload");
    }

    private void ModTypeChanged(int newType)
    {
        ModTargets = BinmodTypeHelper.GetTargets(newType);
        Mod.Target = -1;
        StateHasChanged();
    }

    private void ModTargetChanged(int newTarget)
    {
        if (newTarget == -1)
        {
            return;
        }

        if (Mod.Type == (int)BinmodType.Character)
        {
            Mod.OriginalGmdls = ReplacementPresets.GetOriginalGmdls(newTarget).ToList();
        }

        ModelCount = BinmodTypeHelper.GetModelCount(Mod.Type, newTarget);
        ModelNames = BinmodTypeHelper.GetModelNames(Mod.Type, newTarget);

        if (ModelCount > 1)
        {
            Mod.Model1Name = ModelNames[0].ToSafeString();
            Mod.Model2Name = ModelNames[1].ToSafeString();
        }
    }

    private void VariantChanged(ChangeEventArgs e)
    {
        Mod.Gender = e.Value?.ToString();
    }

    private async Task SelectModel(int index)
    {
        await WpfService.OpenFileDialogAsync(
            "Flagrum Model Data (*.fmd)|*.fmd",
            path =>
            {
                FmdFileNames[index] = path.Split('\\', '/').Last();
                BuildContext.ProcessFmd(index, path);
                Mod.ModDirectoryName = Mod.Uuid;
                Mod.ModelName = path.Split('\\', '/').Last().Split('.')[0].ToSafeString();

                HasSelectedDataForModel[index] = true;

                if (ModelCount == 1)
                {
                    CanSave = true;
                }
                else
                {
                    CanSave = HasSelectedDataForModel[0] && HasSelectedDataForModel[1];
                }

                if (IsNew && CanSave && Mod.Type == (int)BinmodType.StyleEdit)
                {
                    var defaultThumbnailPath = $"{IOHelper.GetExecutingDirectory()}\\Resources\\default.png";
                    var currentThumbnailPath = $"{IOHelper.GetWebRoot()}\\images\\current_thumbnail.png";
                    File.Copy(defaultThumbnailPath, currentThumbnailPath, true);
                    var thumbnailBytes = File.ReadAllBytes(defaultThumbnailPath);
                    BuildContext.ProcessThumbnailImage(thumbnailBytes);

                    // This jank is required or the UI won't update the image if the value hasn't changed
                    ThumbnailName = ThumbnailName == "current_thumbnail" ? "Current_Thumbnail" : "current_thumbnail";
                }

                InvokeAsync(StateHasChanged);
            });
    }
}