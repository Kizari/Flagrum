using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using Flagrum.Core.Archive;
using Flagrum.Core.Utilities.Types;
using Microsoft.Extensions.Logging;
using Steamworks;

namespace Flagrum.Web.Services;

public class WorkshopItemDetails
{
    public ModVisibility Visibility { get; set; }
    public string Description { get; set; } = "";
    public string ChangeNotes { get; set; } = "";
    public List<string> Tags { get; set; } = new();
    public byte[] PreviewBytes { get; set; }
}

public class SteamWorkshopService
{
    private const uint AppId = 637650;
    private readonly AppStateService _appState;
    private readonly ILogger<SteamWorkshopService> _logger;

    private readonly Modmeta _modmeta;

    private readonly Timer _timer;
    private CallResult<CreateItemResult_t> _createItemCallback;

    private WorkshopItemDetails _currentDetails;
    private bool _isInitialized;
    private Action _onCreate;
    private Action<WorkshopItemDetails> _onQueryComplete;
    private Action _onUpdate;
    private CallResult<SteamUGCQueryCompleted_t> _queryItemCallback;
    private CallResult<SubmitItemUpdateResult_t> _submitItemUpdateCallback;
    private string _tempBinmod;
    private string _tempDirectory;
    private string _tempPreview;

    public SteamWorkshopService(
        Modmeta modmeta,
        AppStateService appState,
        ILogger<SteamWorkshopService> logger)
    {
        _modmeta = modmeta;
        _appState = appState;
        _logger = logger;
        _timer = new Timer(1000);
        _timer.Elapsed += (sender, args) => SteamAPI.RunCallbacks();
    }

    public bool Initialize()
    {
        if (_isInitialized)
        {
            return true;
        }

        var success = SteamAPI.Init();

        if (!success)
        {
            return false;
        }

        _isInitialized = true;
        return true;
    }

    public void Get(ulong itemId, Action<WorkshopItemDetails> onComplete)
    {
        _onQueryComplete = onComplete;

        _timer.Start();
        var fileId = new PublishedFileId_t(itemId);
        _queryItemCallback = CallResult<SteamUGCQueryCompleted_t>.Create(OnQueryComplete);
        var request = SteamUGC.CreateQueryUGCDetailsRequest(new[] {fileId}, 1);
        SteamUGC.SetReturnLongDescription(request, true);
        _queryItemCallback.Set(SteamUGC.SendQueryUGCRequest(request));
    }

    public void Publish(WorkshopItemDetails details, Action onComplete)
    {
        _currentDetails = details;
        _onCreate = onComplete;

        _timer.Start();
        _createItemCallback = CallResult<CreateItemResult_t>.Create(OnCreate);
        var appId = new AppId_t(AppId);
        var call = SteamUGC.CreateItem(appId, EWorkshopFileType.k_EWorkshopFileTypeCommunity);
        _createItemCallback.Set(call);
    }

    public void Update(WorkshopItemDetails details, Action onComplete)
    {
        _currentDetails = details;
        _onUpdate = onComplete;

        var fileId = new PublishedFileId_t(_appState.ActiveMod.ItemId);
        var appId = new AppId_t(AppId);
        var updateHandle = SteamUGC.StartItemUpdate(appId, fileId);

        SteamUGC.SetItemTitle(updateHandle, _appState.ActiveMod.WorkshopTitle);
        SteamUGC.SetItemDescription(updateHandle, details.Description);
        SteamUGC.SetItemVisibility(updateHandle, (ERemoteStoragePublishedFileVisibility)details.Visibility);

        _tempPreview = Path.GetTempFileName();
        File.WriteAllBytes(_tempPreview, details.PreviewBytes);
        SteamUGC.SetItemPreview(updateHandle, _tempPreview);

        if (details.Tags.Count > 0)
        {
            SteamUGC.SetItemTags(updateHandle, details.Tags);
        }

        _tempDirectory = $"{Path.GetTempPath()}\\{_appState.ActiveMod.Uuid}";
        Directory.CreateDirectory(_tempDirectory);
        _tempBinmod = $"{_tempDirectory}\\{Path.GetFileName(_appState.ActiveMod.Path)}";
        File.Copy(_appState.ActiveMod.Path, _tempBinmod);

        SteamUGC.SetItemContent(updateHandle, _tempDirectory);

        _timer.Start();
        _submitItemUpdateCallback = CallResult<SubmitItemUpdateResult_t>.Create(OnUpdate);
        var call = SteamUGC.SubmitItemUpdate(updateHandle, details.ChangeNotes);
        _submitItemUpdateCallback.Set(call);
    }

    private void OnCreate(CreateItemResult_t result, bool error)
    {
        _timer.Stop();
        _logger.LogInformation($"OnCreate called with result {result.m_eResult}\r\n");

        if (result.m_eResult == EResult.k_EResultOK)
        {
            _appState.ActiveMod.ItemId = result.m_nPublishedFileId.m_PublishedFileId;

            using var packer = new EbonyArchive(_appState.ActiveMod.Path);
            packer.UpdateFile("index.modmeta", _modmeta.Build(_appState.ActiveMod));
            packer.WriteToFile(_appState.ActiveMod.Path, LuminousGame.FFXV);

            Update(_currentDetails, _onCreate);
        }
    }

    private void OnUpdate(SubmitItemUpdateResult_t result, bool error)
    {
        _timer.Stop();
        _logger.LogInformation($"OnUpdate called with result {result.m_eResult}\r\n");

        if (result.m_eResult == EResult.k_EResultOK)
        {
            _appState.ActiveMod.IsUploaded = true;

            using var packer = new EbonyArchive(_appState.ActiveMod.Path);
            packer.UpdateFile("index.modmeta", _modmeta.Build(_appState.ActiveMod));
            packer.WriteToFile(_appState.ActiveMod.Path, LuminousGame.FFXV);

            _onUpdate();

            File.Delete(_tempPreview);
            File.Delete(_tempBinmod);
            Directory.Delete(_tempDirectory);
        }
    }

    private void OnQueryComplete(SteamUGCQueryCompleted_t result, bool error)
    {
        _timer.Stop();
        _logger.LogInformation($"OnQueryComplete called with result {result.m_eResult}\r\n");

        if (result.m_eResult == EResult.k_EResultOK)
        {
            SteamUGC.GetQueryUGCResult(result.m_handle, 0, out var details);

            if (details.m_eResult == EResult.k_EResultFileNotFound)
            {
                _appState.ActiveMod.ItemId = 0;
                _appState.ActiveMod.IsUploaded = false;
                using var packer = new EbonyArchive(_appState.ActiveMod.Path);
                packer.UpdateFile("index.modmeta", _modmeta.Build(_appState.ActiveMod));
                packer.WriteToFile(_appState.ActiveMod.Path, LuminousGame.FFXV);

                _onQueryComplete(new WorkshopItemDetails());
                return;
            }

            _onQueryComplete(new WorkshopItemDetails
            {
                Description = details.m_rgchDescription,
                Visibility = (ModVisibility)(int)details.m_eVisibility
            });
        }
    }
}